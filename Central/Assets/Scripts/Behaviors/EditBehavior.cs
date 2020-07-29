using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Behaviors
{
    public class EditBehavior
    {
        public List<EditRule> rules = new List<EditRule>();

        public Behavior ToBehavior(EditDataSet editSet, DataSet set)
        {
            // Add our rules to the set
            int rulesOffset = set.rules.Count;
            foreach (var editRule in rules)
            {
                var rule = editRule.ToRule(editSet, set);
                set.rules.Add(rule);
            }

            return new Behavior()
            {
                rulesOffset = (ushort)rulesOffset,
                rulesCount = (ushort)rules.Count
            };
        }

        [System.Serializable]
        struct JsonData
        {
            public List<string> rulesJson;
        }

        public string ToJson(EditDataSet editSet)
        {
            var data = new JsonData();
            data.rulesJson = new List<string>();
            foreach (var rule in rules)
            {
                data.rulesJson.Add(rule.ToJson(editSet));
            }

            return JsonUtility.ToJson(data);
        }

        public void FromJson(EditDataSet editSet, string json)
        {
            var data = JsonUtility.FromJson<JsonData>(json);
            rules.Clear();
            foreach (var ruleJson in data.rulesJson)
            {
                var editRule = new EditRule();
                editRule.FromJson(editSet, ruleJson);
                rules.Add(editRule);
            }
        }

        public EditBehavior Duplicate()
        {
            var ret = new EditBehavior();
            foreach (var r in rules)
            {
                ret.rules.Add(r.Duplicate());
            }
            return ret;
        }
    }
}