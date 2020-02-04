using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using static DroneFlightSim;



//high level control
public class DroneControl : MonoBehaviour
{

	private DroneFlightSim sim;
	private Sensor sensor;
	private Text modeDisplay;
	
	
	public PID rollPid = new PID(0.1f,0,0);
	public PID climbPid = new PID(0.3f,0.1f,0.01f);
	public PID speedPid = new PID(3f,0.05f,0.01f);
	public PID pitchPid = new PID(0.3f,0.1f,0.01f);
	public float baseRollForTurn = 65f;
	public float maximumClimbAngle = 30.0f;
	public float targetSpeed = 30f;
	
	
	private bool isCircling = false;
	private bool isFollowingDirectly = false;
	
	private GameObject target;
	private bool hasTarget = false;
	
	
	// Start is called before the first frame update
	void Start()
	{
		sim = GetComponent<DroneFlightSim>();
		sensor = GetComponent<Sensor>();
		
		modeDisplay = GameObject.Find("Canvas/modeDisplay").GetComponent<Text>();
		
		
	/*	
	Vector3 size;
	MeshRenderer renderer;
    renderer = GetComponent<MeshRenderer>();
    size = renderer.bounds.size;
	Debug.Log(size);
	*/
	}

	// Update is called once per frame
	void Update()
	{
		HandleKeyboardInput();//keyboard override
		
		modeDisplay.text = ("mode : ");
		
		float thrustCommand = speedPid.Update(targetSpeed,sensor.GetSpeed(),Time.deltaTime);
		sim.SetThrust(thrustCommand);
		
		if(sensor.GetSpeed()>18){
		
			//CircleAround(10f);
			Vector3 target3DHeading = target.transform.position-sensor.GetPosition();
			if(target3DHeading.magnitude < 5){
				hasTarget = false;
			}
			
			
			
			isCircling = false;
			isFollowingDirectly = false;
			sim.SetPitchDroneSpeed(0);
			sim.SetRollDroneSpeed(0);
			
			
			
			
			SelectMove();
			
			if(isCircling){
				if(hasTarget){
					CircleAround(10);
				}else{
					CircleAround(0);
				}
			}else if(isFollowingDirectly){
				GoToWaypoint();
			} 
			
		}else{
			sim.SetRollDroneSpeed(0);
			float pitchCommand = pitchPid.Update(-30,sensor.GetPitch(),Time.deltaTime);
			sim.SetPitchDroneSpeed(pitchCommand);
		}
		if(sensor.IsStalling()){
			modeDisplay.text = ("mode : STALLING");
		}
		
	}
	
	
	
	
	public void SetWaypoint(GameObject waypointIndicator){
		target = waypointIndicator;
		hasTarget = true;
	}
	
	public void SetSpeed(float newSpeed){
		targetSpeed = newSpeed;
	}
	
	
	public void CircleAround(float targetClimbAngle){
		targetClimbAngle = Mathf.Clamp(targetClimbAngle,-50,maximumClimbAngle);
		
		modeDisplay.text = ("mode : circle (climb : "+targetClimbAngle + ")");
		
		float turnDirection = -1f; //1 : left, -1 : right;
		
		float currentRoll = sensor.GetRoll();
		float rollCommand = rollPid.Update( turnDirection * baseRollForTurn,currentRoll,Time.deltaTime);
		
		
		
		float currentClimb = sensor.GetRealClimbAngle();
		rollCommand -= turnDirection *climbPid.Update(targetClimbAngle,currentClimb,Time.deltaTime);
		
		sim.SetPitchDroneSpeed(3);
		sim.SetRollDroneSpeed(rollCommand);
		
		//TODO
	}
	
	
	
	
	
	
	
	
	
	
	
	
	private void SelectMove(){
		Vector3 currentPos = transform.position;
		float horizontalDistance = Mathf.Sqrt (Mathf.Pow(target.transform.position.x-currentPos.x,2) + Mathf.Pow(target.transform.position.z-currentPos.z,2));
		float elevationDifference = target.transform.position.y-currentPos.y;
		
		float overallClimbAngle = Mathf.Atan2(elevationDifference,horizontalDistance);
		if(overallClimbAngle > maximumClimbAngle){
			isCircling = true;
		}else{
			isFollowingDirectly = true;
		}//TODO
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
	
	private void CompoundMove(float targetClimbAngle,float turnAngle){//TODO bug pour tourner a droite. Roll mauvais sens ?
		modeDisplay.text = ("mode : compound (turnAngle : " + turnAngle+"; targetClimbAngle : "+targetClimbAngle+")");
		
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
		sim.SetRollDroneSpeed(rollCommand);//positive : roll left
		
		
		
		float pitchSpeedThrottle = Map(0,3,0,baseRollForTurn,Mathf.Abs(currentRoll));//Pitch induced to turn
		sim.SetPitchDroneSpeed(pitchSpeedThrottle + climbCommand);
	}
	
	
	private static float Map(float newFrom, float newTo, float oldFrom, float oldTo, float value){
        if(value <= oldFrom){
            return newFrom;
        }else if(value >= oldTo){
            return newTo;
        }else{
            return (newTo - newFrom) * ((value - oldFrom) / (oldTo - oldFrom)) + newFrom;
        }
    }
	
	private void HandleKeyboardInput(){

		if (Input.GetKey(KeyCode.LeftShift)){
			sim.SetThrust(sim.GetThrust()+0.1f);
        }else if (Input.GetKey(KeyCode.LeftControl)){
			sim.SetThrust(sim.GetThrust()-0.1f);
        }
		
		//pitch
		float pitchSpeed = sim.GetPitchDroneSpeed();
        if (Input.GetKey(KeyCode.Z)){
            sim.SetPitchDroneSpeed(-(pitchSpeed + 2*sim.angularSpeedDecay));
        }else if (Input.GetKey(KeyCode.S)){
            sim.SetPitchDroneSpeed(-(pitchSpeed - 2*sim.angularSpeedDecay));
        }else{
			sim.SetPitchDroneSpeed(-(pitchSpeed - Mathf.Sign(pitchSpeed)*sim.angularSpeedDecay));
		}
		
		//roll
		float rollSpeed = sim.GetRollDroneSpeed();
		if (Input.GetKey(KeyCode.Q)){
            sim.SetRollDroneSpeed(rollSpeed + 2*sim.angularSpeedDecay);
        }else if (Input.GetKey(KeyCode.D)){
            sim.SetRollDroneSpeed(rollSpeed - 2*sim.angularSpeedDecay);
        }else{
			sim.SetRollDroneSpeed(rollSpeed - Mathf.Sign(rollSpeed)*sim.angularSpeedDecay);
		}
		
		//yaw
		float yawSpeed = sim.GetYawDroneSpeed();
		if (Input.GetKey(KeyCode.A)){
            sim.SetYawDroneSpeed(yawSpeed - 2*sim.angularSpeedDecay);
        }else if (Input.GetKey(KeyCode.E)) {
            sim.SetYawDroneSpeed(yawSpeed + 2*sim.angularSpeedDecay);
        }else{
			sim.SetYawDroneSpeed(yawSpeed - Mathf.Sign(yawSpeed)*sim.angularSpeedDecay);
		}
		
	}
}
