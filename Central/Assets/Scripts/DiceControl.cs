using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceControl
	: MonoBehaviour
{
	public GameObject buttonsRoot;
	public UIDie dieUI;
	public Button backButton;
	public CommandButton commandButtonPrefab;

	public void Setup(Die die, System.Action backButtonAction)
	{
		dieUI.die = die;
		backButton.onClick.RemoveAllListeners();
		backButton.onClick.AddListener(() => backButtonAction());

		// Clean any existing child
		for (int i = 0; i < buttonsRoot.transform.childCount; ++i)
		{
			GameObject.Destroy(buttonsRoot.transform.GetChild(i).gameObject);
		}
	}

	public void SetFaceNumber(int number)
	{
		dieUI.faceNumber = number;
	}

	public void AddCommand(string text, System.Action command)
	{
		var commandUI = GameObject.Instantiate<CommandButton>(commandButtonPrefab);
		commandUI.Setup(text, command);
		commandUI.transform.SetParent(buttonsRoot.transform);
	}
}
