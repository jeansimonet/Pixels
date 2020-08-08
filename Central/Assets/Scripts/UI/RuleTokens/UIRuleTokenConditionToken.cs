using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIRuleTokenConditionToken : MonoBehaviour
{
    public abstract IEnumerable<Behaviors.ConditionType> conditionTypes { get; }
    public abstract void Setup(Behaviors.EditCondition condition);
}
