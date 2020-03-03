using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BangBangVector3 : BangBang <Vector3>
{
    
	
    protected BangBangFloat[] linearBangbang = new BangBangFloat[3];
	protected Vector3 maxSpeed;
	protected Vector3 maxAcceleration;
	protected Vector3 speedFactor;

    public BangBangVector3(Vector3 maxSpeed, Vector3 maxAcceleration){
		this.maxSpeed = maxSpeed;
		this.maxAcceleration = maxAcceleration;
		/*
		for(int i=0;i<3;i++){
			linearBangbang[i] = new BangBangFloat(maxSpeed[i],maxAcceleration[i]);
		}
		*/
		linearBangbang[0] = new BangBangFloat(maxSpeed[0],maxAcceleration[0]);
		linearBangbang[1] = new BangBangFloat(maxSpeed[1],maxAcceleration[1]);
		linearBangbang[2] = new BangBangFloat(maxSpeed[2],maxAcceleration[2]);
    }

	public override void StartMovement(Vector3 startPos,Vector3 targetPos,float currentTime){
		base.StartMovement(startPos,targetPos,currentTime);



		
		for (int i=0;i<3;i++){
			linearBangbang[i].StartMovement(startPos[i],targetPos[i],currentTime);
		}

		float longestTime = TimeRemaining(currentTime);
		if(longestTime == 0){
			speedFactor = Vector3.one;
		}else{
			for (int i=0;i<3;i++){
				float currentRemainingTime = linearBangbang[i].TimeRemaining(currentTime);
				speedFactor[i] = currentRemainingTime/longestTime;
			}
		}
		
		
	}

	public override float TimeRemaining(float currentTime){
		float maxTime = 0;
		for(int i=0;i<3;i++){
			float componantRemainingTime = linearBangbang[i].TimeRemaining(currentTime);
			if(componantRemainingTime>maxTime){
				maxTime = componantRemainingTime;
			}
		}
		return maxTime;
	}

	public override Vector3[] GetTarget(float currentTime){
        //linear pos, linear speed
		Vector3[] output = new Vector3[2];
		
		Vector3 expectedPos = Vector3.zero;
		Vector3 expectedSpeed = Vector3.zero;

		//combine the various bangbangs into a vector bangbang
		for(int i=0;i<3;i++){
			float[] command = linearBangbang[i].GetTarget(movementStartTime 
				+ (currentTime-movementStartTime) * speedFactor[i]);
			expectedPos[i] = command[0];
			expectedSpeed[i] = command[1] * speedFactor[i];
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
