﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sensor : MonoBehaviour
{
	
	
	
	private Rigidbody rb;
	private Text rollDisplay;
	private Text pitchDisplay;
	private Text climbAngleDisplay;
	private Text realClimbAngleDisplay;
	private Text forwardSpeedDisplay;
	
	
	private const int numberOfSources = 1;
	private Vector3[] emissionSources = new Vector3[numberOfSources];
	
	
	private bool hasComputedThisFrame = false;
	private Vector3 fromWhere;
	private float lastComputedPowerValue;
	
	private float lastRollValue = 0;
	private float lastPitchValue = 0;
	
    // Start is called before the first frame update
    void Start()
    {
		rb = GetComponent<Rigidbody>();
        emissionSources[0] = new Vector3(50,0,100);
		
		for(int i =0; i<numberOfSources ;i++ ){
			GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			sphere.transform.position = emissionSources[i];
			sphere.name = "signalTransmitter";
		}
		
		
		rollDisplay = GameObject.Find("Canvas/rollDisplay").GetComponent<Text>();
		pitchDisplay = GameObject.Find("Canvas/pitchDisplay").GetComponent<Text>();
		climbAngleDisplay = GameObject.Find("Canvas/climbAngleDisplay").GetComponent<Text>();
		realClimbAngleDisplay = GameObject.Find("Canvas/realClimbAngleDisplay").GetComponent<Text>();
		forwardSpeedDisplay = GameObject.Find("Canvas/forwardSpeedDisplay").GetComponent<Text>();
		

    }

    // Update is called once per frame
    void Update()
    {
		float currentRoll = GetRoll();
		float currentPitch = GetPitch();
		
		rollDisplay.text = "roll : "+currentRoll.ToString();
		pitchDisplay.text = "pitch : "+currentPitch.ToString();
		climbAngleDisplay.text = "climb angle : "+GetAttitudeClimbAngle().ToString();
		realClimbAngleDisplay.text = "real climb angle : "+GetRealClimbAngle().ToString();
		forwardSpeedDisplay.text = "forward speed : "+GetSpeed().ToString();
        hasComputedThisFrame = false;
		
		lastRollValue = currentRoll;
		lastPitchValue = currentPitch;
    }
	
	public float GetSignalPower(Vector3 fromHere){
		if( (fromWhere != fromHere ) || (! hasComputedThisFrame)){
			fromWhere=fromHere;
			ComputePowerValue();
			hasComputedThisFrame = true;
		}
		return lastComputedPowerValue;
	}
	
	public float GetSpeed(){
		return transform.InverseTransformDirection(rb.velocity).z;
	}
	
	public float GetAltitude(){//in m
		return 100f; //TODO
	}
	
	public Vector3 GetPosition(){
		return rb.position;
	}
	
	public Vector2 GetHeading(){
		Vector3 heading = transform.forward;
		/*
		float horizontalComponant = Mathf.Sqrt (Mathf.Pow(heading.x,2) + Mathf.Pow(heading.z,2));
		float globalPitch = Mathf.Rad2Deg * Mathf.Atan2(heading.y,horizontalComponant);
		
		float horizontalHeading = Mathf.Atan2(heading.z,heading.x);
		*/
		
		return new Vector2(heading.x,heading.z);
	}
	
	public float GetAttitudeClimbAngle(){
		Vector3 heading = transform.forward;
		float horizontalComponant = Mathf.Sqrt (Mathf.Pow(heading.x,2) + Mathf.Pow(heading.z,2));
		return Mathf.Rad2Deg * Mathf.Atan2(heading.y,horizontalComponant);
	}
	
	public float GetRealClimbAngle(){
		Vector3 velocityVector = rb.velocity.normalized;
		float horizontalComponant = Mathf.Sqrt (Mathf.Pow(velocityVector.x,2) + Mathf.Pow(velocityVector.z,2));
		return Mathf.Rad2Deg * Mathf.Atan2(velocityVector.y,horizontalComponant);
	}
	
	public float GetPitch(){//TODO valeur erroné quand roll ~90° ??
		Vector3 planeIntersection = Vector3.Cross(transform.right,Vector3.up);
		float pitch = Vector3.Angle(planeIntersection,transform.forward);
		if(pitch>90){
			pitch = 180 - pitch;
		}
		if(transform.forward.y<0){
			pitch *= -1;
		}
		return pitch;
	}
	
	
	public float GetPitchSpeed(){
		return (GetPitch() - lastPitchValue) / Time.deltaTime;
	}
	
	public float GetRoll(){
		Vector3 planeIntersection = Vector3.Cross(Vector3.up,transform.forward);
		float roll = Vector3.Angle(planeIntersection,transform.right);
		if(transform.right.y<0){
			roll *= -1;
		}
		return roll;
	}
	
	public float GetRollSpeed(){
		return (GetRoll() - lastRollValue) / Time.deltaTime;
	}
	
	public bool IsStalling(){
		Vector3 localVelocityVector = transform.InverseTransformDirection(rb.velocity).normalized;
		localVelocityVector.x = 0;
		float angleOfAttack = Vector3.Angle(Vector3.forward,localVelocityVector);
		if(Vector3.Cross(localVelocityVector,Vector3.forward).x<0){
			angleOfAttack *= -1;
		}
		return Mathf.Abs(angleOfAttack)>10f;
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	private void ComputePowerValue(){
		lastComputedPowerValue = 0;
		for(int i =0; i<numberOfSources ;i++ ){
			float dist = Vector3.Distance(fromWhere, emissionSources[i]);
			lastComputedPowerValue += GetPowerFromDist(dist);
		}
		lastComputedPowerValue = Mathf.Clamp(lastComputedPowerValue,0,100);
	}
	
	private float GetPowerFromDist(float dist){
		float rawValue = 100f/ (1+Mathf.Pow((dist/70f),2));
		return Mathf.Clamp(rawValue,0,100);
	}
}
