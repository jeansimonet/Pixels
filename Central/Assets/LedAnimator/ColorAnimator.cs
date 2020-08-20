using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class ColorAnimator : MonoBehaviour, IFocusable
{
    [SerializeField]
    Image _image = null;
    [SerializeField]
	Text _number = null;
	[SerializeField]
	MovableArea _movableColorSlider = null;
	[SerializeField]
	RectTransform _confirmRemovePanel = null;
	[SerializeField]
	Sprite[] _led6Sprites = null;

	TimelineView _timeline;

	public event System.Action<ColorAnimator> GotFocus;
	public bool HasFocus { get; private set; }

	public float LeftBound { get { return _movableColorSlider.LeftBound; } set { _movableColorSlider.LeftBound = value; } }
	public float RightBound { get { return _movableColorSlider.RightBound; } set { _movableColorSlider.RightBound = value; } }

	public MultiSlider ColorSlider => _movableColorSlider.Movable.GetComponentInChildren<MultiSlider>();

    List<int> LedNumbers;

	public void ChangeLed()
	{
		LedSelectorPanel.Instance.Show(LedNumbers, numbers => SetLedNumbers(numbers));
	}

	public void SetLedNumbers(List<int> numbers)
	{
		if (numbers == null || numbers.Count == 0)
		{
			LedNumbers = new List<int>();
			_number.text = "";
		}
		else
		{
			LedNumbers = new List<int>(numbers);
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < numbers.Count; ++i)
			{
				if (i != 0)
					builder.Append(", ");
				builder.Append((numbers[i] + 1).ToString());
			}
			_number.text = builder.ToString();
		}
	}

	public void ShowColor(float cursorPos)
	{
		_image.color = ColorSlider.GetColorAt(cursorPos);
	}

	public void ResetColor()
	{
		_image.color = Color.white;
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
		gameObject.transform.SetParent(null);
		GameObject.Destroy(gameObject);
		//TODO ActiveColorAnimator = null;
		_timeline.Repaint(); //TODO
	}

	public void DuplicateSelf()
	{
		RemoveFocus();
		_timeline.AddTrack(ToAnimationTrack(_timeline.Unit));
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

    public Animations.EditRGBTrack ToAnimationTrack(float unitSize)
    {
        var rect = (ColorSlider.transform as RectTransform).rect;
        return new Animations.EditRGBTrack()
        {
            ledIndices = new List<int>(LedNumbers),
            gradient = ColorSlider.ToGradient()
        };
    }

    public void FromAnimationTrack(Animations.EditRGBTrack track, float unitSize)
    {
        ShowConfirmRemove(false);
		SetLedNumbers(track.ledIndices);
		if (track.empty)
		{
			LeftBound = 0 * unitSize;
			RightBound = _timeline.Duration * unitSize; //TODO
		}
		else
		{
			LeftBound = track.firstTime * unitSize;
			RightBound = track.lastTime * unitSize;
			ColorSlider.FromGradient(track.gradient);
		}
    }

    // public static int LedSpriteToIndex(string spriteName)
	// {
	// 	int face = int.Parse(spriteName[spriteName.IndexOf('-') - 1].ToString());
	// 	int point = int.Parse(spriteName[spriteName.IndexOf('-') + 1].ToString());
	// 	return Enumerable.Range(1, face - 1).Sum() + point - 1;
	// }

	// public static string IndexToLedSprite(int index)
	// {
	// 	int face = 1;
	// 	int count = 0;	
	// 	while ((count + face) <= index)
	// 	{
	// 		count += face;
	// 		++face;
	// 	}
	// 	int point = 1 + index - count;
	// 	return string.Format("Led{0}-{1}", face, point);
	// }

	IFocusable[] FindFocusables()
	{
		return GetComponentsInChildren<IFocusable>().Where(f => (object)f != this).ToArray();
	}

	void ShowConfirmRemove(bool show = true)
	{
		_confirmRemovePanel.gameObject.SetActive(show);
	}

	void Awake()
	{
		_timeline = GetComponentInParent<TimelineView>(); //TODO
		LedNumbers = new List<int>();
	}

	// Use this for initialization
	void Start()
	{
		ShowConfirmRemove(false);
	}
}
