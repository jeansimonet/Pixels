using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandButton
	: MonoBehaviour
{
	public Text commandText;

	public void Setup(string text, System.Action callBack)
	{
		commandText.text = text;
		GetComponent<Button>().onClick.RemoveAllListeners();
		GetComponent<Button>().onClick.AddListener(() => callBack());
	}
}
