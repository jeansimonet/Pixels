using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Presets;

public class UIPresetsView
    : UIPage
{
    [Header("Controls")]
    public Transform contentRoot;
    public Button addPresetButton;
    public RectTransform spacer;

    [Header("Prefabs")]
    public UIPresetToken presetTokenPrefab;

    // The list of controls we have created to display presets
    List<UIPresetToken> presets = new List<UIPresetToken>();

    public override void Enter(object context)
    {
        base.Enter(context);
        if (AppSettings.Instance.presetsTutorialEnabled)
        {
            Tutorial.Instance.StartPresetsTutorial();
        }
    }

    void OnEnable()
    {
        base.SetupHeader(true, false, "Profiles", null);
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
        spacer.SetAsLastSibling();

        // When we click on the pattern main button, go to the edit page
        ret.onClick.AddListener(() => NavigationManager.Instance.GoToPage(UIPage.PageId.Preset, preset));
        ret.onEdit.AddListener(() => NavigationManager.Instance.GoToPage(UIPage.PageId.Preset, preset));
        ret.onDuplicate.AddListener(() => DuplicatePreset(preset));
        ret.onRemove.AddListener(() => DeletePreset(preset));
        ret.onExpand.AddListener(() => ExpandPreset(preset));

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

    void DuplicatePreset(EditPreset editPreset)
    {
        AppDataSet.Instance.DuplicatePreset(editPreset);
        presets.Find(p => p.editPreset == editPreset).Expand(false);
        AppDataSet.Instance.SaveData();
        RefreshView();
    }

    void DeletePreset(EditPreset editPreset)
    {
        PixelsApp.Instance.ShowDialogBox("Delete Preset?", "Are you sure you want to delete " + editPreset.name + "?", "Ok", "Cancel", res =>
        {
            if (res)
            {
                AppDataSet.Instance.DeletePreset(editPreset);
                AppDataSet.Instance.SaveData();
                RefreshView();
            }
        });
    }

    void ExpandPreset(EditPreset editPreset)
    {
        foreach (var uip in presets)
        {
            if (uip.editPreset == editPreset)
            {
                uip.Expand(!uip.isExpanded);
            }
            else
            {
                uip.Expand(false);
            }
        }
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
        NavigationManager.Instance.GoToPage(UIPage.PageId.Preset, newPreset);
    }

}
