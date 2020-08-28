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
    public Button selectBehaviorDropdown;
    public Text behaviorName;

    public EditDieAssignment editAssignment { get; private set; }
    public SingleDiceRenderer dieRenderer { get; private set; }

    public Button.ButtonClickedEvent onDelete => deleteButton.onClick;

    public delegate void OnChangeEvent(EditDieAssignment ass);
    public OnChangeEvent onChange;

    System.Func<EditDie, bool> dieSelector;

    public void Setup(Presets.EditDieAssignment ass, System.Func<EditDie, bool> dieSelector)
    {
        this.editAssignment = ass;
        this.dieSelector = dieSelector;
        selectDieDropdown.onClick.AddListener(PickNewDie);
        selectBehaviorDropdown.onClick.AddListener(PickNewBehavior);
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

    void PickNewBehavior()
    {
        PixelsApp.Instance.ShowBehaviorPicker(
            "Select Behavior",
            this.editAssignment.behavior,
            OnBehaviorSelected);
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

    void OnBehaviorSelected(bool result, Behaviors.EditBehavior newBehavior)
    {
        if (result && newBehavior != editAssignment.behavior)
        {
            editAssignment.behavior = newBehavior;
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
        var dName = "Missing Die";
        if (editAssignment.die != null)
        {
            design = editAssignment.die.designAndColor;
            dName = editAssignment.die.name;
        }

        var bName = "Missing Behavior";
        if (editAssignment.behavior != null)
        {
            bName = editAssignment.behavior.name;
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
        behaviorName.text = bName;
    }
}
