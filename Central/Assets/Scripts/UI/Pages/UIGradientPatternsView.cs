using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Animations;
using System.Text;
using System.Linq;

public class UIGradientPatternsView
    : UIPage
{
    [Header("Controls")]
    public Transform contentRoot;
    public Button addPatternButton;

    [Header("Prefabs")]
    public UIGradientPatternViewToken patternTokenPrefab;

    // The list of controls we have created to display patterns
    List<UIGradientPatternViewToken> patterns = new List<UIGradientPatternViewToken>();

    void OnEnable()
    {
        base.SetupHeader(true, false, "LED Patterns", null);
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

    UIGradientPatternViewToken CreatePatternToken(Animations.EditPattern pattern)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIGradientPatternViewToken>(patternTokenPrefab, Vector3.zero, Quaternion.identity, contentRoot.transform);

        // When we click on the pattern main button, go to the edit page
        ret.onClick.AddListener(() => 
        {
            ret.Expand(false);
            PixelsApp.Instance.ShowPatternEditor(pattern.name, pattern, (r, p) => SetPattern(r, pattern, p));
        });
        ret.onEdit.AddListener(() => 
        {
            ret.Expand(false);
            PixelsApp.Instance.ShowPatternEditor(pattern.name, pattern, (r, p) => SetPattern(r, pattern, p));
        });
        ret.onRemove.AddListener(() => DeletePattern(pattern));
        ret.onExpand.AddListener(() => ExpandPattern(pattern));

        addPatternButton.transform.SetAsLastSibling();
        // Initialize it
        ret.Setup(pattern);
        return ret;
    }

    void Awake()
    {
        addPatternButton.onClick.AddListener(AddNewPattern);
    }

    void DestroyPatternToken(UIGradientPatternViewToken die)
    {
        GameObject.Destroy(die.gameObject);
    }

    void RefreshView()
    {
        // Assume all pool dice will be destroyed
        List<UIGradientPatternViewToken> toDestroy = new List<UIGradientPatternViewToken>(patterns);
        foreach (var pattern in AppDataSet.Instance.patterns)
        {
            int prevIndex = toDestroy.FindIndex(a => a.editPattern == pattern);
            if (prevIndex == -1)
            {
                // New pattern
                var newPatternUI = CreatePatternToken(pattern);
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

    void AddNewPattern()
    {
        // Create a new default animation
        var newPattern = AppDataSet.Instance.AddNewDefaultPattern();
        AppDataSet.Instance.SaveData();
        PixelsApp.Instance.ShowPatternEditor(newPattern.name, newPattern, (r, p) => SetPattern(r, newPattern, p));
    }

    void DeletePattern(Animations.EditPattern pattern)
    {
        PixelsApp.Instance.ShowDialogBox("Delete LED Pattern?", "Are you sure you want to delete " + pattern.name + "?", "Ok", "Cancel", res =>
        {
            if (res)
            {
                var dependentAnimations = AppDataSet.Instance.CollectAnimationsForPattern(pattern);
                if (dependentAnimations.Any())
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("The following animations depend on ");
                    builder.Append(pattern.name);
                    builder.AppendLine(":");
                    foreach (var b in dependentAnimations)
                    {
                        builder.Append("\t");
                        builder.AppendLine(b.name);
                    }
                    builder.Append("Are you sure you want to delete it?");

                    PixelsApp.Instance.ShowDialogBox("Pattern In Use!", builder.ToString(), "Ok", "Cancel", res2 =>
                    {
                        if (res2)
                        {
                            AppDataSet.Instance.DeletePattern(pattern);
                            AppDataSet.Instance.SaveData();
                            RefreshView();
                        }
                    });
                }
                else
                {
                    AppDataSet.Instance.DeletePattern(pattern);
                    AppDataSet.Instance.SaveData();
                    RefreshView();
                }
            }
        });
    }

    void ExpandPattern(Animations.EditPattern pattern)
    {
        foreach (var uip in patterns)
        {
            if (uip.editPattern == pattern)
            {
                uip.Expand(!uip.isExpanded);
            }
            else
            {
                uip.Expand(false);
            }
        }
    }

    void SetPattern(bool res, EditPattern previousPattern, EditPattern newPattern)
    {
        AppDataSet.Instance.ReplacePattern(previousPattern, newPattern);
    }
}
