using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRuleConditionToken : MonoBehaviour
{
    [Header("Controls")]
    public UIParameterEnum conditionSelector;
    public RectTransform parametersRoot;

    public Behaviors.EditRule parentRule { get; private set; }
    public Behaviors.EditCondition editCondition { get; private set; }

    void OnDestroy()
    {
        if (UIParameterManager.Instance != null && editCondition != null)
        {
            UIParameterManager.Instance.DestroyControls(editCondition);
        }
    }

    public void Setup(Behaviors.EditRule rule, Behaviors.EditCondition condition)
    {
        parentRule = rule;
        editCondition = condition;
        conditionSelector.Setup("Condition", () => editCondition.type, (t) => SetConditionType((Behaviors.ConditionType)t));

        // Setup all other parameters
        var paramList = UIParameterManager.Instance.CreateControls(condition, parametersRoot);
    }

    void SetConditionType(Behaviors.ConditionType newType)
    {
        if (newType != editCondition.type)
        {
            // Change the type, which really means create a new condition and replace the old one
            var newCondition = Behaviors.EditCondition.Create(newType);

            // Replace the condition
            parentRule.condition = newCondition;

            // Setup the parameters again
            UIParameterManager.Instance.DestroyControls(editCondition);

            var paramList = UIParameterManager.Instance.CreateControls(newCondition, parametersRoot);

            editCondition = newCondition;
        }
    }
}
