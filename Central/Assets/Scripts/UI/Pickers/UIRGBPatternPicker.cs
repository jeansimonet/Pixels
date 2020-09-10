using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Animations;
using System.IO;

public class UIRGBPatternPicker : MonoBehaviour
{
    [Header("Controls")]
    public Button backButton;
    public Text titleText;
    public RectTransform contentRoot;
    public Button addPatternButton;

    [Header("Prefabs")]
    public UIRGBPatternPickerToken patternTokenPrefab;

    EditRGBPattern currentPattern;
    System.Action<bool, EditRGBPattern> closeAction;

    // The list of controls we have created to display patterns
    List<UIRGBPatternPickerToken> patterns = new List<UIRGBPatternPickerToken>();

    public bool isShown => gameObject.activeSelf;

    /// <summary>
    /// Invoke the color picker
    /// </sumary>
    public void Show(string title, EditRGBPattern previousPattern, System.Action<bool, EditRGBPattern> closeAction)
    {
        if (isShown)
        {
            Debug.LogWarning("Previous Pattern picker still active");
            ForceHide();
        }

        foreach (var editPattern in AppDataSet.Instance.rgbPatterns)
        {
            // New pattern
            var newPatternUI = CreatePatternToken(editPattern);
            patterns.Add(newPatternUI);
        }

        gameObject.SetActive(true);
        currentPattern = previousPattern;
        titleText.text = title;


        this.closeAction = closeAction;
    }

    UIRGBPatternPickerToken CreatePatternToken(EditRGBPattern pattern)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIRGBPatternPickerToken>(patternTokenPrefab, Vector3.zero, Quaternion.identity, contentRoot.transform);

        // When we click on the pattern main button, go to the edit page
        ret.onClick.AddListener(() => Hide(true, ret.editPattern));

        addPatternButton.transform.SetAsLastSibling();
        // Initialize it
        ret.Setup(pattern);
        return ret;
    }

    /// <summary>
    /// If for some reason the app needs to close the dialog box, this will do it!
    /// Normally it closes itself when you tap ok or cancel
    /// </sumary>
    public void ForceHide()
    {
        Hide(false, currentPattern);
    }

    void Awake()
    {
        backButton.onClick.AddListener(Back);
        addPatternButton.onClick.AddListener(AddNewPattern);
    }

    void Hide(bool result, EditRGBPattern pattern)
    {
        foreach (var uipattern in patterns)
        {
            DestroyPatternToken(uipattern);
        }
        patterns.Clear();

        gameObject.SetActive(false);
        closeAction?.Invoke(result, pattern);
        closeAction = null;
    }

    void Back()
    {
        Hide(false, currentPattern);
    }

    void DestroyPatternToken(UIRGBPatternPickerToken token)
    {
        GameObject.Destroy(token.gameObject);
    }

    void AddNewPattern()
    {
        currentPattern = AppDataSet.Instance.AddNewDefaultRGBPattern();
        PixelsApp.Instance.ShowRGBPatternEditor(currentPattern.name, currentPattern, (res, newPattern) => Hide(res, newPattern));
    }
}
