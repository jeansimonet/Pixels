using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class AnimInfoText : MonoBehaviour
{
	[SerializeField]
	TimelineView _timeline = null;
	Text _text;

	// Use this for initialization
	void Start()
	{
		_text = GetComponent<Text>();
	}

	// Update is called once per frame
	void Update()
	{
		_text.text = _timeline.Duration + " second(s)\n"
			+ _timeline.AnimationCount + " animation(s)";
	}
}
