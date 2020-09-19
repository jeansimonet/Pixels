using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIRuleActionToken : MonoBehaviour
{
    [Header("Controls")]
    public UIParameterEnum actionSelector;
    public RectTransform parametersRoot;
    public Text labelText;
    public Button deleteButton;

    public Behaviors.EditRule parentRule { get; private set; }
    public Behaviors.EditAction editAction { get; private set; }
    public Button.ButtonClickedEvent onDelete => deleteButton.onClick;

    UIParameterManager.ObjectParameterList parameters;

    public delegate void ActionChangedEvent(Behaviors.EditRule rule, Behaviors.EditAction action);
    public ActionChangedEvent onActionChanged;

    void OnDestroy()
    {
        foreach (var parameter in parameters.parameters)
        {
            GameObject.Destroy(parameter.gameObject);
        }
        parameters.onParameterChanged -= OnActionChanged;
        parameters = null;
    }

    public void Setup(Behaviors.EditRule rule, Behaviors.EditAction action, bool first)
    {
        parentRule = rule;
        editAction = action;
        labelText.text = first ? "Then" : "And";
        actionSelector.Setup(
            "Action Type",
            () => editAction.type,
            (t) => SetActionType((Behaviors.ActionType)t),
            null);

        // Setup all other parameters
        parameters = UIParameterManager.Instance.CreateControls(action, parametersRoot);
        parameters.onParameterChanged += OnActionChanged;
    }

    void SetActionType(Behaviors.ActionType newType)
    {
        if (newType != editAction.type)
        {
            onActionChanged?.Invoke(parentRule, editAction);
    
            // Change the type, which really means create a new action and replace the old one
            var newAction = Behaviors.EditAction.Create(newType);

            // Replace the action
            parentRule.ReplaceAction(editAction, newAction);

            // Setup the parameters again
            foreach (var parameter in parameters.parameters)
            {
                GameObject.Destroy(parameter.gameObject);
            }
            parameters = UIParameterManager.Instance.CreateControls(newAction, parametersRoot);

            editAction = newAction;
    
            onActionChanged?.Invoke(parentRule, editAction);
        }
    }

    void OnActionChanged(EditObject parentObject, UIParameter parameter, object newValue)
    {
        onActionChanged?.Invoke(parentRule, editAction);
    }
}
