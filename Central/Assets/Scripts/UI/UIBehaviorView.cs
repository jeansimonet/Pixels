using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBehaviorView
    : PixelsApp.Page
{
    [Header("Controls")]
    public Button backButton;
    public InputField behaviorNameText;
    public InputField behaviorDescriptionText;
    public Button menuButton;
    public RawImage previewImage;
    public RectTransform rulesRoot;
    public Button addRuleButton;
    public RectTransform spacer;

    public Behaviors.EditBehavior editBehavior { get; private set; }
    public SingleDiceRenderer dieRenderer { get; private set; }

    [Header("Prefabs")]
    public UIRuleToken ruleTokenPrefab;

    // The list of controls we have created to display rules
    List<UIRuleToken> rules = new List<UIRuleToken>();

    bool behaviorDirty = false;

    public override void Enter(object context)
    {
        base.Enter(context);
        var bh = context as Behaviors.EditBehavior;
        if (bh != null)
        {
            Setup(bh);
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

        foreach (var ruleui in rules)
        {
            GameObject.Destroy(ruleui.gameObject);
        }
        rules.Clear();
    }

    void Setup(Behaviors.EditBehavior behavior)
    {
        editBehavior = behavior;
        this.dieRenderer = DiceRendererManager.Instance.CreateDiceRenderer(editBehavior.defaultPreviewSettings.design, 300);
        if (dieRenderer != null)
        {
            previewImage.texture = dieRenderer.renderTexture;
        }
        behaviorNameText.text = behavior.name;
        behaviorDescriptionText.text = behavior.description;

        RefreshView();

        dieRenderer.SetAuto(true);
        dieRenderer.SetAnimations(this.editBehavior.CollectAnimations());
        dieRenderer.Play(true);
    }

    void Awake()
    {
        backButton.onClick.AddListener(SaveAndGoBack);
        behaviorNameText.onEndEdit.AddListener(newName => editBehavior.name = newName);
        behaviorDescriptionText.onEndEdit.AddListener(newDescription => editBehavior.description = newDescription);
        addRuleButton.onClick.AddListener(AddNewRule);
    }

    void SaveAndGoBack()
    {
        AppDataSet.Instance.SaveData(); // Not sure about this one!
        NavigationManager.Instance.GoBack();
    }

    void AddNewRule()
    {
        var newRule = editBehavior.AddNewDefaultRule();
        RefreshView();
        NavigationManager.Instance.GoToPage(PixelsApp.PageId.Rule, newRule);
    }

    void RefreshView()
    {
        // Assume all rule uis will be destroyed
        List<UIRuleToken> toDestroy = new List<UIRuleToken>(rules);
        foreach (var rule in editBehavior.rules)
        {
            int prevIndex = toDestroy.FindIndex(r => r.editRule == rule);
            if (prevIndex == -1)
            {
                // New rule
                var ruleui = GameObject.Instantiate<UIRuleToken>(ruleTokenPrefab, Vector3.zero, Quaternion.identity, rulesRoot);
                ruleui.Setup(rule);
                ruleui.onClick.AddListener(() => NavigationManager.Instance.GoToPage(PixelsApp.PageId.Rule, rule));
                ruleui.onEdit.AddListener(() => NavigationManager.Instance.GoToPage(PixelsApp.PageId.Rule, rule));
                ruleui.onMoveUp.AddListener(() => MoveUp(rule));
                ruleui.onMoveDown.AddListener(() => MoveDown(rule));
                ruleui.onDuplicate.AddListener(() => DuplicateRule(rule));
                ruleui.onRemove.AddListener(() => DeleteRule(rule));
                ruleui.onExpand.AddListener(() => ExpandRule(rule));
                rules.Add(ruleui);
                spacer.SetAsLastSibling();
            }
            else
            {
                var ruleui = toDestroy[prevIndex];

                // Still there, don't update it
                toDestroy.RemoveAt(prevIndex);

                // Fix the sibling index
                int order = editBehavior.rules.IndexOf(rule);
                ruleui.transform.SetSiblingIndex(order);
            }
        }

        // Remove all remaining rule uis
        foreach (var ruleui in toDestroy)
        {
            rules.Remove(ruleui);
            GameObject.Destroy(ruleui.gameObject);
        }
    }

    void MoveUp(Behaviors.EditRule rule)
    {
        int index = editBehavior.rules.IndexOf(rule);
        if (index > 0)
        {
            editBehavior.rules.RemoveAt(index);
            editBehavior.rules.Insert(index - 1, rule);
            RefreshView();
        }
    }

    void MoveDown(Behaviors.EditRule rule)
    {
        int index = editBehavior.rules.IndexOf(rule);
        if (index < editBehavior.rules.Count - 1)
        {
            editBehavior.rules.RemoveAt(index);
            editBehavior.rules.Insert(index + 1, rule);
            RefreshView();
        }
    }

    void DuplicateRule(Behaviors.EditRule rule)
    {
        var newRule = rule.Duplicate();
        editBehavior.rules.Add(newRule);
        RefreshView();
    }

    void DeleteRule(Behaviors.EditRule rule)
    {
        PixelsApp.Instance.ShowDialogBox("Delete Rule?", "Are you sure you want to delete this rule?", "Ok", "Cancel", res =>
        {
            if (res)
            {
                editBehavior.rules.Remove(rule);
                RefreshView();
            }
        });
    }

    void ExpandRule(Behaviors.EditRule rule)
    {
        foreach (var uip in rules)
        {
            if (uip.editRule == rule)
            {
                uip.Expand(!uip.isExpanded);
            }
            else
            {
                uip.Expand(false);
            }
        }
    }

}
