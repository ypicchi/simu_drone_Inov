using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalDVASearch : Navigation
{
    
    //TODO stump from fixed wing
    public float bandWidth = 10f;
    protected override void GenerateMainWaypoint(){
		Vector3 nextPoint = researchZoneOrigin;
		
		
		//if(nextPoint.x < researchZoneOrigin.x + researchZoneSize.x){
			
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
}
