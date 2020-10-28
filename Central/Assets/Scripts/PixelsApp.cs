using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Dice;
using Behaviors;

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

    public delegate void OnDieBehaviorUpdatedEvent(Dice.EditDie die, Behaviors.EditBehavior behavior);
    public OnDieBehaviorUpdatedEvent onDieBehaviorUpdatedEvent;

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

    public bool ShowEnumPicker(string title, System.Enum previousValue, System.Action<bool, System.Enum> closeAction, List<System.Enum> validValues)
    {
        bool ret = !enumPicker.isShown;
        if (ret)
        {
            enumPicker.Show(title, previousValue, closeAction, validValues);
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

    public void ActivateBehavior(Behaviors.EditBehavior behavior, System.Action<EditDie, bool> callback)
    {
        ShowDialogBox(
            "Activate " + behavior.name + "?",
            "Do you want to activate this profile on one of your dice?",
            "Yes",
            "Cancel",
            (res) =>
            {
                if (res)
                {
                    // Select the die
                    ShowDiePicker("Select Die", null, null, (res2, selectedDie) =>
                    {
                        if (res2)
                        {
                            // Attempt to activate the behavior on the die
                            UploadBehavior(behavior, selectedDie, (res3) =>
                            {
                                callback?.Invoke(selectedDie, res3);
                            });
                        }
                    });
                }
            });
    }

    public void UpdateDieDataSet(Presets.EditDieAssignment editDieAssignment, System.Action<bool> callback)
    {
        UpdateDieDataSet(editDieAssignment.behavior, editDieAssignment.die, callback);
    }

    public void UpdateDieDataSet(Behaviors.EditBehavior behavior, Dice.EditDie die, System.Action<bool> callback)
    {
        // Make sure the die is ready!
        ShowProgrammingBox("Connecting to " + die.name + "...");
        DiceManager.Instance.ConnectDie(die, (editDie, res, message) =>
        {
            if (res)
            {
                // The die is ready to be uploaded to

                // Generate the data to be uploaded
                EditDataSet editSet = new EditDataSet();

                // Grab the behavior
                editSet.behavior = behavior.Duplicate();

                // And add the animations that this behavior uses
                var animations = editSet.behavior.CollectAnimations();

                // Add default rules and animations to behavior / set
                if (AppDataSet.Instance.defaultBehavior != null)
                {
                    // Add animations used by default rules
                    foreach (var editAnim in AppDataSet.Instance.defaultBehavior.CollectAnimations())
                    {
                        animations.Add(editAnim);
                    }

                    foreach (var rule in AppDataSet.Instance.defaultBehavior.rules)
                    {
                        if (!editSet.behavior.rules.Any(r => r.condition.type == rule.condition.type))
                        {
                            editSet.behavior.rules.Add(rule.Duplicate());
                        }
                    }
                }

                editSet.animations.AddRange(animations);

                foreach (var pattern in AppDataSet.Instance.patterns)
                {
                    bool asRGB = false;
                    if (animations.Any(anim => anim.DependsOnPattern(pattern, out asRGB)))
                    {
                        if (asRGB)
                        {
                            editSet.rgbPatterns.Add(pattern);
                        }
                        else
                        {
                            editSet.patterns.Add(pattern);
                        }
                    }
                }

                // Set the behavior
                var dataSet = editSet.ToDataSet();

                // Check the dataset against the one stored in the die
                var hash = dataSet.ComputeHash();

                // Get the hash directly from the die
                editDie.die.GetDieInfo((info_res) =>
                {
                    if (info_res)
                    {
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
                            (res2, errorMsg) =>
                            {
                                if (res2)
                                {
                                    editDie.die.GetDieInfo(res3 =>
                                    {
                                        if (res3)
                                        {
                                            HideProgrammingBox();
                                            if (hash != editDie.die.dataSetHash)
                                            {
                                                ShowDialogBox("Error verifying data sent to " + editDie.name, message, "Ok", null, null);
                                                callback?.Invoke(false);
                                            }
                                            else
                                            {
                                                die.currentBehavior = behavior;
                                                AppDataSet.Instance.SaveData();
                                                onDieBehaviorUpdatedEvent?.Invoke(die, die.currentBehavior);
                                                callback?.Invoke(true);
                                            }
                                            DiceManager.Instance.DisconnectDie(editDie, null);
                                        }
                                        else
                                        {
                                            HideProgrammingBox();
                                            ShowDialogBox("Error fetching profile hash value from " + editDie.name, message, "Ok", null, null);
                                            DiceManager.Instance.DisconnectDie(editDie, null);
                                            callback?.Invoke(false);
                                        }
                                    });
                                }
                                else
                                {
                                    HideProgrammingBox();
                                    ShowDialogBox("Error uploading data to " + editDie.name, errorMsg, "Ok", null, null);
                                    DiceManager.Instance.DisconnectDie(editDie, null);
                                    callback?.Invoke(false);
                                }
                            });
                        }
                        else
                        {
                            Debug.Log("Die " + editDie.name + " already has preset with hash 0x" + hash.ToString("X8") + " programmed.");
                            HideProgrammingBox();
                            ShowDialogBox("Profile already Programmed", "Die " + editDie.name + " already has profile \"" + behavior.name + "\" programmed.", "Ok", null, null);
                            DiceManager.Instance.DisconnectDie(editDie, null);
                            callback?.Invoke(true);
                        }
                    }
                    else
                    {
                        HideProgrammingBox();
                        ShowDialogBox("Error verifying profile hash on " + editDie.name, message, "Ok", null, null);
                        DiceManager.Instance.DisconnectDie(editDie, null);
                        callback?.Invoke(false);
                    }
                });
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
                        // We're done!
                        callback?.Invoke(true);
                    }
                }
                else
                {
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
            callback(false);
        }
    }

    public void UploadBehavior(Behaviors.EditBehavior behavior, Dice.EditDie die, System.Action<bool> callback)
    {
        UpdateDieDataSet(behavior, die, (res) =>
        {
            if (res)
            {
                // We're done!
                callback?.Invoke(true);
            }
            else
            {
                callback?.Invoke(false);
            }
        });
    }

    public void RestartTutorial()
    {
        AppSettings.Instance.EnableAllTutorials();
        Tutorial.Instance.StartMainTutorial();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Pretend to have updated the current preset on load
        foreach (var die in AppDataSet.Instance.dice)
        {
            if (die.currentBehavior != null)
            {
                onDieBehaviorUpdatedEvent?.Invoke(die, die.currentBehavior);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
