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

        var vals = new List<object>();
        foreach (var val in System.Enum.GetValues(enumType))
        {
            vals.Add(val);
        }
        var strings = new List<string>(System.Enum.GetNames(enumType));
        int min = 0;
        int max = vals.Count - 1;
        var skip = (SkipEnumAttribute) System.Attribute.GetCustomAttribute(enumType, typeof (SkipEnumAttribute));
        if (skip != null)
        {
            min = skip.skipCount;
            vals.RemoveRange(0, skip.skipCount);
            strings.RemoveRange(0, skip.skipCount);
        }

        for (int i = 0; i < vals.Count; ++i)
        {
            var bitui = GameObject.Instantiate<UIParameterBitfieldBit>(bitPrefab, buttonRoot);
            int bit = System.Convert.ToInt32(vals[i]);
            int backgroundIndex = 1;
            if (i == 0)
                backgroundIndex = 0;
            else if (i == vals.Count - 1)
                backgroundIndex = 2;

            bitui.Setup(strings[i], (initialValueInt & bit) != 0, backgrounds[backgroundIndex], bitColor, bitColorSelected);
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
