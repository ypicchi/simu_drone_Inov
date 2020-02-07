
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Basic controls and simulation
public abstract class DroneFlightSim : MonoBehaviour
{
	protected Rigidbody rb;
	protected DroneSimProperties simProperties;
	
	
	
	
	
	
	
	
	

	
	
	protected Vector3 combinedTorque = Vector3.zero;
	
	
	

	

    // Start is called before the first frame update
    protected void Start(DroneSimProperties simProperties)
    {
		this.simProperties = simProperties;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    public virtual void Update()
    {
		
		HandleForces();
		combinedTorque = Vector3.zero;//may be an issue with the update ordres of the various classes
    }
	
	
	//----getters----
	
	public virtual float GetMainThrust(){
		if(simProperties.ThrusterThrustValues.Length>0)
			return simProperties.ThrusterThrustValues[0];
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
		if(thrusterIndex >= 0 && thrusterIndex < simProperties.ThrusterThrustValues.Length)
			simProperties.ThrusterThrustValues[thrusterIndex] = thrustValue;
		else
			Debug.Log("Trying to set some thrust but out of index");
	}
	
	public virtual void SetPitchTorque(float torque){
		combinedTorque[0] = torque;
	}
	
	public virtual void SetYawTorque(float torque){
		combinedTorque[1] = torque;
	}
	
	public virtual void SetRollTorque(float torque){
		combinedTorque[2] = torque;
	}
	
	

	
	
	

	
	protected void HandleForces(){
		Vector3 localVelocityVector = transform.InverseTransformDirection(rb.velocity);//world to local
		Vector3 airSpeed = - localVelocityVector;
		
		Vector3 force = Vector3.zero;
		
		Vector3 drag = Vector3.zero;
		for(int i=0;i<3;i++){
			drag[i] = Mathf.Sign(airSpeed[i]) * Mathf.Pow(Mathf.Abs(airSpeed[i]),1.5f) * simProperties.DragCoefficients[i];
			force += simProperties.InducedLift[i] * Mathf.Abs(drag[i]);
			force[i] += drag[i];
		}
		
		
		//Debug.Log("force "+force);
		//Debug.Log("mainThrust "+mainThrust);

		
		Vector3 CG = rb.worldCenterOfMass;
		Vector3 xLiftCenter = CG + transform.TransformDirection(simProperties.XDragCenterOffset);
		Vector3 yLiftCenter = CG + transform.TransformDirection(simProperties.YDragCenterOffset);
		Vector3 zLiftCenter = CG + transform.TransformDirection(simProperties.ZDragCenterOffset);
		
		
		rb.AddForceAtPosition(transform.TransformDirection(Vector3.right * force.x), xLiftCenter);
		rb.AddForceAtPosition(transform.TransformDirection(Vector3.up * force.y), yLiftCenter);
		rb.AddForceAtPosition(transform.TransformDirection(Vector3.forward * force.z), zLiftCenter);
		
		
		for (int i=0;i<simProperties.ThrusterOffset.Length;i++){
			Vector3 worldThrusterPosition = CG + transform.TransformDirection(simProperties.ThrusterOffset[i]);
			rb.AddForceAtPosition(transform.TransformDirection(simProperties.ThrusterThrustVectors[i]*simProperties.ThrusterThrustValues[i]), worldThrusterPosition);
		}
		
		rb.AddRelativeTorque(combinedTorque);
		
	}
	
}
