using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Behaviors;
using System.Linq;

public class UIRuleView
    : UIPage
{
    [Header("Controls")]
    public RectTransform contentRoot;
    public Button addActionButton;

    [Header("Prefabs")]
    public UIRuleConditionToken conditionPrefab;
    public UIRuleActionToken actionPrefab;

    public Behaviors.EditRule editRule { get; private set; }
    Behaviors.EditRule workingRule;
    UIRuleConditionToken conditionToken;
    List<UIRuleActionToken> actionTokens = new List<UIRuleActionToken>();

    public override void Enter(object context)
    {
        base.Enter(context);
        var rule = context as Behaviors.EditRule;
        if (rule != null)
        {
            Setup(rule);
        }

        if (AppSettings.Instance.ruleTutorialEnabled)
        {
            Tutorial.Instance.StartRuleTutorial();
        }
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
        if (conditionToken != null)
        {
            conditionToken.onConditionChanged -= OnConditionChange;
            GameObject.Destroy(conditionToken.gameObject);
            conditionToken = null;
        }
        foreach (var actionToken in actionTokens)
        {
            actionToken.onActionChanged -= OnActionChange;
            GameObject.Destroy(actionToken.gameObject);
        }
        actionTokens.Clear();
    }

    void Setup(Behaviors.EditRule rule)
    {
        base.SetupHeader(false, false, "Edit Rule", null);
        editRule = rule;
        workingRule = editRule.Duplicate();

        conditionToken = GameObject.Instantiate<UIRuleConditionToken>(conditionPrefab, Vector3.zero, Quaternion.identity, contentRoot);
        conditionToken.Setup(workingRule, workingRule.condition);
        conditionToken.onConditionChanged += OnConditionChange;


        for (int i = 0; i < workingRule.actions.Count; ++i)
        {
            var action = workingRule.actions[i];
            AddActionToken(action, i == 0);
        }

        addActionButton.transform.SetAsLastSibling();
    }

    void Awake()
    {
        addActionButton.onClick.AddListener(AddAction);
    }

    public override void OnBack()
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
                        NavigationManager.Instance.GoBack();
                        pageDirty = false;
                    }
                });
        }
        else
        {
            NavigationManager.Instance.GoBack();
        }
    }

    public override void OnSave()
    {
        workingRule.CopyTo(editRule);
        AppDataSet.Instance.SaveData(); // Not sure about this one!
        pageDirty = false;
        NavigationManager.Instance.GoBack();
    }

    void AddAction()
    {
        var action = Behaviors.EditAction.Create(Behaviors.ActionType.PlayAnimation);
        AddActionToken(action, false);
        workingRule.actions.Add(action);
        base.pageDirty = true;
    }

    void AddActionToken(Behaviors.EditAction action, bool first)
    {
        var actionToken = GameObject.Instantiate<UIRuleActionToken>(actionPrefab, Vector3.zero, Quaternion.identity, contentRoot);
        actionToken.Setup(workingRule, action, first);
        actionToken.onDelete.AddListener(() => DeleteAction(action));
        actionToken.onActionChanged += OnActionChange;
        actionTokens.Add(actionToken);
    }

    void DestroyActionToken(Behaviors.EditAction action)
    {
        var index = actionTokens.FindIndex(at => at.editAction == action);
        var token = actionTokens[index];
        GameObject.Destroy(token.gameObject);
        actionTokens.RemoveAt(index);
    }

    void DeleteAction(Behaviors.EditAction action)
    {
        if (workingRule.actions.Count > 1)
        {
            PixelsApp.Instance.ShowDialogBox("Delete Action?", "Are you sure sure you want to delete this action?", "Ok", "Cancel", res =>
            {
                if (res)
                {
                    DestroyActionToken(action);
                    workingRule.actions.Remove(action);
                    base.pageDirty = true;
                }
            });
        }
        else
        {
            PixelsApp.Instance.ShowDialogBox("Can't Delete last action", "You must have at least one action in a rule.", "Ok", null, null);
        }
        // Else can't delete last action
    }

    void OnConditionChange(EditRule rule, EditCondition condition)
    {
        base.pageDirty = true;
    }

    void OnActionChange(EditRule rule, EditAction action)
    {
        base.pageDirty = true;
    }
}
