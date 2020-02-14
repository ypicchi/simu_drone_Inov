using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadcopterFlightSim : DroneFlightSim
{

    public int propellerAnimationSpeedMultiplier = 200;

    // Update is called once per frame
    public virtual void Update()
    {
		for(int i=0;i<4;i++){
            float speed = simProperties.ThrusterThrustValues[i] * propellerAnimationSpeedMultiplier;
            float delta = Time.deltaTime * speed;
            GameObject propeller = GameObject.Find("MotorModule"+i.ToString()+"/Propeller");
		    string assignedMaterialName = propeller.GetComponent<Renderer>().material.name;

            if (assignedMaterialName.Contains("CounterClockwise") ) {
                delta *= -1;
            }
            
            propeller.transform.localEulerAngles += new Vector3(0, delta, 0);
        }
		
    }


    

}
