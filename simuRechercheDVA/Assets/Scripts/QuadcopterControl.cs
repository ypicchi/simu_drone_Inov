using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadcopterControl : DroneControl
{

    protected DroneFlightSim sim;

	protected float[] thrust;
	protected int numberOfThruster;
	protected bool needToRunManual = false;

    // Start is called before the first frame update
	public override void Start()
	{
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

    

    public override void ControlLoop(){

		// Si la cible existe, on peut calculer un itinéraire
		//Vector3 target3DHeading = target.transform.position - sensor.GetPosition();

		if(hasTarget){
			GoToWaypoint();
		}

	}


	private void GoToWaypoint(){
		if(hasTarget==false){
			return;
		}
		
		Vector3 target3DHeading = target.transform.position - sensor.GetPosition();
		Debug.Log("Direction : " + target3DHeading);



		//TODO : agir sur les moteurs, selon la cible

		for(int i=0; i<numberOfThruster; i++){
			thrust[i] += 0.01f;
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


		float turnPower = 0.01f;
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
