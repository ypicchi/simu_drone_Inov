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

		if(hasTarget){
			GoToTarget();
		}
		

	}

	protected void GoToTarget(){
		Vector3 currentPos3D = sensor.GetPosition();
		Vector3 targetPos3D = target.transform.position;

		Vector2 currentPos2D = new Vector2(currentPos3D.x,currentPos3D.z);
		Vector2 targetPos2D = new Vector2(targetPos3D.x,targetPos3D.z);

		float currentHeading = sensor.GetHeadingAsFloat();
		float targetHeading = - Vector2.SignedAngle(Vector2.up, targetPos2D-currentPos2D);



		float distance = (targetPos2D-currentPos2D).magnitude;
		float angularDifference = targetHeading - currentHeading;

		angularDifference = clampAngle180(angularDifference);

		float speedCommand = (100f-1/distance)* (1-(Mathf.Abs(angularDifference)/180f));
		float turningDifferentialCommand = Mathf.Sign(angularDifference) * 30f + angularDifference;
		

		speedCommand = Mathf.Clamp(speedCommand,0f,100f);


		float[] thrust = new float[4];
		for(int i = 0; i < numberOfThruster; i++){
			thrust[i] = speedCommand; 
		}

		float turnMultiplicator = 1f;
		turningDifferentialCommand *= turnMultiplicator;

		thrust[0] += turningDifferentialCommand;
		thrust[1] -= turningDifferentialCommand;
		thrust[2] -= turningDifferentialCommand;
		thrust[3] += turningDifferentialCommand;
		
		for(int i = 0; i < numberOfThruster; i++){
			sim.SetThrusterThrust(i, thrust[i]);
		}
	}

	//make sure the angle is withing +- 180°
	protected float clampAngle180(float angle){
		//we shift the range
		angle += 180f;

		//now we ensure a range within 0-360°
		//TODO
		angle = angle%360f;
		if(angle<0){
			angle += 360f;
		}

		//and we shift again
		angle -= 180f;

		return angle;
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
