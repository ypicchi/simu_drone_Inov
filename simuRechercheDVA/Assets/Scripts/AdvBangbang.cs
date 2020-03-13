using System.Collections;
using UnityEngine;
[System.Serializable]
public class AdvBangBang: BangBang<float>
{

    protected float maxSpeed;
    protected float maxAcceleration;

    protected int scenario = 0;


    public AdvBangBang(float maxSpeed,float maxAcceleration){
        this.maxSpeed = maxSpeed;
        this.maxAcceleration = maxAcceleration;
    }

    public override void StartMovement(float startPos,float targetPos,float currentTime){
        base.StartMovement(startPos,targetPos,currentTime);
        float speedEpilon = 0.005f; //5mm/s
        if(Mathf.Abs(this.startSpeed)<speedEpilon){
            this.startSpeed = 0;
        }
        ComputeScenario();
    }
    public override void StartMovement(float startPos,float targetPos,float startSpeed,float currentTime){
        base.StartMovement(startPos,targetPos,startSpeed,currentTime);
        float speedEpilon = 0.005f; //5mm/s
        if(Mathf.Abs(this.startSpeed)<speedEpilon){
            this.startSpeed = 0;
        }
        ComputeScenario();
    }
    public override float TimeRemaining(float currentTime){
        
        float distanceToTarget = Mathf.Abs(targetPos-startPos);


        //maxSpeed to 0
        float timeZeroToMaxSpeed = maxSpeed/maxAcceleration;
        float distanceZeroToMaxSpeed = maxAcceleration/2 * timeZeroToMaxSpeed*timeZeroToMaxSpeed;

        //startSpeed to 0
        float timeToStop = Mathf.Abs(startSpeed/maxAcceleration);
        float distanceToStop = maxAcceleration/2 * timeToStop*timeToStop;

        float outputTime = 0f;

        switch(scenario){
            case 1:
                float timeToMaxSpeed = (maxSpeed-startSpeed)/maxAcceleration;
                float distanceToMaxSpeed = maxAcceleration/2 * timeToMaxSpeed*timeToMaxSpeed + Mathf.Min(maxSpeed,startSpeed)*timeToMaxSpeed;
                float constantSpeedDuration = (distanceToTarget - (distanceToMaxSpeed+distanceZeroToMaxSpeed)) / maxSpeed;
                outputTime = timeToMaxSpeed + constantSpeedDuration + timeZeroToMaxSpeed;
                break;
            case 2:
                float distToMaxSpeedReached = (distanceToTarget-distanceToStop)/2;
                float timeToMaxSpeedReached = (-startSpeed 
                    + Mathf.Sqrt(startSpeed*startSpeed + 2*maxAcceleration*distToMaxSpeedReached)
                    ) /maxAcceleration;

                outputTime = 2*timeToMaxSpeedReached + timeToStop;
                break;
            case 3:
                distToMaxSpeedReached = (distanceToStop-distanceToTarget)/2;
                timeToMaxSpeedReached = Mathf.Sqrt(2*distToMaxSpeedReached/maxAcceleration);
                

                outputTime = timeToStop + 2*timeToMaxSpeedReached;
                break;
            case 4:
                constantSpeedDuration = (distanceToTarget - distanceToStop + distanceZeroToMaxSpeed) / -maxSpeed;
                outputTime = (timeToStop + timeZeroToMaxSpeed) + constantSpeedDuration + timeZeroToMaxSpeed;
                break;
            case 5:
                distToMaxSpeedReached = (distanceToStop+distanceToTarget)/2;
                timeToMaxSpeedReached = Mathf.Sqrt(2*distToMaxSpeedReached/maxAcceleration);
                outputTime = timeToStop + 2*timeToMaxSpeedReached;
                break;
            case 6:
                timeToMaxSpeed = (maxSpeed-startSpeed)/maxAcceleration;
                distanceToMaxSpeed = maxAcceleration/2 * timeToMaxSpeed*timeToMaxSpeed + startSpeed*timeToMaxSpeed;
                constantSpeedDuration = (distanceToTarget - distanceToMaxSpeed - distanceZeroToMaxSpeed) / maxSpeed;
                outputTime = timeToMaxSpeed + constantSpeedDuration + timeZeroToMaxSpeed;
                break;
            default:Debug.Log("default case bangbang : no scenario set up");
                break;
        }

        return outputTime + movementStartTime - currentTime;
    }
    
    public override float[] GetTarget(float currentTime)
    {
        if( ! isMoving ){
            return new float[2]{targetPos,0};
        }

        bool isReversed = false;
        if(startPos > targetPos){
            isReversed = true;
            startSpeed *= -1;
        }


        float movementTime = currentTime - movementStartTime;
        float distanceToTarget = Mathf.Abs(targetPos-startPos);

        float speed = 0;
        float distanceTravelled = 0;

        //maxSpeed to 0
        float timeZeroToMaxSpeed = maxSpeed/maxAcceleration;
        float distanceZeroToMaxSpeed = maxAcceleration/2 * timeZeroToMaxSpeed*timeZeroToMaxSpeed;

        //startSpeed to 0
        float timeToStop = timeToStop = Mathf.Abs(startSpeed/maxAcceleration);
        float distanceToStop = maxAcceleration/2 * timeToStop*timeToStop;

        
        switch(scenario){
            case 1:
                float timeToMaxSpeed = (maxSpeed-startSpeed)/maxAcceleration;
                if(movementTime < timeToMaxSpeed){
                    //we go to max speed
                    float localTime = movementTime;
                    if(startSpeed>maxSpeed){
                        distanceTravelled += startSpeed*localTime - maxAcceleration/2 * localTime*localTime;
                        speed = startSpeed - maxAcceleration * localTime;
                    }else{
                        distanceTravelled += startSpeed*localTime + maxAcceleration/2 * localTime*localTime;
                        speed = startSpeed + maxAcceleration * localTime;
                    }
                }else{
                    float distanceToMaxSpeed = maxAcceleration/2 * timeToMaxSpeed*timeToMaxSpeed + Mathf.Min(maxSpeed,startSpeed)*timeToMaxSpeed;
                    distanceTravelled += distanceToMaxSpeed;

                    float constantSpeedDuration = (distanceToTarget - (distanceToMaxSpeed+distanceZeroToMaxSpeed)) / maxSpeed;
                    if(movementTime < timeToMaxSpeed + constantSpeedDuration){
                        //we are in cruise
                        float localTime = movementTime - timeToMaxSpeed;
                        distanceTravelled += localTime * maxSpeed;
                        speed = maxSpeed;
                    }else{
                        distanceTravelled += constantSpeedDuration * maxSpeed;
                        if(movementTime < timeToMaxSpeed + constantSpeedDuration + timeZeroToMaxSpeed){
                            //now we brake
                            float localTime = movementTime - timeToMaxSpeed - constantSpeedDuration;
                            distanceTravelled += maxSpeed*localTime - maxAcceleration/2 * localTime*localTime;
                            speed = maxSpeed - maxAcceleration * localTime;
                        }else{
                            //idling
                            distanceTravelled += distanceZeroToMaxSpeed;
                            speed = 0;
                        }
                    }
                }
                break;


            case 2:
                //knowing the time and the distance to the max spead reached
                //require solving the second degree equation :
                //d = startSpeed*t + maxAcceleration/2 * t²
                //where t is the time to the max speed reached and d is the 
                //distance travelled in that time and is :
                //(distanceToTarget-(maxAcceleration/2 * (startSpeed/maxAcceleration)²))/2
                //we get two solution for t, but only one is positive
                

                

                float distToMaxSpeedReached = (distanceToTarget-distanceToStop)/2;
                float timeToMaxSpeedReached = (-startSpeed 
                    + Mathf.Sqrt(startSpeed*startSpeed + 2*maxAcceleration*distToMaxSpeedReached)
                    ) /maxAcceleration;
                float maxSpeedReached = startSpeed + maxAcceleration * timeToMaxSpeedReached;

                if(movementTime < timeToMaxSpeedReached){
                    //accelerating
                    float localTime = movementTime;
                    distanceTravelled += startSpeed*localTime + maxAcceleration/2 * localTime*localTime;
                    speed = startSpeed + maxAcceleration * localTime;
                }else{
                    distanceTravelled += distToMaxSpeedReached;
                    if(movementTime< 2*timeToMaxSpeedReached+timeToStop){
                        //braking
                        float localTime = movementTime - timeToMaxSpeedReached;
                        distanceTravelled += maxSpeedReached*localTime - maxAcceleration/2 * localTime*localTime;
                        speed = maxSpeedReached - maxAcceleration * localTime;
                    }else{
                        //idling
                        float timeToStopThisSpeed = maxSpeedReached/maxAcceleration;
                        float distanceToStopThisSpeed = maxAcceleration/2 * timeToStopThisSpeed*timeToStopThisSpeed;

                        distanceTravelled += distanceToStopThisSpeed;
                        speed = 0;
                    }
                }
                break;


            case 3:
                //scenario 3 : banbang overshot but cannot reach negative max speed


                distToMaxSpeedReached = (distanceToStop-distanceToTarget)/2;
                timeToMaxSpeedReached = Mathf.Sqrt(2*distToMaxSpeedReached/maxAcceleration);
                maxSpeedReached = maxAcceleration * timeToMaxSpeedReached;


                if(movementTime < timeToStop + timeToMaxSpeedReached){
                    //accelerating
                    float localTime = movementTime;
                    distanceTravelled += startSpeed*localTime - maxAcceleration/2 * localTime*localTime;
                    speed = startSpeed - maxAcceleration * localTime;
                }else{
                    distanceTravelled += distanceToStop - distToMaxSpeedReached;

                    if(movementTime < timeToStop + 2*timeToMaxSpeedReached){
                        //now we brake
                        float localTime = movementTime - (timeToStop + timeToMaxSpeedReached);
                        distanceTravelled += -maxSpeedReached*localTime + maxAcceleration/2 * localTime*localTime;
                        speed = -maxSpeedReached + maxAcceleration * localTime;
                    }else{
                        //idling
                        distanceTravelled += -distToMaxSpeedReached;
                        speed = 0;
                    }
                }
                break;


            case 4:
                if(movementTime < timeToStop + timeZeroToMaxSpeed){
                    //accelerating
                    float localTime = movementTime;
                    distanceTravelled += startSpeed*localTime - maxAcceleration/2 * localTime*localTime;
                    speed = startSpeed - maxAcceleration * localTime;
                }else{
                    distanceTravelled += distanceToStop - distanceZeroToMaxSpeed;

                    float constantSpeedDuration = (distanceToTarget - distanceToStop + distanceZeroToMaxSpeed) / -maxSpeed;
                    if(movementTime < (timeToStop + timeZeroToMaxSpeed) + constantSpeedDuration){
                        //we are in cruise
                        float localTime = movementTime - (timeToStop + timeZeroToMaxSpeed);
                        distanceTravelled += localTime * -maxSpeed;
                        speed = -maxSpeed;
                    }else{
                        distanceTravelled += constantSpeedDuration * -maxSpeed;
                        if(movementTime < (timeToStop + timeZeroToMaxSpeed) + constantSpeedDuration + timeZeroToMaxSpeed){
                            //now we brake
                            float localTime = movementTime - (timeToStop + timeZeroToMaxSpeed) - constantSpeedDuration;
                            distanceTravelled += -maxSpeed*localTime + maxAcceleration/2 * localTime*localTime;
                            speed = -maxSpeed + maxAcceleration * localTime;
                        }else{
                            //idling
                            distanceTravelled += -distanceZeroToMaxSpeed;
                            speed = 0;
                        }
                    }

                }
                break;


            case 5:
                distToMaxSpeedReached = (distanceToStop+distanceToTarget)/2;
                timeToMaxSpeedReached = Mathf.Sqrt(2*distToMaxSpeedReached/maxAcceleration);
                maxSpeedReached = maxAcceleration * timeToMaxSpeedReached;


                if(movementTime < timeToStop + timeToMaxSpeedReached){
                    //accelerating
                    float localTime = movementTime;
                    distanceTravelled += startSpeed*localTime + maxAcceleration/2 * localTime*localTime;
                    speed = startSpeed + maxAcceleration * localTime;
                }else{
                    distanceTravelled += - distanceToStop + distToMaxSpeedReached;

                    if(movementTime < timeToStop + 2*timeToMaxSpeedReached){
                        //now we brake
                        float localTime = movementTime - (timeToStop + timeToMaxSpeedReached);
                        distanceTravelled += maxSpeedReached*localTime - maxAcceleration/2 * localTime*localTime;
                        speed = maxSpeedReached - maxAcceleration * localTime;
                    }else{
                        //idling
                        distanceTravelled += +distToMaxSpeedReached;
                        speed = 0;
                    }
                }
                break;


            case 6:
                timeToMaxSpeed = (maxSpeed-startSpeed)/maxAcceleration;
                if(movementTime < timeToMaxSpeed){
                    //we go to max speed
                    float localTime = movementTime;
                    
                    distanceTravelled += startSpeed*localTime + maxAcceleration/2 * localTime*localTime;
                    speed = startSpeed + maxAcceleration * localTime;
                    
                }else{
                    float distanceToMaxSpeed = maxAcceleration/2 * timeToMaxSpeed*timeToMaxSpeed + startSpeed*timeToMaxSpeed;
                    distanceTravelled += distanceToMaxSpeed;

                    float constantSpeedDuration = (distanceToTarget - distanceToMaxSpeed - distanceZeroToMaxSpeed) / maxSpeed;
                    if(movementTime < timeToMaxSpeed + constantSpeedDuration){
                        //we are in cruise
                        float localTime = movementTime - timeToMaxSpeed;
                        distanceTravelled += localTime * maxSpeed;
                        speed = maxSpeed;
                    }else{
                        distanceTravelled += constantSpeedDuration * maxSpeed;
                        if(movementTime < timeToMaxSpeed + constantSpeedDuration + timeZeroToMaxSpeed){
                            //now we brake
                            float localTime = movementTime - timeToMaxSpeed - constantSpeedDuration;
                            distanceTravelled += maxSpeed*localTime - maxAcceleration/2 * localTime*localTime;
                            speed = maxSpeed - maxAcceleration * localTime;
                        }else{
                            //idling
                            distanceTravelled += distanceZeroToMaxSpeed;
                            speed = 0;
                        }
                    }
                }
                break;

                
            default:Debug.Log("default case bangbang : no scenario set up");
                break;
        }


        if(isReversed){
            startSpeed *= -1;
            distanceTravelled *= -1;
            speed *= -1;
        }

        return new float[2]{startPos + distanceTravelled,speed};
    }

    protected void ComputeScenario(){

        //maxspeed to 0
        float timeZeroToMaxSpeed = maxSpeed/maxAcceleration;
        float distanceZeroToMaxSpeed = maxAcceleration/2 * timeZeroToMaxSpeed*timeZeroToMaxSpeed;

        float distanceToTarget = Mathf.Abs(targetPos-startPos);

        bool isReversed = false;
        if(startPos > targetPos){
            isReversed = true;
            startSpeed *= -1;
        }

        //startspeed to 0
        float timeToStop = Mathf.Abs(startSpeed/maxAcceleration);
        float distanceToStop = maxAcceleration/2 * timeToStop*timeToStop;
        if(startSpeed>=0){
            //we are heading toward the target
            if(distanceToStop <= distanceToTarget){
                //We don't overshot the target
                float timeToMaxSpeed = (maxSpeed-startSpeed)/maxAcceleration;
                float distanceToMaxSpeed = maxAcceleration/2 * timeToMaxSpeed*timeToMaxSpeed + startSpeed*timeToMaxSpeed;
                if(startSpeed>=maxSpeed || (distanceZeroToMaxSpeed + distanceToMaxSpeed)<distanceToTarget){
                    //we can reach maxspeed and have a plateau (potentially of 0 second)
                    scenario = 1;
                }else{
                    //we cannot accelerate to maxspeed without overshooting
                    scenario = 2;
                }
            }else{
                //Even at max acceleration we overshot the target
                if((distanceToStop - 2*distanceZeroToMaxSpeed) < distanceToTarget){
                    //we cannot accelerate to negative maxspeed without undershooting
                    scenario = 3;
                }else{
                    //we can reach negative maxspeed and have a plateau (potentially of 0 second)
                    scenario = 4;
                }
            }
        }else{
            //we are currently heading away from the target
            if(distanceToTarget + distanceToStop < 2*distanceZeroToMaxSpeed){
                //we cannot reach positive max speed without overshooting
                scenario = 5;
            }else{
                //we can reach positive maxspeed and have a plateau
                scenario = 6;
            }
        }



        if(isReversed){
            startSpeed *= -1;
        }

        Debug.Log("bangbang scenario : " + scenario);

    }
        
}
