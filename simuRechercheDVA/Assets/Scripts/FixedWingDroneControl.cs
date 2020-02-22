using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using static DroneFlightSim;



//high level control
public class FixedWingDroneControl : DroneControl
{

	
	protected DroneFlightSim sim;
	
	public PID rollPid = new PID(0.1f,0,0);
	public PID climbPid = new PID(0.3f,0.1f,0.01f);
	public PID speedPid = new PID(3f,0.05f,0.01f);
	public PID pitchPid = new PID(0.3f,0.1f,0.01f);
	public float baseRollForTurn = 65f;
	public float maximumClimbAngle = 30.0f;
	public float targetSpeed = 30f;
	
	
	private bool isCircling = false;
	private bool isFollowingDirectly = false;
	
	
	
	
	// Start is called before the first frame update
	public override void Start()
	{
		base.Start();
		sim = GetComponent<FixedWingFlightSim>();
	}


	
	
	public override void ControlLoop(){
		float thrustCommand = speedPid.Update(targetSpeed,sensor.GetSpeed(),Time.deltaTime);
		sim.SetMainThrust(thrustCommand);
		
		if(sensor.GetSpeed()>18){
		
			//CircleAround(10f);
			Vector3 target3DHeading = target.transform.position-sensor.GetPosition();
			if(target3DHeading.magnitude < 5){
				hasTarget = false;
			}
			
			
			
			isCircling = false;
			isFollowingDirectly = false;
			sim.SetPitchTorque(0);
			sim.SetRollTorque(0);
			
			
			
			
			SelectMove();
			
			if(isCircling){
				if(hasTarget){
					CircleAround(10);
				}
				else{
					CircleAround(0);
				}
			}
			else if(isFollowingDirectly){
				GoToWaypoint();
			} 
			
		}
		else{
			sim.SetRollTorque(0);
			float pitchCommand = pitchPid.Update(-30,sensor.GetPitch(),Time.deltaTime);
			sim.SetPitchTorque(-pitchCommand);
		}
		
		if(sensor.IsStalling()){
			modeDisplayText = ("mode : STALLING");
		}
	}
	
	public void SetSpeed(float newSpeed){
		targetSpeed = newSpeed;
	}
	
	
	public void CircleAround(float targetClimbAngle){
		targetClimbAngle = Mathf.Clamp(targetClimbAngle,-50,maximumClimbAngle);
		
		modeDisplayText = ("mode : circle (climb : "+targetClimbAngle + ")");
		
		float turnDirection = -1f; //1 : left, -1 : right;
		
		float currentRoll = sensor.GetRoll();
		float rollCommand = rollPid.Update( turnDirection * baseRollForTurn,currentRoll,Time.deltaTime);
		
		
		
		float currentClimb = sensor.GetRealClimbAngle();
		rollCommand -= turnDirection *climbPid.Update(targetClimbAngle,currentClimb,Time.deltaTime);
		
		sim.SetPitchTorque(-3);
		sim.SetRollTorque(rollCommand);
		
		
	}
	
	
	
	
	
	
	
	
	
	private void SelectMove(){
		Vector3 currentPos = transform.position;
		float horizontalDistance = Mathf.Sqrt (Mathf.Pow(target.transform.position.x-currentPos.x,2) + Mathf.Pow(target.transform.position.z-currentPos.z,2));
		float elevationDifference = target.transform.position.y-currentPos.y;
		
		float overallClimbAngle = Mathf.Atan2(elevationDifference,horizontalDistance);
		if(overallClimbAngle > maximumClimbAngle){
			isCircling = true;
		}
		else{
			isFollowingDirectly = true;
		}
	}
	
	private void GoToWaypoint(){
		if(hasTarget==false){
			isCircling = true;
			return;
		}
		
		Vector3 target3DHeading = target.transform.position-sensor.GetPosition();
		
		Vector2 targetHeading = new Vector2(target3DHeading.x,target3DHeading.z);
		Vector2 currentHeading = sensor.GetHeading();
		
		Vector3 targetHeading3D = new Vector3(targetHeading.x,0,targetHeading.y);
		Vector3 currentHeading3D = new Vector3(currentHeading.x,0,currentHeading.y);
		
		float angleDifference = Vector2.Angle(currentHeading,targetHeading);
		if(Vector3.Cross(currentHeading3D,targetHeading3D).y>0){
			angleDifference *= -1;
		}
		float horizontalComponant = Mathf.Sqrt (Mathf.Pow(target3DHeading.x,2) + Mathf.Pow(target3DHeading.z,2));
		float climbAngle =  Mathf.Rad2Deg * Mathf.Atan2(target3DHeading.y,horizontalComponant);
		
		CompoundMove(climbAngle,angleDifference);
		
	}
	
	private void CompoundMove(float targetClimbAngle,float turnAngle){
		modeDisplayText = ("mode : compound (turnAngle : " + turnAngle+"; targetClimbAngle : "+targetClimbAngle+")");
		
		float rollTargetForThisTurn = Mathf.Clamp(3*turnAngle,-baseRollForTurn,baseRollForTurn);
		
		
		float currentRoll = sensor.GetRoll();
		float baseRollCommand = rollPid.Update(rollTargetForThisTurn ,currentRoll,Time.deltaTime);
		
		float currentClimb = sensor.GetRealClimbAngle();
		float climbCommand = climbPid.Update(targetClimbAngle,currentClimb,Time.deltaTime);
		float climbRollCorrection = - Mathf.Sign(turnAngle) * climbCommand;
		float ratio = Mathf.Abs(climbRollCorrection/baseRollCommand);
		if( ratio > 0.8f ){//the correction for the climbing must always be bellow the one for roll to avoid spinning
			climbRollCorrection = Mathf.Abs(0.8f * baseRollCommand) * Mathf.Sign(climbRollCorrection);
		}
		float rollCommand = baseRollCommand + climbRollCorrection;
		sim.SetRollTorque(rollCommand);//positive : roll left
		
		
		
		float pitchSpeedThrottle = Map(0,3,0,baseRollForTurn,Mathf.Abs(currentRoll));//Pitch induced to turn
		sim.SetPitchTorque(-(pitchSpeedThrottle + climbCommand));
	}
	
	
	
	
	protected override void HandleKeyboardInput(){
		if (Input.GetKey(KeyCode.LeftShift)){
			sim.SetMainThrust(sim.GetMainThrust()+0.1f);
        }
		else if (Input.GetKey(KeyCode.LeftControl)){
			sim.SetMainThrust(sim.GetMainThrust()-0.1f);
        }
		
		//pitch
        if (Input.GetKey(KeyCode.Z)){
            sim.SetPitchTorque(15);
        }
		else if (Input.GetKey(KeyCode.S)){
            sim.SetPitchTorque(-22);
        }
		else{
			sim.SetPitchTorque(0);
		}
		
		//roll
		if (Input.GetKey(KeyCode.Q)){
            sim.SetRollTorque(15);
        }
		else if (Input.GetKey(KeyCode.D)){
            sim.SetRollTorque(-15);
        }
		else{
			sim.SetRollTorque(0);
		}
		
		//yaw
		if (Input.GetKey(KeyCode.A)){
            sim.SetYawTorque(15);
        }
		else if (Input.GetKey(KeyCode.E)) {
            sim.SetYawTorque(-15);
        }
		else{
			sim.SetYawTorque(0);
		}
	}
}
