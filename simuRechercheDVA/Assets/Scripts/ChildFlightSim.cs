﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildFlightSim : DroneFlightSim
{

    protected Vector3[] wheelCenters;
    protected float wheelRadius;
    
    public override void Start(){
        base.Start();

        wheelCenters = new Vector3[simProperties.ThrusterOffset.Length];

        GameObject wheel = GameObject.Find("Wheel0");;
        for(int i=0;i<simProperties.ThrusterOffset.Length;i++){
            wheel = GameObject.Find("Wheel"+i.ToString());
	        wheelCenters[i] = transform.InverseTransformPoint(wheel.transform.position) - transform.position;
        }

        wheelRadius = wheel.transform.lossyScale.y/2;
    }


    

    public override void FixedUpdate(){
        base.FixedUpdate();

        for(int i=0;i<simProperties.ThrusterOffset.Length;i++){
            
            GameObject wheel = GameObject.Find("Wheel"+i.ToString());
            Rigidbody wheelRb = wheel.GetComponent<Rigidbody>();
            if(wheelRb != null){
                float wheelTorque = - simProperties.ThrusterThrustValues[i] / (wheelRadius);
                wheelRb.AddRelativeTorque(Vector3.up*wheelTorque);
            }

        }
	}
    
    protected bool IsWheelGrounded(int i){
        return Physics.Raycast(wheelCenters[i], -Vector3.up, wheelRadius + 0.02f);
    }
}