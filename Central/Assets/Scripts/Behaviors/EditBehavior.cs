using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Behaviors
{
    public class EditBehavior
    {
        public string name;
        public string description;
        public List<EditRule> rules = new List<EditRule>();

        public PreviewSettings defaultPreviewSettings = new PreviewSettings() { design = DiceVariants.DesignAndColor.V5_White };

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
            public string name;
            public string description;
            public List<string> rulesJson;
        }

        public string ToJson(AppDataSet editSet)
        {
            var data = new JsonData();
            data.name = name;
            data.description = description;
            data.rulesJson = new List<string>();
            foreach (var rule in rules)
            {
                data.rulesJson.Add(rule.ToJson(editSet));
            }

            return JsonUtility.ToJson(data);
        }

        public void FromJson(AppDataSet editSet, string json)
        {
            var data = JsonUtility.FromJson<JsonData>(json);
            rules.Clear();
            foreach (var ruleJson in data.rulesJson)
            {
                var editRule = new EditRule();
                editRule.FromJson(editSet, ruleJson);
                rules.Add(editRule);
            }
            name = data.name;
            description = data.description;
        }

        public EditBehavior Duplicate()
        {
            var ret = new EditBehavior();
            ret.name = name;
            foreach (var r in rules)
            {
                ret.rules.Add(r.Duplicate());
            }
            return ret;
        }

        public EditRule AddNewDefaultRule()
        {
            EditRule ret = new EditRule();
            ret.condition = new EditConditionFaceCompare()
            {
                flags = ConditionFaceCompare_Flags.Equal,
                faceIndex = 19
            };
            ret.action = new EditActionPlayAnimation()
            {
                animation = null,
                faceIndex = 0xFF,
                loopCount = 1
            };
            rules.Add(ret);
            return ret;
        }

        public void ReplaceAnimation(Animations.EditAnimation oldAnimation, Animations.EditAnimation newAnimation)
        {
            foreach (var rule in rules)
            {
                rule.ReplaceAnimation(oldAnimation, newAnimation);
            }
        }
    }
}