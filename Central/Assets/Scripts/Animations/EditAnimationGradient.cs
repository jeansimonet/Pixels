using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Animations
{
    [System.Serializable]
    public class EditAnimationGradient
        : EditAnimation
    {
        [Slider, FloatRange(0.1f, 10.0f, 0.1f), Units("sec")]
        public override float duration { get; set; }
        [FaceMask, IntRange(0, 19), Name("Face Mask")]
        public int faces = 0xFFFFF;
        [Gradient]
        public EditRGBGradient gradient = new EditRGBGradient();

        public override AnimationType type => AnimationType.Gradient;

        public override Animation ToAnimation(EditDataSet editSet, DataSet.AnimationBits bits)
        {
            var ret = new AnimationGradient();
            ret.duration = (ushort)(this.duration * 1000.0f);
            ret.faceMask = (uint)this.faces;

            // Add gradient
            ret.gradientTrackOffset = (ushort)bits.rgbTracks.Count;
            var tempTrack = new EditRGBTrack() { gradient = gradient };
            var gradientTrack = tempTrack.ToTrack(editSet, bits);
            bits.rgbTracks.Add(gradientTrack);
            return ret;
        }

        public override EditAnimation Duplicate()
        {
            EditAnimationGradient ret = new EditAnimationGradient();
            ret.name = this.name;
            ret.duration = this.duration;
            ret.faces = this.faces;
            ret.gradient = gradient.Duplicate();
            return ret;
        }
    }
}