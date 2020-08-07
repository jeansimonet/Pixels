using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

namespace Animations
{
    [System.Serializable]
    public class PreviewSettings
    {
        public DiceVariants.DesignAndColor design;
    }

    /// <summary>
    /// An animation is a list of tracks!
    /// </summary>
    [System.Serializable]
    public abstract class EditAnimation
    {
        public string name;
        [Units("sec")]
        [FloatRange(0.1f, 10.0f, 0.1f)]
		public float duration;
        public PreviewSettings defaultPreviewSettings = new PreviewSettings() { design = DiceVariants.DesignAndColor.V5_Grey };

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
