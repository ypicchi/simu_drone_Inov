using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalDVASearch : Navigation
{

	protected string state = "badHeading";
	protected float heading;
	protected float stepDistance = 20f;
	protected int dataPointNeededBeforeValidatingTheWaypoint = 0;


	List<DataPoint> currentSegmentMeasure = new List<DataPoint>();
    
	//TOOD : code sale, il faut récupérer la version MAJ	
	public override void Start()
	{
		base.Start ();
		ctrl = GetComponent<DroneControl>();
		ctrl.SetWaypoint(waypointIndicator);
	}
	
	protected override void LoggingOverload(DataPoint currentPoint){
		currentSegmentMeasure.Add(currentPoint);
		dataPointNeededBeforeValidatingTheWaypoint--;
	}

    //TODO stump from fixed wing
    protected override void GenerateMainWaypoint(){
		//if(nextPoint.x < researchZoneOrigin.x + researchZoneSize.x)
		//AddWaypoint(nextPoint);
		//Debug.Log("done generating waypoints. "+mainWaypoints.Count+" generated");
	}
	
	protected override void GenerateNextNavigationWaypoint(){
		if(mainWaypoints.Count<=0){
			
			SelectMode();
			
		}else if(dataPointNeededBeforeValidatingTheWaypoint<=0){
			Pair<Vector3, Vector3> tmp = mainWaypoints.Dequeue();
			waypointIndicator.transform.position = tmp.First;
			waypointIndicator.transform.eulerAngles = tmp.Second;
			ctrl.SetWaypoint(waypointIndicator);
		}
	}
	
	protected override List<Vector3> computeTargetsPositions(){
        //TODO
		List<Vector3> targets = new List<Vector3>();
		targets.Add(sensor.GetPosition());
        return targets;
    }



	protected void GenerateHeadingWaypoint(float startHeading,float endHeading,float step){
		Vector3 position = sensor.GetPosition();
		dataPointNeededBeforeValidatingTheWaypoint = 0;
		for(float heading = startHeading; heading<endHeading; endHeading += step){
			AddWaypoint(position,new Vector3(0,heading,0));
			dataPointNeededBeforeValidatingTheWaypoint++;
		}
		
	}

	protected float FindHeadingFromCurrentSegment(){
		if(currentSegmentMeasure.Count <= 0){
			Debug.Log("Error, can't find the heading when no measure are logged");
			return Vector2.SignedAngle(Vector2.right, sensor.GetHeading());
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
		dataPointNeededBeforeValidatingTheWaypoint = 3 - currentSegmentMeasure.Count;
		
		Vector3 nextPosition = sensor.GetPosition() + new Vector3(Mathf.Cos(heading),0,Mathf.Sin(heading)) * stepSize;
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

	
	protected void SelectMode(){//TODO to test
		switch(state){
			case "badHeading":
				GenerateHeadingWaypoint(-90,90,30);
				state = "findingHeading";
				break;
			case "findingHeading":
				heading = FindHeadingFromCurrentSegment();
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
				if(IsSignalIncreasing()){
					StepForward(stepDistance);
				}else{
					stepDistance /= 2;
					currentSegmentMeasure.Clear();
					state = "badHeading";
				}
				break;
			default:
				break;
		}
		
	}
}
