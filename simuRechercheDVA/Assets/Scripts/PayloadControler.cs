using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//handle the child drones : their impact on the mother drone and how to drop them
public class PayloadControler : MonoBehaviour
{

    protected List<GameObject> AllConnectedChild = new List<GameObject>();
    protected float mass = 0f;
    protected Vector3 CG = new Vector3(0,0,0);

    
    void Start()
    {
        GameObject[] allChildsGameobject = GameObject.FindGameObjectsWithTag("Child");
        foreach (GameObject obj in allChildsGameobject){
            FixedJoint joint = obj.GetComponent<FixedJoint>();
            if(joint != null && joint.connectedBody == this.gameObject.GetComponent<Rigidbody>()){
                //this child is connected to this drone
                AllConnectedChild.Add(obj);
            }
        }
        ComputeCGandMass();

    }

    public void ComputeCGandMass(){//call this whevener there is a change in the payload
        mass = 0f;
        CG = Vector3.zero;
        foreach (GameObject obj in AllConnectedChild){
            float thisMass = obj.GetComponent<Rigidbody>().mass;
            Vector3 thisCG = obj.GetComponent<Rigidbody>().worldCenterOfMass;


            mass += thisMass;
            CG += thisCG*thisMass;
        }
        //divide the vector so we take the weighted average CG
        CG /= mass;

        //and place this CG in the local referential
        CG = transform.InverseTransformPoint(CG);
    }

    public float GetPayloadMass(){
        return mass;
    }

    public Vector3 GetPayloadCG(){
        return CG;
    }

    public void ReleaseAChild(){
        if(GetRemainingChildAmount() > 0){
            AllConnectedChild[0].GetComponent<Navigation>().StartMission();
            AllConnectedChild.RemoveAt(0);
            ComputeCGandMass();
        }
    }

    public int GetRemainingChildAmount(){
        return AllConnectedChild.Count;
    }

}
