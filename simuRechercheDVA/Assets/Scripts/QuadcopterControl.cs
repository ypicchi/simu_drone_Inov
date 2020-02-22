using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadcopterControl : DroneControl
{

    protected DroneFlightSim sim;

	protected float[] thrust;
	private float thrustEquilibrium = 211f;
	private float thrustMax;
	private float thrustMin;
	private float heightThreshold;

	public PID speedPid = new PID(3f, 0.05f, 0.01f);

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

		/* 
		Debug.Log("Cible : " + target);
		Debug.Log("Cible_position : " + target.transform.position);
		Debug.Log("Cible_angle : " + target.transform.eulerAngles.y);
		*/

	}

	// Variables depending of other variables
    private void InitVariables(){

		// Maximum thrust could be more powerful tho
		thrustMax = thrustEquilibrium * 2;

		// Thrusting in the opposite direction to go down is too extreme
		// Using no thrust is good enough
		thrustMin = 0;

		// Weird (force => height) conversion
		heightThreshold = thrustEquilibrium * 1;

	}

    public override void ControlLoop(){

		if(hasTarget){
			GoToWaypoint();
		}

	}


	private float GetThrurstVertical(float heightDifference){
		
		
		float thrustVertical = 0f;
		float targetSpeed = 0f;

		
		float thrustCommand = speedPid.Update(targetSpeed, sensor.GetVerticalSpeed(), Time.deltaTime);
		Debug.Log("Speed : " + sensor.GetVerticalSpeed() );

		#region old vertical thrust
		/*
		// Need to go UP, the drone is below the waypoint
		if(heightDifference > 0){
			
			// Need to go UP slowly, the drone is just below the waypoint
			if(heightDifference < heightThreshold){
				thrustVertical = GetRawThrurstVertical(heightDifference);
				Debug.Log("UP : " + heightDifference + "=>"+ thrustVertical);	
			}
			else{
				thrustVertical = thrustMax;
				Debug.Log("UP++ : " + heightDifference + "=>" + thrustVertical );
			}
		}

		// Need to go DOWN, the drone is above the waypoint
		else if(heightDifference < 0 ){
			
			// Need to go DOWN slowly, the drone is just below the waypoint
			if(heightDifference > - heightThreshold){
				thrustVertical = thrustMin + ( thrustMax - GetRawThrurstVertical(heightDifference) );
				Debug.Log("DOWN : " + heightDifference + "=>" + thrustVertical );	
			}
			else{
				thrustVertical = thrustMin;
				Debug.Log("DOWN-- : " + heightDifference + "=>" + thrustVertical );
			}
		}

		else{//Non reachable code
			Debug.Log("??? : " + heightDifference);	
		}
		*/
		#endregion


		return thrustVertical;
	}

	private void GoToWaypoint(){
		if(hasTarget==false){
			return;
		}
		
		// Height calculation
		float heightDifference = target.transform.position.y - sensor.GetPosition().y;
		float thrustVertical = GetThrurstVertical(heightDifference);
		
		for(int i = 0; i < numberOfThruster; i++){
			thrust[i] = thrustVertical; 
			sim.SetThrusterThrust(i, thrust[i]);
		}
		
	}

    protected override void HandleKeyboardInput(){
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
