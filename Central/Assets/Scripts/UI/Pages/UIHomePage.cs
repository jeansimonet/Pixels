using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Presets;
using System.Linq;
using Dice;

public class UIHomePage
    : UIPage
{
    [Header("Controls")]
    public Transform newsSection;
    public Transform contentRoot;
    public Button dismissMessagesButton;
    public Button editPresetsButton;

    [Header("Prefabs")]
    public UIHomePresetToken presetTokenPrefab;

    // The list of controls we have created to display presets
    List<UIHomePresetToken> presets = new List<UIHomePresetToken>();

    void OnEnable()
    {
        base.SetupHeader(true, true, "Pixels", null);
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
            //AppDataSet.Instance.OnChange -= OnDataSetChange;
        }
    }

    UIHomePresetToken CreatePresetToken(EditPreset preset)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIHomePresetToken>(presetTokenPrefab, Vector3.zero, Quaternion.identity, contentRoot.transform);

        // When we click on the pattern main button, go to the edit page
        ret.onClick.AddListener(() =>
        {
            ActivatePreset(preset);
        });

        // Initialize it
        ret.Setup(preset);
        return ret;
    }

    void Awake()
    {
        dismissMessagesButton.onClick.AddListener(CloseWhatsNew);
        editPresetsButton.onClick.AddListener(() => NavigationManager.Instance.GoToRoot(UIPage.PageId.Presets));
    }

    void DestroyPresetToken(UIHomePresetToken die)
    {
        GameObject.Destroy(die.gameObject);
    }

    void RefreshView()
    {
        newsSection.gameObject.SetActive(AppSettings.Instance.displayWhatsNew);
        List<UIHomePresetToken> toDestroy = new List<UIHomePresetToken>(presets);
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

        UpdatePresetsStatuses();
    }

    void AddNewPreset()
    {
        // // Create a new default preset
        // var newPreset = AppDataSet.Instance.AddNewDefaultPreset();
        // NavigationManager.Instance.GoToPage(PixelsApp.PageId.Preset, newPreset);
    }

    void ActivatePreset(Presets.EditPreset editPreset)
    {
        PixelsApp.Instance.ShowDialogBox(
            "Activate " + editPreset.name + "?",
            "Do you want to switch to this preset?",
            "Yes",
            "Cancel",
            (res) =>
            {
                if (res)
                {
                    // Attempt to activate the preset
                    PixelsApp.Instance.UploadPreset(editPreset, (res2) =>
                    {
                        if (res2)
                        {
                            UpdatePresetsStatuses();
                        }
                    });
                }
            });
    }

    void CloseWhatsNew()
    {
        newsSection.gameObject.SetActive(false);
        AppSettings.Instance.SetDisplayWhatsNew(false);
    }

    void UpdatePresetsStatuses()
    {
        foreach (var uipresetToken in presets)
        {
            if (AppDataSet.Instance.activePreset == uipresetToken.editPreset)
            {
                uipresetToken.SetState(UIHomePresetToken.State.Active);
            }
            else
            {
                uipresetToken.SetState(UIHomePresetToken.State.Inactive);
            }
        }
    }
}
