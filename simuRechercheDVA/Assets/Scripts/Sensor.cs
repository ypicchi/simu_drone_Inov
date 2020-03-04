using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
	
	
	
	private Rigidbody rb;

	public string rollDisplayText;
	public string pitchDisplayText;
	public string climbAngleDisplayText;
	public string realClimbAngleDisplayText;
	public string forwardSpeedDisplayText;

	protected float ultrasonicRange = 3f;//used to detect the ground distance
	
	protected const int numberOfSources = 1;
	protected Vector3[] emissionSources = new Vector3[numberOfSources];
	
	
	protected bool hasComputedThisFrame = false;
	protected Vector3 fromWhere;
	protected float lastComputedPowerValue;
	
	protected float lastRollValue = 0;
	protected float lastPitchValue = 0;

	private GameObject sphere;
	
    // Start is called before the first frame update
    void Start()
    {
		rb = GetComponent<Rigidbody>();
        emissionSources[0] = new Vector3(50,0,100);
		
		for(int i =0; i<numberOfSources ;i++ ){
			sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			sphere.transform.position = emissionSources[i];
			sphere.name = "signalTransmitter";
		}
		
		

    }

	void OnDestroy()
    {
        Object.Destroy(sphere);
    }

    // Update is called once per frame
    void Update()
    {
		float currentRoll = GetRoll();
		float currentPitch = GetPitch();
		
		rollDisplayText = "roll : "+currentRoll.ToString();
		pitchDisplayText = "pitch : "+currentPitch.ToString();
		climbAngleDisplayText = "climb angle : "+GetAttitudeClimbAngle().ToString();
		realClimbAngleDisplayText = "real climb angle : "+GetRealClimbAngle().ToString();
		forwardSpeedDisplayText = "forward speed : "+GetForwardSpeed().ToString();
		
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
	
	public float GetForwardSpeed(){
		return transform.InverseTransformDirection(rb.velocity).z;
	}

	public float GetSpeedAsFloat(){
		return rb.velocity.magnitude;
	}

	public Vector3 GetSpeed(){
		return transform.InverseTransformDirection(rb.velocity);
	}

	public float GetVerticalSpeed(){
		return transform.InverseTransformDirection(rb.velocity).y;
	}
	
	public float GetAltitude(){
		return rb.position.y;
	}

	public float GetTheoricalRangefinderRange(){
		return ultrasonicRange;
	}
	
	public float GetDistanceToGround(){//simulate an ultrasonic rangefinder so it have a limited range
		RaycastHit hit;
		Ray downRay;

		GameObject rangefinder = GameObject.Find("Ultrason");
		if(rangefinder==null){
			downRay = new Ray(transform.position, -transform.up);
		}else{
			downRay = new Ray(rangefinder.transform.position, -transform.up);
		}

		float distance = float.PositiveInfinity;

		//int layerMask = 1 << Physics.IgnoreRaycastLayer;
		//layerMask = ~layerMask; //everything but this layer
        if (Physics.Raycast(downRay, out hit,ultrasonicRange)){
			if(hit.distance<ultrasonicRange)
			distance = hit.distance;
		}
		return distance;
	}
	
	public Vector3 GetPosition(){
		return rb.position;
	}
	
	public Vector2 GetHeading(){
		Vector3 heading = transform.forward;
		return new Vector2(heading.x,heading.z);
	}

	public float GetHeadingAsFloat(){
		Vector3 heading = transform.forward;
		
		return - Vector2.SignedAngle(Vector2.up, GetHeading());
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
	
	public float GetPitch(){
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
	
	
	
	
	
	
	
	
	
	
	
	
	
	protected virtual void ComputePowerValue(){
		lastComputedPowerValue = 0;
		for(int i =0; i<numberOfSources ;i++ ){
			float dist = Vector3.Distance(fromWhere, emissionSources[i]);
			lastComputedPowerValue += GetPowerFromDist(dist);
		}
		lastComputedPowerValue = Mathf.Clamp(lastComputedPowerValue,0,100);
	}
	
	protected float GetPowerFromDist(float dist){
		float rawValue = 100f/ (1+Mathf.Pow((dist/70f),2));
		return Mathf.Clamp(rawValue,0,100);
	}
}
