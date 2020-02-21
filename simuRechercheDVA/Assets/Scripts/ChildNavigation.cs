using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildNavigation : Navigation
{
    //TODO child Navigation
    protected override void GenerateNextNavigationWaypoint(){

    }
	
	protected override List<Vector3> computeTargetsPositions(){
        //TODO
		List<Vector3> targets = new List<Vector3>();
		targets.Add(sensor.GetPosition());
        return targets;
    }
    
}
