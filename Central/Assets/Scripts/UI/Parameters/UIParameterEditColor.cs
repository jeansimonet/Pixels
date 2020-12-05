using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIParameterEditColor : UIParameter
{
    [Header("Controls")]
    public Text nameText;
    public Button colorButton;
    public Image colorImage;
    public Text valueText;
    public Image disableOverlayImage;
    public Toggle overrideColorToggle;

    public override bool CanEdit(System.Type parameterType, IEnumerable<object> attributes = null)
    {
        return parameterType == typeof(EditColor);
    }

    protected override void SetupControls(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null)
    {
        // Set name
        nameText.text = name;

        // Set initial value
        SetColor((EditColor)getterFunc.Invoke());

        colorButton.onClick.AddListener(() => PixelsApp.Instance.ShowColorPicker("Select " + name, ((EditColor)getterFunc.Invoke()).asColor32, (res, newColor) => 
        {
            if (res)
            {
                var editColor = EditColor.MakeRGB(newColor);
                SetColor(editColor);
                setterAction?.Invoke(editColor);
            }
        }));

        overrideColorToggle.onValueChanged.AddListener((newToggleValue) =>
        {
            var curColor = (EditColor)getterFunc.Invoke();
            if (newToggleValue)
            {
                curColor.type = EditColor.ColorType.Face;
            }
            else
            {
                curColor.type = EditColor.ColorType.RGB;
            }
            SetColor(curColor);
            setterAction?.Invoke(curColor);
        });
    }

    void SetColor(EditColor newColor)
    {
        Color32 col32 = Color.grey;
        switch (newColor.type)
        {
            case EditColor.ColorType.RGB:
                {
                    col32 = newColor.asColor32;
                    // Make sure we always use full alpha
                    col32.a = 255;
                    colorImage.color = col32;
                    valueText.text = "#" + col32.r.ToString("X2") + col32.g.ToString("X2") + col32.b.ToString("X2");
                    disableOverlayImage.gameObject.SetActive(false);
                    overrideColorToggle.isOn = false;
                }
                break;
            case EditColor.ColorType.Face:
                {
                    colorImage.color = Color.grey;
                    valueText.text = "N/A";
                    disableOverlayImage.gameObject.SetActive(true);
                    overrideColorToggle.isOn = true;
                }
                break;
            default:
                throw new System.NotImplementedException();
        }
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
