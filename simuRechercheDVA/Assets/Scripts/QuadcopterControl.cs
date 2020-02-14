using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadcopterControl : DroneControl
{

    private DroneFlightSim sim;

    // Start is called before the first frame update
	public override void Start()
	{
		base.Start();
		sim = GetComponent<DroneFlightSim>();
	}

    

    public override void ControlLoop(){
		
	}

    //SetThrusterThrust(int thrusterIndex,float thrustValue)
	

    protected override void HandleKeyboardInput(){
		if (Input.GetKey(KeyCode.LeftShift)){
			//sim.SetMainThrust(sim.GetMainThrust()+0.1f);
        }else if (Input.GetKey(KeyCode.LeftControl)){
			//sim.SetMainThrust(sim.GetMainThrust()-0.1f);
        }
		
		//pitch
        if (Input.GetKey(KeyCode.Z)){
            //sim.SetPitchTorque(15);
        }else if (Input.GetKey(KeyCode.S)){
            //sim.SetPitchTorque(-22);
        }else{
			//sim.SetPitchTorque(0);
		}
		
		//roll
		if (Input.GetKey(KeyCode.Q)){
            //sim.SetRollTorque(15);
        }else if (Input.GetKey(KeyCode.D)){
            //sim.SetRollTorque(-15);
        }else{
			//sim.SetRollTorque(0);
		}
		
		//yaw
		if (Input.GetKey(KeyCode.A)){
            //sim.SetYawTorque(15);
        }else if (Input.GetKey(KeyCode.E)) {
            //sim.SetYawTorque(-15);
        }else{
			//sim.SetYawTorque(0);
		}
	}
}
