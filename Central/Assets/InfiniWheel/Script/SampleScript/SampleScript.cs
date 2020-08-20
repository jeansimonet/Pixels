using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class SampleScript : MonoBehaviour
{
    public WheelDatePicker datePicker;
    public WheelTimePicker timePicker;

	public WheelDatePicker datePicker2;
	public WheelTimePicker timePicker2;

	public InfiniWheel otherWheel;

    public Text dateLabel;
    public Text timeLabel;

    void Start()
    {
		otherWheel.Init(new string[] { "0", "1", "2", "3", "4", "5" });

        datePicker.ValueChange += () => {
            dateLabel.text = datePicker.date.ToLongDateString();
			if(datePicker2.date != datePicker.date)
				datePicker2.date = datePicker.date;
        };
        
        timePicker.ValueChange += () => {
            timeLabel.text = timePicker.time.ToString();
			if(timePicker2.time != timePicker.time)
				timePicker2.time = timePicker.time;
        };

		datePicker2.ValueChange += () =>
		{
			dateLabel.text = datePicker2.date.ToLongDateString();
			if(datePicker2.date != datePicker.date)
				datePicker.date = datePicker2.date;
		};

		timePicker2.ValueChange += () =>
		{
			timeLabel.text = timePicker2.time.ToString();
			if(timePicker2.time != timePicker.time)
				timePicker.time = timePicker2.time;
		};

        datePicker.date = DateTime.Now;
        timePicker.time = DateTime.Now.TimeOfDay;        
    }
}