using UnityEngine;
using System;
using System.Collections;
using System.Globalization;

public class WheelDatePicker : MonoBehaviour, IDatePicker
{	

    public event Action ValueChange;
    /// <summary>
    /// Fires an event when the value of the datepicker is changed.
    /// </summary>
    private void OnValueChange()
    {
        if (!updatingPicker)
        {
            //Debug.Log("Date changed to " + _date);
            if (ValueChange != null)
            {        
                ValueChange();
            }
        }
    }        
	
	#region IDatePicker implementation
    private DateTime _date;
    /// <summary>
    /// Can be used to set or get the date. When used to set the date, it selects the corresponding values on the date wheels.
    /// </summary>
    public System.DateTime date
    {
        get
        {
            return _date;
        }
        set
        {
            updatingPicker = true;
        
            _date = value;

            if (year != null && month != null && day != null)
            {
                year.Select(value.Year - minYear);
                month.Select(value.Month - 1);
                day.Select(value.Day - 1);
            }
            
            updatingPicker = false;
            
            UpdateDate();
        }
    }
	#endregion

    /// <summary>
    /// The minimum year. The lowest value on the year wheel.
    /// </summary>
    public int minYear;
    /// <summary>
    /// The maximum year. The highest value on the year wheel.
    /// </summary>
    public int maxYear;	

    /// <summary>
    /// The day wheel.
    /// </summary>
    public InfiniWheel day;
    /// <summary>
    /// The month wheel.
    /// </summary>
    public InfiniWheel month;
    /// <summary>
    /// The year wheel.
    /// </summary>
    public InfiniWheel year;
	
    private bool updatingPicker = false;
            	
    private void Awake()
    {		
        year.ValueChange += OnYearChange;   
        month.ValueChange += OnMonthChange;
        day.ValueChange += OnDayChange;
        
        string[] days = new string[31];
        for (int i = 0; i < 31; i++)
        {
            days [i] = (i + 1).ToString();
        }
        
        string[] months = new string[12];
        for (int i = 0; i < 12; i++)
            months [i] = DateTimeFormatInfo.CurrentInfo.GetMonthName(i + 1);
        
        int deltaYear = maxYear - minYear;
        string[] years = new string[deltaYear]; 
        for (int i = 0; i < deltaYear; i++)
        {
            years [i] = (minYear + i).ToString();
        }
        
        year.Init(years);
        month.Init(months);
        day.Init(days);		
        
        UpdateDate();
    }

    /// <summary>
    /// Triggered when the value of the year wheel changes.
    /// </summary>
    /// <param name="id">item index of the new value.</param>
    /// <param name="obj">Text object of the new item.</param>
    private void OnYearChange(int id, UnityEngine.UI.Text obj)
    {
        int m = month.SelectedItem + 1;
        
        // Update the amount of days if it's february
        if (m == 2)
        {
            // Set right amount of days
            OnMonthChange(id, obj);
        } else
        {
            UpdateDate();
        }                
    }

    /// <summary>
    /// Triggered when the value of the month wheel changes.
    /// </summary>
    /// <param name="id">item index of the new value.</param>
    /// <param name="obj">Text object of the new item.</param>>
    private void OnMonthChange(int id, UnityEngine.UI.Text obj)
    {
        int m = month.SelectedItem + 1;
        int y = year.SelectedItem + minYear;
        int days = DateTime.DaysInMonth(y, m);
        string[] dayStrings = new string[days];
        for (int i = 0; i < days; i++)
        {
            dayStrings [i] = (i + 1).ToString();
        }
        
        // Set right amount of days
        day.SetData(dayStrings);
        
        UpdateDate();
    }
    
    void OnDayChange(int id, UnityEngine.UI.Text obj)
    {
        UpdateDate();
    }
    
    /// <summary>
    /// Updates the Date.
    /// </summary>
    private void UpdateDate()
    {
        int d = day.SelectedItem + 1;
        int m = month.SelectedItem + 1;
        int y = year.SelectedItem + minYear;
        
        _date = new DateTime(y, m, d);
        
        OnValueChange();
    }
}
