using UnityEngine;
using System.Collections;
using System;

public class WheelTimePicker : MonoBehaviour, ITimePicker
{
	
    public event Action ValueChange;
    /// <summary>
    /// Fires an event when the value of the timepicker is changed.
    /// </summary>
    private void OnValueChange()
    {
        if (!updatingPicker)
        {
            //Debug.Log("Time changed to " + _time);
            if (ValueChange != null)
            {        
                ValueChange();
            }
        }
    }

	#region ITimePicker implementation
    TimeSpan _time;
    /// <summary>
    /// Gets or sets the time. When used to set the time, it sets the corresponding values on the time wheels.
    /// </summary>
    public System.TimeSpan time
    {
        get
        {
            return _time;
        }
        set
        {
            updatingPicker = true;
        
            _time = value;
			
            hour.Select(_time.Hours);
            minute.Select(_time.Minutes);
            
            updatingPicker = false;
            
            UpdateTime();
        }
    }
	#endregion

    /// <summary>
    /// The hour wheel.
    /// </summary>
    public InfiniWheel hour;
    /// <summary>
    /// The minute wheel
    /// </summary>
    public InfiniWheel minute;

    bool updatingPicker = false;
	
    private void Awake()
    {
        hour.ValueChange += WheelChanged;
        minute.ValueChange += WheelChanged;
        
        string[] hours = new string[24];
        for (int i = 0; i < 24; i++)
        {
            hours [i] = i.ToString();
        }
		
        string[] minutes = new string[60];
        for (int i = 0; i < 60; i++)
        {
            if (i < 10) 
                minutes [i] = "0" + i.ToString();
            else 
                minutes [i] = i.ToString();
        }
		
        hour.Init(hours);
        minute.Init(minutes);

        UpdateTime();      
    }

    /// <summary>
    /// Calls OnValueChanged when one of the wheels' value changes.
    /// </summary>
    /// <param name="arg1">Id of the new value</param>
    /// <param name="arg2">Text object of the new item</param>
    private void WheelChanged(int arg1, UnityEngine.UI.Text arg2)
    {
        if (!updatingPicker)
            UpdateTime();        
    }

    /// <summary>
    /// Updates the time.
    /// </summary>
    private void UpdateTime()
    {
        int h = hour.SelectedItem;
        int m = minute.SelectedItem;
		
        _time = new TimeSpan(h, m, 0);
        
        OnValueChange();
    }
}
