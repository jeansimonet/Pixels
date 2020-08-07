using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPatternsView
    : PixelsApp.Page
{
    [Header("Controls")]
    public Transform contentRoot;
    public Button addPatternButton;
    public Button menuButton;

    [Header("Prefabs")]
    public UIPatternToken patternTokenPrefab;

    // The list of controls we have created to display patterns
    List<UIPatternToken> patterns = new List<UIPatternToken>();

    void OnEnable()
    {
        RefreshView();
    }

    void OnDisable()
    {
        if (AppDataSet.Instance != null) // When quiting the app, it may be null
        {
            foreach (var uipattern in patterns)
            {
                DestroyPatternToken(uipattern);
            }
            patterns.Clear();
        }
    }

    UIPatternToken CreatePatternToken(Animations.EditAnimation anim)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIPatternToken>(patternTokenPrefab, Vector3.zero, Quaternion.identity, contentRoot.transform);

        // When we click on the pattern main button, go to the edit page
        ret.onClick.AddListener(() => NavigationManager.Instance.GoToPage(PixelsApp.PageId.Pattern, anim));

        addPatternButton.transform.SetAsLastSibling();
        // Initialize it
        ret.Setup(anim);
        return ret;
    }

    void DestroyPatternToken(UIPatternToken die)
    {
        GameObject.Destroy(die.gameObject);
    }

    void RefreshView()
    {
        // Assume all pool dice will be destroyed
        List<UIPatternToken> toDestroy = new List<UIPatternToken>(patterns);
        foreach (var anim in AppDataSet.Instance.animations)
        {
            int prevIndex = toDestroy.FindIndex(a => a.editAnimation == anim);
            if (prevIndex == -1)
            {
                // New pattern
                var newPatternUI = CreatePatternToken(anim);
                patterns.Add(newPatternUI);
            }
            else
            {
                // Previous die is still advertising, good
                toDestroy.RemoveAt(prevIndex);
            }
        }

        // Remove all remaining dice
        foreach (var uipattern in toDestroy)
        {
            patterns.Remove(uipattern);
            DestroyPatternToken(uipattern);
        }
    }

}
