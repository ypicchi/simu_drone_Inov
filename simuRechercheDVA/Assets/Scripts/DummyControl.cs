using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyControl : DroneControl
{


    //private Rigidbody rb;
    public float speed = 10;
    

    public override void Start(){
        //rb = GetComponent<Rigidbody>();
        base.Start();
    }

    public override void ControlLoop(){
        modeDisplayText = ("mode : auto");

        //rb.velocity = Vector3.Normalize(target.transform.position - transform.position) * speed;
        
        float stepDistance = speed*Time.deltaTime;
        Vector3 nextPosition = Vector3.MoveTowards(transform.position, 
                    target.transform.position, stepDistance);
        transform.position = nextPosition;
        
    }

    protected override void HandleKeyboardInput(){
        //do nothing, we can't control it manually
    }
}
