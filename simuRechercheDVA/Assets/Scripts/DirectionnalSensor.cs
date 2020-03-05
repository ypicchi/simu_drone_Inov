using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionnalSensor : Sensor
{
    
    protected override void ComputePowerValue(){
		lastComputedPowerValue = 0;
		for(int i =0; i<numberOfSources ;i++ ){
			float dist = Vector3.Distance(fromWhere, emissionSources[i]);
			lastComputedPowerValue += GetPowerFromDist(dist) * DirectionalMultiplier(emissionSources[i]);
		}
		lastComputedPowerValue = Mathf.Clamp(lastComputedPowerValue,0,100);
	}

    protected float DirectionalMultiplier(Vector3 source){
        Vector3 heading = transform.forward;
        Vector3 targetHeading = source - fromWhere;
        float angle = Vector3.Angle(heading, targetHeading);
        return Map(1f,0f,0f,180f,angle);
    }


    //utility function. Map the value in old range to the value in new range (effectively scale and shift the value)
	protected static float Map(float newFrom, float newTo, float oldFrom, float oldTo, float value){
        if(value <= oldFrom){
            return newFrom;
        }else if(value >= oldTo){
            return newTo;
        }else{
            return (newTo - newFrom) * ((value - oldFrom) / (oldTo - oldFrom)) + newFrom;
        }
    }
}
