using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using static DroneFlightSim;



//high level control
public abstract class DroneControl : MonoBehaviour
{

	protected FixedWingFlightSim sim;
	protected Sensor sensor;
	protected Text modeDisplay;
	
	
	
	protected GameObject target;
	protected bool hasTarget = false;
	
	
	// Start is called before the first frame update
	public virtual void Start()
	{
		sensor = GetComponent<Sensor>();
		modeDisplay = GameObject.Find("Canvas/modeDisplay").GetComponent<Text>();
		
	}

	// Update is called once per frame
	public virtual void Update()
	{
		HandleKeyboardInput();//keyboard override
		modeDisplay.text = ("mode : ");
		
	}
	
	
	
	
	public void SetWaypoint(GameObject waypointIndicator){
		target = waypointIndicator;
		hasTarget = true;
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	//utility function. Map the value in old range to the value in new range (effectively scale and shift the value)
	protected static float Map(float newFrom, float newTo, float oldFrom, float oldTo, float value){
        if(value <= oldFrom){
            return newFrom;
        }else if(value >= oldTo){
            return newTo;
        }else{
            return (newTo - newFrom) * ((value - oldFrom) / (oldTo - oldFrom)) + newFrom;
        }
    }
	
	protected abstract void HandleKeyboardInput();
}
