using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataPoint {
    private Vector3 position;
	private Vector3 orientation;
    private float sensorPower;

    public Vector3 Position { get => position;}
    public Vector3 Orientation { get => orientation;}

    public float SensorPower { get => sensorPower;}


    public DataPoint(float sensorPower,Vector3 position,Vector3 orientation){
		this.position = position;
		this.sensorPower = sensorPower;
		this.orientation = orientation;
	}
	

	
	public string ToWSVLine(){
		return "" + Position.x + " " + Position.y + " "+Position.z + " " + Orientation.x + " " + Orientation.y + " " + Position.z + " " + SensorPower + "\n";
	}
}