using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class IntSliderAttribute
    : System.Attribute
{
}

public class UIParameterIntSlider
    : UIParameter
{
    [Header("Controls")]
    public Text nameText;
    public Slider valueSlider;
    public Text valueText;

    public override bool CanEdit(System.Type parameterType, IEnumerable<object> attributes = null)
    {
        return parameterType == typeof(int) && attributes.Any(a => a.GetType() == typeof(IntSliderAttribute));
    }

    protected override void SetupControls(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null)
    {
        int initialValue = (int)getterFunc();
        
        // Look for decorating attributes
        int min = 0;
        int max = 10;
        var rangeAttribute = attributes.FirstOrDefault(att => att.GetType() == typeof (IntRangeAttribute)) as IntRangeAttribute;
        if (rangeAttribute != null)
        {
            min = rangeAttribute.min;
            max = rangeAttribute.max;
        }
        valueSlider.minValue = min;
        valueSlider.maxValue = max;

        string unitString = "";
        var unitAttribute = attributes.FirstOrDefault(att => att.GetType() == typeof(UnitsAttribute)) as UnitsAttribute;
        if (unitAttribute != null)
        {
            unitString = unitAttribute.unit;
        }

        // Set name
        nameText.text = name;

        // Set initial value
        valueSlider.value = (float)initialValue;

        void setTextFromSlider(int sliderValue)
        {
            valueText.text = sliderValue.ToString() + (string.IsNullOrEmpty(unitString) ? "" : (" " + unitString));
        }

        setTextFromSlider(initialValue);

        // Attach to events
        valueSlider.onValueChanged.AddListener(newValue =>
        {
            setTextFromSlider((int)newValue);
            setterAction((int)newValue);
        });
    }
}
