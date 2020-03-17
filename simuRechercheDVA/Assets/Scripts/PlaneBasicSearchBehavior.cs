using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaneBasicSearchBehavior : Navigation
{
	public override void Start(){
		base.Start();
		StartMission();
	}

	protected MaxHeap allDataPoint = new MaxHeap(1000);



	protected override void OnLoggingDataPoint(DataPoint currentPoint){
		allDataPoint.Add(currentPoint);
	}

	
	
	public float bandWidth = 10f;
	
	protected override void GenerateMainWaypoint(){
		Vector3 nextPoint = researchZoneOrigin;
		
		bool reverse = false;
		while(nextPoint.x < researchZoneOrigin.x + researchZoneSize.x){
			if(reverse){
				nextPoint.z = researchZoneOrigin.z + researchZoneSize.z;
				AddWaypoint(nextPoint);
				nextPoint.z -= researchZoneSize.z;
				AddWaypoint(nextPoint);
				nextPoint.z -= 120;
				AddWaypoint(nextPoint);
			}else{
				nextPoint.z = researchZoneOrigin.z;
				AddWaypoint(nextPoint);
				nextPoint.z += researchZoneSize.z;
				AddWaypoint(nextPoint);
				nextPoint.z += 120;
				AddWaypoint(nextPoint);
			}
			nextPoint.x += bandWidth;
			reverse = !reverse;
		}
		Debug.Log("done generating waypoints. "+mainWaypoints.Count+" generated");
	}
	
	protected override void GenerateNextNavigationWaypoint(){
		if(mainWaypoints.Count<=0){
			if(isSearching){	
				isSearching = false;
				fileLog.Close();
				
				List<Vector3> allTarget = ComputeTargetsPositions();
				Debug.Log("estimated target at : "+allTarget[0]);
				
			}else{
				waypointIndicator.transform.position = Vector3.zero;
				ctrl.SetWaypoint(waypointIndicator);
			}
		}else{
			Pair<Vector3, Vector3> tmp = mainWaypoints.Dequeue();
			waypointIndicator.transform.position = tmp.First;
			waypointIndicator.transform.eulerAngles = tmp.Second;
			ctrl.SetWaypoint(waypointIndicator);
		}
	}
	
	protected override List<Vector3> ComputeTargetsPositions(){
		DataPoint strongestPoint = allDataPoint.Pop();
		float cutThreshold = 0.9f * strongestPoint.SensorPower;
		
		List<DataPoint> relevantPoint = new List<DataPoint>();
		relevantPoint.Add(strongestPoint);
		
		while(allDataPoint.Peek().SensorPower>cutThreshold){
			relevantPoint.Add(allDataPoint.Pop());
		}
		
		//we need some point cloud segmentation if we wants to be able to find 
		//multiple target in the same area, so for now we find only one target.
		Vector3 sumVector = Vector3.zero;
		for(int i=0;i<relevantPoint.Count;i++){
			sumVector = sumVector + relevantPoint[i].Position;
		}
		sumVector = sumVector / relevantPoint.Count;
		
		List<Vector3> outputList = new List<Vector3>();
		outputList.Add(sumVector);
		return outputList;
	}
	
}
