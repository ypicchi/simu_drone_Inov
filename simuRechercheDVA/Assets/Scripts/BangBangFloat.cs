using System.Collections;
using UnityEngine;
[System.Serializable]
public class BangBangFloat: BangBang<float>
{

    protected float maxSpeed;
    protected float maxAcceleration;


    public BangBangFloat(float maxSpeed,float maxAcceleration){
        this.maxSpeed = maxSpeed;
        this.maxAcceleration = maxAcceleration;
    }

    public override float TimeRemaining(float currentTime){
        float dist = Mathf.Abs(targetPos-startPos);
        float maxSpeedlocalTime = maxSpeed/maxAcceleration;
        float distanceToMaxSpeed = maxAcceleration/2 * maxSpeedlocalTime*maxSpeedlocalTime;


        if(distanceToMaxSpeed > dist/2){
            //we never reach max speed

            float midCourseLocalTime = Mathf.Sqrt(dist/maxAcceleration);
            return Mathf.Max(2*midCourseLocalTime-currentTime,0);
        }else{
            float timeAtMaxSpeed = (dist - 2*distanceToMaxSpeed)/maxSpeed;
            return Mathf.Max(2*maxSpeedlocalTime + timeAtMaxSpeed - currentTime,0);
        }
    }
    
    public override float[] GetTarget(float currentTime)
    {
        if( ! isMoving ){
            return new float[2]{targetPos,0};
        }


        float localT = currentTime - movementStartTime;
        float dist = Mathf.Abs(targetPos-startPos);

        
        
        float maxSpeedlocalTime = maxSpeed/maxAcceleration;
        float distanceToMaxSpeed = maxAcceleration/2 * maxSpeedlocalTime*maxSpeedlocalTime;

        float speed = 0;
        float pos = 0;

        if(distanceToMaxSpeed > dist/2){
            //we never reach max speed

            float midCourseLocalTime = Mathf.Sqrt(dist/maxAcceleration);
            float midCourseSpeed = maxAcceleration * midCourseLocalTime;
            float midCourseDistance = maxAcceleration/2 * midCourseLocalTime*midCourseLocalTime;

            if(localT < 2*midCourseLocalTime){
            
                if(localT<midCourseLocalTime){
                    speed = localT * maxAcceleration;
                    pos += maxAcceleration/2 * localT*localT;
                }else{
                    float time3 = Mathf.Max(0,localT-midCourseLocalTime);
                    speed = midCourseSpeed - time3 * maxAcceleration;
                    pos += midCourseDistance + midCourseSpeed * time3 - maxAcceleration /2 * time3*time3;
				}
			}else{
                //idling
                isMoving = false;
                speed = 0;
                pos = targetPos;
                return new float[2]{pos,speed};
			}
        }else{
            //we do reach max speed

            float timeAtMaxSpeed = (dist - 2*distanceToMaxSpeed)/maxSpeed;
            
            float time1 = Mathf.Min(localT,maxSpeedlocalTime);
            pos += maxAcceleration/2 * time1*time1;
            float time2 = Mathf.Clamp(localT-maxSpeedlocalTime,0,timeAtMaxSpeed);
            pos += maxSpeed * time2;
            float time3 = Mathf.Max(0,localT-maxSpeedlocalTime-timeAtMaxSpeed);
            pos += maxSpeed * time3 - maxAcceleration /2 * time3*time3;
            
            if(localT<maxSpeedlocalTime){
                //acceleration
                speed = time1 * maxAcceleration;
			}
            if(localT >= maxSpeedlocalTime && localT<maxSpeedlocalTime + timeAtMaxSpeed){
                //at max speed
                speed = maxSpeed;
			}
            if(localT >= maxSpeedlocalTime + timeAtMaxSpeed && localT < 2*maxSpeedlocalTime + timeAtMaxSpeed){
                //braking
                speed = maxSpeed - time3 * maxAcceleration;
			}
            if(localT >= 2*maxSpeedlocalTime + timeAtMaxSpeed){
                //idling
                isMoving = false;
                speed = 0;
                pos = targetPos;
                return new float[2]{pos,speed};
			}
        }    
        float sign = Mathf.Sign(targetPos - startPos);
        pos *= sign;
        speed *= sign;

        pos += startPos;

        //print("goal -- pos : ",pos,"; speed : ",speed)
        return new float[2]{pos,speed};
    }
}
