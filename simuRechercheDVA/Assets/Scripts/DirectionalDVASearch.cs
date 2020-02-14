using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalDVASearch : Navigation
{
    
    //TODO stump from fixed wing
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
			
			waypointIndicator.transform.position = Vector3.zero;
		    ctrl.SetWaypoint(waypointIndicator);
			
		}else{
			waypointIndicator.transform.position = mainWaypoints.Dequeue();
			ctrl.SetWaypoint(waypointIndicator);
		}
	}
	
	protected override List<Vector3> computeTargetsPositions(){
        //TODO
        return new List<Vector3>();
    }
}
