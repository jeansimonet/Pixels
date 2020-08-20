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
    public class EditRGBTrack
    {
        public List<int> ledIndices = new List<int>();
        public EditRGBGradient gradient = new EditRGBGradient();

        public bool empty => gradient.empty;
        public float duration => gradient.duration;
        public float firstTime => gradient.firstTime;
        public float lastTime => gradient.lastTime;

        public EditRGBTrack Duplicate()
        {
            var track = new EditRGBTrack();
            track.ledIndices = new List<int>(ledIndices);
            track.gradient = gradient.Duplicate();
            return track;
        }

        public RGBTrack ToTrack(EditDataSet editSet, DataSet set)
        {
            RGBTrack ret = new RGBTrack();
            ret.keyframesOffset = (ushort)set.rgbKeyframes.Count;
            ret.keyFrameCount = (byte)gradient.keyframes.Count;
            ret.ledMask = 0;
            foreach (int index in ledIndices)
            {
                ret.ledMask |= (uint)(1 << index);
            }

            // Add the keyframes
            foreach (var editKeyframe in gradient.keyframes)
            {
                var kf = editKeyframe.ToKeyframe(editSet, set);
                set.rgbKeyframes.Add(kf);
            }

            return ret;
        }
    }

    [System.Serializable]
    public class EditAnimationKeyframed
        : EditAnimation
    {
		public SpecialColor specialColorType; // is really SpecialColor
		public List<EditRGBTrack> tracks = new List<EditRGBTrack>();

        public override AnimationType type => AnimationType.Keyframed;
        public bool empty => tracks?.Count == 0;

        public override Animation ToAnimation(EditDataSet editSet, DataSet set)
        {
            var ret = new AnimationKeyframed();
		    ret.duration = (ushort)(duration * 1000); // stored in milliseconds
		    ret.specialColorType = specialColorType;
		    ret.tracksOffset = (ushort)set.rgbTracks.Count;
		    ret.trackCount = (ushort)tracks.Count;

            // Add the tracks
            foreach (var editTrack in tracks)
            {
                var track = editTrack.ToTrack(editSet, set);
                set.rgbTracks.Add(track);
            }

            return ret;
        }

        public override EditAnimation Duplicate()
        {
            EditAnimationKeyframed ret = new EditAnimationKeyframed();
            ret.name = this.name;
		    ret.duration = this.duration;
            ret.specialColorType = this.specialColorType;
            foreach (var track in tracks)
            {
                ret.tracks.Add(track.Duplicate());
            }
            return ret;
        }
    }
}