using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class FloatRangeAttribute
    : System.Attribute
{
    public float min { get; private set; }
    public float max { get; private set; }
    public float step { get; private set; }
    public float range
    {
        get
        {
            return max - min;
        }
    }
    public FloatRangeAttribute(float min, float max, float step = 0.0f)
    {
        this.min = min;
        this.max = max;
        this.step = step;
    }
}

public class UnitsAttribute
    : System.Attribute
{
    public string unit { get; private set; }
    public UnitsAttribute(string unit)
    {
        this.unit = unit;
    }
}

public class UIParameterSlider
    : UIParameter
{
    [Header("Controls")]
    public Text nameText;
    public Slider valueSlider;
    public Text valueText;

    public override System.Type parameterType { get { return typeof(float); } }

    protected override void SetupControls(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null)
    {
        float initialValue = (float)getterFunc();
        
        // Look for decorating attributes
        float min = 0.0f;
        float max = 1.0f;
        var rangeAttribute = attributes.FirstOrDefault(att => att.GetType() == typeof (FloatRangeAttribute)) as FloatRangeAttribute;
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
        setTextFromSlider((float)initialValue);

        void setTextFromSlider(float sliderValue)
        {
            valueText.text = ((float)sliderValue).ToString("F1") + (string.IsNullOrEmpty(unitString) ? "" : (" " + unitString));
        }

        // Attach to events
        valueSlider.onValueChanged.AddListener(newValue =>
        {
            setTextFromSlider(newValue);
            setterAction(newValue);
        });
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
