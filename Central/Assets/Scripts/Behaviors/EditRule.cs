using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Behaviors
{
    [System.Serializable]
    public class EditRule
        : EditObject
    {
        public EditCondition condition;
        public EditAction action;

        public Rule ToRule(EditDataSet editSet, DataSet set)
        {
            // Create our condition
            var cond = condition.ToCondition(editSet, set);
            set.conditions.Add(cond);
            int conditionIndex = set.conditions.Count - 1;

            // Create our action
            var act = action.ToAction(editSet, set);
            set.actions.Add(act);
            int actionIndex = set.actions.Count - 1;

            return new Rule()
            {
                condition = (ushort)conditionIndex,
                action = (ushort)actionIndex
            };
        }

        public EditRule Duplicate()
        {
            return new EditRule()
            {
                condition = condition.Duplicate(),
                action = action.Duplicate()
            };
        }

        public void ReplaceAnimation(Animations.EditAnimation oldAnimation, Animations.EditAnimation newAnimation)
        {
            action.ReplaceAnimation(oldAnimation, newAnimation);
        }
    }
}
