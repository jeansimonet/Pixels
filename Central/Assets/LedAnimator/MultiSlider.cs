using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MultiSlider : MonoBehaviour, IFocusable
{
	[Header("Controls")]
	public RawImage gradientImage;

	public enum SliderDirection { Horizontal, Verical }

	public SliderDirection Direction = SliderDirection.Horizontal;
	public float HandlePosition = 0.5f;

	public event System.Action<MultiSliderHandle> HandleSelected;

	public bool HasFocus { get; private set; }
	public MultiSliderHandle ActiveHandle { get; private set; }
	public MultiSliderHandle[] AllHandles => transform.GetComponentsInChildren<MultiSliderHandle>();

	Texture2D _texture;
	bool _suspendRepaint;
	float _sliderWidth => (transform as RectTransform).rect.width;

	public void GiveFocus()
	{
		HasFocus = true;
		//GetComponentsInParent<IFocusable>().First(f => (object)f != this).GiveFocus();
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

	public Animations.EditRGBGradient ToGradient()
	{
		return new Animations.EditRGBGradient()
		{
			keyframes = GetColorAndPos()
		};
	}

	public void FromGradient(Animations.EditRGBGradient gradient)
	{
		_suspendRepaint = true;

		try
		{
			Clear();

			float startTime = 0.0f;
			float endTime = 1.0f;
			float eps = 0.0001f;

			bool dupHandle = false;
            for (int i = 0; i < gradient.keyframes.Count; ++i)
            {
				var kf = gradient.keyframes[i];

				// Skip automatically inserted keyframes
				if ((i == 0) && (Mathf.Abs(kf.time - startTime) < eps) && (kf.color == Color.black))
				{
					continue;
				}
				if (((i + 1) == gradient.keyframes.Count) && (Mathf.Abs(kf.time - endTime) < eps) && (kf.color == Color.black))
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
                pos.x = kf.time * _sliderWidth;
                handle.transform.localPosition = pos;

                dupHandle = true;
            }

            SelectHandle(null);
		}
		finally
		{
			_suspendRepaint = false;
			Repaint();
		}
	}

	public void Repaint()
	{
		if (_suspendRepaint) return;

		var colorsAndPos = GetColorAndPos();

		Color[] pixels = _texture.GetPixels();
		int x = 0, lastMax = 0;
		for (int i = 1; i < colorsAndPos.Count; ++i)
		{
			int max = Mathf.RoundToInt(colorsAndPos[i].time * pixels.Length);
			for (; x < max; ++x)
			{
				pixels[x] = Color.Lerp(colorsAndPos[i - 1].color, colorsAndPos[i].color, ((float)x - lastMax) / (max - lastMax));
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
			for (int i = 1; i < colorsAndPos.Count; ++i)
			{
				float max = colorsAndPos[i].time;
				if (cursorPercent <= max)
				{
					return Color.Lerp(colorsAndPos[i - 1].color, colorsAndPos[i].color, (cursorPercent - lastMax) / (max - lastMax));
				}
				lastMax = max;
			}
			throw new System.InvalidOperationException();
		}
		return new Color();
	}

	public void LeftBoundChanged(float offset, bool moveStretch)
	{
		foreach (var handle in AllHandles)
		{
			var pos = handle.transform.localPosition;
			if (!moveStretch)
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

	public void RightBoundChanged(float offset, bool moveStretch)
	{
		float width = (transform as RectTransform).rect.width;
		float stretchFactor = width / (width - offset);

		foreach (var handle in AllHandles)
		{
			var pos = handle.transform.localPosition;
			if (moveStretch)
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

	List<Animations.EditRGBKeyframe> GetColorAndPos()
	{
		float width = (transform as RectTransform).rect.width;
		var list = AllHandles
			.OrderBy(h => h.transform.localPosition.x)
			.Select(h => new Animations.EditRGBKeyframe()
			{
				color = h.Color,
				time = h.transform.localPosition.x / width
			}).ToList();
		// Insert key at beginning and end to transion from black color
		if (list[0].time > 0)
		{
			list.Insert(0, new Animations.EditRGBKeyframe()
			{
				color = Color.black,
				time = 0
			});
		}
		if (list.Last().time < 1)
		{
			list.Add(new Animations.EditRGBKeyframe()
			{
				color = Color.black,
				time = 1
			});
		}
		return list;
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
		gradientImage.texture = null;
		Object.Destroy(_texture);
		_texture = null;
	}

	void Awake()
	{
		// Create owned texture because it will modify it
		_texture = new Texture2D(512, 1, TextureFormat.ARGB32, false);
		gradientImage.texture = _texture;
	}

	// Use this for initialization
	void Start()
	{
		Repaint();
	}
}
