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

        public GameObject targetDrone;
        
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
            

            
            GameObject targetDrone = GameObject.Instantiate(Resources.Load<GameObject>("Dummy"));
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
        public IEnumerator TestOscillation()//TODO I don't have much faith in this test....
        {
            int nbSample = 200;
            float proportionOfSamplesUsedForTheAverage = 0.1f;
            Vector3 startingPosition = Vector3.zero;
            Vector3 targetPosition = new Vector3(5,5,5);
            float proportionOfSamplesUsedForTheOscillation = 0.3f;
            float attenuationThreshold = 0.8f;
            float acceptableContinuousOscillationAmplitude = 0.2f;
            

            
            GameObject targetDrone = GameObject.Instantiate(Resources.Load<GameObject>("Dummy"));
            Debug.Log("target drone : "+targetDrone.name);

            Navigation nav = targetDrone.GetComponent<Navigation>();
            DroneControl ctrl = targetDrone.GetComponent<DroneControl>();
            List<Vector3> allPositions = new List<Vector3>();

            targetDrone.transform.position = startingPosition;
            nav.waypointValidationDistance = 0f; //never validate the waypoint

            yield return null;//Wait one frame so we can overwrite the waypoints


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
            

            for(int axis = 0; axis<3;axis++){
                List<float> maxList;
                List<float> minList;

                getNextMax(allPositions,axis,out minList,out maxList);
                float endValue = accumulator[axis];

                //Test if max is attenuated enough
                int nbSampleOscillation = (int)(((float)maxList.Count)*proportionOfSamplesUsedForTheOscillation);
                int index = 0;
                float previousOvershot = 0f;
                foreach(float currentMax in maxList){
                    float currentOvershot = currentMax - endValue;
                    if(index == 0){
                        previousOvershot = currentOvershot;
                        continue;
                    }
                    if(index>=nbSampleOscillation){
                        break;
                    }

                    bool isMaxOk = currentOvershot < acceptableContinuousOscillationAmplitude || currentOvershot < attenuationThreshold*previousOvershot;
                    Assert.IsTrue(isMaxOk);

                    previousOvershot = currentOvershot;
                }

                //Test if min is attenuated enough
                nbSampleOscillation = (int)(((float)minList.Count)*proportionOfSamplesUsedForTheOscillation);
                index = 0;
                foreach(float currentMin in maxList){
                    float currentOvershot = endValue - currentMin;
                    if(index == 0){
                        previousOvershot = currentOvershot;
                        continue;
                    }
                    if(index>=nbSampleOscillation){
                        break;
                    }

                    bool isMinOk = currentOvershot < acceptableContinuousOscillationAmplitude || currentOvershot < attenuationThreshold*previousOvershot;
                    Assert.IsTrue(isMinOk);

                    previousOvershot = currentOvershot;
                }
            }

            Object.Destroy(targetDrone);
        }


        private bool isLocalMin (float previous, float current, float next){
            return (previous > current && current < next);
        }

        private bool isLocalMax (float previous, float current, float next){
            return (previous < current && current > next);
        }

        private void getNextMax(List<Vector3> list,int axis,out List<float> minList,out List<float> maxList){
            maxList = new List<float>();
            minList = new List<float>();

            int index = 0;
            float previousPrevious = 0;
            float previous = 0;
            float current = 0;
            foreach(Vector3 element in list){
                current = element[axis];
                if(index == 0){
                    previousPrevious = current;
                }else if(index ==1){
                    previous = current;
                }else{
                    if(isLocalMin(previousPrevious,previous,current)){
                        minList.Add(previous);
                    }else if(isLocalMax(previousPrevious,previous,current)){
                        maxList.Add(previous);
                    }

                    previousPrevious = previous;
                    previous = current;
                }
                index++;
            }
        }
       
    }
}
