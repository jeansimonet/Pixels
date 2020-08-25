using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIParameterColor : UIParameter
{
    [Header("Controls")]
    public Text nameText;
    public Button colorButton;
    public Image colorImage;
    public Text valueText;

    public override bool CanEdit(System.Type parameterType, IEnumerable<object> attributes = null)
    {
        return parameterType == typeof(Color32);
    }

    protected override void SetupControls(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null)
    {
        // Set name
        nameText.text = name;

        // Set initial value
        SetColor((Color32)getterFunc.Invoke());
        colorButton.onClick.AddListener(() => PixelsApp.Instance.ShowColorPicker("Select " + name, (Color32)getterFunc.Invoke(), (res, newColor) => 
        {
            if (res)
            {
                SetColor(newColor);
                setterAction?.Invoke((Color32)newColor);
            }
        }));
    }

    void SetColor(Color32 newColor)
    {
        var col32 = newColor;
        // Make sure we always use full alpha
        col32.a = 255;
        colorImage.color = col32;
        valueText.text = "#" + col32.r.ToString("X2") + col32.g.ToString("X2") + col32.b.ToString("X2");
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
