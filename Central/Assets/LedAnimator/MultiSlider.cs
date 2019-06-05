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
	public MultiSliderHandle[] AllHandles { get { return transform.GetComponentsInChildren<MultiSliderHandle>(); } }

	Texture2D _texture;
	Sprite _sprite;
	bool _suspendRepaint;

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
			ActiveHandle = multiSliderHandle;
			if (HandleSelected != null) HandleSelected(ActiveHandle);
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

	public Animations.RGBKeyframe[] Serialize()
	{
		return GetColorAndPos().Select(c => new Animations.RGBKeyframe()
		{
			time = (byte)Mathf.RoundToInt(255 * c.Pos),
			red = ((Color32)c.Color).r,
			green = ((Color32)c.Color).g,
			blue = ((Color32)c.Color).b,
		}).ToArray();
	}

	public void Deserialize(Animations.RGBKeyframe[] keyframes)
	{
		_suspendRepaint = true;

		try
		{
			Clear();

			bool first = true;
			foreach (var kf in keyframes)
			{
				var handle = AllHandles[0]; // Initially we should have only one handle
				if (!first)
				{
					handle = handle.Duplicate();
				}

				handle.ChangeColor(new Color32(kf.red, kf.green, kf.blue, 255), noRepaint: true);

				var rect = (transform as RectTransform).rect;
				Vector2 pos = handle.transform.localPosition;
				pos.x = kf.time / 255f * rect.width + rect.xMin;
				handle.transform.localPosition = pos;

				first = false;
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
		colorsAndPos.Insert(0, new ColorAndPos(colorsAndPos[0].Color, 0));
		colorsAndPos.Add(new ColorAndPos(colorsAndPos.Last().Color, 1));

		Color[] pixels = _texture.GetPixels();
		int x = 0, lastMax = 0;
		for (int i = 1; i < colorsAndPos.Count; ++i)
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

	List<ColorAndPos> GetColorAndPos()
	{
		var rect = (transform as RectTransform).rect;
		return transform.OfType<RectTransform>().Select(t => t.GetComponent<MultiSliderHandle>())
			.Where(h => h != null)
			.OrderBy(h => h.transform.localPosition.x)
			.Select(h => new ColorAndPos(h.Color, (h.transform.localPosition.x - rect.xMin) / rect.width)).ToList();
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

	// Update is called once per frame
	void Update()
	{

	}
}
