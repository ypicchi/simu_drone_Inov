using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Basic controls and simulation
public class FixedWingFlightSim : MonoBehaviour
{
	//public static maVar
	
	//autre script : usingdroneFlightSim.cs
	
	public float maxThrust = 30.0f;
	public Vector3 dragCoefficients = new Vector3(1.5f,5f,0.1f);
	public float turndroneSpeed = 5f;
	
	public Vector3 xDragCenterOffset = new Vector3(0,0,-5f);
	public Vector3 yDragCenterOffset = new Vector3(0,0,-0.1f);
	public Vector3 zDragCenterOffset = new Vector3(0,0,0);
	
	public Vector3[] inducedLift = {new Vector3(0,0,0),new Vector3(0,0,0),new Vector3(0,10f,0)};
	
	private Rigidbody rb;
	private float droneSpeed;
	
	private Vector3 angularSpeedVector = Vector3.zero;
	
	
	protected Vector3[] thrusterOffset = {new Vector3(0,0,0)};
	protected Vector3[] thrusterThrustVectors = {new Vector3(0,0,1)};
	protected float[] thrusterThrustValues = {15f};
	
	
	

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
	public float GetMainThrust(){
		if(thrusterThrustValues.Length>0)
			return thrusterThrustValues[0];
		else{
			Debug.Log("Trying to get the main thrust but no thruster registered");
			return 0f;
		}
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
	public  void SetThrust(float thrustValue){
		SetThrusterThrust(0,thrustValue);
	}
	
	public void SetThrusterThrust(int thrusterIndex,float thrustValue){
		if(thrusterIndex >= 0 && thrusterIndex < thrusterThrustValues.Length)
			thrusterThrustValues[thrusterIndex] = thrustValue;
		else
			Debug.Log("Trying to set some thrust but out of index");
	}
	
	
	
	//----other----
	
	
	
	private void UpdateAngularVelocity(){
		rb.angularVelocity = Vector3.zero;
		
		rb.angularVelocity = angularSpeedVector * droneSpeed * Time.deltaTime;
		rb.angularVelocity = transform.TransformDirection(rb.angularVelocity);//change base
	}
	
	
	

	
	private void HandleForces(){
		Vector3 localVelocityVector = transform.InverseTransformDirection(rb.velocity);//world to local
		Vector3 airSpeed = - localVelocityVector;
		
		Vector3 force = Vector3.zero;
		
		Vector3 drag = Vector3.zero;
		for(int i=0;i<3;i++){
			drag[i] = Mathf.Sign(airSpeed[i]) * Mathf.Pow(Mathf.Abs(airSpeed[i]),1.5f) * dragCoefficients[i];
			force += inducedLift[i] * Mathf.Abs(drag[i]);
			force[i] += drag[i];
		}
		
		
		//Debug.Log("force "+force);=

		
		Vector3 CG = rb.worldCenterOfMass;
		Vector3 xLiftCenter = CG + transform.TransformDirection(xDragCenterOffset);
		Vector3 yLiftCenter = CG + transform.TransformDirection(yDragCenterOffset);
		Vector3 zLiftCenter = CG + transform.TransformDirection(zDragCenterOffset);
		
		
		rb.AddForceAtPosition(transform.TransformDirection(Vector3.right * force.x), xLiftCenter);
		rb.AddForceAtPosition(transform.TransformDirection(Vector3.up * force.y), yLiftCenter);
		rb.AddForceAtPosition(transform.TransformDirection(Vector3.forward * force.z), zLiftCenter);
		
		
		for (int i=0;i<thrusterOffset.Length;i++){
			Vector3 worldThrusterPosition = CG + transform.TransformDirection(thrusterOffset[i]);
			rb.AddForceAtPosition(transform.TransformDirection(thrusterThrustVectors[i]*thrusterThrustValues[i]), worldThrusterPosition);
		}
		
	}
	
}
