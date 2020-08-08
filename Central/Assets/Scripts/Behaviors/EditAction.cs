using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Behaviors
{
    /// <summary>
    /// Base interface for Actions. Stores the actual type so that we can cast the data
    /// to the proper derived type and access the parameters.
    /// </summary>
    [System.Serializable]
    public abstract class EditAction
    {
        public abstract ActionType type { get; }
        public abstract Action ToAction(EditDataSet editSet, DataSet set);
        public abstract string ToJson(AppDataSet editSet);
        public abstract void FromJson(AppDataSet editSet, string Json);
        public abstract EditAction Duplicate();
        public abstract void ReplaceAnimation(Animations.EditAnimation oldAnimation, Animations.EditAnimation newAnimation);

        public static EditAction Create(ActionType type)
        {
            switch (type)
            {
                case ActionType.PlayAnimation:
                    return new EditActionPlayAnimation();
                default:
                    throw new System.Exception("Unknown condition type");
            }
        }
    };

    /// <summary>
    /// Action to play an animation, really! 
    /// </summary>
    [System.Serializable]
    public class EditActionPlayAnimation
        : EditAction
    {
        public Animations.EditAnimation animation;
        public byte faceIndex;
        public byte loopCount;

        public override ActionType type { get { return ActionType.PlayAnimation; } }
        public override Action ToAction(EditDataSet editSet, DataSet set)
        {
            return new ActionPlayAnimation()
            {
                animIndex = (byte)editSet.animations.IndexOf(animation),
                faceIndex = this.faceIndex,
                loopCount = this.loopCount
            };
        }

        [System.Serializable]
        struct JsonData
        {
            public int animationIndex;
            public byte faceIndex;
            public byte loopCount;
        }

        public override string ToJson(AppDataSet editSet)
        {
            var data = new JsonData()
            {
                animationIndex = editSet.animations.IndexOf(animation),
                faceIndex = faceIndex,
                loopCount = loopCount
            };
            return JsonUtility.ToJson(data);
        }

        public override void FromJson(AppDataSet editSet, string json)
        {
            var data = JsonUtility.FromJson<JsonData>(json);
            animation = editSet.animations[data.animationIndex];
            faceIndex = data.faceIndex;
            loopCount = data.loopCount;
        }
        public override EditAction Duplicate()
        {
            return new EditActionPlayAnimation()
            {
                animation = this.animation,
                faceIndex = this.faceIndex,
                loopCount = this.loopCount
            };
        }
        public override void ReplaceAnimation(Animations.EditAnimation oldAnimation, Animations.EditAnimation newAnimation)
        {
            if (animation == oldAnimation)
            {
                animation = newAnimation;
            }
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (animation != null)
            {
                builder.Append("play \"" + animation.name + "\"");
                if (loopCount > 1)
                {
                    builder.Append("x");
                    builder.Append(loopCount);
                }
                if (faceIndex != 0xFF)
                {
                    builder.Append(" on face ");
                    builder.Append(faceIndex + 1);
                }
            }
            else
            {
                builder.Append("- Please select a Pattern -");
            }
            return builder.ToString();
        }
    };
}