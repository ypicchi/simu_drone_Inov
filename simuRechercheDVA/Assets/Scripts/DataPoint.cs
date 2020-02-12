using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataPoint {
	private Vector3 m_position;
	private float m_sensorPower;
	
	
	public DataPoint(Vector3 position,float sensorPower){
		m_position = position;
		m_sensorPower = sensorPower;
	}
	
	public float GetPower(){
		return m_sensorPower;
	}
	
	public Vector3 GetPosition(){
		return m_position;
	}
	
	public string ToWSVLine(){
		return ""+m_position.x+" "+m_position.z+" "+m_sensorPower+"\n";
	}
}