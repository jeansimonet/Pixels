using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Text;

public class FaceMaskAttribute
    : System.Attribute
{
}

public class UIParameterFaceMask : UIParameter
{
    [Header("Controls")]
    public Text nameText;
    public Button valueButton;
    public Text valueText;

    public override bool CanEdit(System.Type parameterType, IEnumerable<object> attributes = null)
    {
        return parameterType == typeof(int) && attributes.Any(a => a.GetType() == typeof(FaceMaskAttribute));
    }

    protected override void SetupControls(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null)
    {
        int initialValue = (int)getterFunc();
        
        // Look for decorating attributes
        int min = 0;
        int max = 19;
        var rangeAttribute = attributes.FirstOrDefault(att => att.GetType() == typeof (IntRangeAttribute)) as IntRangeAttribute;
        if (rangeAttribute != null)
        {
            min = rangeAttribute.min;
            max = rangeAttribute.max;
        }
        string[] values = new string[max - min + 1];
        for (int i = min; i <= max; ++i)
        {
            values[i - min] = i.ToString();
        }

        // Set name
        nameText.text = name;

        // Value
        valueText.text = UIParameterFaceMask.GenerateString(initialValue);
        valueButton.onClick.AddListener(
        () =>
        {
            PixelsApp.Instance.ShowFacePicker("Select Faces", (int)getterFunc(),
            (res, mask) =>
            {
                if (res)
                {
                    setterAction(mask);
                    valueText.text = UIParameterFaceMask.GenerateString(mask);
                }
            });
        });
    }

    static string GenerateString(int value)
    {
        StringBuilder builder = new StringBuilder();
        bool first = true;
        for (int i = 0; i < 20; ++i)
        {
            int mask = 1 << i;
            if ((value & mask) != 0)
            {
                if (!first)
                {
                    builder.Append(" | ");
                }
                first = false;
                builder.Append(i+1);
            }
        }
        return builder.ToString();
    }
}
