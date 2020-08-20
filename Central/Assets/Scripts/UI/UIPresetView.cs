using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIPresetView : PixelsApp.Page
{
    [Header("Controls")]
    public Button backButton;
    public InputField presetNameText;
    public Button menuButton;
    public RawImage previewImage;
    public RectTransform assignmentsRoot;
    public Button addAssignmentButton;

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
    }

    void OnEnable()
    {
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
        editPreset = preset;
        var designs = new List<DiceVariants.DesignAndColor>(preset.dieAssignments.Select(ass =>
        {
            return (ass.die != null) ? ass.die.designAndColor : DiceVariants.DesignAndColor.Unknown;
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
        presetNameText.text = preset.name;
        dieRenderer.rotating = true;

        RefreshView();
    }

    void Awake()
    {
        backButton.onClick.AddListener(SaveAndGoBack);
        presetNameText.onEndEdit.AddListener(newName => editPreset.name = newName);
        addAssignmentButton.onClick.AddListener(AddNewAssignment);
    }

    void SaveAndGoBack()
    {
        AppDataSet.Instance.SaveData(); // Not sure about this one!
        NavigationManager.Instance.GoBack();
    }

    void AddNewAssignment()
    {
        editPreset.dieAssignments.Add(new Presets.EditDieAssignment()
        {
            die = null,
            behavior = null
        });
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
        uiass.Setup(assignment);
        return uiass;
    }

    void DestroyAssignmentToken(UIAssignmentToken uiass)
    {
        GameObject.Destroy(uiass.gameObject);
    }
}
