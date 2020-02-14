using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LookAtCamera : MonoBehaviour {
    public GameObject target;
    public float distance = 4;
    Vector3 offset;
     
    void Start() {
        offset = target.transform.position - transform.position;
    }
     
    void LateUpdate() {
        //float currentAngle = transform.eulerAngles.y;
        //float desiredAngle = target.transform.eulerAngles.y;
        //float angle = Mathf.LerpAngle(currentAngle, desiredAngle, Time.deltaTime * damping);
         
        //Quaternion rotation = Quaternion.Euler(0, desiredAngle, 0);
		
		transform.position = target.transform.position - (distance * (target.transform.forward - target.transform.up /4 ));
		
        //transform.position = target.transform.position - (rotation * offset);
         
        //transform.LookAt(target.transform);
		//need to set the forward axis's rotation to get the roll
		//transform.rotation = target.transform.rotation;

        transform.LookAt(target.transform);
		
		
		
		
    }
}