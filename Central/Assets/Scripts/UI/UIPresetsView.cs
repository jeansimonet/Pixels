using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Presets;

public class UIPresetsView
    : PixelsApp.Page
{
    [Header("Controls")]
    public Transform contentRoot;
    public Button addPresetButton;
    public Button menuButton;

    [Header("Prefabs")]
    public UIPresetToken presetTokenPrefab;

    // The list of controls we have created to display presets
    List<UIPresetToken> presets = new List<UIPresetToken>();

    void OnEnable()
    {
        RefreshView();
    }

    void OnDisable()
    {
        if (AppDataSet.Instance != null) // When quiting the app, it may be null
        {
            foreach (var uipreset in presets)
            {
                DestroyPresetToken(uipreset);
            }
            presets.Clear();
        }
    }

    UIPresetToken CreatePresetToken(EditPreset preset)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIPresetToken>(presetTokenPrefab, Vector3.zero, Quaternion.identity, contentRoot.transform);

        // When we click on the pattern main button, go to the edit page
        ret.onClick.AddListener(() => NavigationManager.Instance.GoToPage(PixelsApp.PageId.Preset, preset));

        addPresetButton.transform.SetAsLastSibling();

        // Initialize it
        ret.Setup(preset);
        return ret;
    }

    void Awake()
    {
        addPresetButton.onClick.AddListener(AddNewPreset);
    }

    void DestroyPresetToken(UIPresetToken die)
    {
        GameObject.Destroy(die.gameObject);
    }

    void RefreshView()
    {
        List<UIPresetToken> toDestroy = new List<UIPresetToken>(presets);
        foreach (var preset in AppDataSet.Instance.presets)
        {
            int prevIndex = toDestroy.FindIndex(a => a.editPreset == preset);
            if (prevIndex == -1)
            {
                // New preset
                var newPresetUI = CreatePresetToken(preset);
                presets.Add(newPresetUI);
            }
            else
            {
                // Previous die is still advertising, good
                toDestroy.RemoveAt(prevIndex);
            }
        }

        // Remove all remaining dice
        foreach (var uipreset in toDestroy)
        {
            presets.Remove(uipreset);
            DestroyPresetToken(uipreset);
        }
    }

    void AddNewPreset()
    {
        // Create a new default preset
        var newPreset = AppDataSet.Instance.AddNewDefaultPreset();
        NavigationManager.Instance.GoToPage(PixelsApp.PageId.Preset, newPreset);
    }
}
