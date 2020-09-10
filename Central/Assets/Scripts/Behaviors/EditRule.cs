using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

namespace Behaviors
{
    [System.Serializable]
    public class EditRule
        : EditObject
    {
        public EditCondition condition;
        public List<EditAction> actions = new List<EditAction>();

        public Rule ToRule(EditDataSet editSet, DataSet set)
        {
            // Create our condition
            var cond = condition.ToCondition(editSet, set);
            set.conditions.Add(cond);
            int conditionIndex = set.conditions.Count - 1;

            // Create our action
            var act = actions[0].ToAction(editSet, set);
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
            var actionsCopy = new List<EditAction>();
            foreach (var action in actions)
            {
                actionsCopy.Add(action.Duplicate());
            }
            return new EditRule()
            {
                condition = condition.Duplicate(),
                actions = actionsCopy
            };
        }

        public void CopyTo(EditRule dest)
        {
            dest.condition = condition.Duplicate();
            dest.actions.Clear();
            foreach (var action in actions)
            {
                dest.actions.Add(action.Duplicate());
            }
        }

        public void ReplaceAction(EditAction prevAction, EditAction newAction)
        {
            int index = actions.IndexOf(prevAction);
            actions[index] = newAction;
        }

        public void ReplaceAnimation(Animations.EditAnimation oldAnimation, Animations.EditAnimation newAnimation)
        {
            foreach (var action in actions)
            {
                action.ReplaceAnimation(oldAnimation, newAnimation);
            }
        }

        public void DeleteAnimation(Animations.EditAnimation animation)
        {
            foreach (var action in actions)
            {
                action.DeleteAnimation(animation);
            }
        }

        public bool DependsOnAnimation(Animations.EditAnimation animation)
        {
            return actions.Any(a => a.DependsOnAnimation(animation));
        }
    }
}
