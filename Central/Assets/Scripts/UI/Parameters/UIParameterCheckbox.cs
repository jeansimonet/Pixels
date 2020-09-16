using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIParameterCheckbox
    : UIParameter
{
    [Header("Controls")]
    public Text nameText;
    public Toggle checkbox;
    public Text valueText;

    public override bool CanEdit(System.Type parameterType, IEnumerable<object> attributes = null)
    {
        return parameterType == typeof(bool);
    }

    protected override void SetupControls(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null)
    {
        bool initialValue = (bool)getterFunc();
        
        // Set name
        nameText.text = name;

        // Set initial value
        valueText.text = name;
        checkbox.isOn = initialValue;

        // Attach to events
        checkbox.onValueChanged.AddListener(newValue =>
        {
            setterAction((bool)newValue);
        });
    }
}
