using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Presets;
using System.Linq;
using Dice;

public class UIHomePage
    : PixelsApp.Page
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
        RefreshView();
        StartCoroutine(UpdatePresetStatusesCr());
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
        editPresetsButton.onClick.AddListener(() => NavigationManager.Instance.GoToRoot(PixelsApp.PageId.Presets));
    }

    IEnumerator UpdatePresetStatusesCr()
    {
        yield return new WaitUntil(() => Central.Instance.state == Central.State.Idle);
        UpdatePresetsStatus();
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
                        UpdatePresetsStatus();
                        // if (res2)
                        // {
                        //     foreach (var t in presets)
                        //     {
                        //         t.SetSelected(t.editPreset == editPreset);
                        //     }
                        // }
                    });
                }
            });
    }

    void CloseWhatsNew()
    {
        newsSection.gameObject.SetActive(false);
        AppSettings.Instance.SetDisplayWhatsNew(false);
    }

    class EditDieInfo
    {
        public Die die;
        public EditDataSet editDataSet;
        public DataSet dataSet;
        public bool upToDate;
    }

    void UpdatePresetsStatus()
    {
        foreach (var uipresetToken in presets)
        {
            uipresetToken.SetState(UIHomePresetToken.State.Unknown);
        }

        // Collect all the dice in all the presets
        var editDice = new HashSet<EditDie>();
        foreach (var editPreset in AppDataSet.Instance.presets)
        {
            foreach (var editDie in editPreset.dieAssignments.Select(da => da.die).Where(d => d != null))
            {
                editDice.Add(editDie);
            }
        }

        // Now we have all the dice, try to connect to get their dataset and and active behavior
        int testedDieCount = 0;
        foreach (var editDie in editDice)
        {
            var editDieInfos = new Dictionary<EditDie, EditDieInfo>();

            DiceManager.Instance.ConnectDie(editDie, (ed, die, errorMsg) =>
            {
                if (die != null)
                {
                    die.GetDieInfo((res2) =>
                    {
                        var editSet = AppDataSet.Instance.ExtractEditSetForDie(editDie);
                        var dataSet = editSet.ToDataSet();
                        editDieInfos[editDie] = new EditDieInfo()
                        {
                            die = die,
                            editDataSet = editSet,
                            dataSet = dataSet,
                            upToDate = die.connectionState == Die.ConnectionState.Ready && (die.dataSetHash == dataSet.ComputeHash()),
                        };
                        testedDieCount++;

                        if (testedDieCount == editDice.Count)
                        {
                            // We've tried to connect to all the dice, and either succeeded or not
                            // Now derive the state of each preset
                            foreach (var uip in presets)
                            {
                                var presetDice = uip.editPreset.dieAssignments.Select(da => da.die);
                                bool allPresetDiceReady = presetDice.All(ed2 => ed2 != null && editDieInfos[ed2] != null && editDieInfos[ed2].die.connectionState == Die.ConnectionState.Ready);
                                if (allPresetDiceReady)
                                {
                                    uip.SetState(UIHomePresetToken.State.Reachable);

                                    // Check the dataset
                                    bool allPresetDiceUpToDate = presetDice.All(ed2 => editDieInfos[ed2].upToDate);
                                    if (allPresetDiceUpToDate)
                                    {
                                        uip.SetState(UIHomePresetToken.State.UpToDate);

                                        // Check that the active behavior is correct too
                                        bool allBehaviorsActive = uip.editPreset.dieAssignments.All(da => editDieInfos[da.die].die.currentBehaviorIndex == editDieInfos[da.die].editDataSet.behaviors.IndexOf(da.behavior));
                                        if (allBehaviorsActive)
                                        {
                                            uip.SetState(UIHomePresetToken.State.Active);
                                        }
                                        // Else leave as uptodate
                                    }
                                    // Else leave as available
                                }
                                // Else leave as unknown
                            }

                            // Now that we're done we can disconnect all
                            foreach (var di in editDieInfos.Values)
                            {
                                if (di.die.connectionState == Die.ConnectionState.Ready)
                                {
                                    DiceManager.Instance.DisconnectDie(editDie);
                                }
                            }
                        }
                        // Else keep waiting

                    });
                }
            });
        }
    }
}
