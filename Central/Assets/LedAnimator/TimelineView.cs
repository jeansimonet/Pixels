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
	float _heightPadding = 40;
	[SerializeField]
	float _unitWidth = 200; // Width for 1 second
	[SerializeField]
	float _snapInterval = 0.1f; // In seconds
	[SerializeField]
	float _ticksLength = 0.5f;
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
	[SerializeField]
	Toggle _moveStrechToogle = null;
	[SerializeField]
	Dropdown _namesDropdown = null;

	Animations.EditAnimationSet _animationSet;
	int _animIndex = -1;
	float _widthPadding;
	float _animPosX0;
	bool _playAnims;
	float _playTime;

	public float Duration { get; private set; }
	public int Zoom { get; private set; }
	public float SnapInterval => _snapInterval;
	public float Unit =>_unitWidth * Zoom;
	public bool IsMoveStretchOn => _moveStrechToogle?.isOn ?? true;
	public int ColorAnimCount => _colorAnimsRoot.childCount - 1;
	public Animations.EditAnimation CurrentAnimation => (_animIndex < 0 ? null : _animationSet.animations[_animIndex]);
	public bool PlayAnimations => _playAnims;
	public ColorAnimator ActiveColorAnimator { get; private set; }
	public ColorAnimator[] ColorAnimators => _colorAnimsRoot.GetComponentsInChildren<ColorAnimator>();

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
		LedSelectorPanel.Instance.Show(sprite =>
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

	public void AddTrack(Animations.EditTrack track)
	{
		var colorAnim = CreateAnimation();
		colorAnim.FromAnimationTrack(track, Unit);
		Repaint();
	}

	public void SetAnimations(Animations.EditAnimationSet animations)
	{
		if (animations.animations == null)
		{
			animations.animations = new List<Animations.EditAnimation>();
		}
		if (animations.animations.Count == 0)
		{
			animations.animations.Add(new Animations.EditAnimation());
		}

		// Drop current animation
		_animIndex = -1;

		// Store animation set
		_animationSet = animations;

		RefreshNames();
		ShowAnimation(0);
	}

	public void AddNewAnimation()
	{
		_animationSet.animations.Add(new Animations.EditAnimation());

		RefreshNames();
		ShowAnimation(_animationSet.animations.Count - 1);
	}

	public void DeleteAnimation()
	{
		int index = _animIndex;

		// Drop current animation
		_animIndex = -1;

		_animationSet.animations.RemoveAt(index);

		RefreshNames();
		ShowAnimation(index);
	}

	public void EditAnimationName()
	{
		AnimationNamePanel.Instance.Show(CurrentAnimation.name, ChangeAnimName);
	}

	public void TogglePlayAnimations()
	{
		_playAnims = !_playAnims;
	}

    public void ApplyChanges()
    {
        SerializeAnimation();
    }

	void ChangeAnimName(string name)
	{
		if (!string.IsNullOrWhiteSpace(name))
		{
			CurrentAnimation.name = name;
			RefreshNames();
		}
	}

	void RefreshNames()
	{
		int index = 0;
		foreach (var anim in _animationSet.animations)
		{
			if (string.IsNullOrWhiteSpace(anim.name))
			{
				anim.name = $"Anim#{index + 1}";
			}
			++index;
		}
		_namesDropdown.options = _animationSet.animations.Select(a => new Dropdown.OptionData(a.name)).ToList();
	}

	void ShowAnimation(int animIndex)
	{
		// Make sure to use a valid index
		animIndex = (int)Mathf.Clamp(animIndex, 0, _animationSet.animations.Count - 1);

        if (animIndex != _animIndex)
		{
			if (CurrentAnimation != null)
			{
				SerializeAnimation();
			}

			// Select animation
			_animIndex = animIndex;
			_namesDropdown.value = _animIndex;

			// Load data and display
			DeserializeAnimation();
			Repaint();
		}
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

    void SerializeAnimation()
	{
        CurrentAnimation.tracks.Clear();
        foreach (var t in GetComponentsInChildren<ColorAnimator>())
        {
            CurrentAnimation.tracks.Add(t.ToAnimationTrack(Unit));
        }
	}

	void DeserializeAnimation()
	{
        Clear();

		if (!CurrentAnimation.empty)
		{
			Duration = CurrentAnimation.duration;

			foreach (var track in CurrentAnimation.tracks)
			{
				var colorAnim = CreateAnimation();
				colorAnim.FromAnimationTrack(track, Unit);
			}
		}

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
		Duration = _minDuration;
		Zoom = _minZoom;
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

		// Update size
		var size = rectTransf.sizeDelta;
		size.x = _widthPadding + Unit * Duration;
		size.y = _heightPadding + _colorAnimsRoot.childCount * 80 + (_colorAnimsRoot.childCount - 1) * 10; //TODO
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
		_widthPadding = (transform as RectTransform).rect.width - _ticksRoot.rect.width;
		_animPosX0 = _colorAnimsRoot.GetComponentInChildren<MovableArea>().transform.localPosition.x;
		Clear();
	}

	// Use this for initialization
	void Start()
	{
		_namesDropdown.onValueChanged.AddListener(ShowAnimation);
		Repaint();
	}

	// Update is called once per frame
	void Update()
	{
		bool play = _playAnims && (ColorAnimCount > 0);
		_playCursor.gameObject.SetActive(play);
		if (play)
		{
			// Compute animation time
			_playTime += Time.unscaledDeltaTime;
			if (_playTime > Duration)
			{
				_playTime = 0;
			}

			// Update current color for each animation
			float cursorPos = _playTime * Unit;
			foreach (var colorAnim in ColorAnimators)
			{
				colorAnim.ShowColor(cursorPos);
			}

			// Move cursor
			var transf = _playCursor.transform;
			var pos = transf.localPosition;
			pos.x = _animPosX0 + cursorPos;
			transf.localPosition = pos;
		}
		else
		{
			_playTime = 0;
			foreach (var colorAnim in ColorAnimators)
			{
				colorAnim.ResetColor();
			}
		}
	}
}
