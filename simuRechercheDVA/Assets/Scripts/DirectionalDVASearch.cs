using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalDVASearch : Navigation
{
	

	protected string state = "badHeading";
	protected float heading;
	protected float stepDistance = 20f;
	protected int dataPointNeededBeforeValidatingTheWaypoint = 0;
	protected int dataPointAquiredSinceLastValidation = 0;


	protected List<DataPoint> currentSegmentMeasure = new List<DataPoint>();
    
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
		if(dataPointAquiredSinceLastValidation>=dataPointNeededBeforeValidatingTheWaypoint){
			Pair<Vector3, Vector3> tmp = mainWaypoints.Dequeue();
			waypointIndicator.transform.position = tmp.First;
			waypointIndicator.transform.eulerAngles = tmp.Second;
			ctrl.SetWaypoint(waypointIndicator);
			dataPointAquiredSinceLastValidation = 0;
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

	
	protected void SelectMode(){//TODO to test
		//Debug.Log("Previous state : "+state);
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
				
			default:
				break;
		}

		Debug.Log("Next state : "+state);
	}
}
