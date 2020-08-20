using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animations
{
    [System.Serializable]
    public class EditAnimationRainbow
        : EditAnimation
    {
        [FaceMask, IntRange(0, 19)]
		public int faces = 0xFFFFF;
        [Index, IntRange(1, 10)]
        public int count = 1;
        [Slider]
        [FloatRange(0.1f, 1.0f)]
        public float fade = 0.1f;

        public override AnimationType type { get { return AnimationType.Rainbow; } }
        public override Animation ToAnimation(EditDataSet editSet, DataSet set)
        {
            var ret = new AnimationRainbow();
            ret.duration = (ushort)(this.duration * 1000.0f);
            ret.faceMask = (uint)this.faces;
            ret.fade = (byte)(255.0f * fade);
            ret.count = (byte)count;
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
            return ret;
        }
   }
}