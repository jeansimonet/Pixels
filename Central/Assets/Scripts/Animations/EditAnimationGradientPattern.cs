using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Animations
{
    /// <summary>
    /// Simple list of keyframes for a led
    /// </summary>
    [System.Serializable]
    public class EditPattern
    {
        public List<EditGradient> gradients = new List<EditGradient>();
        public float duration => gradients.Max(g => g.duration);

        public EditPattern Duplicate()
        {
            var track = new EditPattern();
            track.gradients = new List<EditGradient>(gradients);
            return track;
        }

        public Track[] ToTracks(EditDataSet editSet, DataSet.AnimationBits bits)
        {
            Track[] ret = new Track[gradients.Count];
            for (int i = 0; i < gradients.Count; ++i)
            {
                Track t = new Track();
                t.keyframesOffset = (ushort)bits.keyframes.Count;
                t.keyFrameCount = (byte)gradients[i].keyframes.Count;
                t.ledMask = 0;
                t.ledMask = (uint)(1 << i);

                // Add the keyframes
                foreach (var editKeyframe in gradients[i].keyframes)
                {
                    var kf = editKeyframe.ToKeyframe(editSet);
                    bits.keyframes.Add(kf);
                }
                ret[i] = t;
            }

            return ret;
        }

        public void FromTexture(Texture2D texture)
        {
            gradients.Clear();
            for (int i = 0; i < texture.height; ++i)
            {
                var gradientPixels = texture.GetPixels(0, i, texture.width, 1, 0);
                var keyframes = ColorUtils.extractKeyframes(gradientPixels);
                // Convert to greyscale (right now us the red channel only)
                var gradient = new EditGradient();
                foreach (var k in keyframes)
                {
                    gradient.keyframes.Add(new EditKeyframe() {time = k.time, intensity = ((Color)k.color).r});
                }
                gradients.Add(gradient);
            }
        }
    }

    [System.Serializable]
    public class EditAnimationGradientPattern
        : EditAnimation
    {
        [Pattern]
		public EditPattern pattern = new EditPattern();
        [Gradient]
        public EditRGBGradient gradient = new EditRGBGradient();

        public override AnimationType type => AnimationType.GradientPattern;

        public override Animation ToAnimation(EditDataSet editSet, DataSet.AnimationBits bits)
        {
            var ret = new AnimationGradientPattern();
		    ret.duration = (ushort)(duration * 1000); // stored in milliseconds
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
		    ret.duration = this.duration;
            ret.pattern = this.pattern.Duplicate();
            ret.gradient = gradient.Duplicate();
            return ret;
        }
    }
}