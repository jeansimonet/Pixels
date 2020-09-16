using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Dice;
using Presets;

public class UIAssignmentToken : MonoBehaviour
{
    [Header("Controls")]
    public Button deleteButton;
    public RawImage diePreview;
    public Button selectDieDropdown;
    public Text dieName;
    public Button editBehaviorButton;

    public EditPreset parentPreset { get; private set; }
    public EditDieAssignment editAssignment { get; private set; }
    public SingleDiceRenderer dieRenderer { get; private set; }

    public Button.ButtonClickedEvent onDelete => deleteButton.onClick;

    public delegate void OnChangeEvent(EditDieAssignment ass);
    public OnChangeEvent onChange;

    System.Func<EditDie, bool> dieSelector;

    public void Setup(EditPreset preset, EditDieAssignment ass, System.Func<EditDie, bool> dieSelector)
    {
        this.parentPreset = preset;
        this.editAssignment = ass;
        this.dieSelector = dieSelector;
        selectDieDropdown.onClick.AddListener(PickNewDie);
        editBehaviorButton.onClick.AddListener(EditBehavior);
        UpdateView();
    }

    void OnDestroy()
    {
        if (this.dieRenderer != null)
        {
            DiceRendererManager.Instance.DestroyDiceRenderer(this.dieRenderer);
            this.dieRenderer = null;
        }
    }

    void PickNewDie()
    {
        PixelsApp.Instance.ShowDiePicker(
            "Select Die",
            this.editAssignment.die,
            dieSelector,
            OnDieSelected);
    }

    void EditBehavior()
    {
        // Are we editing or adding?
        if (editAssignment.behavior == null)
        {
            // Create the behavior and add it to the assignment
            var newBehavior = AppDataSet.Instance.AddNewDefaultBehavior();
            editAssignment.behavior = newBehavior;
            onChange?.Invoke(editAssignment);
        }

        UIBehaviorView.Context context = new UIBehaviorView.Context()
        {
            behavior = editAssignment.behavior,
            parentPreset = parentPreset,
            dieAssignment = editAssignment
        };
        NavigationManager.Instance.GoToPage(UIPage.PageId.Behavior, context);
    }

    void OnDieSelected(bool result, EditDie newDie)
    {
        if (result && newDie != editAssignment.die)
        {
            editAssignment.die = newDie;
            onChange?.Invoke(editAssignment);
            UpdateView();
        }
    }

    void UpdateView()
    {
        if (this.dieRenderer != null)
        {
            DiceRendererManager.Instance.DestroyDiceRenderer(this.dieRenderer);
            this.dieRenderer = null;
        }

        var design = DesignAndColor.Unknown;
        var dName = "Select Die";
        if (editAssignment.die != null)
        {
            design = editAssignment.die.designAndColor;
            dName = editAssignment.die.name;
        }
        
        this.dieRenderer = DiceRendererManager.Instance.CreateDiceRenderer(design);
        if (dieRenderer != null)
        {
            diePreview.texture = dieRenderer.renderTexture;
        }
        dieRenderer.SetAuto(true);
        if (editAssignment.behavior != null)
        {
            dieRenderer.SetAnimations(editAssignment.behavior.CollectAnimations());
            dieRenderer.Play(true);
        }

        dieName.text = dName;
    }
}
