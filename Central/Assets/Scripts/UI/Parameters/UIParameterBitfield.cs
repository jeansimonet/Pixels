using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class BitfieldAttribute
    : System.Attribute
{
}

public class UIParameterBitfield : UIParameter
{
    [Header("Controls")]
    public Text nameText;
    public RectTransform buttonRoot;

    [Header("Parameters")]
    public UIParameterBitfieldBit bitPrefab;
    public Sprite[] backgrounds;
    public Color bitColor;
    public Color bitColorSelected;
    // public Color textColorSelected;
    // public Color textColor;

    List<UIParameterBitfieldBit> bits = new List<UIParameterBitfieldBit>();

    public override bool CanEdit(System.Type parameterType, IEnumerable<object> attributes = null)
    {
        return typeof(System.Enum).IsAssignableFrom(parameterType) && attributes.Any(a => a.GetType() == typeof(BitfieldAttribute));
    }

    protected override void SetupControls(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null)
    {
        // Set name
        nameText.text = name;

        // List all the enum values and populate the dropdown
        var initialValue = getterFunc();
        int initialValueInt = System.Convert.ToInt32(initialValue);
        var enumType = initialValue.GetType();

        var vals = System.Enum.GetValues(enumType);
        var validValues = new List<System.Enum>();
        foreach (var val in vals)
        {
            if (!UIParameterEnum.ShouldSkipValue(val))
            {
                validValues.Add(val as System.Enum);
            }
        }

        for (int i = 0; i < validValues.Count; ++i)
        {
            var bitui = GameObject.Instantiate<UIParameterBitfieldBit>(bitPrefab, buttonRoot);
            int bit = System.Convert.ToInt32(validValues[i]);
            int backgroundIndex = 1;
            if (i == 0)
                backgroundIndex = 0;
            else if (i == validValues.Count - 1)
                backgroundIndex = 2;

            bitui.Setup(UIParameterEnum.GetNameAttribute(validValues[i], validValues[i].ToString()), (initialValueInt & bit) != 0, backgrounds[backgroundIndex], bitColor, bitColorSelected);
            bitui.onValueChanged.AddListener((val) =>
            {
                if (val)
                {
                    setterAction(System.Enum.Parse(enumType, (System.Convert.ToInt32(getterFunc()) | bit).ToString(), true));
                }
                else
                {
                    setterAction(System.Enum.Parse(enumType, (System.Convert.ToInt32(getterFunc()) & ~bit).ToString(), true));
                }
            });
            bits.Add(bitui);
        }
    }
}
