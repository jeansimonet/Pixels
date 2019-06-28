using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TimelineView : MonoBehaviour
{
	[SerializeField]
	float _minDuration = 1;
	[SerializeField]
	int _minZoom = 1;
	[SerializeField]
	int _maxZoom = 10;
	[SerializeField]
	float _unitWidth = 200; // Width for 1 second
	[SerializeField]
	float _snapInterval = 0.1f; // In seconds
	[SerializeField]
	float _ticksLength = 0.5f;
	[SerializeField]
	RectTransform _animSetTogglesRoot = null;
	[SerializeField]
	RectTransform _ticksRoot = null;
	[SerializeField]
	RectTransform _colorAnimsRoot = null;
	[SerializeField]
	RectTransform _animBottomButtons = null;
	[SerializeField]
	ColorAnimator _colorAnimPrefab = null;
	[SerializeField]
	Transform _playCursor = null;

	float _widthPadding;
	Animations.EditAnimationSet _animSet;
	bool _playAnims;
	float _playTime;

	public float Duration { get; private set; }
	public int Zoom { get; private set; }
	public float SnapInterval => _snapInterval;
	public float Unit =>_unitWidth * Zoom;
	public int AnimationCount =>_colorAnimsRoot.childCount - 1;
	public int CurrentFace { get; private set; }
	public bool PlayAnimations => _playAnims;

	public ColorAnimator ActiveColorAnimator { get; private set; }

	public void ModifyDuration(float signedOffset)
	{
		Duration = Mathf.Max(_minDuration, Duration + signedOffset);
		Repaint();
	}

	public void ModifyZoom(int signedOffset)
	{
		Zoom = Mathf.Clamp(Zoom + signedOffset, _minZoom, _maxZoom);
		Repaint();
	}

	public void AddAnim()
	{
		LedSelector.Instance.PickLed(sprite =>
		{
			if (sprite != null)
			{
				var colorAnim = CreateAnimation(sprite);
				colorAnim.LeftBound = 0;
				colorAnim.RightBound = Unit * Duration;
				colorAnim.GiveFocus();
				colorAnim.ColorSlider.SelectHandle(colorAnim.ColorSlider.AllHandles[0]);
				Repaint();
			}
		});
	}

	public void ChangeCurrentFace(int index)
	{
		if (CurrentFace != index)
		{
			// Save current anims
			Serialize();

			// And show
			ShowFace(index);
		}
	}

	public void SaveToFile()
	{
		Debug.Log("SaveToFile");
		Serialize();
		PrintData();

		// ----> Save _animSet in file
	}

	public void LoadFromFile()
	{
		Debug.Log("LoadFromFile");

        // Load from file then update _animSet with data from dice

		ShowFace(0);
		_animSetTogglesRoot.GetComponentsInChildren<Toggle>().First().isOn = true;
	}

	public void UploadToDice()
	{
		Debug.Log("UploadToDice");
		Serialize();

		// ----> Send _animSet to dice
	}

	public void DownloadFromDice()
	{
		Debug.Log("DownloadFromDice");

		// ----> Update _animSet with data from dice

		ShowFace(0);
		_animSetTogglesRoot.GetComponentsInChildren<Toggle>().First().isOn = true;
	}

	public void PrintData()
	{
        _animSet.ToString();
	}

	public void TogglePlayAnimations()
	{
		_playAnims = !_playAnims;
	}

	ColorAnimator CreateAnimation(Sprite sprite = null)
	{
		var colorAnim = GameObject.Instantiate<ColorAnimator>(_colorAnimPrefab, _colorAnimsRoot);
		_animBottomButtons.SetAsLastSibling(); // Keep controls at the bottom
		if (sprite != null)
		{
			colorAnim.SetLedSprite(sprite);
		}
		colorAnim.GotFocus += OnColorAnimatorGotFocus;
		return colorAnim;
	}

	void ShowFace(int index)
	{
		// Select face to show
		CurrentFace = index;

		// Load data and display
		Deserialize();
		Repaint();
	}

	void Serialize()
	{
		//var anims = GetComponentsInChildren<ColorAnimator>();

		//Debug.LogFormat("Serializing {0} color animations", anims.Length);

		//var anim = new Animations.EditAnimation()
		//{
		//	tracks = anims.Select(a => a.Serialize(Unit)).ToArray()
		//};
		//_animSet.animations[CurrentFace] = anim;
	}

	void Deserialize()
	{
		//var data = _animSet.animations[CurrentFace];
		//if (data == null)
		//{
		//	Debug.LogError("Null animation data at index " + CurrentFace);
		//	return;
		//}

		//Debug.LogFormat("Deserializing {0} color animations", data.tracks.Length);

		//Clear();
		//Duration = data.duration / 1000f;

		//foreach (var track in data.tracks)
		//{
		//	var colorAnim = CreateAnimation();
		//	colorAnim.Deserialize(track, Unit);
		//}

		Repaint();
	}

	void OnColorAnimatorGotFocus(ColorAnimator colorAnim)
	{
		if (ActiveColorAnimator != colorAnim)
		{
			if (ActiveColorAnimator != null)
			{
				ActiveColorAnimator.RemoveFocus();
			}
			ActiveColorAnimator = colorAnim;
		}
	}

	void Clear()
	{
		ActiveColorAnimator = null;
		for (int i = _colorAnimsRoot.childCount - 1; i >= 0; --i)
		{
			var child = _colorAnimsRoot.GetChild(i);
			if (child != _animBottomButtons)
			{
				child.SetParent(null);
				GameObject.Destroy(child.gameObject);
			}
		}
		for (int i = _ticksRoot.childCount - 1; i > 0; --i)
		{
			var child = _ticksRoot.GetChild(i);
			child.SetParent(null);
			GameObject.Destroy(child.gameObject);
		}
	}

	public void Repaint() //TODO public for temp code in ColorAnimator
	{
		var rectTransf = transform as RectTransform;

		// Update width
		var size = rectTransf.sizeDelta;
		size.x = _widthPadding + Unit * Duration;
		size.y = 40 + _colorAnimsRoot.childCount * 80 + (_colorAnimsRoot.childCount - 1) * 10; //TODO
		rectTransf.sizeDelta = size;

		// Update vertical lines
		int numExistingLines = _ticksRoot.childCount;
		int numLines = Mathf.RoundToInt(Duration / _ticksLength);
		float time = 0;
		for (int i = 0; i <= numLines; ++i)
		{
			RectTransform lineTransf;
			if (i < numExistingLines)
			{
				lineTransf = _ticksRoot.GetChild(i) as RectTransform;
			}
			else
			{
				// Duplicate first line
				lineTransf = GameObject.Instantiate(_ticksRoot.GetChild(0).gameObject, _ticksRoot).transform as RectTransform;
			}

			bool intT = (int)time == time;

			// Update text
			var text = lineTransf.GetComponentInChildren<Text>();
			text.text = time + "s";
			text.fontStyle = intT ? FontStyle.Bold : FontStyle.Normal;
			text.fontSize = intT ? 24 : 20;

			// Update image
			var img = lineTransf.GetComponentInChildren<Image>();
			var imgRectTransf = (img.transform as RectTransform);
			imgRectTransf.offsetMin = new Vector2(intT ? -2 :  -1, imgRectTransf.offsetMin.y);
			imgRectTransf.offsetMax = new Vector2(intT ? 2 : 1, imgRectTransf.offsetMax.y);

			// And position
			Vector2 pos = lineTransf.anchoredPosition;
			pos.x = Unit * time;
			lineTransf.anchoredPosition = pos;

			time += _ticksLength;
		}
		for (int t = numLines + 1; t < numExistingLines; ++t)
		{
			GameObject.Destroy(_ticksRoot.GetChild(t).gameObject);
		}
	}

	void Awake()
	{
        _animSet = new Animations.EditAnimationSet();
		_widthPadding = (transform as RectTransform).rect.width - _ticksRoot.rect.width;
		Duration = _minDuration;
		Zoom = _minZoom;
		Clear();
	}

	// Use this for initialization
	void Start()
	{
		Repaint();
	}

	// Update is called once per frame
	void Update()
	{
		bool play = _playAnims && (_colorAnimsRoot.childCount > 0);
		_playCursor.gameObject.SetActive(play);
		if (play)
		{
			_playTime += Time.unscaledDeltaTime;
			if (_playTime > Duration)
			{
				_playTime = 0;
			}

			var slider = _colorAnimsRoot.GetChild(0).GetComponent<ColorAnimator>().ColorSlider;
			_playCursor.GetComponentInChildren<Image>().color = slider.GetColorAt(_playTime * Unit);

			var transf = _colorAnimsRoot.GetChild(0).GetComponentInChildren<MovableArea>().transform;
			float x0 = transf.localPosition.x;

			transf = _playCursor.transform;
			var pos = transf.localPosition;
			pos.x = x0 + _playTime * Unit;
			transf.localPosition = pos;
		}
		else
		{
			_playTime = 0;
		}
	}
}
