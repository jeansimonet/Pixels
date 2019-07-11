using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimationNamePanel : SingletonMonoBehaviour<AnimationNamePanel>
{
	[SerializeField]
	InputField _input = null;

	System.Action<string> _doneCb;

	public void Show(string name, System.Action<string> doneCB)
	{
        _input.text = name;
		_doneCb = doneCB;
		gameObject.SetActive(true);
        _input.Select();
	}

	public void Apply()
	{
        string name = _input.text;
        if (!string.IsNullOrWhiteSpace(name))
        {
            _doneCb(name);
        }
		Close();
	}

	public void Close()
	{
		gameObject.SetActive(false);
	}

	// Use this for initialization
	void Start()
	{
	}
}
