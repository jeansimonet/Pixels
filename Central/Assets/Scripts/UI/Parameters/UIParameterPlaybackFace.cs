using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

class PlaybackFaceAttribute
    : System.Attribute
{
}

public class UIParameterPlaybackFace
    : UIParameter
{
    [Header("Controls")]
    public Text nameText;
    public Toggle currentFaceToggle;
    public InfiniWheel faceWheel;
    public Image faceWheelMask;

    int currentFace = -1;

    public override bool CanEdit(System.Type parameterType, IEnumerable<object> attributes = null)
    {
        return parameterType == typeof(int) && attributes.Any(a => a.GetType() == typeof(PlaybackFaceAttribute));
    }

    protected override void SetupControls(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null)
    {
        int initialValue = (int)getterFunc();
        currentFace = initialValue;
        
        // Look for decorating attributes
        int faceCount = 20;
        string[] values = new string[faceCount];
        for (int i = 0; i < faceCount; ++i)
        {
            values[i] = (i + 1).ToString();
        }
        faceWheel.Init(values);
        void faceWheelValueChange(int index, Text text)
        {
            currentFace = index;
            setterAction(index);
        }

        // Set name
        nameText.text = name;
        currentFaceToggle.isOn = initialValue == -1;
        faceWheelMask.gameObject.SetActive(initialValue == -1);
        faceWheel.GetComponent<CanvasGroup>().alpha = initialValue == -1 ? 0.25f : 1.0f;
        if (currentFace == -1)
        {
            currentFace = 19;
        }
        faceWheel.Select(currentFace);
        if (initialValue != -1)
        {
            faceWheel.ValueChange += faceWheelValueChange;
        }

        currentFaceToggle.onValueChanged.AddListener(
            (newOn) =>
            {
                if (newOn)
                {
                    faceWheel.ValueChange -= faceWheelValueChange;
                    faceWheelMask.gameObject.SetActive(true);
                    faceWheel.GetComponent<CanvasGroup>().alpha = 0.25f;
                    setterAction(-1);
                }
                else
                {
                    faceWheel.ValueChange += faceWheelValueChange;
                    faceWheelMask.gameObject.SetActive(false);
                    faceWheel.GetComponent<CanvasGroup>().alpha = 1.0f;
                    faceWheel.Select(currentFace);
                    setterAction(currentFace);
                }
            });

    }
}
