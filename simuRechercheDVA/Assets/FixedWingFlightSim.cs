using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Basic controls and simulation
public class FixedWingFlightSim : DroneFlightSim
{
	

	public float turndroneSpeed = 40f;
	
	
	Vector3 localVelocityVector;
	
	//private Vector3 angularSpeedVector = Vector3.zero;
	
	
	
	
	
    // Start is called before the first frame update
	void Start()
    {
		base.Start(GetComponent<FixedWingDroneSimProperties>());
    }

    // Update is called once per frame
    public override void Update()
    {
		localVelocityVector = transform.InverseTransformDirection(rb.velocity);
		//Debug.Log("velocity "+localVelocityVector);
		
		base.Update();
    }
	
	
	
	
	
	
	//----setters----
	
	public void SetPitchDroneSpeed(float angularSpeed){
		SetPitchTorque(angularSpeed);
	}
	public void SetRollDroneSpeed(float angularSpeed){
		SetRollTorque(angularSpeed);
	}
	public void SetYawDroneSpeed(float angularSpeed){
		SetYawTorque(angularSpeed);
	}
	
	public override void SetPitchTorque(float command){
		command = Mathf.Clamp(command,-turndroneSpeed,turndroneSpeed);
		base.SetPitchTorque(command * localVelocityVector.z/100);
		Transform tailTransform = transform.Find("TailFins");
		tailTransform.localEulerAngles = new Vector3(-command, 0, 0);
	}
	
	public override void SetYawTorque(float command){
		command = Mathf.Clamp(command,-turndroneSpeed,turndroneSpeed);
		base.SetYawTorque(command * localVelocityVector.z/100);
		Transform RudderTransform = transform.Find("Rudder");
		RudderTransform.localEulerAngles = new Vector3(0, command, 0);
	}
	
	public override void SetRollTorque(float command){
		command = Mathf.Clamp(command,-turndroneSpeed,turndroneSpeed);
		base.SetRollTorque(command * localVelocityVector.z/100);
		Transform LeftFlapTransform = transform.Find("LeftFlap");
		LeftFlapTransform.localEulerAngles = new Vector3(command, 0, 0);
		Transform RightFlapTransform = transform.Find("RightFlap");
		RightFlapTransform.localEulerAngles = new Vector3(-command, 0, 0);
	}
	
	
	
	
	//----other----
	
	
	
	
	
	
}
