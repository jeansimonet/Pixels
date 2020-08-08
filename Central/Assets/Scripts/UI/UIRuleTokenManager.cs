using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Behaviors;

public class UIRuleTokenManager : SingletonMonoBehaviour<UIRuleTokenManager>
{
    [Header("Token Prefabs")]
    public List<UIRuleTokenConditionToken> conditionTokenPrefabs;
    public List<UIRuleTokenActionToken> actionTokenPrefabs;

    public UIRuleTokenConditionToken CreateConditionToken(EditCondition condition, RectTransform root)
    {
        // Find the prefab that supports this condition type
        var prefab = conditionTokenPrefabs.FirstOrDefault(cond => cond.conditionTypes.Contains(condition.type));
        if (prefab != null)
        {
            // Create the UI
            var uitoken = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, root);
            uitoken.Setup(condition);
            return uitoken;
        }
        else
        {
            return null;
        }
    }

    public UIRuleTokenActionToken CreateActionToken(EditAction action, bool first, RectTransform root)
    {
        // Find the prefab that supports this action type
        var prefab = actionTokenPrefabs.FirstOrDefault(act => act.actionTypes.Contains(action.type));
        if (prefab != null)
        {
            // Create the UI
            var uitoken = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, root);
            uitoken.Setup(action, first);
            return uitoken;
        }
        else
        {
            return null;
        }
    }
}
