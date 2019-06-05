using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ColorAnimator : MonoBehaviour, IFocusable
{
	[SerializeField]
	Image _image = null;
	[SerializeField]
	MovableArea _movableColorSlider = null;
	[SerializeField]
	RectTransform _confirmRemovePanel = null;

	public event System.Action<ColorAnimator> GotFocus;
	public bool HasFocus { get; private set; }

	public float LeftBound { get { return _movableColorSlider.LeftBound; } set { _movableColorSlider.LeftBound = value; } }
	public float RightBound { get { return _movableColorSlider.RightBound; } set { _movableColorSlider.RightBound = value; } }

	public MultiSlider ColorSlider { get { return _movableColorSlider.Movable.GetComponentInChildren<MultiSlider>(); } }

	public void ChangeLed()
	{
		LedSelector.Instance.PickLed(sprite =>
		{
			if (sprite != null)
			{
				SetLedSprite(sprite);
			}
		});
	}

	public void SetLedSprite(Sprite sprite)
	{
		_image.sprite = sprite;
	}

	public void Maximize()
	{
		_movableColorSlider.Maximize();
	}

	public void ConfirmRemoveSelf()
	{
		ColorSlider.SelectHandle(null);
		ShowConfirmRemove();
	}

	public void RemoveSelf()
	{
		var timeline = GetComponentInParent<TimelineView>();
		gameObject.transform.SetParent(null);
		GameObject.Destroy(gameObject);
		//TODO ActiveColorAnimator = null;
		timeline.Repaint(); //TODO
	}

	public void GiveFocus()
	{
		if (!HasFocus)
		{
			HasFocus = true;
			if (GotFocus != null) GotFocus(this);
		}
	}

	public void RemoveFocus()
	{
		HasFocus = false;
		foreach (var focusable in FindFocusables())
		{
			focusable.RemoveFocus();
		}
	}

	public Animations.RGBAnimationTrack Serialize(float unitSize)
	{
		var rect = (ColorSlider.transform as RectTransform).rect;
		return new Animations.RGBAnimationTrack()
		{
			startTime = (short)Mathf.RoundToInt(LeftBound * 1000 / unitSize),
			duration = (short)Mathf.RoundToInt((RightBound - LeftBound) * 1000 / unitSize),
			ledIndex = (byte)LedSpriteToIndex(_image.sprite.name),
			count = 1,
			keyframes = ColorSlider.Serialize(),
		};
	}

	public void Deserialize(Animations.RGBAnimationTrack track, float unitSize)
	{
		ShowConfirmRemove(false);
		SetLedSprite(LedSelector.Instance.GetLedSprite(ColorAnimator.IndexToLedSprite(track.ledIndex)));
		LeftBound = track.startTime / 1000f * unitSize;
		RightBound = LeftBound + track.duration / 1000f * unitSize;
		ColorSlider.Deserialize(track.keyframes);
	}

	public static int LedSpriteToIndex(string spriteName)
	{
		int face = int.Parse(spriteName[spriteName.IndexOf('-') - 1].ToString());
		int point = int.Parse(spriteName[spriteName.IndexOf('-') + 1].ToString());
		return Enumerable.Range(1, face - 1).Sum() + point - 1;
	}

	public static string IndexToLedSprite(int index)
	{
		int face = 1;
		int count = 0;	
		while ((count + face) <= index)
		{
			count += face;
			++face;
		}
		int point = 1 + index - count;
		return string.Format("Led{0}-{1}", face, point);
	}

	IFocusable[] FindFocusables()
	{
		return GetComponentsInChildren<IFocusable>().Where(f => (object)f != this).ToArray();
	}

	void ShowConfirmRemove(bool show = true)
	{
		_confirmRemovePanel.gameObject.SetActive(show);
	}

	// Use this for initialization
	void Start()
	{
		ShowConfirmRemove(false);
	}

	// Update is called once per frame
	void Update()
	{

	}
}
