using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Behaviors;
using System.Linq;

public class UIRuleView
    : PixelsApp.Page
{
    [Header("Controls")]
    public Button backButton;
    public Button menuButton;
    public Button saveButton;
    public RectTransform contentRoot;
    public Button addActionButton;

    [Header("Prefabs")]
    public UIRuleConditionToken conditionPrefab;
    public UIRuleActionToken actionPrefab;

    public Behaviors.EditRule editRule { get; private set; }
    UIRuleConditionToken conditionToken;
    List<UIRuleActionToken> actionTokens = new List<UIRuleActionToken>();

    bool ruleDirty = false;

    public override void Enter(object context)
    {
        base.Enter(context);
        var rule = context as Behaviors.EditRule;
        if (rule != null)
        {
            Setup(rule);
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
        editRule = rule;

        conditionToken = GameObject.Instantiate<UIRuleConditionToken>(conditionPrefab, Vector3.zero, Quaternion.identity, contentRoot);
        conditionToken.Setup(rule, rule.condition);
        conditionToken.onConditionChanged += OnConditionChange;


        for (int i = 0; i < rule.actions.Count; ++i)
        {
            var action = rule.actions[i];
            AddActionToken(action, i == 0);
        }

        addActionButton.transform.SetAsLastSibling();
        saveButton.gameObject.SetActive(false);
        ruleDirty = false;
    }

    void Awake()
    {
        addActionButton.onClick.AddListener(AddAction);
        backButton.onClick.AddListener(DiscardAndGoBack);
        saveButton.onClick.AddListener(SaveAndGoBack);
    }

    void DiscardAndGoBack()
    {
        if (ruleDirty)
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
        AppDataSet.Instance.SaveData(); // Not sure about this one!
        NavigationManager.Instance.GoBack();
    }

    void AddAction()
    {
        var action = Behaviors.EditAction.Create(Behaviors.ActionType.PlayAnimation);
        AddActionToken(action, false);
        editRule.actions.Add(action);
        ruleDirty = true;
        saveButton.gameObject.SetActive(true);
    }

    void AddActionToken(Behaviors.EditAction action, bool first)
    {
        var actionToken = GameObject.Instantiate<UIRuleActionToken>(actionPrefab, Vector3.zero, Quaternion.identity, contentRoot);
        actionToken.Setup(editRule, action, first);
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
        if (editRule.actions.Count > 1)
        {
            PixelsApp.Instance.ShowDialogBox("Delete Action?", "Are you sure sure you want to delete this action?", "Ok", "Cancel", res =>
            {
                if (res)
                {
                    DestroyActionToken(action);
                    editRule.actions.Remove(action);
                    ruleDirty = true;
                    saveButton.gameObject.SetActive(true);
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
        ruleDirty = true;
        saveButton.gameObject.SetActive(true);
    }

    void OnActionChange(EditRule rule, EditAction action)
    {
        ruleDirty = true;
        saveButton.gameObject.SetActive(true);
    }
}
