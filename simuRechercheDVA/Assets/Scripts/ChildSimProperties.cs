﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildSimProperties : DroneSimProperties
{
    public Vector3 dragCoefficients = new Vector3(0.1f, 0.5f, 0.1f);



    public Vector3 xDragCenterOffset = new Vector3(0, 0, 0);
    public Vector3 yDragCenterOffset = new Vector3(0, 0, 0);
    public Vector3 zDragCenterOffset = new Vector3(0, 0, 0);


    public Vector3[] inducedLift = {
        new Vector3(0, 0, 0),
        new Vector3(0, 0, 0),
        new Vector3(0, 0, 0)
    };


    private Vector3[] thrusterOffset = new Vector3[4];

    private Vector3[] thrusterThrustVectors = {
        new Vector3(0, 0, 1),
        new Vector3(0, 0, 1),
        new Vector3(0, 0, 1),
        new Vector3(0, 0, 1)
    };


    public float[] thrusterThrustValues = {
        0f,
        0f,
        0f,
        0f
    };

    
    private float[] thrusterPowerDrain = {// W/kg
        50f,
        50f,
        50f,
        50f
    };


    public override void Start(){
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 CG = rb.centerOfMass;
        for (int i=0;i<thrusterThrustValues.Length;i++){
            GameObject wheel = GameObject.Find("Wheel"+i.ToString());
		    thrusterOffset[i] = new Vector3(0, -wheel.transform.lossyScale.y/2, 0) + transform.InverseTransformPoint(wheel.transform.position) - CG;
        }
	}


    public override Vector3 DragCoefficients { get => dragCoefficients; set => dragCoefficients = value; }
    public override Vector3 XDragCenterOffset { get => xDragCenterOffset; set => xDragCenterOffset = value; }
    public override Vector3 YDragCenterOffset { get => yDragCenterOffset; set => yDragCenterOffset = value; }
    public override Vector3 ZDragCenterOffset { get => zDragCenterOffset; set => zDragCenterOffset = value; }
    public override Vector3[] InducedLift { get => inducedLift; set => inducedLift = value; }
    public override Vector3[] ThrusterOffset { get => thrusterOffset; set => thrusterOffset = value; }
    public override Vector3[] ThrusterThrustVectors { get => thrusterThrustVectors; set => thrusterThrustVectors = value; }
    public override float[] ThrusterThrustValues { get => thrusterThrustValues; set => thrusterThrustValues = value; }
    public override float[] ThrusterPowerDrain { get => thrusterPowerDrain;}
}
