using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MultiSlider : MonoBehaviour, IFocusable
{
	struct ColorAndPos
	{
		public Color Color;
		public float Pos;
		public ColorAndPos(Color color, float pos)
		{
			Color = color; Pos = pos;
		}
	}

	public enum SliderDirection { Horizontal, Verical }

	public SliderDirection Direction = SliderDirection.Horizontal;
	public float HandlePosition = 0.5f;

	public event System.Action<MultiSliderHandle> HandleSelected;

	public bool HasFocus { get; private set; }
	public MultiSliderHandle ActiveHandle { get; private set; }
	public MultiSliderHandle[] AllHandles => transform.GetComponentsInChildren<MultiSliderHandle>();

	Texture2D _texture;
	Sprite _sprite;
	bool _suspendRepaint;
	float _sliderPosX => transform.parent.InverseTransformPoint(transform.position).x;
	float _sliderWidth => (transform as RectTransform).rect.width;

	public void GiveFocus()
	{
		HasFocus = true;
		GetComponentsInParent<IFocusable>().First(f => (object)f != this).GiveFocus();
		GetComponent<Outline>().enabled = HasFocus;
	}

	public void RemoveFocus()
	{
		HasFocus = false;
		SelectHandle(null);
		GetComponent<Outline>().enabled = HasFocus;
	}

	public void SelectHandle(MultiSliderHandle multiSliderHandle)
	{
		if (multiSliderHandle != ActiveHandle)
		{
			// First deselect
			ActiveHandle = null;
			HandleSelected?.Invoke(ActiveHandle);

			// Then select new handle
			ActiveHandle = multiSliderHandle;
			HandleSelected?.Invoke(ActiveHandle);
		}
		if (ActiveHandle != null)
		{
			GiveFocus();
		}
	}

	public void RemoveHandle(MultiSliderHandle handle)
	{
		var all = AllHandles;
		if ((all.Length > 1) && (all.Contains(handle)))
		{
			handle.transform.SetParent(null);
			GameObject.Destroy(handle.gameObject);
			if (ActiveHandle == handle)
			{
				SelectHandle(null);
			}
			Repaint();
		}
	}

	public List<Animations.EditKeyframe> ToAnimationKeyFrames(float unitSize)
	{
        var ret = new List<Animations.EditKeyframe>(
            GetColorAndPos().Select(colPos =>
            {
                float time = (colPos.Pos * _sliderWidth + _sliderPosX) / unitSize;
                return new Animations.EditKeyframe()
				{
                    time = time,
                    color = colPos.Color
				};
            }));
        return ret;
	}

	public void FromAnimationKeyframes(List<Animations.EditKeyframe> keyframes, float unitSize)
	{
		_suspendRepaint = true;

		try
		{
			Clear();

			float startTime = _sliderPosX / unitSize;
			float endTime = (_sliderPosX + _sliderWidth) / unitSize;
			float eps = 0.0001f;

			bool dupHandle = false;
            for (int i = 0; i < keyframes.Count; ++i)
            {
				var kf = keyframes[i];

				// Skip automatically inserted keyframes
				if ((i == 0) && (Mathf.Abs(kf.time - startTime) < eps) && (kf.color == Color.black))
				{
					continue;
				}
				if (((i + 1) == keyframes.Count) && (Mathf.Abs(kf.time - endTime) < eps) && (kf.color == Color.black))
				{
					continue;
				}

                var handle = AllHandles[0]; // Initially we should have only one handle
                if (dupHandle)
                {
                    handle = handle.Duplicate();
                }

                handle.ChangeColor(kf.color, noRepaint: true);

                var rect = (transform as RectTransform).rect;
                Vector2 pos = handle.transform.localPosition;
                pos.x = kf.time * unitSize - _sliderPosX;
                handle.transform.localPosition = pos;

                dupHandle = true;
            }

            SelectHandle(null);
		}
		finally
		{
			_suspendRepaint = false;
		}
	}

	public void Repaint()
	{
		if (_suspendRepaint) return;

		var colorsAndPos = GetColorAndPos();

		Color[] pixels = _texture.GetPixels();
		int x = 0, lastMax = 0;
		for (int i = 1; i < colorsAndPos.Length; ++i)
		{
			int max = Mathf.RoundToInt(colorsAndPos[i].Pos * pixels.Length);
			for (; x < max; ++x)
			{
				pixels[x] = Color.Lerp(colorsAndPos[i - 1].Color, colorsAndPos[i].Color, ((float)x - lastMax) / (max - lastMax));
			}
			lastMax = max;
		}
		_texture.SetPixels(pixels);
		_texture.Apply(false);
	}

	public Color GetColorAt(float cursorPos)
	{
		float width = (transform as RectTransform).rect.width;
		float x0 = transform.parent.InverseTransformPoint(transform.position).x;
		float cursorPercent = (cursorPos - x0) / width;

		if ((cursorPercent >= 0) && (cursorPercent <= 1))
		{
			var colorsAndPos = GetColorAndPos();
			float lastMax = 0;
			for (int i = 1; i < colorsAndPos.Length; ++i)
			{
				float max = colorsAndPos[i].Pos;
				if (cursorPercent <= max)
				{
					return Color.Lerp(colorsAndPos[i - 1].Color, colorsAndPos[i].Color, (cursorPercent - lastMax) / (max - lastMax));
				}
				lastMax = max;
			}
			throw new System.InvalidOperationException();
		}
		return new Color();
	}

	public void LeftBoundChanged(float offset, bool stretch)
	{
		foreach (var handle in AllHandles)
		{
			var pos = handle.transform.localPosition;
			if (!stretch)
			{
				pos.x -= offset;
			}
			if (pos.x < 0)
			{
				pos.x = 0;
			}
			handle.transform.localPosition = pos;
		}
	}

	public void RightBoundChanged(float offset, bool stretch)
	{
		float width = (transform as RectTransform).rect.width;
		float stretchFactor = width / (width - offset);

		foreach (var handle in AllHandles)
		{
			var pos = handle.transform.localPosition;
			if (stretch)
			{
				pos.x *= stretchFactor;
			}
			if (pos.x > width)
			{
				pos.x = width;
			}
			handle.transform.localPosition = pos;
		}
	}

	ColorAndPos[] GetColorAndPos()
	{
		float width = (transform as RectTransform).rect.width;
		var list = AllHandles
			.OrderBy(h => h.transform.localPosition.x)
			.Select(h => new ColorAndPos(h.Color, h.transform.localPosition.x / width)).ToList();
		// Insert key at beginning and end to transion from black color
		if (list[0].Pos > 0)
		{
			list.Insert(0, new ColorAndPos(Color.black, 0));
		}
		if (list.Last().Pos < 1)
		{
			list.Add(new ColorAndPos(Color.black, 1));
		}
		return list.ToArray();
	}

	void Clear()
	{
		SelectHandle(null);

		// Keep just one handle
		var all = AllHandles;
		foreach (var handle in all.Skip(1))
		{
			handle.transform.SetParent(null);
			GameObject.Destroy(handle.gameObject);
		}
	}

	void OnDestroy()
	{
		Object.Destroy(_sprite);
		_sprite = null;
		Object.Destroy(_texture);
		_texture = null;
	}

	void Awake()
	{
		var img = GetComponent<Image>();
		var texture = img.sprite.texture;
		if (texture.mipmapCount != 1)
		{
			Debug.LogWarning("Texture used for color gradient should have only one mipmap level");
		}

		// Create owned texture because it will modify it
		_texture = new Texture2D(texture.width, texture.height, texture.format, false);
		Graphics.CopyTexture(texture, _texture);
		_sprite = Sprite.Create(_texture, img.sprite.rect, img.sprite.pivot);
		img.sprite = _sprite;
	}

	// Use this for initialization
	void Start()
	{
		Repaint();
	}
}
