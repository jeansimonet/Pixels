using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LedSelectorPanel : SingletonMonoBehaviour<LedSelectorPanel>
{
	[SerializeField]
	RectTransform _leds6Root = null;
	[SerializeField]
	RectTransform _leds20Root = null;

	DiceType _diceType;
	System.Action<int> _doneCb;

	RectTransform _LedsRoot
	{
		get
		{
			switch (_diceType)
			{
				case DiceType.D6: return _leds6Root;
				case DiceType.D20: return _leds20Root;
				default: return null;
			}
		}
	}

	public void Show(DiceType diceType, System.Action<int> doneCB)
	{
		_diceType = diceType;
		_doneCb = doneCB;
		gameObject.SetActive(true);

		_leds6Root.gameObject.SetActive(false);
		_leds20Root.gameObject.SetActive(false);
		_LedsRoot.gameObject.SetActive(true);
	}

	public void Close()
	{
		DoClose(-1);
	}

	void DoClose(int number)
	{
		gameObject.SetActive(false);
		_doneCb(number);
	}

	// Use this for initialization
	void Start()
	{
        int ledIndex = 0;
		foreach (var btn in _leds6Root.GetComponentsInChildren<Button>())
		{
            int ledIndexCopy = ledIndex;
			btn.onClick.AddListener(() => DoClose(ledIndexCopy));
            ++ledIndex;
		}

        ledIndex = 0;
		foreach (var btn in _leds20Root.GetComponentsInChildren<Button>())
		{
            int ledIndexCopy = ledIndex;
			btn.onClick.AddListener(() => DoClose(ledIndexCopy));
            ++ledIndex;
		}
	}
}
