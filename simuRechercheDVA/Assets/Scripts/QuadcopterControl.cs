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


	public StreamWriter file;
	public Boolean isFileOpen = false;

	protected int numberOfThruster;
	protected bool needToRunManual = false;

	protected BangBangVector3 bangbang;

	public PID xPosPid = new PID(1f, 0f, 0f);
	public PID yPosPid = new PID(1f, 0f, 0f);
	public PID zPosPid = new PID(1f, 0f, 0f);
	public PID xSpeedPid = new PID(1f, 0f, 0f);
	public PID ySpeedPid = new PID(1f, 0f, 0f);
	public PID zSpeedPid = new PID(1f, 0f, 0f);

	//Awake is made to initialize variables. It is called before any Start()
	public override void Awake(){
		base.Awake();
		InitVariables();
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
		bangbang = new BangBangVector3(Vector3.one * 20f, Vector3.one * 3f);
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

	public override void SetWaypoint(GameObject waypointIndicator){
		base.SetWaypoint(waypointIndicator);
		bangbang.StartMovement(sensor.GetPosition(),target.transform.position,Time.time);
	}

	private void GoToWaypoint(){
		if(hasTarget==false){
			return;
		}
		if(!bangbang.IsMoving){
			bangbang.StartMovement(sensor.GetPosition(),target.transform.position,Time.time);
		}
		
		
		if(true){
			


			Vector3[] tmp = bangbang.GetTarget(Time.time);
			Vector3 expectedPosition = tmp[0];
			Vector3 expectedSpeed = tmp[1];
			Vector3 currentPosition = sensor.GetPosition();
			Vector3 currentSpeed = transform.TransformDirection(sensor.GetSpeed());

			//Everything is in the world's reference

			if(enableGroundClearance){
				//Rise the target position and velocity if too close to the ground
				float requiredGroundClearance = 2f;
				if(sensor.GetDistanceToGround()<requiredGroundClearance){
					float currentClearance = sensor.GetDistanceToGround();
					float delta = requiredGroundClearance - currentClearance;
					expectedPosition.y += sensor.GetPosition().y + 2*delta;
					expectedSpeed.y = Mathf.Max(expectedSpeed.y+delta,delta);
					target.transform.Translate(Vector3.up * delta, Space.World);//move the target up
				}
			}
			
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



			//Forces are in the drone's reference
			Vector3 forceVector = accelerationCommand*mass;
			float totalThrust = forceVector.magnitude;

			Vector2 pitchAxis = new Vector2(forceVector.z,forceVector.y);
			float requiredPitch = -Vector2.SignedAngle(Vector2.up,pitchAxis);
			//positive pitch means an acceleration in negative z


			Vector2 rollAxis = new Vector2(forceVector.x,forceVector.y);
			float requiredRoll = Vector2.SignedAngle(Vector2.up,rollAxis);
			//positive roll = roll to the left, so an acceleration in negative x

			
			float targetHeading = target.transform.eulerAngles.y;

			//TODO temporary
			transform.rotation = Quaternion.FromToRotation(Vector3.up, forceVector) * Quaternion.Euler(0,targetHeading,0);
			//transform.rotation = Quaternion.Euler(0,targetHeading,0) * Quaternion.Euler(requiredPitch,0,requiredRoll);
			//transform.eulerAngles = new Vector3(requiredPitch,targetHeading,requiredRoll);
			
			
			
			//float currentHeading = Sensor.getHeadingAsfloat();


			for(int i = 0; i < numberOfThruster; i++){
				thrust[i] = totalThrust/4; 
				sim.SetThrusterThrust(i, thrust[i]);
			}


			if(isFileOpen){
				int axis = 2;
				file.WriteLine(Time.fixedTime + ";" + expectedPosition[axis] 
					+ ";" + currentPosition[axis] + ";" + (expectedSpeed[axis]+speedCorrection[axis])
					+ ";" + currentSpeed[axis] + ";" + totalThrust);
			}
			
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
