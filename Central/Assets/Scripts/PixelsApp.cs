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
    public UIPatternEditor patternEditor;
    public UIProgrammingBox programmingBox;

    public enum PageId
    {
        Home,
        DicePool,
        DicePoolScanning,
        Patterns,
        Pattern,
        Presets,
        Preset,
        Behaviors,
        Behavior,
        Rule,
        LiveView
    }

    public class Page
        : MonoBehaviour
    {
        public virtual void Enter(object context)
        {
            gameObject.SetActive(true);
        }
        public virtual void Leave()
        {
            gameObject.SetActive(false);
        }
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

    public bool ShowDiePicker(string title, Dice.EditDie previousDie, System.Func<Dice.EditDie, Dice.Die, bool> selector, System.Action<bool, Dice.EditDie> closeAction)
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
        DiceManager.Instance.ConnectDie(editDieAssignment.die, (ed, die, message) =>
        {
            if (die != null)
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
                    if (currentBehaviorIndex != die.currentBehaviorIndex)
                    {
                        Debug.Log("Setting active behavior on " + die.name + " to " + currentBehaviorIndex);
                        UpdateProgrammingBox(1.0f, "Activating behavior " + editDieAssignment.behavior.name + " on " + die.name);
                        die.SetCurrentBehavior(currentBehaviorIndex, checkActivateCallback);
                    }
                    else
                    {
                        Debug.Log("Die " + die.name + " already has behavior index " + currentBehaviorIndex + " active.");
                        checkActivateCallback?.Invoke(true);
                    }
                }

                // Check the dataset against the one stored in the die
                var hash = dataSet.ComputeHash();
                if (hash != die.dataSetHash)
                {
                    // We need to upload the dataset first
                    Debug.Log("Uploading dataset to die " + die.name);
                    UpdateProgrammingBox(0.0f, "Uploading data to " + die.name + "...");
                    die.UploadDataSet(dataSet,
                    (pct) =>
                    {
                        UpdateProgrammingBox(pct, "Uploading data to " + die.name + "...");
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
                                    DicePool.Instance.DisconnectDie(die);
                                    callback(true);
                                }
                                else
                                {
                                    HideProgrammingBox();
                                    ShowDialogBox("Error activating behavior on " + die.name, message, "Ok", null, null);
                                    DicePool.Instance.DisconnectDie(die);
                                    callback(false);
                                }
                            });
                        }
                        else
                        {
                            HideProgrammingBox();
                            ShowDialogBox("Error uploading data to " + die.name, message, "Ok", null, null);
                            DicePool.Instance.DisconnectDie(die);
                            callback(false);
                        }
                    });
                }
                else
                {
                    Debug.Log("Die " + die.name + " already has preset with hash 0x" + hash.ToString("X8") + " programmed.");
                    checkAndActivateSet(res3 =>
                    {
                        if (res3)
                        {
                            HideProgrammingBox();
                            DicePool.Instance.DisconnectDie(die);
                            callback(true);
                        }
                        else
                        {
                            HideProgrammingBox();
                            ShowDialogBox("Error activating behavior on " + die.name, message, "Ok", null, null);
                            DicePool.Instance.DisconnectDie(die);
                            callback(false);
                        }
                    });
                }
            }
            else
            {
                HideProgrammingBox();
                ShowDialogBox("Error connecting to " + die.name, message, "Ok", null, null);
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
                    callback(false);
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
