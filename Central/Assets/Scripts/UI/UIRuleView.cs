using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Behaviors;

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
    UIRuleActionToken actionToken;


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
        if (actionToken != null)
        {
            GameObject.Destroy(actionToken.gameObject);
            actionToken = null;
        }
    }

    void Setup(Behaviors.EditRule rule)
    {
        editRule = rule;

        conditionToken = GameObject.Instantiate<UIRuleConditionToken>(conditionPrefab, Vector3.zero, Quaternion.identity, contentRoot);
        conditionToken.Setup(rule, rule.condition);

        actionToken = GameObject.Instantiate<UIRuleActionToken>(actionPrefab, Vector3.zero, Quaternion.identity, contentRoot);
        actionToken.Setup(rule, rule.action);

        addActionButton.transform.SetAsLastSibling();
    }

    void Awake()
    {
        backButton.onClick.AddListener(SaveAndGoBack);
    }

    void SaveAndGoBack()
    {
        AppDataSet.Instance.SaveData(); // Not sure about this one!
        NavigationManager.Instance.GoBack();
    }

}
