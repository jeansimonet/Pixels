using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Animations;

public class PatternAttribute
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

    public override bool CanEdit(System.Type parameterType, IEnumerable<object> attributes = null)
    {
        return parameterType == typeof(EditPattern) && attributes.Any(a => a.GetType() == typeof(PatternAttribute));
    }

    protected override void SetupControls(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null)
    {
        var initialValue = (EditPattern)getterFunc();

        // Set name
        nameText.text = name;

        // Value
        valueButton.onClick.AddListener(
        () =>
        {
            PixelsApp.Instance.ShowPatternEditor("Edit Pattern", (EditPattern)getterFunc(),
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
		Color[] pixels = _texture.GetPixels();
        for (int i = 0; i < pixels.Length; ++i)
        {
            pixels[i] = Color.black;
        }
        for (int j = 0; j < currentPattern.gradients.Count; ++j)
        {
            var currentGradient = currentPattern.gradients[j];
            int x = 0, lastMax = 0;
            for (int i = 1; i < currentGradient.keyframes.Count; ++i)
            {
                int max = Mathf.RoundToInt(currentGradient.keyframes[i].time / 0.02f);
                for (; x < max; ++x)
                {
                    Color prevColor = new Color(currentGradient.keyframes[i - 1].intensity, currentGradient.keyframes[i - 1].intensity, currentGradient.keyframes[i - 1].intensity);
                    Color nextColor = new Color(currentGradient.keyframes[i].intensity, currentGradient.keyframes[i].intensity, currentGradient.keyframes[i].intensity);
                    pixels[j * _texture.width + x] = Color.Lerp(prevColor, nextColor, ((float)x - lastMax) / (max - lastMax));
                }
                lastMax = max;
            }
        }
		_texture.SetPixels(pixels);
		_texture.Apply(false);
	}

    void Awake()
    {
		_texture = new Texture2D(512, 20, TextureFormat.ARGB32, false);
        _texture.filterMode = FilterMode.Point;
        _texture.wrapMode = TextureWrapMode.Clamp;
        patternImage.texture = _texture;
    }

	void OnDestroy()
	{
		patternImage.texture = null;
		Object.Destroy(_texture);
		_texture = null;
	}

}
