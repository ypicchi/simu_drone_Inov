using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadcopterFlightSim : DroneFlightSim
{

    public int propellerAnimationSpeedMultiplier = 200;
    public float propellerYawTorqueMultiplier = 0.05f;

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

		for (int i=0;i<simProperties.ThrusterOffset.Length;i++){
            float speed = simProperties.ThrusterThrustValues[i] * propellerAnimationSpeedMultiplier;
            float delta = Time.deltaTime * speed;
            GameObject propeller = GameObject.Find("MotorModule"+i.ToString()+"/Propeller");
		    string assignedMaterialName = propeller.GetComponent<Renderer>().material.name;

            if (assignedMaterialName.Contains("CounterClockwise") ) {
                delta *= -1;
            }
            
            propeller.transform.localEulerAngles += new Vector3(0, delta, 0);
        }

        float totalYawTorque = 0;
        for (int i=0;i<simProperties.ThrusterOffset.Length;i++){
            float currentBladeTorque = - simProperties.ThrusterThrustValues[i] * propellerYawTorqueMultiplier;
            
            GameObject propeller = GameObject.Find("MotorModule"+i.ToString()+"/Propeller");
		    string assignedMaterialName = propeller.GetComponent<Renderer>().material.name;

            if (assignedMaterialName.Contains("CounterClockwise") ) {
                currentBladeTorque *= -1;
            }

            totalYawTorque += currentBladeTorque;
        }
        SetYawTorque(totalYawTorque);
		
    }



}
