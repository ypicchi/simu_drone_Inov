#define TEST

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;



namespace Tests
{
    public class TestControl
    {

        
        
        /*
        // A Test behaves as an ordinary method
        [Test]
        public void TestSuiteSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestSuiteWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            //Assert.Less(asteroid.transform.position.y, initialYPos);
            yield return null;
        }
        */


        [UnityTest]
        public IEnumerator TestStaticError()
        {
            int nbSample = 200;
            float proportionOfSamplesUsedForTheAverage = 0.1f;
            Vector3 startingPosition = Vector3.zero;
            Vector3 targetPosition = new Vector3(5,5,5);
            float staticErrorFailThreshold = 0.5f;
            

            
            GameObject targetDrone = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Dummy"));
            Debug.Log("target drone : "+targetDrone.name);

            Navigation nav = targetDrone.GetComponent<Navigation>();
            DroneControl ctrl = targetDrone.GetComponent<DroneControl>();
            List<Vector3> allPositions = new List<Vector3>();

            targetDrone.transform.position = startingPosition;
            nav.waypointValidationDistance = 0f; //never validate the waypoint

            yield return null;//Wait one frame so we can overwrite the waypoints

            //nav.ClearWaypoint();//in case the waypoint generation in navigation.Starts() runs before this function
            //nav.AddWaypoint(targetPosition);

            GameObject waypointIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		    waypointIndicator.transform.position = targetPosition;
            ctrl.SetWaypoint(waypointIndicator);
            Debug.Log("single waypoint set");


            //now we collect the position for nbSample frames
            for(int i =0; i<nbSample; i++){
                allPositions.Add(targetDrone.transform.position);
                //Debug.Log("position : "+targetDrone.transform.position);
                yield return null;
            }


            //now we have all the positions, we can estimate the error
            int nbSampleAveraged = (int)(((float)nbSample)*proportionOfSamplesUsedForTheAverage);
            Vector3 accumulator = Vector3.zero;
            for(int i = 0;i<nbSampleAveraged;i++){
                accumulator = accumulator + allPositions[nbSample - 1 - i];
            }
            accumulator = accumulator / nbSampleAveraged;

            Vector3 positionError = targetPosition - accumulator;
            Debug.Log("static positioning error : "+positionError+"; after "+nbSample+" frames");

            Assert.Less(positionError.magnitude, staticErrorFailThreshold);

            Object.Destroy(targetDrone);

        }

        [UnityTest]
        public IEnumerator TestOscillation()
        {
            int nbSample = 200;
            Vector3 startingPosition = Vector3.zero;
            Vector3 targetPosition = new Vector3(5,5,5);
            

            
            GameObject targetDrone = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Dummy"));
            Debug.Log("target drone : "+targetDrone.name);

            Navigation nav = targetDrone.GetComponent<Navigation>();
            DroneControl ctrl = targetDrone.GetComponent<DroneControl>();
            List<Vector3> allPositions = new List<Vector3>();

            targetDrone.transform.position = startingPosition;
            nav.waypointValidationDistance = 0f; //never validate the waypoint

            yield return null;//Wait one frame so we can overwrite the waypoints

            //nav.ClearWaypoint();//in case the waypoint generation in navigation.Starts() runs before this function
            //nav.AddWaypoint(targetPosition);

            GameObject waypointIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		    waypointIndicator.transform.position = targetPosition;
            ctrl.SetWaypoint(waypointIndicator);
            Debug.Log("single waypoint set");


            //now we collect the position for nbSample frames
            for(int i =0; i<nbSample; i++){
                allPositions.Add(targetDrone.transform.position);
                //Debug.Log("position : "+targetDrone.transform.position);
                yield return null;
            }


            //now we have all the positions, we can estimate the error
            Assert.IsTrue(false);




            Object.Destroy(targetDrone);
            yield break;
        }

       
    }
}
