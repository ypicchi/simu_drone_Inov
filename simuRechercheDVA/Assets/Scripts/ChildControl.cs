using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class ChildControl : DroneControl
{
    protected DroneFlightSim sim;
    protected int numberOfThruster;


    public StreamWriter file;
	public bool isFileOpen = false;

    public override void Awake(){
		base.Awake();

		sim = GetComponent<DroneFlightSim>();
		DroneSimProperties simProperties = GetComponent<DroneSimProperties>();


		numberOfThruster = simProperties.ThrusterThrustValues.Length;
		
    }

    //TODO child control
    public override void ControlLoop(){

		
		

	}


    
    protected override void HandleKeyboardInput(){
        //Open file
		if (Input.GetKey(KeyCode.O)){
			file = new StreamWriter("pid_tunning.txt");
			isFileOpen = true;
		}

		//Close file
		if (Input.GetKey(KeyCode.C)){
			isFileOpen = false;
			file.Close();
		}


		

        float[] thrust = new float[4];
        for(int i = 0; i < numberOfThruster; i++){
			thrust[i]=0;
		}


		float force = 40f;


		
        if(Input.GetKey(KeyCode.Z)){ //------------------ Z
            thrust[0] += 2*force;
			thrust[1] += 2*force;
			thrust[2] += 2*force;
			thrust[3] += 2*force;
        }
		if (Input.GetKey(KeyCode.S)){//------------------ S
            thrust[0] -= 2*force;
			thrust[1] -= 2*force;
			thrust[2] -= 2*force;
			thrust[3] -= 2*force;
        }
		if (Input.GetKey(KeyCode.Q)){//------------------ Q
			thrust[0] -= force;
			thrust[1] += force;
			thrust[2] += force;
			thrust[3] -= force;
        }
		if (Input.GetKey(KeyCode.D)){//------------------ D
            thrust[0] += force;
			thrust[1] -= force;
			thrust[2] -= force;
			thrust[3] += force;
        }

        for(int i = 0; i<numberOfThruster; i++){
			sim.SetThrusterThrust(i, thrust[i]);
		}

    }
}
