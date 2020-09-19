using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SkipEnumValueAttribute
    : System.Attribute
{
}

public class AdvancedEnumValueAttribute
    : System.Attribute
{    
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

    public static bool ShouldSkipValue(object enumVal)
    {
        var type = enumVal.GetType();
        var memInfo = type.GetMember(enumVal.ToString());
        var skipAttribute = memInfo[0].GetCustomAttributes(typeof(SkipEnumValueAttribute), false);
        if (skipAttribute.Length > 0)
        {
            return true;
        }
        else
        {
            var advAttribute = memInfo[0].GetCustomAttributes(typeof(AdvancedEnumValueAttribute), false);
            if (advAttribute.Length > 0 && Application.platform != RuntimePlatform.WindowsEditor)
            {
                return true;
            }
        }
        return false;
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
        var validValues = new List<System.Enum>();
        foreach (var val in vals)
        {
            if (!ShouldSkipValue(val))
            {
                validValues.Add(val as System.Enum);
            }
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
            validValues);
        });
    }
}
