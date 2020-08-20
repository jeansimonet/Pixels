using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Animations;

public class GradientAttribute
    : System.Attribute
{
}

public class UIParameterGradient : UIParameter
{
    [Header("Controls")]
    public Text nameText;
    public Button valueButton;
    public RawImage gradientImage;

	Texture2D _texture;

    public override bool CanEdit(System.Type parameterType, IEnumerable<object> attributes = null)
    {
        return parameterType == typeof(EditRGBGradient) && attributes.Any(a => a.GetType() == typeof(GradientAttribute));
    }

    protected override void SetupControls(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null)
    {
        var initialValue = (EditRGBGradient)getterFunc();

        // Set name
        nameText.text = name;

        // Value
        valueButton.onClick.AddListener(
        () =>
        {
            PixelsApp.Instance.ShowGradientEditor("Edit Gradient", (EditRGBGradient)getterFunc(),
            (res, gradient) =>
            {
                if (res)
                {
                    setterAction(gradient);
                    Repaint(gradient);
                }
            });
        });
        Repaint(initialValue);
    }

	void Repaint(EditRGBGradient currentGradient)
	{
		Color[] pixels = _texture.GetPixels();
		int x = 0, lastMax = 0;
		for (int i = 1; i < currentGradient.keyframes.Count; ++i)
		{
			int max = Mathf.RoundToInt(currentGradient.keyframes[i].time * pixels.Length);
			for (; x < max; ++x)
			{
				pixels[x] = Color.Lerp(currentGradient.keyframes[i - 1].color, currentGradient.keyframes[i].color, ((float)x - lastMax) / (max - lastMax));
			}
			lastMax = max;
		}
		_texture.SetPixels(pixels);
		_texture.Apply(false);
	}

    void Awake()
    {
		_texture = new Texture2D(512, 1, TextureFormat.ARGB32, false);
        gradientImage.texture = _texture;        
    }

	void OnDestroy()
	{
		gradientImage.texture = null;
		Object.Destroy(_texture);
		_texture = null;
	}

}
