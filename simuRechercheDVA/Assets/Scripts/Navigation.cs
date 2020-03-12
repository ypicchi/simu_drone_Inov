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
	protected bool isMissionStarted = false;
	protected bool isSearching = false;
	
	protected Queue<Pair<Vector3, Vector3>> mainWaypoints = new Queue<Pair<Vector3, Vector3>>();
	
	protected bool useWaypointY = true;

	protected List<Vector3> targetsFound = new List<Vector3>();


	//Awake is made to initialize variables. It is called before any Start()
	public virtual void Awake(){
		ctrl = GetComponent<DroneControl>();
		sensor = GetComponent<Sensor>();
	}

	// Start is called before the first frame update
	public virtual void Start()
	{
		waypointIndicator = GameObject.Instantiate(Resources.Load<GameObject>("Arrow"));
		waypointIndicator.name = "Waypoint";
		waypointIndicator.transform.position = sensor.GetPosition();
	}

	protected virtual void StartLog(){
		string path = "Assets/sensorOutput.wsv";

		FileStream stream = new FileStream(path, FileMode.OpenOrCreate,FileAccess.Write);  
		fileLog = new StreamWriter(stream);
	
		fileLog.WriteLine("x y z rx ry rz power");
	}

	void OnDestroy()
    {
        Object.Destroy(waypointIndicator);
    }

	public virtual void StartMission(){
		ctrl.SetWaypoint(waypointIndicator);
		isMissionStarted = true;
		isSearching = true;
		GenerateMainWaypoint();
		GenerateNextNavigationWaypoint();
		StartLog();
    }

    // Update is called once per frame
    public virtual void Update()
    {
		if(isMissionStarted){
			if( ! useWaypointY){
				Vector3 waypointPosition = waypointIndicator.transform.position;
    			waypointPosition.y = sensor.GetPosition().y;
        		waypointIndicator.transform.position = waypointPosition;
			}
			Vector3 linearDifference = waypointIndicator.transform.position-sensor.GetPosition();
			Vector2 targetHeadingt = new Vector2(waypointIndicator.transform.forward.x,waypointIndicator.transform.forward.z);
			float angularDifference = Vector2.Angle(targetHeadingt, sensor.GetHeading());
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
	
	
	
	protected virtual void GenerateMainWaypoint(){
		//nothing
	}
	
	protected abstract void GenerateNextNavigationWaypoint();
	
	protected abstract List<Vector3> ComputeTargetsPositions();
	
}
