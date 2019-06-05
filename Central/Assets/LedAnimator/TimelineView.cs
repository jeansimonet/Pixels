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

	float _widthPadding;
	Animations.AnimationSet _animSet;

	public float Duration { get; private set; }
	public int Zoom { get; private set; }
	public float Unit { get { return _unitWidth * Zoom; } }
	public int AnimationCount { get { return _colorAnimsRoot.childCount - 1; } }
	public int CurrentFace { get; private set; }

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
		// Generate random anims
		for (int i = 0; i < _animSet.animations.Length; ++i)
		{
			var data = new Animations.RGBAnimation();
			data.duration = (short)Mathf.RoundToInt(1000 * Random.Range(1, 11));
			data.tracks = new Animations.RGBAnimationTrack[Random.Range(1, 11)];
			for (int j = 0; j < data.tracks.Length; ++j)
			{
				Animations.RGBAnimationTrack track = new Animations.RGBAnimationTrack();
				track.startTime = (short)Random.Range(0, data.duration - 100);
				track.duration = (short)Random.Range(100, data.duration - track.startTime);
				track.ledIndex = (byte)Random.Range(0, 21);
				track.keyframes = new Animations.RGBKeyframe[Random.Range(1, 6)];
				for (int k = 0; k < track.keyframes.Length; ++k)
				{
					track.keyframes[k].time = (byte)Random.Range(0, 256);
					track.keyframes[k].red = (byte)Random.Range(0, 256);
					track.keyframes[k].blue = (byte)Random.Range(0, 256);
					track.keyframes[k].green = (byte)Random.Range(0, 256);
				}
				data.tracks[j] = track;
			}
			_animSet.animations[i] = data;
		}

		// ----> Update _animSet with data from dice

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
		int i = 0;
		foreach (var data in _animSet.animations)
		{
			var str = new System.Text.StringBuilder();
			str.Append("#");
			str.Append(i);
			str.Append(" -> ");
			str.Append(data.tracks.Length);
			str.Append(" color animations");
			str.Append(", duration of ");
			str.Append(data.duration);
			str.AppendLine(" seconds");
			foreach (var t in data.tracks)
			{
				str.Append(" * ");
				str.Append(t.startTime);
				str.Append(", ");
				str.Append(t.duration);
				str.Append(", ");
				str.Append(t.ledIndex);
				str.AppendLine();
				foreach (var k in t.keyframes)
				{
					str.Append("    * ");
					str.Append(k.time);
					str.Append(", (");
					str.Append(k.red);
					str.Append(", ");
					str.Append(k.green);
					str.Append(", ");
					str.Append(k.blue);
					str.Append(")");
					str.AppendLine();
				}
			}
			Debug.Log(str.ToString());
			++i;
		}
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
		var anims = GetComponentsInChildren<ColorAnimator>();

		Debug.LogFormat("Serializing {0} color animations", anims.Length);

		var anim = new Animations.RGBAnimation()
		{
			duration = (short)Mathf.RoundToInt(1000 * Duration),
			tracks = anims.Select(a => a.Serialize(Unit)).ToArray()
		};
		_animSet.animations[CurrentFace] = anim;
	}

	void Deserialize()
	{
		var data = _animSet.animations[CurrentFace];
		if (data == null)
		{
			Debug.LogError("Null animation data at index " + CurrentFace);
			return;
		}

		Debug.LogFormat("Deserializing {0} color animations", data.tracks.Length);

		Clear();
		Duration = data.duration / 1000f;

		foreach (var track in data.tracks)
		{
			var colorAnim = CreateAnimation();
			colorAnim.Deserialize(track, Unit);
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
		_animSet = new Animations.AnimationSet
		{
			animations = Enumerable.Range(0, 6)
				.Select(i => new Animations.RGBAnimation() { duration = 1000, tracks = new Animations.RGBAnimationTrack[0] })
				.ToArray()
		};
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

	}
}
