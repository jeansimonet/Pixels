using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Dice;

public class UIPresetView : UIPage
{
    [Header("Controls")]
    public RawImage previewImage;
    public RectTransform assignmentsRoot;
    public Button addAssignmentButton;
    public InputField presetDescriptionText;

    [Header("Prefabs")]
    public UIAssignmentToken assignmentTokenPrefab;

    public MultiDiceRenderer dieRenderer { get; private set; }
    public Presets.EditPreset editPreset { get; private set; }
    List<UIAssignmentToken> assignments = new List<UIAssignmentToken>();

    public override void Enter(object context)
    {
        base.Enter(context);
        var preset = context as Presets.EditPreset;
        if (preset != null)
        {
            Setup(preset);
        }

        if (AppSettings.Instance.presetTutorialEnabled)
        {
            Tutorial.Instance.StartPresetTutorial();
        }
    }

    void OnDisable()
    {
        if (DiceRendererManager.Instance != null && this.dieRenderer != null)
        {
            DiceRendererManager.Instance.DestroyDiceRenderer(this.dieRenderer);
            this.dieRenderer = null;
        }

        foreach (var uiass in assignments)
        {
            DestroyAssignmentToken(uiass);
        }
        assignments.Clear();
    }

    void Setup(Presets.EditPreset preset)
    {
        base.SetupHeader(false, false, preset.name, SetName);
        editPreset = preset;
        presetDescriptionText.text = editPreset.description;
        var designs = new List<DesignAndColor>(preset.dieAssignments.Select(ass =>
        {
            return (ass.die != null) ? ass.die.designAndColor : DesignAndColor.Unknown;
        }));

        this.dieRenderer = DiceRendererManager.Instance.CreateMultiDiceRenderer(designs, 400);
        if (dieRenderer != null)
        {
            previewImage.texture = dieRenderer.renderTexture;
            for (int i = 0; i < preset.dieAssignments.Count; ++i)
            {
                if (preset.dieAssignments[i].behavior != null)
                {
                    dieRenderer.SetDieAnimations(i, preset.dieAssignments[i].behavior.CollectAnimations().Where(anim => anim != null));
                    dieRenderer.Play(i, false);
                }
            }
        }
        dieRenderer.rotating = true;
        RefreshView();
    }

    void Awake()
    {
        addAssignmentButton.onClick.AddListener(AddNewAssignment);
        presetDescriptionText.onEndEdit.AddListener(SetDescription);
    }

    void DiscardAndGoBack()
    {
        if (pageDirty)
        {
            PixelsApp.Instance.ShowDialogBox(
                "Discard Changes",
                "You have unsaved changes, are you sure you want to discard them?",
                "Discard",
                "Cancel", discard => 
                {
                    if (discard)
                    {
                        // Reload from file
                        AppDataSet.Instance.LoadData();
                        NavigationManager.Instance.GoBack();
                    }
                });
        }
        else
        {
            NavigationManager.Instance.GoBack();
        }
    }

    void SaveAndGoBack()
    {
        AppDataSet.Instance.SaveData();
        NavigationManager.Instance.GoBack();
    }

    void AddNewAssignment()
    {
        editPreset.dieAssignments.Add(new Presets.EditDieAssignment()
        {
            die = null,
            behavior = null
        });
        base.pageDirty = true;
        RefreshView();
    }

    void RefreshView()
    {
        List<UIAssignmentToken> toDestroy = new List<UIAssignmentToken>(assignments);
        foreach (var dass in editPreset.dieAssignments)
        {
            int prevIndex = toDestroy.FindIndex(a => a.editAssignment == dass);
            if (prevIndex == -1)
            {
                // New preset
                var newassui = CreateAssignmentToken(dass);
                assignments.Add(newassui);
            }
            else
            {
                // Previous die is still advertising, good
                toDestroy.RemoveAt(prevIndex);
            }
        }

        // Remove all remaining dice
        foreach (var uiass in toDestroy)
        {
            assignments.Remove(uiass);
            DestroyAssignmentToken(uiass);
        }

        addAssignmentButton.transform.SetAsLastSibling();
    }

    UIAssignmentToken CreateAssignmentToken(Presets.EditDieAssignment assignment)
    {
        var uiass = GameObject.Instantiate<UIAssignmentToken>(assignmentTokenPrefab, assignmentsRoot);
        uiass.Setup(editPreset, assignment, (ed) => !editPreset.dieAssignments.Where(ass => ass != assignment).Any(ass => ass.die.deviceId == ed.deviceId));
        uiass.onChange += OnAssignmentChanged;
        uiass.onDelete.AddListener(() => DeleteAssignment(assignment));
        return uiass;
    }

    void DestroyAssignmentToken(UIAssignmentToken uiass)
    {
        GameObject.Destroy(uiass.gameObject);
    }

    void DeleteAssignment(Presets.EditDieAssignment assignment)
    {
        PixelsApp.Instance.ShowDialogBox(
            "Delete Assignment?",
            "Are you sure you want to delete this assignment?",
            "Yes",
            "Cancel",
            (res) =>
            {
                base.pageDirty = true;
                editPreset.dieAssignments.Remove(assignment);
                RefreshView();
            });
    }

    void OnAssignmentChanged(Presets.EditDieAssignment editAssignment)
    {
        base.pageDirty = true;
        RefreshView();
    }

    void SetName(string newName)
    {
        editPreset.name = newName;
        base.pageDirty = true;
    }

    void SetDescription(string newDescription)
    {
        editPreset.description = newDescription;
        base.pageDirty = true;
    }
}
