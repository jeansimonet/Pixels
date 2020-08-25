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
    public RectTransform contentRoot;
    public Button addActionButton;

    [Header("Prefabs")]
    public UIRuleConditionToken conditionPrefab;
    public UIRuleActionToken actionPrefab;

    public Behaviors.EditRule editRule { get; private set; }
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
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
        if (conditionToken != null)
        {
            GameObject.Destroy(conditionToken.gameObject);
            conditionToken = null;
        }
        foreach (var actionToken in actionTokens)
        {
            GameObject.Destroy(actionToken.gameObject);
        }
        actionTokens.Clear();
    }

    void Setup(Behaviors.EditRule rule)
    {
        editRule = rule;

        conditionToken = GameObject.Instantiate<UIRuleConditionToken>(conditionPrefab, Vector3.zero, Quaternion.identity, contentRoot);
        conditionToken.Setup(rule, rule.condition);


        for (int i = 0; i < rule.actions.Count; ++i)
        {
            var action = rule.actions[i];
            AddActionToken(action, i == 0);
        }

        addActionButton.transform.SetAsLastSibling();
    }

    void Awake()
    {
        addActionButton.onClick.AddListener(AddAction);
        backButton.onClick.AddListener(SaveAndGoBack);
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
    }

    void AddActionToken(Behaviors.EditAction action, bool first)
    {
        var actionToken = GameObject.Instantiate<UIRuleActionToken>(actionPrefab, Vector3.zero, Quaternion.identity, contentRoot);
        actionToken.Setup(editRule, action, first);
        actionToken.onDelete.AddListener(() => DeleteAction(action));
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
                }
            });
        }
        else
        {
            PixelsApp.Instance.ShowDialogBox("Can't Delete last action", "You must have at least one action in a rule.", "Ok", null, null);
        }
        // Else can't delete last action
    }
}
