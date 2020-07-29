using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Behaviors
{
    public class EditRule
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

        [System.Serializable]
        struct JsonData
        {
            public ConditionType conditionType;
            public string conditionJson;
            public ActionType actionType;
            public string actionJson;
        }

        public string ToJson(EditDataSet editSet)
        {
            var data = new JsonData()
            {
                conditionType = condition.type,
                conditionJson = condition.ToJson(),
                actionType = action.type,
                actionJson = action.ToJson(editSet)
            };
            return JsonUtility.ToJson(data);
        }

        public void FromJson(EditDataSet editSet, string json)
        {
            // Parse json string in
            var data = JsonUtility.FromJson<JsonData>(json);
            condition = EditCondition.Create(data.conditionType);
            condition.FromJson(data.conditionJson);
            action = EditAction.Create(data.actionType);
            action.FromJson(editSet, data.actionJson);
        }

        public EditRule Duplicate()
        {
            return new EditRule()
            {
                condition = condition.Duplicate(),
                action = action.Duplicate()
            };
        }
    }
}
