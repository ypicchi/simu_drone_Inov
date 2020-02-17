using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalDVASearch : Navigation
{

	
	List<DataPoint> currentSegmentMeasure = new List<DataPoint>();
    protected override void LoggingOverload(DataPoint currentPoint){
		currentSegmentMeasure.Add(currentPoint);
	}

    //TODO stump from fixed wing
    protected override void GenerateMainWaypoint(){
		//if(nextPoint.x < researchZoneOrigin.x + researchZoneSize.x)
		//AddWaypoint(nextPoint);
		Debug.Log("done generating waypoints. "+mainWaypoints.Count+" generated");
	}
	
	protected override void GenerateNextNavigationWaypoint(){
		if(mainWaypoints.Count<=0){
			
			waypointIndicator.transform.position = Vector3.zero;
			waypointIndicator.transform.eulerAngles = Vector3.zero;
		    ctrl.SetWaypoint(waypointIndicator);
			
		}else{
			Pair<Vector3, Vector3> tmp = mainWaypoints.Dequeue();
			waypointIndicator.transform.position = tmp.First;
			waypointIndicator.transform.eulerAngles = tmp.Second;
			ctrl.SetWaypoint(waypointIndicator);
		}
	}
	
	protected override List<Vector3> computeTargetsPositions(){
        //TODO
        return new List<Vector3>();
    }



	protected void GenerateHeadingWaypoint(float startHeading,float endHeading,float step){
		Vector3 position = sensor.GetPosition();
		for(float heading = startHeading; heading<endHeading; endHeading += step){
			AddWaypoint(position,new Vector3(0,heading,0));
		}
	}

	protected float FindHeadingFromCurrentSegment(){
		float max = 0f;
		DataPoint maxPoint;
		foreach(DataPoint currentPoint in currentSegmentMeasure){
			if (currentPoint.GetPower() > max)
			{
				max = currentPoint.GetPower();
				maxPoint = currentPoint;
			}
		}

		//return maxPoint.heading;
		return 0f;

		//currentSegmentMeasure.Clear();
	}
}
