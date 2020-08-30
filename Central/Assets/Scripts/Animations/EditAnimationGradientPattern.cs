using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Animations
{
    [System.Serializable]
    public class EditAnimationGradientPattern
        : EditAnimation
    {
        public float speedMultiplier = 1.0f;
        [Slider, FloatRange(0.1f, 10.0f, 0.1f), Units("sec")]
        public override float duration
        {
            get
            {
                return pattern.duration * speedMultiplier;
            }
            set
            {
                speedMultiplier = value / pattern.duration;
            }
        }
        [Pattern]
		public EditPattern pattern = new EditPattern();
        [Gradient]
        public EditRGBGradient gradient = new EditRGBGradient();

        public override AnimationType type => AnimationType.GradientPattern;

        public override Animation ToAnimation(EditDataSet editSet, DataSet.AnimationBits bits)
        {
            var ret = new AnimationGradientPattern();
		    ret.duration = (ushort)(duration * 1000); // stored in milliseconds
            ret.speedMultiplier256 = (ushort)(this.speedMultiplier * 256.0f);
		    ret.tracksOffset = (ushort)bits.tracks.Count;
            var tracks = pattern.ToTracks(editSet, bits);
		    ret.trackCount = (ushort)tracks.Length;
            bits.tracks.AddRange(tracks);

            // Add gradient
            ret.gradientTrackOffset = (ushort)bits.rgbTracks.Count;
            var tempTrack = new EditRGBTrack() { gradient = gradient };
            var gradientTrack = tempTrack.ToTrack(editSet, bits);
            bits.rgbTracks.Add(gradientTrack);

            return ret;
        }

        public override EditAnimation Duplicate()
        {
            EditAnimationGradientPattern ret = new EditAnimationGradientPattern();
            ret.name = this.name;
            ret.pattern = this.pattern.Duplicate();
            ret.speedMultiplier = this.speedMultiplier;
		    ret.duration = this.duration;
            ret.gradient = gradient.Duplicate();
            return ret;
        }
    }
}