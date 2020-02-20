using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DroneSimProperties : MonoBehaviour
{
    public abstract Vector3 DragCoefficients { get; set; }
    //the drag coefficient for each directions (sideway,upward,forward)
	public abstract Vector3[] InducedLift{ get; set; }
	//inducedLift[0][2] means the lift induced on the Z axis by the drag on the X axis (X drag is multiplied by the value to created the Z lift)

	public abstract Vector3 XDragCenterOffset { get; set; }
	public abstract Vector3 YDragCenterOffset { get; set; }
	public abstract Vector3 ZDragCenterOffset { get; set; }
	//Where the drag will be applied relative to the center of mass. 
	//Any lift is applied at the center of drag of the corresponding direction
	
	public abstract Vector3[] ThrusterOffset { get; set; }
	public abstract Vector3[] ThrusterThrustVectors { get; set; }
	public abstract float[] ThrusterThrustValues { get; set; }

    public DroneSimProperties()
    {
    }

    // Start is called before the first frame update
	public virtual void Start()
	{
	}

	// Update is called once per frame
	public virtual void Update()
	{
	}
}
