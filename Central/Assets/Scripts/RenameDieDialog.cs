using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenameDieDialog : MonoBehaviour
{
    public Central central;

    [Header("Fields")]
    public Button renameButton;
    public Button cancelButton;
    public InputField nameField;
    public CanvasGroup canvasGroup;

    Die selectedDice;

    // Use this for initialization
    private void Awake()
    {
        Hide();
    }

    void Start () {
		
	}

    public void Show(Die dieToRename)
    {
        selectedDice = dieToRename;

        canvasGroup.gameObject.SetActive(true);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;

        // Setup buttons
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() =>
        {
            Hide();
        });

        renameButton.onClick.RemoveAllListeners();
        renameButton.onClick.AddListener(() =>
        {
            if (nameField.text != null && nameField.text.Length > 0)
            {
                selectedDice.Rename(nameField.text);
                Hide();
            }
        });
    }

    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        renameButton.interactable = nameField.text != null && nameField.text.Length > 0;
    }
}
