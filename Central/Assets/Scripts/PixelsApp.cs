using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelsApp : SingletonMonoBehaviour<PixelsApp>
{
    [Header("Panels")]
    public UIDialogBox dialogBox;
    public UIColorPicker colorPicker;
    public UIAnimationPicker animationPicker;
    public UIDiePicker diePicker;
    public UIBehaviorPicker behaviorPicker;
    public UIFacePicker facePicker;
    public UIGradientEditor gradientEditor;
    public UIEnumPicker enumPicker;
    public UIProgrammingBox programmingBox;
    public UIPatternEditor patternEditor;
    public UIPatternPicker patternPicker;
    public UIAudioClipPicker audioClipPicker;

    public delegate void OnPresetDownloadEvent(Presets.EditPreset activePreset);
    public OnPresetDownloadEvent onPresetDownloadEvent;

    [Header("Controls")]
    public UIMainMenu mainMenu;

    public void ShowMainMenu()
    {
        mainMenu.Show();
    }

    public void HideMainMenu()
    {
        mainMenu.Hide();
    }

    public bool ShowDialogBox(string title, string message, string okMessage, string cancelMessage, System.Action<bool> closeAction)
    {
        bool ret = !dialogBox.isShown;
        if (ret)
        {
            dialogBox.Show(title, message, okMessage, cancelMessage, closeAction);
        }
        return ret;
    }

    public bool ShowColorPicker(string title, Color previousColor, System.Action<bool, Color> closeAction)
    {
        bool ret = !colorPicker.isShown;
        if (ret)
        {
            colorPicker.Show(title, previousColor, closeAction);
        }
        return ret;
    }

    public bool ShowAnimationPicker(string title, Animations.EditAnimation previousAnimation, System.Action<bool, Animations.EditAnimation> closeAction)
    {
        bool ret = !animationPicker.isShown;
        if (ret)
        {
            animationPicker.Show(title, previousAnimation, closeAction);
        }
        return ret;
    }

    public bool ShowDiePicker(string title, Dice.EditDie previousDie, System.Func<Dice.EditDie, bool> selector, System.Action<bool, Dice.EditDie> closeAction)
    {
        bool ret = !diePicker.isShown;
        if (ret)
        {
            diePicker.Show(title, previousDie, selector, closeAction);
        }
        return ret;
    }

    public bool ShowBehaviorPicker(string title, Behaviors.EditBehavior previousBehavior, System.Action<bool, Behaviors.EditBehavior> closeAction)
    {
        bool ret = !behaviorPicker.isShown;
        if (ret)
        {
            behaviorPicker.Show(title, previousBehavior, closeAction);
        }
        return ret;
    }

    public bool ShowFacePicker(string title, int previousFaceMask, System.Action<bool, int> closeAction)
    {
        bool ret = !facePicker.isShown;
        if (ret)
        {
            facePicker.Show(title, previousFaceMask, closeAction);
        }
        return ret;
    }

    public bool ShowGradientEditor(string title, Animations.EditRGBGradient previousGradient, System.Action<bool, Animations.EditRGBGradient> closeAction)
    {
        bool ret = !gradientEditor.isShown;
        if (ret)
        {
            gradientEditor.Show(title, previousGradient, closeAction);
        }
        return ret;
    }

    public bool ShowEnumPicker(string title, System.Enum previousValue, System.Action<bool, System.Enum> closeAction, int min, int max)
    {
        bool ret = !enumPicker.isShown;
        if (ret)
        {
            enumPicker.Show(title, previousValue, closeAction, min, max);
        }
        return ret;
    }

    public bool ShowPatternEditor(string title, Animations.EditPattern previousPattern, System.Action<bool, Animations.EditPattern> closeAction)
    {
        bool ret = !patternEditor.isShown;
        if (ret)
        {
            patternEditor.Show(title, previousPattern, closeAction);
        }
        return ret;
    }

    public bool ShowPatternPicker(string title, Animations.EditPattern previousPattern, System.Action<bool, Animations.EditPattern> closeAction)
    {
        bool ret = !patternPicker.isShown;
        if (ret)
        {
            patternPicker.Show(title, previousPattern, closeAction);
        }
        return ret;
    }

    public bool ShowAudioClipPicker(string title, AudioClipManager.AudioClipInfo previousClip, System.Action<bool, AudioClipManager.AudioClipInfo> closeAction)
    {
        bool ret = !audioClipPicker.isShown;
        if (ret)
        {
            audioClipPicker.Show(title, previousClip, closeAction);
        }
        return ret;
    }

    public bool ShowProgrammingBox(string description)
    {
        bool ret = !programmingBox.isShown;
        if (ret)
        {
            programmingBox.Show(description);
        }
        return ret;
    }

    public bool UpdateProgrammingBox(float percent, string description = null)
    {
        bool ret = programmingBox.isShown;
        if (ret)
        {
            programmingBox.SetProgress(percent, description);
        }
        return ret;
    }

    public bool HideProgrammingBox()
    {
        bool ret = programmingBox.isShown;
        if (ret)
        {
            programmingBox.Hide();
        }
        return ret;
    }

    public void UpdateDieDataSet(Presets.EditDieAssignment editDieAssignment, System.Action<bool> callback)
    {
        // Make sure the die is ready!
        ShowProgrammingBox("Connecting to " + editDieAssignment.die.name + "...");
        DiceManager.Instance.ConnectDie(editDieAssignment.die, (editDie, res, message) =>
        {
            if (res)
            {
                // The die is ready to be uploaded to

                // Generate the data to be uploaded
                var editSet = AppDataSet.Instance.ExtractEditSetForDie(editDieAssignment.die);

                // Set the behavior
                var dataSet = editSet.ToDataSet();

                void checkAndActivateSet(System.Action<bool> checkActivateCallback)
                {
                    // Still need to check current behavior index
                    int currentBehaviorIndex = editSet.behaviors.IndexOf(editDieAssignment.behavior);
                    if (currentBehaviorIndex != editDie.die.currentBehaviorIndex)
                    {
                        Debug.Log("Setting active behavior on " + editDie.name + " to " + currentBehaviorIndex);
                        UpdateProgrammingBox(1.0f, "Activating behavior " + editDieAssignment.behavior.name + " on " + editDie.name);
                        editDie.die.SetCurrentBehavior(currentBehaviorIndex, checkActivateCallback);
                    }
                    else
                    {
                        Debug.Log("Die " + editDie.name + " already has behavior index " + currentBehaviorIndex + " active.");
                        checkActivateCallback?.Invoke(true);
                    }
                }

                // Check the dataset against the one stored in the die
                var hash = dataSet.ComputeHash();
                if (hash != editDie.die.dataSetHash)
                {
                    // We need to upload the dataset first
                    Debug.Log("Uploading dataset to die " + editDie.name);
                    UpdateProgrammingBox(0.0f, "Uploading data to " + editDie.name + "...");
                    editDie.die.UploadDataSet(dataSet,
                    (pct) =>
                    {
                        UpdateProgrammingBox(pct, "Uploading data to " + editDie.name + "...");
                    },
                    (res2) =>
                    {
                        if (res2)
                        {
                            checkAndActivateSet(res3 =>
                            {
                                if (res3)
                                {
                                    HideProgrammingBox();
                                    DiceManager.Instance.DisconnectDie(editDie);
                                    callback(true);
                                }
                                else
                                {
                                    HideProgrammingBox();
                                    ShowDialogBox("Error activating behavior on " + editDie.name, message, "Ok", null, null);
                                    DiceManager.Instance.DisconnectDie(editDie);
                                    callback(false);
                                }
                            });
                        }
                        else
                        {
                            HideProgrammingBox();
                            ShowDialogBox("Error uploading data to " + editDie.name, message, "Ok", null, null);
                            DiceManager.Instance.DisconnectDie(editDie);
                            callback(false);
                        }
                    });
                }
                else
                {
                    Debug.Log("Die " + editDie.name + " already has preset with hash 0x" + hash.ToString("X8") + " programmed.");
                    checkAndActivateSet(res3 =>
                    {
                        if (res3)
                        {
                            HideProgrammingBox();
                            DiceManager.Instance.DisconnectDie(editDie);
                            callback(true);
                        }
                        else
                        {
                            HideProgrammingBox();
                            ShowDialogBox("Error activating behavior on " + editDie.name, message, "Ok", null, null);
                            DiceManager.Instance.DisconnectDie(editDie);
                            callback(false);
                        }
                    });
                }
            }
            else
            {
                HideProgrammingBox();
                ShowDialogBox("Error connecting to " + editDie.name, message, "Ok", null, null);
                callback(false);
            }
        });
    }

    public void UploadPreset(Presets.EditPreset editPreset, System.Action<bool> callback)
    {
        int currentAssignment = 0;
        void updateNextDie()
        {
            UpdateDieDataSet(editPreset.dieAssignments[currentAssignment], (res) =>
            {
                if (res)
                {
                    currentAssignment++;
                    if (currentAssignment < editPreset.dieAssignments.Count)
                    {
                        updateNextDie();
                    }
                    else
                    {
                        AppDataSet.Instance.activePreset = editPreset;
                        // We're done!
                        onPresetDownloadEvent?.Invoke(AppDataSet.Instance.activePreset);
                        callback?.Invoke(true);
                    }
                }
                else
                {
                    AppDataSet.Instance.activePreset = null;
                    onPresetDownloadEvent?.Invoke(AppDataSet.Instance.activePreset);
                    callback?.Invoke(false);
                }
            });
        }

        // Kick off the upload chain
        if (editPreset.dieAssignments.Count > 0)
        {
            updateNextDie();
        }
        else
        {
            AppDataSet.Instance.activePreset = null;
            onPresetDownloadEvent?.Invoke(AppDataSet.Instance.activePreset);
            callback(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Pretend to have updated the current preset on load
        onPresetDownloadEvent?.Invoke(AppDataSet.Instance.activePreset);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
