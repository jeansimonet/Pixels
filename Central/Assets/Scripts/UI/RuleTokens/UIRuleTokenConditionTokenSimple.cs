using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Behaviors;

public class UIRuleTokenConditionTokenSimple
    : UIRuleTokenConditionToken
{
    [Header("Controls")]
    public Text ruleText;

    static readonly ConditionType[] supportedConditionTypes = new ConditionType[]
    {
        ConditionType.HelloGoodbye,
        ConditionType.Handling,
		ConditionType.Rolling,
		ConditionType.FaceCompare,
		ConditionType.Crooked,
        ConditionType.ConnectionState,
        ConditionType.BatteryState,
        ConditionType.Idle,
    };

    public override IEnumerable<ConditionType> conditionTypes
    {
        get { return supportedConditionTypes; }
    }

    public override void Setup(EditCondition condition)
    {
        ruleText.text = condition.ToString();
    }
}
