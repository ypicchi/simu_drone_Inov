using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;


public abstract class Navigation : MonoBehaviour
{
	

	
    protected DroneControl ctrl;
    protected Sensor sensor;
	protected GameObject waypointIndicator;
	
	public Vector3 researchZoneSize = new Vector3(100,0,100);
	public Vector3 researchZoneOrigin = new Vector3(10,5,50);
	
	public float samplingInterval = 0.2f;//in second
	public float waypointValidationDistance = 5f;//in m
	public float waypointValidationAngularThreshold = 10f;
	
	
	protected float previousSamplingTime = 0;
	
	
	protected StreamWriter fileLog;
	protected bool isSearching = true;
	
	protected Queue<Pair<Vector3, Vector3>> mainWaypoints = new Queue<Pair<Vector3, Vector3>>();
	
	// Start is called before the first frame update
	public virtual void Start()
	{

		ctrl = GetComponent<DroneControl>();
		sensor = GetComponent<Sensor>();
		//waypointIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		waypointIndicator = GameObject.Instantiate(Resources.Load<GameObject>("Arrow"));
		waypointIndicator.name = "Waypoint";
		waypointIndicator.transform.position = new Vector3(0,5,-50);
		ctrl.SetWaypoint(waypointIndicator);
		GenerateMainWaypoint();
		
		string path = "Assets/sensorOutput.wsv";

		FileStream stream = new FileStream(path, FileMode.OpenOrCreate,FileAccess.Write);  
		fileLog = new StreamWriter(stream);
	
		fileLog.WriteLine("x y z rx ry rz power");
		
	}

	void OnDestroy()
    {
        Object.Destroy(waypointIndicator);
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 linearDifference = waypointIndicator.transform.position-sensor.GetPosition();
		Vector2 targetHeadingt = new Vector2(waypointIndicator.transform.forward.x,waypointIndicator.transform.forward.z);
		float angularDifference = Vector2.SignedAngle(targetHeadingt, sensor.GetHeading());
		if(ValidateWaypoint(linearDifference,angularDifference)){
			GenerateNewWaypoint();
		}
		
		float power = sensor.GetSignalPower(transform.position);

		
		if(isSearching){
			float currentTime = Time.time;
			if(currentTime>previousSamplingTime+samplingInterval){
				previousSamplingTime = currentTime;
				LogDataPoint(power);
			}
		}
		
		
    }

	protected void LogDataPoint(float power){
		float heading = sensor.GetHeadingAsFloat();
		DataPoint currentPoint = new DataPoint(power,sensor.GetPosition(),
			new Vector3(sensor.GetPitch(),heading,sensor.GetRoll()));
		fileLog.Write(currentPoint.ToWSVLine());
		LoggingOverload(currentPoint);
	}

	protected virtual void LoggingOverload(DataPoint currentPoint){

	}

	protected virtual bool ValidateWaypoint(Vector3 linearDifference,float angularDifference){
		return linearDifference.magnitude < waypointValidationDistance && angularDifference < waypointValidationAngularThreshold;
	}
	
	public void AddWaypoint(Vector3 nextPoint){
		AddWaypoint(nextPoint, Vector3.zero);
	}
	public void AddWaypoint(Vector3 nextPoint, Vector3 eulerAngle){
		mainWaypoints.Enqueue(new Pair<Vector3, Vector3>(nextPoint, eulerAngle));
	}

	public void ClearWaypoint(){
		mainWaypoints.Clear();
	}

	protected void GenerateNewWaypoint(){
		GenerateNextNavigationWaypoint();
	}
	
	protected void GenerateRandomWaypoint(){
		Vector3 waypointPosition = new Vector3(Random.Range(-50.0f, 50.0f),Random.Range(-50.0f, 50.0f),Random.Range(-50.0f, 50.0f));
		waypointIndicator.transform.position = waypointPosition;
		ctrl.SetWaypoint(waypointIndicator);
	}
	
	
	
	protected abstract void GenerateMainWaypoint();
	
	protected abstract void GenerateNextNavigationWaypoint();
	
	protected abstract List<Vector3> computeTargetsPositions();
	
}
