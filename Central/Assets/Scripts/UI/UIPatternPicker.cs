using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Animations;
using System.IO;

public class UIPatternPicker : MonoBehaviour
{
    [Header("Controls")]
    public Button backButton;
    public Text titleText;
    public RectTransform contentRoot;
    public Button addPatternButton;

    [Header("Prefabs")]
    public UIPatternPickerToken patternTokenPrefab;

    EditPattern currentPattern;
    System.Action<bool, EditPattern> closeAction;

    // The list of controls we have created to display patterns
    List<UIPatternPickerToken> patterns = new List<UIPatternPickerToken>();

    public bool isShown => gameObject.activeSelf;

    /// <summary>
    /// Invoke the color picker
    /// </sumary>
    public void Show(string title, EditPattern previousPattern, System.Action<bool, EditPattern> closeAction)
    {
        if (isShown)
        {
            Debug.LogWarning("Previous Pattern picker still active");
            ForceHide();
        }

        foreach (var editPattern in AppDataSet.Instance.patterns)
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

    UIPatternPickerToken CreatePatternToken(EditPattern pattern)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIPatternPickerToken>(patternTokenPrefab, Vector3.zero, Quaternion.identity, contentRoot.transform);

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

    void Hide(bool result, EditPattern pattern)
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

    void DestroyPatternToken(UIPatternPickerToken token)
    {
        GameObject.Destroy(token.gameObject);
    }

    void AddNewPattern()
    {
        currentPattern = AppDataSet.Instance.AddNewDefaultPattern();
        PixelsApp.Instance.ShowPatternEditor(currentPattern.name, currentPattern, (res, newPattern) => Hide(res, newPattern));
    }
}
