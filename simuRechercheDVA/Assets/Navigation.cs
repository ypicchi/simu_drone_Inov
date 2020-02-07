using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Navigation : MonoBehaviour
{
	
	protected Text sensorPowerDisplay;
	
    protected DroneControl ctrl;
    protected Sensor sensor;
	protected GameObject waypointIndicator;
	
	public Vector3 researchZoneSize = new Vector3(100,0,100);
	public Vector3 researchZoneOrigin = new Vector3(10,5,50);
	
	public float samplingInterval = 0.2f;//in second
	public int waypointValidationDistance = 5;
	
	
	protected int numberWaypointReached = 0;
	protected float previousSamplingTime = 0;
	protected MaxHeap allDataPoint = new MaxHeap(1000);
	
	protected StreamWriter fileLog;
	protected bool isSearching = true;
	
	protected Queue<Vector3> mainWaypoints = new Queue<Vector3>();
	
	// Start is called before the first frame update
	public virtual void Start()
	{
		
		sensor = GetComponent<Sensor>();
		sensorPowerDisplay = GameObject.Find("Canvas/signalPowerDisplay").GetComponent<Text>();
		waypointIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		waypointIndicator.transform.position = new Vector3(0,5,-50);
		GenerateMainWaypoint();
		
		
		
		
		string path = "Assets/sensorOutput.wsv";

		FileStream stream = new FileStream(path, FileMode.OpenOrCreate,FileAccess.Write);  
		fileLog = new StreamWriter(stream);
	
		fileLog.WriteLine("x z power");
		
	}

    // Update is called once per frame
    void Update()
    {
		Vector3 target3DHeading = waypointIndicator.transform.position-sensor.GetPosition();
		if(target3DHeading.magnitude < waypointValidationDistance){
			GenerateNewWaypoint();
		}
		
		float power = sensor.GetSignalPower(transform.position);
		//Debug.Log("sensor power "+power);
		sensorPowerDisplay.text = "Signal power : "+power.ToString();
		
		if(isSearching){
			float currentTime = Time.time;
			if(currentTime>previousSamplingTime+samplingInterval){
				previousSamplingTime = currentTime;
				DataPoint currentPoint = new DataPoint(sensor.GetPosition(),power);
				allDataPoint.Add(currentPoint);
				fileLog.Write(currentPoint.ToWSVLine());
			}
		}
		
		
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
