using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
public class QuadcopterControl : DroneControl
{

    protected DroneFlightSim sim;
	protected PayloadControler payloadCtrl;

	protected float droneMass;
	protected float[] thrust;

	

	public PID altitudePid = new PID(3f, 0.03f, 0.05f);
	//public PID altitudePid = new PID(3f, 0.05f, 0.01f);


	public PID speedPid = new PID(2f, 0.02f, 0f);
	//public PID speedPid = new PID(400f, 800f, 10f);


	public PID yawPid = new PID(0.2f, 0f, 0.3f);
	private float yawThrustLimit = 10f;


	private float pitchTarget = 0f;
	private PID pitchPid = new PID(10f, 0f, 2f);
	private float pitchThrustLimit = 100f;

	private float rollTarget = 0f;
	private PID rollPid = new PID(10f, 0f, 2f);
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

	
    public int axisRecorded = 2;

	//Awake is made to initialize variables. It is called before any Start()
	public override void Awake(){
		base.Awake();

		payloadCtrl = GetComponent<PayloadControler>();
		droneMass = GetComponent<Rigidbody>().mass;

		sim = GetComponent<DroneFlightSim>();
		DroneSimProperties simProperties = GetComponent<DroneSimProperties>();


		thrust = simProperties.ThrusterThrustValues;
		numberOfThruster = simProperties.ThrusterThrustValues.Length;

		/* 
		Debug.Log("Cible : " + target);
		Debug.Log("Cible_position : " + target.transform.position);
		Debug.Log("Cible_angle : " + target.transform.eulerAngles.y);
		*/
		bangbang = new BangBangVector3(Vector3.one * 10f, Vector3.one * 6f);
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


	//TODO : change name : actual => current
	private float GetThrustDifferenceToYaw( float targetAngle ){

		float thrustDifference = 0f;

		//float actualAngle = sensor.GetUnityYaw(); //good but dirty
		float actualAngle = sensor.GetHeadingAsFloat(); //OK-ish
	
		float targetAngleDifference = 0f;
		float actualAngleDifference = Mathf.DeltaAngle(actualAngle, targetAngle);
		
		thrustDifference = yawPid.Update(targetAngleDifference, actualAngleDifference, Time.deltaTime);

		if( Math.Abs(thrustDifference) > yawThrustLimit){
			float sign  = Math.Abs(thrustDifference) / thrustDifference;
			thrustDifference = yawThrustLimit * sign;
		}

		/*
		Debug.Log(Time.fixedTime);
		if(isFileOpen){
			file.WriteLine(Time.fixedTime + ";" + targetAngle + ";" + actualAngle + ";"  + targetAngleDifference + ";" + actualAngleDifference + ";" + thrustDifference);
		}
		*/

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

		/*
		Debug.Log(targetPitch + " ; " + actualPitch);
		if(isFileOpen){
			file.WriteLine(Time.fixedTime + ";" + targetPitch + ";" + actualPitch + ";" + thrustDifference);
		}
		*/

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

		/*
		Debug.Log(Time.fixedTime);
		if(isFileOpen){
			file.WriteLine(Time.fixedTime + ";" + targetRoll + ";" + actualRoll + ";" + thrustDifference);
		}
		*/

		return thrustDifference;
	}

	public override void SetWaypoint(GameObject waypointIndicator){
		base.SetWaypoint(waypointIndicator);
		bangbang.StartMovement(sensor.GetPosition(),target.transform.position,transform.TransformDirection(sensor.GetSpeed()),Time.time);

	}

	private void GoToWaypoint(){
		if(hasTarget==false){
			return;
		}

		Vector3 currentPosition = sensor.GetPosition();
		Vector3 currentSpeed = transform.TransformDirection(sensor.GetSpeed());
		/*
		if(enableGroundClearance){
			//Rise the target position and velocity if too close to the ground
			float requiredGroundClearance = 2f;
			if(sensor.GetDistanceToGround()<requiredGroundClearance){
				float currentClearance = sensor.GetDistanceToGround();
				float delta = requiredGroundClearance - currentClearance;
				
				if(target.transform.position.y < currentPosition.y + 2*delta){//TODO marche pas. Le waypoint se met pas a jours
					//target.transform.Translate(Vector3.up * delta * 2, Space.World);//move the target up
					target.transform.position = new Vector3(target.transform.position.x,currentPosition.y + 2*delta,target.transform.position.z);
					bangbang.StartMovement(currentPosition,target.transform.position,currentSpeed,Time.time);
				}
			}
		}
		*/
		if(!bangbang.IsMoving){
			bangbang.StartMovement(currentPosition,target.transform.position,currentSpeed,Time.time);
		}
		
		//TODO limitation : on considere que le CG est au centre de tout les moteurs. 
		//Avec plusieurs drone filles ce n'est pas necessairement le cas
		
		Vector3[] tmp = bangbang.GetTarget(Time.time);
		Vector3 expectedPosition = tmp[0];
		Vector3 expectedSpeed = tmp[1];
		

		//Everything is in the world's reference
		

		/*
		if(enableGroundClearance){
			//Rise the target position and velocity if too close to the ground
			float requiredGroundClearance = 2f;
			if(sensor.GetDistanceToGround()<requiredGroundClearance){
				float currentClearance = sensor.GetDistanceToGround();
				float delta = requiredGroundClearance - currentClearance;
				expectedPosition.y += 2*delta;
				expectedSpeed.y += delta;
				if(target.transform.position.y<expectedPosition.y){
					target.transform.Translate(Vector3.up * delta * 2, Space.World);//move the target up
					bangbang.StartMovement(currentPosition,target.transform.position,currentSpeed,Time.time);
				}
			}
		}
		*/
		
		Vector3 speedCorrection = Vector3.zero;
		speedCorrection[0] = xPosPid.Update(expectedPosition[0], currentPosition[0], Time.deltaTime);
		speedCorrection[1] = yPosPid.Update(expectedPosition[1], currentPosition[1], Time.deltaTime);
		speedCorrection[2] = zPosPid.Update(expectedPosition[2], currentPosition[2], Time.deltaTime);
		
		
		Vector3 accelerationCommand = Vector3.zero;
		accelerationCommand[0] = xSpeedPid.Update(expectedSpeed[0]+speedCorrection[0], currentSpeed[0], Time.deltaTime);
		accelerationCommand[1] = ySpeedPid.Update(expectedSpeed[1]+speedCorrection[1], currentSpeed[1], Time.deltaTime);
		accelerationCommand[2] = zSpeedPid.Update(expectedSpeed[2]+speedCorrection[2], currentSpeed[2], Time.deltaTime);
		
		//Add the gravity in the acceleration/force required
		Vector3 counterGravityAcceleration = - Physics.gravity;
		accelerationCommand += counterGravityAcceleration;
		accelerationCommand.y = Mathf.Max(accelerationCommand.y,0.1f);//we don't allow the drone to flip

		//Now we have the acceleration required.
		//We can find the angle and the trust to apply



		Vector3 forceVector = accelerationCommand*(droneMass+payloadCtrl.GetPayloadMass());
		float totalThrust = forceVector.magnitude;

		Vector2 pitchAxis = new Vector2(forceVector.z,forceVector.y);
		float requiredPitch = -Vector2.SignedAngle(Vector2.up,pitchAxis);
		//positive pitch means an acceleration in negative z


		Vector2 rollAxis = new Vector2(forceVector.x,forceVector.y);
		float requiredRoll = Vector2.SignedAngle(Vector2.up,rollAxis);
		//positive roll = roll to the left, so an acceleration in negative x

		
		

		bool usePhysicalTorque = false;
		if(usePhysicalTorque){
		// YAW
			float requiredYaw = target.transform.eulerAngles.y;



			//TODO : tester bangbang
			float mainThrust = totalThrust/4;
			float thrustPitchDifference = GetThrustDifferenceToPitch( -1 * requiredPitch );
			float thrustRollDifference = GetThrustDifferenceToRoll( -1 * requiredRoll );
			float thrustYawDifference = 0f; //GetThrustDifferenceToYaw( requiredYaw );
			

			//Debug.Log(Time.fixedTime + " ; pitch:" +  requiredPitch + " ; roll:" + requiredRoll + " ; yaw:" + requiredYaw);
			//Debug.Log(Time.fixedTime + " ; p:" +  thrustPitchDifference + " ; r:" + thrustRollDifference + " ; y:" + thrustYawDifference);

			//TODO : réguler les forces
			sim.SetThrusterThrust(0, mainThrust + thrustPitchDifference + thrustRollDifference - thrustYawDifference);
			sim.SetThrusterThrust(1, mainThrust + thrustPitchDifference - thrustRollDifference + thrustYawDifference);
			sim.SetThrusterThrust(2, mainThrust - thrustPitchDifference - thrustRollDifference - thrustYawDifference);
			sim.SetThrusterThrust(3, mainThrust - thrustPitchDifference + thrustRollDifference + thrustYawDifference);
		}else{
			float targetHeading = target.transform.eulerAngles.y;

			
			transform.rotation = Quaternion.FromToRotation(Vector3.up, forceVector) * Quaternion.Euler(0, targetHeading, 0);
			//transform.rotation = Quaternion.Euler(0,targetHeading,0) * Quaternion.Euler(requiredPitch,0,requiredRoll);
			//transform.eulerAngles = new Vector3(requiredPitch,targetHeading,requiredRoll);
			//float currentHeading = sensor.GetHeadingAsFloat();
			
			for(int i = 0; i < numberOfThruster; i++){
				thrust[i] = totalThrust/4; 
				sim.SetThrusterThrust(i, thrust[i]);
			}
			
		}

		/*
		for(int i = 0; i < numberOfThruster; i++){
			thrust[i] = thrustEquilibrium; 
			sim.SetThrusterThrust(i, thrust[i]);
		}
		*/
		

		//TODO temporary
		//transform.rotation = Quaternion.FromToRotation(Vector3.up, forceVector) * Quaternion.Euler(0, targetHeading, 0);
		//transform.rotation = Quaternion.Euler(0,targetHeading,0) * Quaternion.Euler(requiredPitch,0,requiredRoll);
		//transform.eulerAngles = new Vector3(requiredPitch,targetHeading,requiredRoll);
		//float currentHeading = sensor.GetHeadingAsFloat();
		/*
		for(int i = 0; i < numberOfThruster; i++){
			thrust[i] = totalThrust/4; 
			sim.SetThrusterThrust(i, thrust[i]);
		}
		*/
		


		if(isFileOpen){
			int axis = axisRecorded;
			file.WriteLine(Time.fixedTime + ";" + expectedPosition[axis] 
				+ ";" + currentPosition[axis] + ";" + (expectedSpeed[axis]/*+speedCorrection[axis]*/)
				+ ";" + currentSpeed[axis] + ";" + totalThrust);
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
