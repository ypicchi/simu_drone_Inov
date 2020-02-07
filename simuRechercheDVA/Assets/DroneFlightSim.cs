﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Basic controls and simulation
public abstract class DroneFlightSim : MonoBehaviour
{
	//public static maVar
	
	//autre script : usingdroneFlightSim.cs
	
	protected Rigidbody rb;
	
	public float maxThrust = 30.0f;
	
	
	public Vector3 dragCoefficients = new Vector3(1.5f,5f,0.1f);//the drag coefficient for each directions (sideway,upward,forward)
	
	
	//Where the drag will be applied relative to the center of mass. 
	//Any lift is applied at the center of drag of the corresponding direction
	public Vector3 xDragCenterOffset = new Vector3(0,0,-5f);
	public Vector3 yDragCenterOffset = new Vector3(0,0,-0.1f);
	public Vector3 zDragCenterOffset = new Vector3(0,0,0);
	
	//inducedLift[0][2] means the lift induced on the Z axis by the drag on the X axis (X drag is multiplied by the value to created the Z lift)
	public Vector3[] inducedLift = {new Vector3(0,0,0),new Vector3(0,0,0),new Vector3(0,10f,0)};
	
	
	public float mainThrust = 15.0f;//start thrust
	
	
	protected Vector3 combinedTorque = Vector3.zero;
	
	protected Vector3[] thrusterOffset = {new Vector3(0,0,0)};
	protected Vector3[] thrusterThrustVectors = {new Vector3(0,0,1)};
	protected float[] thrusterThrustValues = {15f};
	
	

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
		
		
		
    }

    // Update is called once per frame
    void Update()
    {
		combinedTorque = Vector3.zero;//may be an issue with the update ordres of the various classes
		HandleForces();
    }
	
	
	//----getters----
	
	public virtual float GetMainThrust(){
		if(thrusterThrustValues.Length>0)
			return thrusterThrustValues[0];
		else{
			Debug.Log("Trying to get the main thrust but no thruster registered");
			return 0f;
		}
	}
	
	
	
	//----setters----
	
	public virtual void SetMainThrust(float thrustValue){
		SetThrusterThrust(0,thrustValue);
		//mainThrust = Mathf.Clamp(thrustValue,0,maxThrust);
	}
	
	public void SetThrusterThrust(int thrusterIndex,float thrustValue){
		if(thrusterIndex >= 0 && thrusterIndex < thrusterThrustValues.Length)
			thrusterThrustValues[thrusterIndex] = thrustValue;
		else
			Debug.Log("Trying to set some thrust but out of index");
	}
	
	public void SetPitchTorque(float torque){
		combinedTorque[0] = torque;
	}
	
	public void SetYawTorque(float torque){
		combinedTorque[1] = torque;
	}
	
	public void SetRollTorque(float torque){
		combinedTorque[2] = torque;
	}
	
	

	
	
	

	
	protected void HandleForces(){
		Vector3 localVelocityVector = transform.InverseTransformDirection(rb.velocity);//world to local
		Vector3 airSpeed = - localVelocityVector;
		
		Vector3 force = Vector3.zero;
		
		Vector3 drag = Vector3.zero;
		for(int i=0;i<3;i++){
			drag[i] = Mathf.Sign(airSpeed[i]) * Mathf.Pow(Mathf.Abs(airSpeed[i]),1.5f) * dragCoefficients[i];
			force += inducedLift[i] * Mathf.Abs(drag[i]);
			force[i] += drag[i];
		}
		
		
		//Debug.Log("force "+force);
		//Debug.Log("mainThrust "+mainThrust);

		
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
		
		rb.AddRelativeTorque(combinedTorque);
		
	}
	
}
