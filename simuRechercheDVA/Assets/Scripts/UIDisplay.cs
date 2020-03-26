using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIDisplay : MonoBehaviour
{

    protected Sensor sensor;
    protected DroneControl control;
    protected GameObject canvas;



    
    private Text sensorPowerDisplay;
    private Text modeDisplay;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        canvas = GameObject.Find("Canvas");

        sensor = GetComponent<Sensor>();
        sensorPowerDisplay = CreateText(canvas.transform, -114, 193, "Signal power : ").GetComponent<Text>();
        
        control = GetComponent<DroneControl>();
        modeDisplay = CreateText(canvas.transform, -114, 173, "mode : ").GetComponent<Text>();

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        float power = sensor.GetSignalPower(transform.position);
		sensorPowerDisplay.text = "Signal power : "+power.ToString();
        modeDisplay.text = control.modeDisplayText;

    }

    protected GameObject CreateText(Transform canvas_transform, float x, float y, string text_to_print)
    {
        GameObject UItextGO = new GameObject("Text2");
        UItextGO.layer = 5;
        UItextGO.transform.SetParent(canvas_transform);

        RectTransform trans = UItextGO.AddComponent<RectTransform>();
        trans.anchoredPosition = new Vector2(x, y);
        trans.sizeDelta = new Vector2 (160, 30);

        Text text = UItextGO.AddComponent<Text>();
        text.text = text_to_print;
        text.color = Color.black;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        return UItextGO;
    }
}
