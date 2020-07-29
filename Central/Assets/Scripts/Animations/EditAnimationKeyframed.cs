using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Animations
{
    /// <summary>
    /// Simple anition keyframe, time in seconds and color!
    /// </summary>
    [System.Serializable]
    public class EditKeyframe
    {
        public float time = -1;
        public Color32 color;

        public EditKeyframe Duplicate()
        {
            var keyframe = new EditKeyframe();
            keyframe.time = time;
            keyframe.color = color;
            return keyframe;
        }

        public RGBKeyframe ToKeyframe(EditDataSet editSet, DataSet set)
        {
            RGBKeyframe ret = new RGBKeyframe();

            // Add the color to the palette if not already there, otherwise grab the color index
            int colorIndex = set.palette.IndexOf(color);
            if (colorIndex == -1)
            {
                colorIndex = set.palette.Count;
                set.palette.Add(color);
            }

            ret.setTimeAndColorIndex((ushort)(time * 1000), (ushort)colorIndex);            
            return ret;
        }

        public class EqualityComparer
            : IEqualityComparer<EditKeyframe>
        {
            public bool Equals(EditKeyframe x, EditKeyframe y)
            {
                return x.time == y.time && x.color.Equals(y.color);
            }

            public int GetHashCode(EditKeyframe obj)
            {
                return obj.time.GetHashCode() ^ obj.color.GetHashCode();
            }
        }
        public static EqualityComparer DefaultComparer = new EqualityComparer();
    }

    /// <summary>
    /// Simple list of keyframes for a led
    /// </summary>
    [System.Serializable]
    public class EditTrack
    {
        public List<int> ledIndices = new List<int>();
        public List<EditKeyframe> keyframes = new List<EditKeyframe>();

        public bool empty => keyframes?.Count == 0;
        public float duration => keyframes.Count == 0 ? 0 : keyframes.Max(k => k.time);
        public float firstTime => keyframes.Count == 0 ? 0 : keyframes.First().time;
        public float lastTime => keyframes.Count == 0 ? 0 : keyframes.Last().time;

        public EditTrack Duplicate()
        {
            var track = new EditTrack();
            track.ledIndices = new List<int>(ledIndices);
            if (keyframes != null)
            {
                track.keyframes = new List<EditKeyframe>(keyframes.Count);
                foreach (var keyframe in keyframes)
                {
                    track.keyframes.Add(keyframe.Duplicate());
                }
            }
            return track;
        }

        public RGBTrack ToTrack(EditDataSet editSet, DataSet set)
        {
            RGBTrack ret = new RGBTrack();
            ret.keyframesOffset = (ushort)set.keyframes.Count;
            ret.keyFrameCount = (byte)keyframes.Count;
            ret.ledMask = 0;
            foreach (int index in ledIndices)
            {
                ret.ledMask |= (uint)(1 << index);
            }

            // Add the keyframes
            foreach (var editKeyframe in keyframes)
            {
                var kf = editKeyframe.ToKeyframe(editSet, set);
                set.keyframes.Add(kf);
            }

            return ret;
        }
    }

    [System.Serializable]
    public class EditAnimationKeyframed
        : EditAnimation
    {
		public SpecialColor specialColorType; // is really SpecialColor
		public List<EditTrack> tracks = new List<EditTrack>();

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

        public override string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public override void FromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
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