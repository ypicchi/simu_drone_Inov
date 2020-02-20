using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadcopterControl : DroneControl
{

    protected DroneFlightSim sim;

	protected float[] thrust;
	private float thrustEquilibrium = 2.035f;
	private float thrustMax = 10.0f;
	private float thrustMaxNegative;
	private float heightThreshold = 2.0f;


	protected int numberOfThruster;
	protected bool needToRunManual = false;

    // Start is called before the first frame update
	public override void Start()
	{
		initVariables();

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

    private void initVariables(){
		thrustMaxNegative = -thrustMax;
	}

    public override void ControlLoop(){

		// Si la cible existe, on peut calculer un itinéraire
		//Vector3 target3DHeading = target.transform.position - sensor.GetPosition();

		if(hasTarget){
			GoToWaypoint();
		}

	}


	private float GetThrurstVertical(float heightDifference){
		
		
		float thrustVertical = 0f;
		float thrustRatio;


		// Need to go UP, the drone is below the waypoint
		if(heightDifference > 0){
			
			// Need to go UP slowly, the drone is just below the waypoint
			if(heightDifference < heightThreshold){
				thrustRatio = heightDifference  / heightThreshold;
				thrustVertical = thrustEquilibrium + thrustRatio * (thrustMax - thrustEquilibrium);
				Debug.Log("UP : " + heightDifference);	
			}
			else{
				thrustVertical = thrustMax;
				Debug.Log("UP++ : " + heightDifference);
			}
		}

		// Need to go DOWN, the drone is above the waypoint
		else if(heightDifference < 0 ){
			
			// Need to go DOWN slowly, the drone is just below the waypoint
			if(heightDifference > - heightThreshold){
				thrustRatio = heightDifference  / heightThreshold;
				thrustVertical = thrustEquilibrium - thrustRatio * (thrustMaxNegative - thrustEquilibrium);
				Debug.Log("DOWN : " + heightDifference);	
			}
			else{
				thrustVertical = thrustMaxNegative;
				Debug.Log("DOWN-- : " + heightDifference);	
			}
		}

		else{//Non reachable code
			Debug.Log("??? : " + heightDifference);	
		}

		return thrustVertical;
	}

	private void GoToWaypoint(){
		if(hasTarget==false){
			return;
		}
		
		// Height calculation
		float heightDifference = target.transform.position.y - sensor.GetPosition().y;

		for(int i=0; i<numberOfThruster; i++){
			thrust[i] = GetThrurstVertical(heightDifference);//GetVerticalThrurst(targetHeightDifference, thrust[i]);
			sim.SetThrusterThrust(i, thrust[i]);
		}

		
	}

    protected override void HandleKeyboardInput(){
		if (Input.GetKey(KeyCode.LeftShift)){
			for(int i=0;i<numberOfThruster;i++){
				thrust[i] += 0.05f;
				sim.SetThrusterThrust(i,thrust[i]);
			}
        }else if (Input.GetKey(KeyCode.LeftControl)){
			for(int i=0;i<numberOfThruster;i++){
				thrust[i] -= 0.05f;
				sim.SetThrusterThrust(i,thrust[i]);
			}
        }

		float averageThrust = 0;
		for(int i=0;i<numberOfThruster;i++){
			averageThrust += thrust[i];
		}
		averageThrust /= numberOfThruster;


		for(int i=0;i<numberOfThruster;i++){
			thrust[i] = averageThrust;
			sim.SetThrusterThrust(i,thrust[i]);
		}


		float turnPower = 3f;
		needToRunManual = true;
		
        if(Input.GetKey(KeyCode.Z)){      //------------------ Z
            thrust[0] -= turnPower;
			thrust[1] -= turnPower;
			thrust[2] += turnPower;
			thrust[3] += turnPower;
			for(int i=0;i<numberOfThruster;i++){
				sim.SetThrusterThrust(i,thrust[i]);
			}
        }else if (Input.GetKey(KeyCode.S)){//------------------ S
            thrust[0] += turnPower;
			thrust[1] += turnPower;
			thrust[2] -= turnPower;
			thrust[3] -= turnPower;
			for(int i=0;i<numberOfThruster;i++){
				sim.SetThrusterThrust(i,thrust[i]);
			}
        }else if (Input.GetKey(KeyCode.Q)){//------------------ Q
			thrust[0] -= turnPower;
			thrust[1] += turnPower;
			thrust[2] += turnPower;
			thrust[3] -= turnPower;
			for(int i=0;i<numberOfThruster;i++){
				sim.SetThrusterThrust(i,thrust[i]);
			}
        }else if (Input.GetKey(KeyCode.D)){//------------------ D
            thrust[0] += turnPower;
			thrust[1] -= turnPower;
			thrust[2] -= turnPower;
			thrust[3] += turnPower;
			for(int i=0;i<numberOfThruster;i++){
				sim.SetThrusterThrust(i,thrust[i]);
			}
        }else if (Input.GetKey(KeyCode.A)){//------------------ A
            thrust[0] -= turnPower;
			thrust[1] += turnPower;
			thrust[2] -= turnPower;
			thrust[3] += turnPower;
			for(int i=0;i<numberOfThruster;i++){
				sim.SetThrusterThrust(i,thrust[i]);
			}
        }else if (Input.GetKey(KeyCode.E)){//------------------ E
            thrust[0] += turnPower;
			thrust[1] -= turnPower;
			thrust[2] += turnPower;
			thrust[3] -= turnPower;
			for(int i=0;i<numberOfThruster;i++){
				sim.SetThrusterThrust(i,thrust[i]);
			}
        }else{
			needToRunManual = false;
		}
	}
}
