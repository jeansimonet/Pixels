using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Behaviors;

public class UIRuleToken : MonoBehaviour
{
    [Header("Controls")]
    public Button editButtton;
    public Button menuButton;
    public RectTransform tokenRoot;

    public EditRule editRule { get; private set; }

    public Button.ButtonClickedEvent onClick => editButtton.onClick;

    public void Setup(EditRule rule)
    {
        editRule = rule;
        // Create the lines describing the rule.
        // First the condition
        UIRuleTokenManager.Instance.CreateConditionToken(rule.condition, tokenRoot);
        UIRuleTokenManager.Instance.CreateActionToken(rule.action, true, tokenRoot);
    }
}
