using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class IntRangeAttribute
    : System.Attribute
{
    public int min { get; private set; }
    public int max { get; private set; }
    public int range
    {
        get
        {
            return max - min + 1;
        }
    }
    public IntRangeAttribute(int min, int max)
    {
        this.min = min;
        this.max = max;
    }
}

public class IndexAttribute
    : System.Attribute
{
}

public class FaceIndexAttribute
    : System.Attribute
{
}

public class UIParameterIndex
    : UIParameter
{
    [Header("Controls")]
    public Text nameText;
    public InfiniWheel wheel;

    public override bool CanEdit(System.Type parameterType, IEnumerable<object> attributes = null)
    {
        return parameterType == typeof(int) && attributes.Any(a => a.GetType() == typeof(IndexAttribute) || a.GetType() == typeof(FaceIndexAttribute));
    }

    protected override void SetupControls(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null)
    {
        int initialValue = (int)getterFunc();
        
        // Look for decorating attributes
        int min = 0;
        int max = 9;
        var rangeAttribute = attributes.FirstOrDefault(att => att.GetType() == typeof (IntRangeAttribute)) as IntRangeAttribute;
        if (rangeAttribute != null)
        {
            min = rangeAttribute.min;
            max = rangeAttribute.max;
        }
        string[] values = new string[max - min + 1];
        int offset = 0;
        if (attributes.FirstOrDefault(att => att.GetType() == typeof(FaceIndexAttribute)) != null)
        {
            offset = 1;
        }
        for (int i = min; i <= max; ++i)
        {
            values[i - min] = (i + offset).ToString();
        }
        wheel.Init(values);
        wheel.Select(initialValue - min);
        wheel.ValueChange += (index, text) =>
        {
            setterAction(index + min);
        };

        // Set name
        nameText.text = name;
    }
}
