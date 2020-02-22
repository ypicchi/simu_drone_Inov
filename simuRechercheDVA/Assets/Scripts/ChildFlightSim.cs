using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildFlightSim : DroneFlightSim
{
    //TODO child sim

    protected float distToGround = 0.1f;
    protected bool IsGrounded(){
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
    }
}
