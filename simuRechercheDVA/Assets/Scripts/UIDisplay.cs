using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIDisplay : MonoBehaviour
{

    protected Sensor sensor;
    protected DroneControl control;



    private Text rollDisplay;
	private Text pitchDisplay;
	private Text climbAngleDisplay;
	private Text realClimbAngleDisplay;
	private Text forwardSpeedDisplay;
    private Text sensorPowerDisplay;
    private Text modeDisplay;

    // Start is called before the first frame update
    void Start()
    {
        sensor = GetComponent<Sensor>();
        rollDisplay = GameObject.Find("Canvas/rollDisplay").GetComponent<Text>();
		pitchDisplay = GameObject.Find("Canvas/pitchDisplay").GetComponent<Text>();
		climbAngleDisplay = GameObject.Find("Canvas/climbAngleDisplay").GetComponent<Text>();
		realClimbAngleDisplay = GameObject.Find("Canvas/realClimbAngleDisplay").GetComponent<Text>();
		forwardSpeedDisplay = GameObject.Find("Canvas/forwardSpeedDisplay").GetComponent<Text>();
        sensorPowerDisplay = GameObject.Find("Canvas/signalPowerDisplay").GetComponent<Text>();
        
        control = GetComponent<DroneControl>();
        modeDisplay = GameObject.Find("Canvas/modeDisplay").GetComponent<Text>();

    }

    // Update is called once per frame
    void Update()
    {
        float power = sensor.GetSignalPower(transform.position);
		sensorPowerDisplay.text = "Signal power : "+power.ToString();
        rollDisplay.text = sensor.rollDisplayText;
        pitchDisplay.text = sensor.pitchDisplayText;
        climbAngleDisplay.text = sensor.climbAngleDisplayText;
        realClimbAngleDisplay.text = sensor.realClimbAngleDisplayText;
        forwardSpeedDisplay.text = sensor.forwardSpeedDisplayText;

        modeDisplay.text = control.modeDisplayText;

        
    }
}
