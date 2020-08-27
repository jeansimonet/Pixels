using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

namespace Behaviors
{
    [System.Serializable]
    public class EditBehavior
        : EditObject
    {
        public string name;
        public string description;
        public List<EditRule> rules = new List<EditRule>();

        public PreviewSettings defaultPreviewSettings = new PreviewSettings() { design = Dice.DesignAndColor.V5_Grey };

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

        public EditBehavior Duplicate()
        {
            var ret = new EditBehavior();
            ret.name = name;
            ret.description = description;
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
            ret.actions = new List<EditAction> ()
            {
                new EditActionPlayAnimation()
                {
                    animation = null,
                    faceIndex = 0xFF,
                    loopCount = 1
                }
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

        public void DeleteAnimation(Animations.EditAnimation animation)
        {
            foreach (var rule in rules)
            {
                rule.DeleteAnimation(animation);
            }
        }

        public bool DependsOnAnimation(Animations.EditAnimation animation)
        {
            return rules.Any(r => r.DependsOnAnimation(animation));
        }

        public IEnumerable<Animations.EditAnimation> CollectAnimations()
        {
            foreach (var action in rules.SelectMany(r => r.actions))
            {
                foreach (var anim in action.CollectAnimations())
                {
                    yield return anim;
                }
            }
        }
    }
}