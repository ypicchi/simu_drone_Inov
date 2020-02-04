using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Basic controls and simulation
public class DroneFlightSim : MonoBehaviour
{
	//public static maVar
	
	//autre script : usingdroneFlightSim.cs
	
	public float maxThrust = 30.0f;
	public int finess = 10;
	public Vector3 dragCoefficients = new Vector3(1.5f,5f,0.1f);
	public float turndroneSpeed = 5f;
	public float angularSpeedDecay = 0.05f;
	public float yLiftCenterZOffset = - 0.1f;
	public float xLiftCenterZOffset = - 5f;
	
	private Rigidbody rb;
	public float thrust = 15.0f;
	private float droneSpeed;
	
	private Vector3 angularSpeedVector = Vector3.zero;
	
	
	

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
		//dragCoefficients = new Vector3(1.5f,5f,0.1f);
		
		
		
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 localVelocityVector = transform.InverseTransformDirection(rb.velocity);
		//Debug.Log("velocity "+localVelocityVector);
		droneSpeed = localVelocityVector.magnitude;
		
		UpdateAngularVelocity();//rotation speed in every axis
		//HandleBasicForces();
		HandleForces();
    }
	
	
	//----getters----
	public float GetPitchDroneSpeed(){
		return angularSpeedVector[0];
	}
	public float GetRollDroneSpeed(){
		return angularSpeedVector[2];
	}
	public float GetYawDroneSpeed(){
		return angularSpeedVector[1];
	}
	public float GetThrust(){
		return thrust;
	}
	public float GetForwardSpeed(){
		return droneSpeed;
	}
	
	
	//----setters----
	public void SetPitchDroneSpeed(float angularSpeed){
		angularSpeed = Mathf.Clamp(angularSpeed,-turndroneSpeed,turndroneSpeed);
		angularSpeedVector[0] = -angularSpeed;
	}
	public void SetRollDroneSpeed(float angularSpeed){
		angularSpeed = Mathf.Clamp(angularSpeed,-turndroneSpeed,turndroneSpeed);
		angularSpeedVector[2] = angularSpeed;
	}
	public void SetYawDroneSpeed(float angularSpeed){
		angularSpeed = Mathf.Clamp(angularSpeed,-turndroneSpeed,turndroneSpeed);
		angularSpeedVector[1] = angularSpeed;
	}
	public void SetThrust(float targetThrust){
		thrust = Mathf.Clamp(targetThrust,0,maxThrust);
	}
	
	
	
	
	
	//----other----
	
	
	
	private void UpdateAngularVelocity(){
		rb.angularVelocity = Vector3.zero;
		
		rb.angularVelocity = angularSpeedVector * droneSpeed * Time.deltaTime;
		rb.angularVelocity = transform.TransformDirection(rb.angularVelocity);//change base
	}
	
	
	

	
	private void HandleForces(){
		Vector3 localVelocityVector = transform.InverseTransformDirection(rb.velocity);
		Vector3 airSpeed = - localVelocityVector;
		
		//Debug.Log("airSpeed "+airSpeed);
		
		float dragZ = Mathf.Sign(airSpeed.z) * Mathf.Pow(Mathf.Abs(airSpeed.z),1.5f) * dragCoefficients.z;
		float liftY = finess * Mathf.Abs(dragZ);
		liftY += Mathf.Sign(airSpeed.y) * Mathf.Pow(Mathf.Abs(airSpeed.y),1.5f) * dragCoefficients.y;
		
		float dragX = Mathf.Sign(airSpeed.x) * Mathf.Pow(Mathf.Abs(airSpeed.x),1.5f) * dragCoefficients.x;
		//float windInducedYawTorque = dragXToYawTorqueCoeff * dragX;
		
		/*
		Debug.Log("dragZ "+dragZ);
		Debug.Log("liftY "+liftY);
		Debug.Log("dragX "+dragX);
		Debug.Log("thrust "+thrust);
		*/
		
		rb.AddRelativeForce(Vector3.forward * thrust);
		rb.AddRelativeForce(Vector3.forward * dragZ);
		//rb.AddRelativeForce(Vector3.up * liftY);
		//rb.AddRelativeTorque(Vector3.up * windInducedYawTorque);
		
		
		Vector3 CG = rb.worldCenterOfMass;
		Vector3 yLiftCenter = CG + transform.forward * yLiftCenterZOffset;
		Vector3 xLiftCenter = CG + transform.forward * xLiftCenterZOffset;
		
		rb.AddForceAtPosition(transform.TransformDirection(Vector3.up * liftY), yLiftCenter);
		rb.AddForceAtPosition(transform.TransformDirection(Vector3.right * dragX), xLiftCenter);
		
		
	}
	
}
