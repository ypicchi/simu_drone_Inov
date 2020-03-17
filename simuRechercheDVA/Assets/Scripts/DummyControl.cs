using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
public class DummyControl : DroneControl
{


    //private Rigidbody rb;
    //public float speed = 10;

    protected BangBangVector3 bangbang;

    public StreamWriter file;
    private Boolean isFileOpen = false;

    public Vector3 lastFrameSpeed = new Vector3(0,0,0);
    public int axisRecorded = 2;

    public bool IsFileOpen { 
        get{
            return isFileOpen;
        } 
        set{
            if(value != isFileOpen){
                if(value){
                    file = new StreamWriter("pid_tunning.txt");
                }else{
                    file.Close();
                }
            }
            isFileOpen = value;
        }
    }

    public override void Awake(){
        base.Awake();
        bangbang = new BangBangVector3(Vector3.one * 6f, Vector3.one * 6f);
    }
    
    public override void SetWaypoint(GameObject waypointIndicator){
		base.SetWaypoint(waypointIndicator);
		bangbang.StartMovement(sensor.GetPosition(),target.transform.position,lastFrameSpeed,Time.time);
	}

    public override void ControlLoop(){
        modeDisplayText = ("mode : auto");

        
        

        transform.rotation = Quaternion.RotateTowards(transform.rotation, target.transform.rotation, 10);


        Vector3 currentPosition = sensor.GetPosition();
		Vector3 currentSpeed = lastFrameSpeed;


        if(!bangbang.IsMoving){
			bangbang.StartMovement(currentPosition,target.transform.position,currentSpeed,Time.time);
		}
        
		
		Vector3[] tmp = bangbang.GetTarget(Time.time);
		Vector3 expectedPosition = tmp[0];
		Vector3 expectedSpeed = tmp[1];

        
        
        Vector3 nextPosition = transform.position + expectedSpeed*Time.deltaTime;
        transform.position = nextPosition;
        //transform.position = expectedPosition;


        if(isFileOpen){
			int axis = axisRecorded;
			file.WriteLine(Time.fixedTime + ";" + expectedPosition[axis] 
				+ ";" + currentPosition[axis] + ";" + (expectedSpeed[axis]/*+speedCorrection[axis]*/)
				+ ";" + currentSpeed[axis] + ";" + 0);
		}

        lastFrameSpeed = expectedSpeed;
        
    }

    protected override void HandleKeyboardInput(){
        //Open file
		if (Input.GetKey(KeyCode.O)){
			IsFileOpen = true;
            Debug.Log("recording...");
		}

		//Close file
		if (Input.GetKey(KeyCode.C)){
			IsFileOpen = false;
            Debug.Log("recording saved");
		}
    }
}
