using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalDVASearch : Navigation
{
	
	protected PayloadControler payloadCtrl;

	public string state = "badHeading";
	protected float heading;
	protected float stepDistance = 50f;
	protected float findPrecisionThreshold = 0.5f;
	protected int dataPointNeededBeforeValidatingTheWaypoint = 0;
	protected int dataPointAquiredSinceLastValidation = 0;


	protected List<DataPoint> currentSegmentMeasure = new List<DataPoint>();
	
	
	protected bool hadReachedClearanceRequest = true;
    public override void Awake(){
		base.Awake();
		payloadCtrl = GetComponent<PayloadControler>();
	}

	public override void Start(){
		base.Start();
		StartMission();

	}

	//Called on instantiation, thus replacing the parent's class default values
	private void Reset() {
		waypointValidationDistance = 0.5f;//in m
		waypointValidationAngularThreshold = 1f;
	}
	
	protected override void OnLoggingDataPoint(DataPoint currentPoint){
		currentSegmentMeasure.Add(currentPoint);
		dataPointAquiredSinceLastValidation++;
	}

	protected override bool ValidateWaypoint(Vector3 linearDifference,float angularDifference){
		bool hasAquiredEnoughPoints = dataPointAquiredSinceLastValidation>=dataPointNeededBeforeValidatingTheWaypoint;
		return base.ValidateWaypoint(linearDifference,angularDifference) && (hasAquiredEnoughPoints || ! isSearching);
	}

	public override void Update() {
		base.Update();
		if(enableGroundClearance){
			HandleGroundClearance();
		}
	}

	protected void HandleGroundClearance(){
		//Rise the target position if too close to the ground
		/*
		Vector3 currentPosition = sensor.GetPosition();
		float requiredGroundClearance = 5f;
		float currentClearance = sensor.GetDistanceToGround();
		if(currentClearance<requiredGroundClearance){
			float delta = requiredGroundClearance - currentClearance;
			
			if(waypointIndicator.transform.position.y < currentPosition.y){
				Vector3 nextPoint = currentPosition + Vector3.up * 3 * delta;

				navigationWaypoints.Peek().First += Vector3.up * 3 * delta;

				if(hadReachedClearanceRequest){
				
					AddNavigationWaypoint(nextPoint,waypointIndicator.transform.eulerAngles);
					UpdateWaypoint();
					Debug.Log("ground detected");
					hadReachedClearanceRequest = false;
				}

				
			}
		}
		*/
		Vector3 currentPosition = sensor.GetPosition();
		Vector3 nextTarget = waypointIndicator.transform.position;
		float requiredGroundClearance = 5f;
		float newTargetHeightAboveRequiredGroundClearance = 2f;
		float currentClearance = sensor.GetDistanceToGround();

		Vector2 targetDifference = new Vector2(nextTarget.x - currentPosition.x, nextTarget.z - currentPosition.z);
		
		List<Vector3> groundPoints = sensor.GetGroundAltitude(targetDifference,10f);
		if(groundPoints.Count>0){
			groundPoints.Reverse();

			bool hasGeneratedACorrection = false;
			foreach(Vector3 point in groundPoints){
				targetDifference = new Vector2(nextTarget.x - currentPosition.x, nextTarget.z - currentPosition.z);
				float hypothenuseMultiplier = Vector3.Distance(nextTarget,currentPosition)/targetDifference.magnitude;
				float pointDistance = Vector3.Distance(point-Vector3.up * point.y,currentPosition-Vector3.up * currentPosition.y);

				Vector3 plannedPoint = Vector3.MoveTowards(currentPosition,nextTarget,pointDistance*hypothenuseMultiplier);
				float plannedClearance = plannedPoint.y - point.y;
				if(plannedClearance<requiredGroundClearance){
					nextTarget = plannedPoint + Vector3.up * (requiredGroundClearance + newTargetHeightAboveRequiredGroundClearance - plannedClearance);
					if(AddNavigationWaypoint(nextTarget)){
						hasGeneratedACorrection = true;
						RaiseNearbyTargetToAtLeastThisAltitude(nextTarget,nextTarget.y);
					}
				}
			}
			if(hasGeneratedACorrection){
				UpdateWaypoint();
			}
		}



	}

	//Modify all the stored waypoint so that their altitude is at minimum "height"
	//This way we can prevent a waypoint from being stuck in the ground forever.
	protected void RaiseNearbyTargetToAtLeastThisAltitude(Vector3 centerPoint, float height){
		float affectedRadius = 1f;
		foreach(Pair<Vector3,Vector3> point in navigationWaypoints){
			centerPoint.y = point.First.y;
			if((Vector3.Distance(centerPoint,point.First)<affectedRadius) && (point.First.y < height)){
				Vector3 newPos = point.First;
				newPos.y = height;
				point.First = newPos;
			}
		}
		foreach(Pair<Vector3,Vector3> point in mainWaypoints){
			centerPoint.y = point.First.y;
			if((Vector3.Distance(centerPoint,point.First)<affectedRadius) && (point.First.y < height)){
				Vector3 newPos = point.First;
				newPos.y = height;
				point.First = newPos;
			}
		}

	}


	protected override void OnWaypointValidation(){
		base.OnWaypointValidation();
		hadReachedClearanceRequest = true;
	}
	

	
	protected override void GenerateNextNavigationWaypoint(){
		
		if(navigationWaypoints.Count<=0){
			if(mainWaypoints.Count<=0){
				SelectMode();
			}
			navigationWaypoints.Push(mainWaypoints.Dequeue());
			dataPointAquiredSinceLastValidation = 0;
		}

		//in case no waypoint are found, we don't change the order, ie : we hover in place
		if(navigationWaypoints.Count>0){
			UpdateWaypoint();
		}
		
	}


	//-----generating the waypoints------

	protected void GenerateHeadingWaypoint(float startHeading,float endHeading,float step){
		Vector3 position = sensor.GetPosition();
		dataPointNeededBeforeValidatingTheWaypoint = 1;
		for(float heading = startHeading; heading<=endHeading; heading += step){
			AddWaypoint(position,new Vector3(0,heading,0));
		}
		
	}

	protected float FindHeadingFromCurrentSegment(){
		if(currentSegmentMeasure.Count <= 0){
			Debug.Log("Error, can't find the heading when no measure are logged");
			return sensor.GetHeadingAsFloat();
		}

		float max = 0f;
		DataPoint maxPoint = currentSegmentMeasure[0];
		foreach(DataPoint currentPoint in currentSegmentMeasure){
			if (currentPoint.SensorPower > max)
			{
				max = currentPoint.SensorPower;
				maxPoint = currentPoint;
			}
		}
		return maxPoint.Orientation.y;
	}

	protected void StepForward(float stepSize){//go forward for a distance up to stepSize
		dataPointNeededBeforeValidatingTheWaypoint = 3;

		Vector3 nextPosition = sensor.GetPosition() + new Vector3(Mathf.Sin(heading * (Mathf.PI / 180)),0,Mathf.Cos(heading * (Mathf.PI / 180))) * stepSize;
		AddWaypoint(nextPosition,new Vector3(0,heading,0));
	}

	protected bool IsSignalIncreasing(){

		//not enough value
		if(currentSegmentMeasure.Count <3){
			return true;
		}
		int lastElementIndex = currentSegmentMeasure.Count - 1;

		//compare against the last two value to avoid some noises
		if(currentSegmentMeasure[lastElementIndex].SensorPower > currentSegmentMeasure[lastElementIndex-1].SensorPower 
			|| currentSegmentMeasure[lastElementIndex].SensorPower > currentSegmentMeasure[lastElementIndex-2].SensorPower)
		{
			return true;
		}
		return false;
	}

	
	protected void SelectMode(){
		if(stepDistance<findPrecisionThreshold/2 && isSearching){
			//we found the source
			state = "startDeliveringChild";
			ComputeTargetsPositions();
			isSearching = false;//TODO sauf si d'autres signal trouvé
		}
		
		
		switch(state){
		case "badHeading":
			GenerateHeadingWaypoint(-160,160,30);
			state = "findingHeading";
			break;

		case "findingHeading":
			heading = FindHeadingFromCurrentSegment();
			currentSegmentMeasure.Clear();
			GenerateHeadingWaypoint(heading-20,heading+20,5);
			state = "refiningHeading";
			break;

		case "refiningHeading":
			heading = FindHeadingFromCurrentSegment();
			currentSegmentMeasure.Clear();
			StepForward(stepDistance);
			state = "goodHeading";
			break;

		case "goodHeading":
			StepForward(stepDistance);
			if( ! IsSignalIncreasing()){
				stepDistance /= 2;
				currentSegmentMeasure.Clear();
				state = "badHeading";
			}
			break;


		case "startDeliveringChild":
			Vector3 targetPos = targetsFound[0];
			enableGroundClearance = false;
			targetsFound[0] = GetCloseToGround(targetPos, 0.8f);
			state = "deliveringChild";
			break;

		case "deliveringChild":
			targetPos = targetsFound[0];
			targetsFound[0] = GetCloseToGround(targetPos, 0.8f);
			if(sensor.GetDistanceToGround()<1f){
				payloadCtrl.ReleaseAChild(targetPos);
				state = "childDelivered";
			}
			break;

		case "childDelivered":
			enableGroundClearance = true;
			AddWaypoint(targetsFound[0] + Vector3.up * 2);
			state = "??";//TODO more than one child
			break;
		default:
			Debug.Log("Unknown state : "+state);
			break;
		}
		//Debug.Log("Next state : "+state);
		

		
	}


	//-----after zeroing in-----

	protected List<Vector3> ComputeTargetsPositions(){
		if(currentSegmentMeasure.Count != 0){
			DataPoint max = currentSegmentMeasure[0];
			foreach(DataPoint point in currentSegmentMeasure){
				if(max.SensorPower < point.SensorPower){
					max = point;
				}
			}
			targetsFound.Add(max.Position);
		}else{
			targetsFound.Add(sensor.GetPosition());
		}
		
        return targetsFound;
    }

	protected Vector3 GetCloseToGround(Vector3 basePosition,float requestedDistance){
		float rangeFinderRange = sensor.GetTheoricalRangefinderRange();

		if(requestedDistance>rangeFinderRange){
			Debug.Log("Invalid parameters : the drone can't hover at "
			+requestedDistance+"m from the ground when it can't detect the ground from above"
			+rangeFinderRange+"m");
			return basePosition;
		}

		if(sensor.GetDistanceToGround()>rangeFinderRange){
			//far from the ground
			basePosition.y-= (rangeFinderRange*0.5f);
			//we only go down for 50% of the theorical range to be safer from oscillation and bad measures
		}else{
			//near the ground
			basePosition.y -= (sensor.GetDistanceToGround()-requestedDistance);
		}

		AddWaypoint(basePosition);
		return basePosition;
	}
}
