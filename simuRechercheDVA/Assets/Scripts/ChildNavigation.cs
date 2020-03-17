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

    public override void Awake(){
        ConnectAttachment();
        base.Awake();
    }

    public override void Start(){
        useWaypointY = false;
        base.Start();
    }

    protected override void StartLog(){
		string path = "Assets/null";

		FileStream stream = new FileStream(path, FileMode.OpenOrCreate,FileAccess.Write);  
		fileLog = new StreamWriter(stream);

        samplingInterval = float.PositiveInfinity;
	}

    public override void StartMission(){
        waypointValidationDistance = 0.05f;//5cm
        waypointValidationAngularThreshold = 360f;
        ReleaseAttachment();
        base.StartMission();
    }

    public override void Update(){
        
        base.Update();
        if(debugClickStartMission){
            StartMission();
            debugClickStartMission = false;
        }
    }
    
    protected void ConnectAttachment(){
        if(! isAttached){
            attachJoint = this.gameObject.AddComponent<FixedJoint>();
            attachJoint.connectedBody = parentDrone.GetComponent<Rigidbody>();
            attachJoint.connectedMassScale = 0.09f;
            isAttached = true;
        }
    }
    protected void ReleaseAttachment(){
        if(isAttached){
            this.gameObject.GetComponent<WheelJointSpawner>().CreateJoints();
            Destroy(attachJoint);
        }
    }

    
    protected override void GenerateNextNavigationWaypoint(){
        if(navigationWaypoints.Count<=0){
            if(mainWaypoints.Count>0){
                navigationWaypoints.Push(mainWaypoints.Peek());
            }else{
                Debug.Log("Child doesn't know where to go");
            }
            
        }
        
        //in case no waypoint are found, we don't change the order, ie : we hover in place
		if(navigationWaypoints.Count>0){
			UpdateWaypoint();
		}
    }

	
	protected override List<Vector3> ComputeTargetsPositions(){
        //TODO
		List<Vector3> targets = new List<Vector3>();
		targets.Add(sensor.GetPosition());
        return targets;
    }
    
}
