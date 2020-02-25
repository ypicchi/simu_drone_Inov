using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildFlightSim : DroneFlightSim
{
    //TODO child sim

    protected float[] thrusts;
    protected Vector3[] wheelCenters;
    protected float wheelRadius;
    public override void Start(){
        base.Start();

        thrusts = new float[simProperties.ThrusterOffset.Length];
        wheelCenters = new Vector3[simProperties.ThrusterOffset.Length];

        GameObject wheel = GameObject.Find("Wheel0");;
        for(int i=0;i<simProperties.ThrusterOffset.Length;i++){
            wheel = GameObject.Find("Wheel"+i.ToString());
	        wheelCenters[i] = transform.InverseTransformPoint(wheel.transform.position) - transform.position;
        }

        wheelRadius = wheel.transform.lossyScale.y/2;
    }


    public override void Update(){
        base.Update();
        /*
        //animation
        for (int i=0;i<simProperties.ThrusterOffset.Length;i++){
            GameObject wheel = GameObject.Find("Wheel"+i.ToString());
        }
        */
    }

    public override void FixedUpdate(){

		for(int i=0;i<simProperties.ThrusterOffset.Length;i++){
            if( ! IsWheelGrounded(i)){
                thrusts[i] = simProperties.ThrusterThrustValues[i];
                simProperties.ThrusterThrustValues[i] = 0;
            }
        }
        //we temporarily set the thrust to 0 if not grounded
        //then restore it after the forces calculation are done
        base.FixedUpdate();

        for(int i=0;i<simProperties.ThrusterOffset.Length;i++){
            if( ! IsWheelGrounded(i)){
                simProperties.ThrusterThrustValues[i] = thrusts[i];
            }
        }
	}
    
    protected bool IsWheelGrounded(int i){
        return Physics.Raycast(wheelCenters[i], -Vector3.up, wheelRadius + 0.02f);
    }
}