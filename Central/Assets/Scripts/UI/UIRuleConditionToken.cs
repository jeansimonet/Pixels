using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIRuleConditionToken : MonoBehaviour
{
    [Header("Controls")]
    public UIParameterEnum conditionSelector;
    public RectTransform parametersRoot;

    public Behaviors.EditRule parentRule { get; private set; }
    public Behaviors.EditCondition editCondition { get; private set; }

    UIParameterManager.ObjectParameterList parameters;

    public delegate void ConditionChangedEvent(Behaviors.EditRule rule, Behaviors.EditCondition condition);
    public ConditionChangedEvent onConditionChanged;

    void OnDestroy()
    {
        foreach (var parameter in parameters.parameters)
        {
            GameObject.Destroy(parameter.gameObject);
        }
        parameters.onParameterChanged -= OnConditionChanged;
        parameters = null;
    }

    public void Setup(Behaviors.EditRule rule, Behaviors.EditCondition condition)
    {
        parentRule = rule;
        editCondition = condition;
        conditionSelector.Setup(
            "Condition Type",
            () => editCondition.type,
            (t) => SetConditionType((Behaviors.ConditionType)t),
            Enumerable.Repeat(new SkipEnumAttribute(1), 1));

        // Setup all other parameters
        parameters = UIParameterManager.Instance.CreateControls(condition, parametersRoot);
        parameters.onParameterChanged += OnConditionChanged;
    }

    void SetConditionType(Behaviors.ConditionType newType)
    {
        if (newType != editCondition.type)
        {
            onConditionChanged?.Invoke(parentRule, editCondition);

            // Change the type, which really means create a new condition and replace the old one
            var newCondition = Behaviors.EditCondition.Create(newType);

            // Replace the condition
            parentRule.condition = newCondition;

            // Setup the parameters again
            foreach (var parameter in parameters.parameters)
            {
                GameObject.Destroy(parameter.gameObject);
            }
            parameters = UIParameterManager.Instance.CreateControls(newCondition, parametersRoot);
            parameters.onParameterChanged += OnConditionChanged;

            editCondition = newCondition;

            onConditionChanged?.Invoke(parentRule, editCondition);
        }
    }

    void OnConditionChanged(EditObject parentObject, UIParameter parameter, object newValue)
    {
        onConditionChanged?.Invoke(parentRule, editCondition);
    }
}
