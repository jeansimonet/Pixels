using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Wheel item, used for populating the InfiniWheel
/// </summary>
public class WheelItem : MonoBehaviour {

	public Text ItemText;
	public int ItemIndex {get; set;}

	public virtual void Init(string text, int index)
	{
		if (ItemText != null)
			ItemText.text = text;
		ItemIndex = index;
	}
}
