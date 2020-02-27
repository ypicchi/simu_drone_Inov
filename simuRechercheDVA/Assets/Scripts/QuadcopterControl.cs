using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
public class QuadcopterControl : DroneControl
{

    protected DroneFlightSim sim;

	protected float[] thrust;
	private float thrustEquilibrium = 211.1668f;
	private float thrustMax;
	private float thrustMin;
	private float heightThreshold;

	public PID altitudePid = new PID(3f, 0.03f, 0.05f);
	//public PID altitudePid = new PID(3f, 0.05f, 0.01f);


	public PID speedPid = new PID(2f, 0.02f, 0f);
	//public PID speedPid = new PID(400f, 800f, 10f);


	public StreamWriter file;
	public Boolean isFileOpen = false;

	protected int numberOfThruster;
	protected bool needToRunManual = false;

    // Start is called before the first frame update
	public override void Start()
	{
		InitVariables();

		base.Start();
		sim = GetComponent<DroneFlightSim>();
		DroneSimProperties simProperties = GetComponent<DroneSimProperties>();
		thrust = simProperties.ThrusterThrustValues;
		numberOfThruster = simProperties.ThrusterThrustValues.Length;

	}

	// Variables depending of other variables
    private void InitVariables(){

		// Maximum thrust could be more powerful tho
		thrustMax = thrustEquilibrium * 2;

		// Thrusting in the opposite direction to go down is too extreme
		// Using no thrust is good enough
		thrustMin = 0;

	}

    public override void ControlLoop(){

		if(hasTarget){
			GoToWaypoint();
		}

	}


	private float GetThrustToReachWaypointAltitude(){

		float targetAltitude = target.transform.position.y;
		float actualAltitude = sensor.GetPosition().y;
		float actualVerticalSpeed = sensor.GetVerticalSpeed();

		float targetVerticalSpeed = altitudePid.Update(targetAltitude, actualAltitude, Time.deltaTime);
		float thrustCommand = speedPid.Update(targetVerticalSpeed, actualVerticalSpeed, Time.deltaTime);

		//Log
		Debug.Log(Time.fixedTime);
		if(isFileOpen){
			file.WriteLine(Time.fixedTime + ";" + targetAltitude + ";" + actualAltitude + ";" + targetVerticalSpeed + ";" + actualVerticalSpeed + ";" + thrustCommand);
		}

		return thrustCommand;
	}


	private float GetThrustToStabilizeAltitude(){
		
		float targetSpeed = 0f;
		float thrustCommand = speedPid.Update(targetSpeed, sensor.GetVerticalSpeed(), Time.deltaTime);

		//Log
		Debug.Log(Time.fixedTime);
		if(isFileOpen){
			file.WriteLine(Time.fixedTime + ";" + targetSpeed + ";" + sensor.GetVerticalSpeed() + ";" + thrustCommand + ";" + sensor.GetPosition().y);
		}
		
		return thrustCommand;
	}

	private void GoToWaypoint(){
		if(hasTarget==false){
			return;
		}
		
		// Height calculation
		//float heightDifference = target.transform.position.y - sensor.GetPosition().y;

		float thrustVertical;

		thrustVertical = GetThrustToReachWaypointAltitude();
		//thrustVertical = GetThrustToStabilizeAltitude();
		
		for(int i = 0; i < numberOfThruster; i++){
			thrust[i] = thrustVertical; 
			sim.SetThrusterThrust(i, thrust[i]);
		}
		
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

		if (Input.GetKey(KeyCode.LeftShift)){
			for(int i = 0; i < numberOfThruster; i++){
				thrust[i] += 0.05f;
				sim.SetThrusterThrust(i, thrust[i]);
			}
        }
		else if (Input.GetKey(KeyCode.LeftControl)){
			for(int i = 0; i<numberOfThruster; i++){
				thrust[i] -= 0.05f;
				sim.SetThrusterThrust(i, thrust[i]);
			}
        }

		float averageThrust = 0;
		for(int i = 0; i < numberOfThruster;i++){
			averageThrust += thrust[i];
		}
		averageThrust /= numberOfThruster;


		for(int i = 0; i < numberOfThruster; i++){
			thrust[i] = averageThrust;
			sim.SetThrusterThrust(i, thrust[i]);
		}


		float turnPower = 3f;
		needToRunManual = true;
		
        if(Input.GetKey(KeyCode.Z)){      //------------------ Z
            thrust[0] -= turnPower;
			thrust[1] -= turnPower;
			thrust[2] += turnPower;
			thrust[3] += turnPower;
			for(int i = 0; i < numberOfThruster; i++){
				sim.SetThrusterThrust(i, thrust[i]);
			}
        }
		else if (Input.GetKey(KeyCode.S)){//------------------ S
            thrust[0] += turnPower;
			thrust[1] += turnPower;
			thrust[2] -= turnPower;
			thrust[3] -= turnPower;
			for(int i = 0; i < numberOfThruster; i++){
				sim.SetThrusterThrust(i, thrust[i]);
			}
        }
		else if (Input.GetKey(KeyCode.Q)){//------------------ Q
			thrust[0] -= turnPower;
			thrust[1] += turnPower;
			thrust[2] += turnPower;
			thrust[3] -= turnPower;
			for(int i = 0; i < numberOfThruster; i++){
				sim.SetThrusterThrust(i, thrust[i]);
			}
        }
		else if (Input.GetKey(KeyCode.D)){//------------------ D
            thrust[0] += turnPower;
			thrust[1] -= turnPower;
			thrust[2] -= turnPower;
			thrust[3] += turnPower;
			for(int i = 0; i < numberOfThruster; i++){
				sim.SetThrusterThrust(i, thrust[i]);
			}
        }
		else if (Input.GetKey(KeyCode.A)){//------------------ A
            thrust[0] -= turnPower;
			thrust[1] += turnPower;
			thrust[2] -= turnPower;
			thrust[3] += turnPower;
			for(int i = 0; i < numberOfThruster; i++){
				sim.SetThrusterThrust(i, thrust[i]);
			}
        }
		else if (Input.GetKey(KeyCode.E)){//------------------ E
            thrust[0] += turnPower;
			thrust[1] -= turnPower;
			thrust[2] += turnPower;
			thrust[3] -= turnPower;
			for(int i = 0; i < numberOfThruster; i++){
				sim.SetThrusterThrust(i, thrust[i]);
			}
        }
		else{
			needToRunManual = false;
		}
	}
}
