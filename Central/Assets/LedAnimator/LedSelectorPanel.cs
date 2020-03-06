using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Text.RegularExpressions;

public class LedSelectorPanel : SingletonMonoBehaviour<LedSelectorPanel>
{
	[SerializeField]
	InputField _leds = null;

	System.Action<List<int>> _doneCb;

	public void Show(List<int> leds, System.Action<List<int>> doneCB)
	{
		_doneCb = doneCB;
		gameObject.SetActive(true);
		StringBuilder builder = new StringBuilder();
		for (int i = 0; i < leds.Count; ++i)
		{
			if (i != 0)
				builder.Append(", ");
			builder.Append((leds[i] + 1).ToString());
		}
		_leds.text = builder.ToString();
	}

	public void Close()
	{
		DoClose();
	}

	void DoClose()
	{
		gameObject.SetActive(false);
		List<int> newList = new List<int>();
		var leds = _leds.text.Split(',', ' ');
		for (int i = 0; i < leds.Length; ++i)
		{
			if (!string.IsNullOrEmpty(leds[i]))
			{
				newList.Add(System.Convert.ToInt32(leds[i]) - 1);
			}
		}
		_doneCb(newList);
	}

	// Use this for initialization
	void Start()
	{
	}
}
