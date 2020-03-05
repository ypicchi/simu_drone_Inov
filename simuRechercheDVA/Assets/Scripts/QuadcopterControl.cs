using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
public class QuadcopterControl : DroneControl
{

    protected DroneFlightSim sim;

	protected float mass;
	protected float[] thrust;
	private float thrustEquilibrium = 211.1668f;
	private float thrustMax;
	private float thrustMin;
	private float heightThreshold;

	public PID altitudePid = new PID(3f, 0.03f, 0.05f);
	//public PID altitudePid = new PID(3f, 0.05f, 0.01f);


	public PID speedPid = new PID(2f, 0.02f, 0f);
	//public PID speedPid = new PID(400f, 800f, 10f);


	public PID yawPid = new PID(0.2f, 0f, 0.3f);
	private float yawThrustLimit = 10f;


	private float pitchTarget = 0f;
	public PID pitchPid = new PID(10f, 0f, 2f);
	private float pitchThrustLimit = 100f;

	public float rollTarget = 0f;
	public PID rollPid = new PID(10f, 0f, 2f);
	private float rollThrustLimit = 100f;

	public StreamWriter file;
	private Boolean isFileOpen = false;

	protected int numberOfThruster;
	protected bool needToRunManual = false;

	protected BangBangVector3 bangbang;

	public PID xPosPid = new PID(1f, 0f, 0f);
	public PID yPosPid = new PID(1f, 0f, 0f);
	public PID zPosPid = new PID(1f, 0f, 0f);
	public PID xSpeedPid = new PID(1f, 0f, 0f);
	public PID ySpeedPid = new PID(1f, 0f, 0f);
	public PID zSpeedPid = new PID(1f, 0f, 0f);

    // Start is called before the first frame update
	public override void Start()
	{
		InitVariables();

		base.Start();
		sim = GetComponent<DroneFlightSim>();
		DroneSimProperties simProperties = GetComponent<DroneSimProperties>();
		thrust = simProperties.ThrusterThrustValues;
		numberOfThruster = simProperties.ThrusterThrustValues.Length;
		mass = GetComponent<Rigidbody>().mass;

		/* 
		Debug.Log("Cible : " + target);
		Debug.Log("Cible_position : " + target.transform.position);
		Debug.Log("Cible_angle : " + target.transform.eulerAngles.y);
		*/
		bangbang = new BangBangVector3(Vector3.one * 10f, Vector3.one * 1f);
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

	private float GetThrustDifferenceToYaw(){

		float thrustDifference = 0f;

		float targetAngle = target.transform.eulerAngles.y;
		float actualAngle = sensor.GetUnityYaw();

		float targetAngleDifference = 0f;
		float actualAngleDifference = Mathf.DeltaAngle(actualAngle, targetAngle);
		
		thrustDifference = yawPid.Update(targetAngleDifference, actualAngleDifference, Time.deltaTime);

		if( Math.Abs(thrustDifference) > yawThrustLimit){
			float sign  = Math.Abs(thrustDifference) / thrustDifference;
			thrustDifference = yawThrustLimit * sign;
		}

		Debug.Log(Time.fixedTime);
		if(isFileOpen){
			file.WriteLine(Time.fixedTime + ";" + targetAngle + ";" + actualAngle + ";"  + targetAngleDifference + ";" + actualAngleDifference + ";" + thrustDifference);
		}

		return thrustDifference;
	}

	private float GetThrustDifferenceToPitch(float targetPitch){

		float thrustDifference = 0f;
		float actualPitch = sensor.GetPitch();

		thrustDifference = pitchPid.Update(targetPitch, actualPitch, Time.deltaTime);

		if( Math.Abs(thrustDifference) > pitchThrustLimit){
			float sign  = Math.Abs(thrustDifference) / thrustDifference;
			thrustDifference = pitchThrustLimit * sign;
		}

		Debug.Log(Time.fixedTime);
		if(isFileOpen){
			file.WriteLine(Time.fixedTime + ";" + targetPitch + ";" + actualPitch + ";" + thrustDifference);
		}

		return thrustDifference;
	}


	private float GetThrustDifferenceToRoll(float targetRoll){

		float thrustDifference = 0f;

		// Unity Z rotation is not like Roll
		float actualRoll = -1 * sensor.GetRoll();

		thrustDifference = rollPid.Update(targetRoll, actualRoll, Time.deltaTime);

		if( Math.Abs(thrustDifference) > rollThrustLimit){
			float sign  = Math.Abs(thrustDifference) / thrustDifference;
			thrustDifference = rollThrustLimit * sign;
		}

		Debug.Log(Time.fixedTime);
		if(isFileOpen){
			file.WriteLine(Time.fixedTime + ";" + targetRoll + ";" + actualRoll + ";" + thrustDifference);
		}

		return thrustDifference;
	}

	private void GoToWaypoint(){
		if(hasTarget==false){
			return;
		}
		
		// Height calculation
		// float heightDifference = target.transform.position.y - sensor.GetPosition().y;

		//float mainThrust = thrustEquilibrium;
		//float thrustDifference = 0f;
		//thrustDifference = GetThrustDifferenceToPitch( pitchTarget );
		//thrustDifference = GetThrustDifferenceToRoll( rollTarget ); 

		// mainThrust = GetThrustToReachWaypointAltitude();
		// mainThrust = GetThrustToStabilizeAltitude();
		// thrustDifference = GetThrustDifferenceToYaw();

		/*
		// YAW
		sim.SetThrusterThrust(0, mainThrust - thrustDifference);
		sim.SetThrusterThrust(1, mainThrust + thrustDifference);
		sim.SetThrusterThrust(2, mainThrust - thrustDifference);
		sim.SetThrusterThrust(3, mainThrust + thrustDifference);
		*/

		/*
		// PITCH
		sim.SetThrusterThrust(0, mainThrust + thrustDifference);
		sim.SetThrusterThrust(1, mainThrust + thrustDifference);
		sim.SetThrusterThrust(2, mainThrust - thrustDifference);
		sim.SetThrusterThrust(3, mainThrust - thrustDifference);
		*/

		/*
		// ROLL
		sim.SetThrusterThrust(0, mainThrust + thrustDifference);
		sim.SetThrusterThrust(1, mainThrust - thrustDifference);
		sim.SetThrusterThrust(2, mainThrust - thrustDifference);
		sim.SetThrusterThrust(3, mainThrust + thrustDifference);
		*/

		//TODO : mix forces 

		/*	
		for(int i = 0; i < numberOfThruster; i++){
			thrust[i] = mainThrust; 
			sim.SetThrusterThrust(i, thrust[i]);
		}
		*/
		
		if(true){
			Vector3[] tmp = bangbang.GetTarget(Time.time);

			//TODO : comprendre bangbang
			Debug.Log(tmp);

			Vector3 expectedPosition = tmp[0];
			Vector3 expectedSpeed = tmp[1];
			Vector3 currentPosition = sensor.GetPosition();
			Vector3 currentSpeed = sensor.GetSpeed();

			//Position is in the world reference
			Vector3 speedCorrection = Vector3.zero;
			speedCorrection[0] = xPosPid.Update(expectedPosition[0], currentPosition[0], Time.deltaTime);
			speedCorrection[1] = yPosPid.Update(expectedPosition[1], currentPosition[1], Time.deltaTime);
			speedCorrection[2] = zPosPid.Update(expectedPosition[2], currentPosition[1], Time.deltaTime);
			
			//Speed is in the drone's reference
			speedCorrection = transform.InverseTransformDirection(speedCorrection);
			Vector3 accelerationCommand = Vector3.zero;
			accelerationCommand[0] = xSpeedPid.Update(expectedSpeed[0]-speedCorrection[0], currentSpeed[0], Time.deltaTime);
			accelerationCommand[1] = ySpeedPid.Update(expectedSpeed[1]-speedCorrection[1], currentSpeed[1], Time.deltaTime);
			accelerationCommand[2] = zSpeedPid.Update(expectedSpeed[2]-speedCorrection[2], currentSpeed[1], Time.deltaTime);
			
			//Now we have the acceleration required.
			//We can find the angle and the trust to apply

			//TODO add the gravity in the acceleration/force required

			//TODO find in what direction does the force go (thrust direction or force direction ??)
			//this will change the pitch and roll required


			//Forces are in the drone's reference
			Vector3 forceVector = accelerationCommand/mass;
			float totalThrust = forceVector.magnitude;

			Vector2 pitchAxis = new Vector2(forceVector.z, forceVector.y);
			float requiredPitch = Vector2.SignedAngle(Vector2.right, pitchAxis);
			//positive pitch means an acceleration in negative z


			Vector2 rollAxis = new Vector2(forceVector.x, forceVector.y);
			float requiredRoll = Vector2.SignedAngle(Vector2.right, rollAxis);
			//positive roll = roll to the left, so an acceleration in negative x



			//float targetHeading = target.transform.eulerAngles.y;
			//float currentHeading = Sensor.getHeadingAsfloat();
			


	
			//Debug.Log(currentPosition + ";" + expectedPosition);
			//Debug.Log(Time.fixedTime + " ; pitch:" +  requiredPitch + " ; roll:" + requiredRoll);



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
