using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Animations;

public class RGBPatternAttribute
    : System.Attribute
{
}

public class GreyscalePatternAttribute
    : System.Attribute
{
}

public class UIParameterPattern : UIParameter
{
    [Header("Controls")]
    public Text nameText;
    public Button valueButton;
    public RawImage patternImage;

	Texture2D _texture;
    bool greyscale = false;

    public override bool CanEdit(System.Type parameterType, IEnumerable<object> attributes = null)
    {
        return parameterType == typeof(EditPattern) && attributes.Any(a => a.GetType() == typeof(RGBPatternAttribute) || a.GetType() == typeof(GreyscalePatternAttribute));
    }

    protected override void SetupControls(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null)
    {
        var initialValue = (EditPattern)getterFunc();

        // Set name
        nameText.text = name;

        if (attributes.Any(a => a.GetType() == typeof(GreyscalePatternAttribute)))
        {
            greyscale = true;
        }

        // Value
        valueButton.onClick.AddListener(
        () =>
        {
            PixelsApp.Instance.ShowPatternPicker("Select Pattern", (EditPattern)getterFunc(),
            (res, pattern) =>
            {
                if (res)
                {
                    setterAction(pattern);
                    Repaint(pattern);
                }
            });
        });
        Repaint(initialValue);
    }

	void Repaint(EditPattern currentPattern)
	{
		Object.Destroy(_texture);
        if (greyscale)
        {
            _texture = currentPattern.ToGreyscaleTexture();
        }
        else
        {
            _texture = currentPattern.ToTexture();
        }
        patternImage.texture = _texture;
	}

    void Awake()
    {
    }

	void OnDestroy()
	{
		patternImage.texture = null;
		Object.Destroy(_texture);
		_texture = null;
	}

}
