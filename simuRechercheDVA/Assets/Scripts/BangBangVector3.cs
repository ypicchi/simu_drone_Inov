using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BangBangVector3 : BangBang <Vector3>
{
    
    

    //TODO make it so the accelerations are modular (get them through the constructor)
    protected float maxAngularAcceleration = 1f;
	protected float maxLinearAcceleration = 1f;//bound to maxThrust and maxTiltAngle, but we simplify with this
	protected float maxLinearSpeed = 10f;
	protected float maxThrust = 300f;

	
    protected BangBangFloat[] linearBangbang = new BangBangFloat[3];

    public BangBangVector3(){
        linearBangbang[0] = new BangBangFloat(maxLinearSpeed,maxLinearAcceleration);
		linearBangbang[1] = new BangBangFloat(maxLinearSpeed,maxLinearAcceleration);
		linearBangbang[2] = new BangBangFloat(maxLinearSpeed,maxLinearAcceleration);
    }

	public override Vector3[] GetTarget(float currentTime){
        //linear pos, linear speed
		Vector3[] output = new Vector3[2];
		
		Vector3 expectedPos = Vector3.zero;
		Vector3 expectedSpeed = Vector3.zero;

		//combine the various bangbangs into a vector bangbang
		for(int i=0;i<3;i++){
			if( ! linearBangbang[i].IsMoving){
				linearBangbang[i].StartMovement(startPos[i],targetPos[i],Time.time);
			}
			float[] command = linearBangbang[i].GetTarget(Time.time);
			expectedPos[i] = command[0];
			expectedSpeed[i] = command[1];
		}
		output[0] = expectedPos;
		output[1] = expectedSpeed;
		
        //update the isMoving field of this class
        isMoving = false;
        for(int i=0;i<3;i++){
			isMoving |= linearBangbang[i].IsMoving;
        }
		

		return output;
	}




}
