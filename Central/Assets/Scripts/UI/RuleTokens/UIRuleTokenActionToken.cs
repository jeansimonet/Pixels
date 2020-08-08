using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIRuleTokenActionToken : MonoBehaviour
{
    public abstract IEnumerable<Behaviors.ActionType> actionTypes { get; }
    public abstract void Setup(Behaviors.EditAction action, bool first);
}
