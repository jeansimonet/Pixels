using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

namespace Animations
{
    /// <summary>
    /// An animation is a list of tracks!
    /// </summary>
    [System.Serializable]
    public abstract class EditAnimation
    {
        public string name;
		public float duration;

        public abstract AnimationType type { get; }
        public abstract Animation ToAnimation(EditDataSet editSet, DataSet set);
        public abstract string ToJson();
        public abstract void FromJson(string Json);
        public abstract EditAnimation Duplicate();

        public static EditAnimation Create(AnimationType type)
        {
            switch (type)
            {
                case AnimationType.Simple:
                    return new EditAnimationSimple();
                case AnimationType.Keyframed:
                    return new EditAnimationKeyframed();
                default:
                    throw new System.Exception("Unknown animation type");
            }
        }
    }
}
