using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalDVASearch : Navigation
{
	

	public string state = "badHeading";
	protected float heading;
	protected float stepDistance = 50f;
	protected float findPrecisionThreshold = 0.5f;
	protected int dataPointNeededBeforeValidatingTheWaypoint = 0;
	protected int dataPointAquiredSinceLastValidation = 0;
	protected GameObject[] allChildsGameobject;


	protected List<DataPoint> currentSegmentMeasure = new List<DataPoint>();
	protected List<Vector3> targetsFound = new List<Vector3>();

    

	public override void Start(){
		base.Start();
		allChildsGameobject = GameObject.FindGameObjectsWithTag("Child");
		StartMission();

	}

	//Called on instantiation, thus replacing the parent's class default values
	private void Reset() {
		waypointValidationDistance = 0.5f;//in m
		waypointValidationAngularThreshold = 1f;
	}
	
	protected override void LoggingOverload(DataPoint currentPoint){
		currentSegmentMeasure.Add(currentPoint);
		dataPointAquiredSinceLastValidation++;
	}

	
	protected override void GenerateNextNavigationWaypoint(){
		
		if(mainWaypoints.Count<=0){
			SelectMode();
		}
		
		if(dataPointAquiredSinceLastValidation>=dataPointNeededBeforeValidatingTheWaypoint || ! isSearching){
			Pair<Vector3, Vector3> tmp = mainWaypoints.Dequeue();
			waypointIndicator.transform.position = tmp.First;
			waypointIndicator.transform.eulerAngles = tmp.Second;
			ctrl.SetWaypoint(waypointIndicator);
			dataPointAquiredSinceLastValidation = 0;
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
			GenerateHeadingWaypoint(-120,120,30);
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
			ctrl.enableGroundClearance = false;
			targetsFound[0] = GetCloseToGround(targetPos, 0.8f);
			state = "deliveringChild";
			break;

		case "deliveringChild"://TODO marche pas
			targetPos = targetsFound[0];
			targetsFound[0] = GetCloseToGround(targetPos, 0.8f);
			if(sensor.GetDistanceToGround()<1f){
				
				allChildsGameobject[0].GetComponent<Navigation>().StartMission();
					
				state = "childDelivered";
			}
			break;

		case "childDelivered":
			ctrl.enableGroundClearance = true;
			state = "??";//TODO more than one child
			break;
		default:
			Debug.Log("Unknown state : "+state);
			break;
		}
		Debug.Log("Next state : "+state);
		

		
	}


	//-----after zeroing in-----

	protected override List<Vector3> ComputeTargetsPositions(){
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
			basePosition.y-= (rangeFinderRange*0.8f);
			//we only go down for 80% of the theorical range to be safer from oscillation and bad measures
		}else{
			//near the ground
			basePosition.y -= (sensor.GetDistanceToGround()-requestedDistance);
		}

		AddWaypoint(basePosition,new Vector3(0,0,0));//TODO to test ? (ejecté du cremi avant de test)
		return basePosition;
	}
}
