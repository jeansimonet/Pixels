using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPageHeader : MonoBehaviour
{
    [Header("Controls")]
    public Button menuButton;
    public Button backButton;
    public Button saveButton;
    public Image logo;
    public InputField titleField;

    public Button.ButtonClickedEvent onMenuClicked => menuButton.onClick;
    public Button.ButtonClickedEvent onBackClicked => backButton.onClick;
    public Button.ButtonClickedEvent onSaveClicked => saveButton.onClick;

    void Awake()
    {
        menuButton.onClick.AddListener(() => PixelsApp.Instance.ShowMainMenu());
    }

    public void Setup(bool root, bool home, bool dirty, string title, System.Action<string> onTitleChanged)
    {
        if (root)
        {
            menuButton.gameObject.SetActive(true);
            backButton.gameObject.SetActive(false);
        }
        else
        {
            menuButton.gameObject.SetActive(false);
            backButton.gameObject.SetActive(true);
        }
        if (home)
        {
            logo.gameObject.SetActive(true);
            titleField.gameObject.SetActive(false);
        }
        else
        {
            logo.gameObject.SetActive(false);
            titleField.gameObject.SetActive(true);
            titleField.onValueChanged.RemoveAllListeners();
            titleField.text = title;
            if (onTitleChanged != null)
            {
                titleField.interactable = true;
                titleField.onValueChanged.AddListener(t => onTitleChanged.Invoke(t));
            }
            else
            {
                titleField.interactable = false;
            }
        }
        saveButton.gameObject.SetActive(dirty);
    }

    public void EnableSaveButton(bool enable)
    {
        saveButton.gameObject.SetActive(enable);
    }
}
