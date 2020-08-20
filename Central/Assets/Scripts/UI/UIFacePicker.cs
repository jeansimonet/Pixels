using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFacePicker : MonoBehaviour
{
    [Header("Controls")]
    public Button backButton;
    public Button saveButton;
    public Text titleText;
    public RectTransform contentRoot;

    [Header("Prefabs")]
    public UIFacePickerToken faceTokenPrefab;

    int previousFaceMask = -1;
    System.Action<bool, int> closeAction;

    // The list of controls we have created to display dice
    List<UIFacePickerToken> faces = new List<UIFacePickerToken>();

    public bool isShown => gameObject.activeSelf;

    /// <summary>
    /// Invoke the die picker
    /// </sumary>
    public void Show(string title, int previousFaceMask, System.Action<bool, int> closeAction)
    {
        if (isShown)
        {
            Debug.LogWarning("Previous Face picker still active");
            ForceHide();
        }

        for (int i = 0; i < 20; ++i)
        {
            // New pattern
            bool faceSelected = (previousFaceMask & (1 << i)) != 0;
            var newFaceUI = CreateFaceToken(i, faceSelected);
            faces.Add(newFaceUI);
        }

        gameObject.SetActive(true);
        this.previousFaceMask = previousFaceMask;
        titleText.text = title;


        this.closeAction = closeAction;
    }

    UIFacePickerToken CreateFaceToken(int faceIndex, bool selected)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIFacePickerToken>(faceTokenPrefab, contentRoot.transform);

        // Initialize it
        ret.Setup(faceIndex, selected);
        ret.onValueChanged.AddListener(UpdateButtons);
        return ret;
    }

    /// <summary>
    /// If for some reason the app needs to close the dialog box, this will do it!
    /// Normally it closes itself when you tap ok or cancel
    /// </sumary>
    public void ForceHide()
    {
        DiscardAndBack();
    }

    void Awake()
    {
        backButton.onClick.AddListener(DiscardAndBack);
        saveButton.onClick.AddListener(SaveAndBack);
    }

    void Hide(bool result, int faceMask)
    {
        foreach (var uiface in faces)
        {
            DestroyFaceToken(uiface);
        }
        faces.Clear();

        gameObject.SetActive(false);
        closeAction?.Invoke(result, faceMask);
        closeAction = null;
    }

    void UpdateButtons(bool newToggle)
    {
        saveButton.gameObject.SetActive(ComputeNewFaceMask() != previousFaceMask);
    }

    void DiscardAndBack()
    {
        Hide(false, previousFaceMask);
    }

    void SaveAndBack()
    {
        Hide(true, ComputeNewFaceMask());
    }

    int ComputeNewFaceMask()
    {
        int newFaceMask = 0;
        for (int i = 0; i < 20; ++i)
        {
            if (faces[i].isOn)
            {
                newFaceMask |= (1 << i);
            }
        }
        return newFaceMask;
    }

    void DestroyFaceToken(UIFacePickerToken token)
    {
        GameObject.Destroy(token.gameObject);
    }
}
