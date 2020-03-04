using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class ChildNavigation : Navigation
{

    public GameObject parentDrone;
    protected FixedJoint attachJoint;
    protected bool isAttached = false;

    public bool debugClickStartMission = false;

    public override void Start(){
        base.Start();
        ConnectAttachment();
    }

    protected override void StartLog(){
		string path = "Assets/null";

		FileStream stream = new FileStream(path, FileMode.OpenOrCreate,FileAccess.Write);  
		fileLog = new StreamWriter(stream);

        samplingInterval = float.PositiveInfinity;
	}

    public override void StartMission(){
        ReleaseAttachment();
    }

    public override void Update(){
        base.Update();
        if(debugClickStartMission){
            StartMission();
        }
    }
    
    protected void ConnectAttachment(){
        if(! isAttached){
            attachJoint = this.gameObject.AddComponent<FixedJoint>();
            attachJoint.connectedBody = parentDrone.GetComponent<Rigidbody>();
            isAttached = true;
        }
    }
    protected void ReleaseAttachment(){
        if(isAttached){
            Destroy(attachJoint);
        }
    }

    //TODO child Navigation
    protected override void GenerateNextNavigationWaypoint(){

    }
	
	protected override List<Vector3> ComputeTargetsPositions(){
        //TODO
		List<Vector3> targets = new List<Vector3>();
		targets.Add(sensor.GetPosition());
        return targets;
    }
    
}
