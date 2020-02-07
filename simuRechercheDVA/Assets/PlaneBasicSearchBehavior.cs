using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaneBasicSearchBehavior : Navigation
{
	
	
	// Start is called before the first frame update
	public override void Start()
	{
		base.Start ();
		ctrl = GetComponent<FixedWingDroneControl>();
		ctrl.SetWaypoint(waypointIndicator);
	
		
	}

	
	
	public float bandWidth = 10f;
	
	protected override void GenerateMainWaypoint(){
		Vector3 nextPoint = researchZoneOrigin;
		
		bool reverse = false;
		while(nextPoint.x < researchZoneOrigin.x + researchZoneSize.x){
			if(reverse){
				nextPoint.z = researchZoneOrigin.z + researchZoneSize.z;
				mainWaypoints.Enqueue(nextPoint);
				nextPoint.z -= researchZoneSize.z;
				mainWaypoints.Enqueue(nextPoint);
				nextPoint.z -= 120;
				mainWaypoints.Enqueue(nextPoint);
			}else{
				nextPoint.z = researchZoneOrigin.z;
				mainWaypoints.Enqueue(nextPoint);
				nextPoint.z += researchZoneSize.z;
				mainWaypoints.Enqueue(nextPoint);
				nextPoint.z += 120;
				mainWaypoints.Enqueue(nextPoint);
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
				
				List<Vector3> allTarget = computeTargetsPositions();
				Debug.Log("estimated target at : "+allTarget[0]);
				
			}else{
				waypointIndicator.transform.position = Vector3.zero;
				ctrl.SetWaypoint(waypointIndicator);
			}
		}else{
			waypointIndicator.transform.position = mainWaypoints.Dequeue();
			ctrl.SetWaypoint(waypointIndicator);
		}
	}
	
	protected override List<Vector3> computeTargetsPositions(){
		DataPoint strongestPoint = allDataPoint.Pop();
		float cutThreshold = 0.9f * strongestPoint.GetPower();
		
		List<DataPoint> relevantPoint = new List<DataPoint>();
		relevantPoint.Add(strongestPoint);
		
		while(allDataPoint.Peek().GetPower()>cutThreshold){
			relevantPoint.Add(allDataPoint.Pop());
		}
		
		//we need some point cloud segmentation if we wants to be able to find 
		//multiple target in the same area, so for now we find only one target.
		Vector3 sumVector = Vector3.zero;
		for(int i=0;i<relevantPoint.Count;i++){
			sumVector = sumVector + relevantPoint[i].GetPosition();
		}
		sumVector = sumVector / relevantPoint.Count;
		
		List<Vector3> outputList = new List<Vector3>();
		outputList.Add(sumVector);
		return outputList;
	}
	
}
