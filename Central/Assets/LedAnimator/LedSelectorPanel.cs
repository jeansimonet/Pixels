using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LedSelectorPanel : SingletonMonoBehaviour<LedSelectorPanel>
{
	[SerializeField]
	RectTransform _ledsRoot = null;

	System.Action<int> _doneCb;

	public void Show(System.Action<int> doneCB)
	{
		_doneCb = doneCB;
		gameObject.SetActive(true);
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
		foreach (var btn in _ledsRoot.GetComponentsInChildren<Button>())
		{
            int ledIndexCopy = ledIndex;
			btn.onClick.AddListener(() => DoClose(ledIndexCopy));
            ledIndex++;
		}
	}
}
