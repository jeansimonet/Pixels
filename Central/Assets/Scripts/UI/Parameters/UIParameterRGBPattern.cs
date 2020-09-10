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

public class UIParameterRGBPattern : UIParameter
{
    [Header("Controls")]
    public Text nameText;
    public Button valueButton;
    public RawImage patternImage;

	Texture2D _texture;

    public override bool CanEdit(System.Type parameterType, IEnumerable<object> attributes = null)
    {
        return parameterType == typeof(EditRGBPattern) && attributes.Any(a => a.GetType() == typeof(RGBPatternAttribute));
    }

    protected override void SetupControls(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null)
    {
        var initialValue = (EditRGBPattern)getterFunc();

        // Set name
        nameText.text = name;

        // Value
        valueButton.onClick.AddListener(
        () =>
        {
            PixelsApp.Instance.ShowRGBPatternPicker("Select Pattern", (EditRGBPattern)getterFunc(),
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

	void Repaint(EditRGBPattern currentPattern)
	{
		Object.Destroy(_texture);
        _texture = currentPattern.ToTexture();
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
