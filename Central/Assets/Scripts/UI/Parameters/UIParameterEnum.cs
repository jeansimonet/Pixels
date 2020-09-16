using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SkipEnumAttribute
    : System.Attribute
{
    public int skipCount {get; private set;}
    public SkipEnumAttribute(int skipCount)
    {
        this.skipCount = skipCount;
    }
}

public class DropdowndAttribute
    : System.Attribute
{
}


public class UIParameterEnum : UIParameter
{
    [Header("Controls")]
    public Text nameText;
    public Text valueText;
    public Button valueButton;

    public static string GetNameAttribute(object enumVal, string fallback)
    {
        var type = enumVal.GetType();
        var memInfo = type.GetMember(enumVal.ToString());
        var attributes = memInfo[0].GetCustomAttributes(typeof(NameAttribute), false);
        return (attributes.Length > 0) ? ((NameAttribute)attributes[0]).name : fallback;
    }

    public override bool CanEdit(System.Type parameterType, IEnumerable<object> attributes = null)
    {
        return typeof(System.Enum).IsAssignableFrom(parameterType) && attributes.Any(a => a.GetType() == typeof(DropdowndAttribute));
    }

    protected override void SetupControls(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null)
    {
        // List all the enum values and populate the dropdown
        var initialValue = getterFunc();
        var enumType = initialValue.GetType();
        var vals = System.Enum.GetValues(enumType);

        int min = 0;
        int max = vals.Length - 1;
        var skip = (SkipEnumAttribute) System.Attribute.GetCustomAttribute(enumType, typeof (SkipEnumAttribute));
        if (skip != null)
        {
            min = skip.skipCount;
        }

        // Set name
        nameText.text = name;
        valueText.text = GetNameAttribute(initialValue, initialValue.ToString());
        valueButton.onClick.AddListener(() => 
        {
            PixelsApp.Instance.ShowEnumPicker("Select " + name, (System.Enum)getterFunc(), (ret, newVal) =>
            {
                if (ret)
                {
                    valueText.text = GetNameAttribute(newVal, newVal.ToString());
                    setterAction(newVal);
                }
            },
            min, max);
        });
    }
}
