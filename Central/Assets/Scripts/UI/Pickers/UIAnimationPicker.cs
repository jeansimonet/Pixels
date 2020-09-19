using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Animations;

public class UIAnimationPicker : MonoBehaviour
{
    [Header("Controls")]
    public Button backButton;
    public Text titleText;
    public RectTransform contentRoot;
    public Button addPatternButton;

    [Header("Prefabs")]
    public UIAnimationSelectorPatternToken patternTokenPrefab;

    EditAnimation currentAnimation;
    System.Action<bool, EditAnimation> closeAction;

    // The list of controls we have created to display patterns
    List<UIAnimationSelectorPatternToken> patterns = new List<UIAnimationSelectorPatternToken>();

    public bool isShown => gameObject.activeSelf;

    /// <summary>
    /// Invoke the color picker
    /// </sumary>
    public void Show(string title, EditAnimation previousAnimation, System.Action<bool, EditAnimation> closeAction)
    {
        if (isShown)
        {
            Debug.LogWarning("Previous Animation picker still active");
            ForceHide();
        }

        foreach (var editAnim in AppDataSet.Instance.animations)
        {
            // New pattern
            var newPatternUI = CreatePatternToken(editAnim);
            newPatternUI.SetSelected(editAnim == previousAnimation);
            patterns.Add(newPatternUI);
        }

        gameObject.SetActive(true);
        currentAnimation = previousAnimation;
        titleText.text = title;


        this.closeAction = closeAction;
    }

    UIAnimationSelectorPatternToken CreatePatternToken(Animations.EditAnimation anim)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIAnimationSelectorPatternToken>(patternTokenPrefab, Vector3.zero, Quaternion.identity, contentRoot.transform);

        // When we click on the pattern main button, go to the edit page
        ret.onClick.AddListener(() => Hide(true, ret.editAnimation));

        addPatternButton.transform.SetAsLastSibling();
        // Initialize it
        ret.Setup(anim);
        return ret;
    }

    /// <summary>
    /// If for some reason the app needs to close the dialog box, this will do it!
    /// Normally it closes itself when you tap ok or cancel
    /// </sumary>
    public void ForceHide()
    {
        Hide(false, currentAnimation);
    }

    void Awake()
    {
        backButton.onClick.AddListener(Back);
        addPatternButton.onClick.AddListener(AddNewPattern);
    }

    void Hide(bool result, EditAnimation animation)
    {
        foreach (var uipattern in patterns)
        {
            DestroyPatternToken(uipattern);
        }
        patterns.Clear();

        gameObject.SetActive(false);
        closeAction?.Invoke(result, animation);
        closeAction = null;
    }

    void Back()
    {
        Hide(false, currentAnimation);
    }

    void DestroyPatternToken(UIAnimationSelectorPatternToken token)
    {
        GameObject.Destroy(token.gameObject);
    }

    void AddNewPattern()
    {
        // Create a new default animation
        var newAnim = AppDataSet.Instance.AddNewDefaultAnimation();
        Hide(true, newAnim);
        NavigationManager.Instance.GoToPage(UIPage.PageId.Pattern, newAnim);
    }
}
