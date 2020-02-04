using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchBehavior : MonoBehaviour
{
	
	private Text sensorPowerDisplay;
	
    private DroneControl ctrl;
    private Sensor sensor;
	private GameObject waypointIndicator;
	
	public Vector3 researchZoneSize = new Vector3(100,0,100);
	public Vector3 researchZoneOrigin = new Vector3(10,5,50);
	
	public float samplingInterval = 0.2f;//in second
	
	private int numberWaypointReached = 0;
	private float previousSamplingTime = 0;
	private MaxHeap allDataPoint = new MaxHeap(1000);
	
	private StreamWriter fileLog;
	private bool isSweaping = true;
	
	
	
	// Start is called before the first frame update
	void Start()
	{
		ctrl = GetComponent<DroneControl>();
		sensor = GetComponent<Sensor>();
		sensorPowerDisplay = GameObject.Find("Canvas/signalPowerDisplay").GetComponent<Text>();
		waypointIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		waypointIndicator.transform.position = new Vector3(0,5,-50);
		GenerateMainWaypoint();
		
		
		ctrl.SetWaypoint(waypointIndicator);
		
		string path = "Assets/sensorOutput.wsv";

		FileStream stream = new FileStream(path, FileMode.OpenOrCreate,FileAccess.Write);  
		// Create a StreamWriter from FileStream  
		fileLog = new StreamWriter(stream);
	
        //Write some text to the test.txt file
        //fileLog = new StreamWriter(path, FileMode.Create);
		/*
		string path = Application.persistentDataPath + Path.DirectorySeparatorChar + "sensorOutput.csv";
		FileStream fileLog = File.Open(path, FileMode.OpenOrCreate);
		fileLog = new StreamWriter(fileLog);
		*/
		fileLog.WriteLine("x z power");
		
	}

    // Update is called once per frame
    void Update()
    {
		Vector3 target3DHeading = waypointIndicator.transform.position-sensor.GetPosition();
		if(target3DHeading.magnitude < 5){
			GenerateNewWaypoint();
		}
		
		float power = sensor.GetSignalPower(transform.position);
		//Debug.Log("sensor power "+power);
		sensorPowerDisplay.text = "Signal power : "+power.ToString();
		
		if(isSweaping){
			float currentTime = Time.time;
			if(currentTime>previousSamplingTime+samplingInterval){
				previousSamplingTime = currentTime;
				DataPoint currentPoint = new DataPoint(sensor.GetPosition(),power);
				allDataPoint.Add(currentPoint);
				fileLog.Write(currentPoint.ToWSVLine());
			}
		}
		
		
    }
	
	
	
	private void GenerateNewWaypoint(){
		GenerateNextNavigationWaypoint();
		
	}
	
	private void GenerateRandomWaypoint(){
		Vector3 waypointPosition = new Vector3(Random.Range(-50.0f, 50.0f),Random.Range(-50.0f, 50.0f),Random.Range(-50.0f, 50.0f));
		waypointIndicator.transform.position = waypointPosition;
		ctrl.SetWaypoint(waypointIndicator);
	}
	
	private Queue<Vector3> mainWaypoints = new Queue<Vector3>();
	public float bandWidth = 10f;
	
	private void GenerateMainWaypoint(){
		Vector3 nextPoint = researchZoneOrigin;
		
		bool reverse = false;
		while(nextPoint.x < researchZoneOrigin.x + researchZoneSize.x){
			if(reverse){
				nextPoint.z = researchZoneOrigin.z + researchZoneSize.z;
				mainWaypoints.Enqueue(nextPoint);
				nextPoint.z -= researchZoneSize.z;
				mainWaypoints.Enqueue(nextPoint);
				nextPoint.z -= 120;
				mainWaypoints.Enqueue(nextPoint);
			}else{
				nextPoint.z = researchZoneOrigin.z;
				mainWaypoints.Enqueue(nextPoint);
				nextPoint.z += researchZoneSize.z;
				mainWaypoints.Enqueue(nextPoint);
				nextPoint.z += 120;
				mainWaypoints.Enqueue(nextPoint);
			}
			nextPoint.x += bandWidth;
			reverse = !reverse;
		}
		Debug.Log("done generating waypoints. "+mainWaypoints.Count+" generated");
	}
	
	private void GenerateNextNavigationWaypoint(){
		if(mainWaypoints.Count<=0){
			if(isSweaping){	
				isSweaping = false;
				fileLog.Close();
				
				List<Vector3> allTarget = computeTargetsPositions();
				Debug.Log("estimated target at : "+allTarget[0]);
				
			}else{
				waypointIndicator.transform.position = Vector3.zero;
				ctrl.SetWaypoint(waypointIndicator);
			}
		}else{
			waypointIndicator.transform.position = mainWaypoints.Dequeue();
			ctrl.SetWaypoint(waypointIndicator);
		}
	}
	
	private List<Vector3> computeTargetsPositions(){
		DataPoint strongestPoint = allDataPoint.Pop();
		float cutThreshold = 0.9f * strongestPoint.GetPower();
		
		List<DataPoint> relevantPoint = new List<DataPoint>();
		relevantPoint.Add(strongestPoint);
		
		while(allDataPoint.Peek().GetPower()>cutThreshold){
			relevantPoint.Add(allDataPoint.Pop());
		}
		
		//we need some point cloud segmentation if we wants to be able to find 
		//multiple target in the same area, so for now we find only one target.
		Vector3 sumVector = Vector3.zero;
		for(int i=0;i<relevantPoint.Count;i++){
			sumVector = sumVector + relevantPoint[i].GetPosition();
		}
		sumVector = sumVector / relevantPoint.Count;
		
		List<Vector3> outputList = new List<Vector3>();
		outputList.Add(sumVector);
		return outputList;
	}
	
}
