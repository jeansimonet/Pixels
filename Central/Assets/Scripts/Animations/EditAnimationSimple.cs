using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animations
{
    [System.Serializable]
    public class EditAnimationSimple
        : EditAnimation
    {
        [FaceMask, IntRange(0, 19)]
		public int faces = 0xFFFFF;
        public Color32 color = new Color32(0xFF, 0x30, 0x00, 0xff);
        [Index, IntRange(1, 10)]
        public int count = 1;
        [Slider]
        [FloatRange(0.1f, 1.0f)]
        public float fade = 0.1f;

        public override AnimationType type { get { return AnimationType.Simple; } }
        public override Animation ToAnimation(EditDataSet editSet, DataSet.AnimationBits bits)
        {
            var ret = new AnimationSimple();
            ret.duration = (ushort)(this.duration * 1000.0f);
            ret.faceMask = (uint)this.faces;
            ret.color = ColorUtils.toColor(this.color.r, this.color.g, this.color.b);
            ret.fade = (byte)(255.0f * fade);
            ret.count = (byte)count;
            return ret;
        }
 
        public override EditAnimation Duplicate()
        {
            EditAnimationSimple ret = new EditAnimationSimple();
            ret.name = this.name;
		    ret.duration = this.duration;
            ret.faces = this.faces;
            ret.color = this.color;
            ret.count = this.count;
            return ret;
        }
   }
}