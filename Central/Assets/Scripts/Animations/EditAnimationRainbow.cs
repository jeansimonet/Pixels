using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animations
{
    [System.Serializable]
    public class EditAnimationRainbow
        : EditAnimation
    {
        [Slider, FloatRange(0.1f, 10.0f, 0.1f), Units("sec")]
        public override float duration { get; set; }
        [FaceMask, IntRange(0, 19), Name("Face Mask")]
		public int faces = 0xFFFFF;
        [Index, IntRange(1, 10), Name("Repeat Count")]
        public int count = 1;
        [Slider]
        [FloatRange(0.1f, 1.0f), Name("Fading Sharpness")]
        public float fade = 0.1f;
        [Name("Traveling Order")]
        public bool traveling = true;

        public override AnimationType type { get { return AnimationType.Rainbow; } }
        public override Animation ToAnimation(EditDataSet editSet, DataSet.AnimationBits bits)
        {
            var ret = new AnimationRainbow();
            ret.duration = (ushort)(this.duration * 1000.0f);
            ret.faceMask = (uint)this.faces;
            ret.fade = (byte)(255.0f * fade);
            ret.count = (byte)count;
            ret.traveling = traveling ? (byte)1 : (byte)0;
            return ret;
        }
 
        public override EditAnimation Duplicate()
        {
            EditAnimationRainbow ret = new EditAnimationRainbow();
            ret.name = this.name;
		    ret.duration = this.duration;
            ret.faces = this.faces;
            ret.fade = this.fade;
            ret.count = this.count;
            ret.traveling = this.traveling;
            return ret;
        }
   }
}