using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelJointSpawner : MonoBehaviour
{

    private bool areJointCreated = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateJoints(){
        if(!areJointCreated){
            for(int i=0;i<4;i++){
                GameObject wheel = GameObject.Find("Wheel"+i.ToString());

                Rigidbody wrb = wheel.AddComponent<Rigidbody>();
                wrb.mass = 0.05f;

                HingeJoint attachJoint = wheel.AddComponent<HingeJoint>();
                attachJoint.connectedBody = this.gameObject.GetComponent<Rigidbody>();
                attachJoint.axis = Vector3.up;
                attachJoint.massScale = 0.01f;
            }
            areJointCreated = true;
        }
    }

    
}
