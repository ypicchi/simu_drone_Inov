using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BangBangVector3 : BangBang <Vector3>
{
    
	
    protected BangBangFloat[] linearBangbang = new BangBangFloat[3];

    public BangBangVector3(Vector3 maxSpeed, Vector3 maxAcceleration){
		for(int i=0;i<3;i++){
			linearBangbang[i] = new BangBangFloat(maxSpeed[i],maxAcceleration[i]);
		}
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
