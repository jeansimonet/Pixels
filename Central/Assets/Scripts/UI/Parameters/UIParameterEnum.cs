using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class EnumRangeAttribute
    : System.Attribute
{
    public int first {get; private set; }
    public int last {get; private set; }
    public int count
    {
        get
        {
            return last - first + 1;
        }
    }
    public EnumRangeAttribute(int first, int last)
    {
        this.first = first;
        this.last = last;
    }
}

public class UIParameterEnum : UIParameter
{
    [Header("Controls")]
    public Text nameText;
    public Dropdown dropdownControl;

    public override System.Type parameterType { get { return typeof(System.Enum); } }

    protected override void SetupControls(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null)
    {
        // List all the enum values and populate the dropdown
        var initialValue = getterFunc();
        var enumType = initialValue.GetType();

        List<string> enumValueNames;
        System.Array enumValues;

        var rangeAttribute = (EnumRangeAttribute) System.Attribute.GetCustomAttribute(enumType, typeof (EnumRangeAttribute));
        if (rangeAttribute != null)
        {
            enumValues = System.Array.CreateInstance(typeof(object), rangeAttribute.count);
            System.Array.Copy(System.Enum.GetValues(enumType), rangeAttribute.first, enumValues, 0, rangeAttribute.count);
            enumValueNames = System.Enum.GetNames(enumType).ToList().GetRange(rangeAttribute.first, rangeAttribute.count);
        }
        else
        {
            enumValues = System.Enum.GetValues(enumType);
            enumValueNames = new List<string>(System.Enum.GetNames(enumType));
        }

        // Set name
        nameText.text = name;

        dropdownControl.ClearOptions();
        dropdownControl.AddOptions(enumValueNames);

        // Set initial value
        dropdownControl.value = System.Array.IndexOf(enumValues, initialValue);

        // Attach to events
        dropdownControl.onValueChanged.AddListener(newIndex => setterAction(enumValues.GetValue(newIndex)));
    }
}
