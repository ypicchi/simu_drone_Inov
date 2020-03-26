using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIDisplayfixedWing : UIDisplay
{



    private Text rollDisplay;
	private Text pitchDisplay;
	private Text climbAngleDisplay;
	private Text realClimbAngleDisplay;
	private Text forwardSpeedDisplay;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        rollDisplay = CreateText(canvas.transform, -114, -183, "roll : 0").GetComponent<Text>();
        pitchDisplay = CreateText(canvas.transform, -114, -163, "pitch : 0").GetComponent<Text>();
        climbAngleDisplay = CreateText(canvas.transform, -114, -143, "climb angle : 0").GetComponent<Text>();
        realClimbAngleDisplay = CreateText(canvas.transform, -114, -123, "real climb angle : 0").GetComponent<Text>();
        forwardSpeedDisplay = CreateText(canvas.transform, -114, -103, "forward speed : 0").GetComponent<Text>();

    }

    

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        rollDisplay.text = sensor.rollDisplayText;
        pitchDisplay.text = sensor.pitchDisplayText;
        climbAngleDisplay.text = sensor.climbAngleDisplayText;
        realClimbAngleDisplay.text = sensor.realClimbAngleDisplayText;
        forwardSpeedDisplay.text = sensor.forwardSpeedDisplayText;


        
    }
}
