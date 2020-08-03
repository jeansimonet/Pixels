using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple modal dialog box, where you can set the text and ok/cancel buttons
/// </sumary>
public class UIDialogBox : MonoBehaviour
{
    [Header("Controls")]
    public Button okButton;
    public Button cancelButton;
    public Text titleText;
    public Text messageText;
    public Text cancelText;
    public Text okText;

    public bool isShown => gameObject.activeSelf;

    System.Action<bool> closeAction;

    /// <summary>
    /// Invoke the modal dialog box, passing in all the parameters to configure it and a callback
    /// </sumary>
    public void Show(string title, string message, string okMessage, string cancelMessage, System.Action<bool> closeAction)
    {
        if (isShown)
        {
            Debug.LogWarning("Previous Message box still active");
            ForceHide();
        }

        gameObject.SetActive(true);
        if (string.IsNullOrEmpty(cancelMessage))
        {
            // No cancel button
            cancelButton.gameObject.SetActive(false);
        }
        else
        {
            cancelButton.gameObject.SetActive(true);
            cancelText.text = cancelMessage;
            cancelButton.onClick.AddListener(() =>
            {
                Hide(false);
            });
        }

        Debug.Assert(!string.IsNullOrEmpty(okMessage));
        okText.text = okMessage;
        okButton.onClick.AddListener(() =>
        {
            Hide(true);
        });

        this.closeAction = closeAction;
    }

    /// <summary>
    /// If for some reason the app needs to close the dialog box, this will do it!
    /// Normally it closes itself when you tap ok or cancel
    /// </sumary>
    public void ForceHide()
    {
        Hide(false);
    }

    void Hide(bool result)
    {
        gameObject.SetActive(false);
        closeAction?.Invoke(result);
        closeAction = null;
    }
}
