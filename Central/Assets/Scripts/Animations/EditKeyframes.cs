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
    public class EditRGBKeyframe
    {
        public float time = -1;
        public Color32 color;

        public EditRGBKeyframe Duplicate()
        {
            var keyframe = new EditRGBKeyframe();
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
            : IEqualityComparer<EditRGBKeyframe>
        {
            public bool Equals(EditRGBKeyframe x, EditRGBKeyframe y)
            {
                return x.time == y.time && x.color.Equals(y.color);
            }

            public int GetHashCode(EditRGBKeyframe obj)
            {
                return obj.time.GetHashCode() ^ obj.color.GetHashCode();
            }
        }
        public static EqualityComparer DefaultComparer = new EqualityComparer();
    }

    [System.Serializable]
    public class EditRGBGradient
    {
        public List<EditRGBKeyframe> keyframes = new List<EditRGBKeyframe>();

        public bool empty => keyframes?.Count == 0;
        public float duration => keyframes.Count == 0 ? 0 : keyframes.Max(k => k.time);
        public float firstTime => keyframes.Count == 0 ? 0 : keyframes.First().time;
        public float lastTime => keyframes.Count == 0 ? 0 : keyframes.Last().time;

        public EditRGBGradient Duplicate()
        {
            var track = new EditRGBGradient();
            if (keyframes != null)
            {
                track.keyframes = new List<EditRGBKeyframe>(keyframes.Count);
                foreach (var keyframe in keyframes)
                {
                    track.keyframes.Add(keyframe.Duplicate());
                }
            }
            return track;
        }

    }

    /// <summary>
    /// Simple anition keyframe, time in seconds and color!
    /// </summary>
    [System.Serializable]
    public class EditKeyframe
    {
        public float time = -1;
        public float intensity;

        public EditKeyframe Duplicate()
        {
            var keyframe = new EditKeyframe();
            keyframe.time = time;
            keyframe.intensity = intensity;
            return keyframe;
        }

        public Keyframe ToKeyframe(EditDataSet editSet, DataSet set)
        {
            Keyframe ret = new Keyframe();

            // Add the color to the palette if not already there, otherwise grab the color index
            ret.setTimeAndIntensity((ushort)(time * 1000), (byte)(intensity * 255.0f));
            return ret;
        }

        public class EqualityComparer
            : IEqualityComparer<EditKeyframe>
        {
            public bool Equals(EditKeyframe x, EditKeyframe y)
            {
                return x.time == y.time && x.intensity == y.intensity;
            }

            public int GetHashCode(EditKeyframe obj)
            {
                return obj.time.GetHashCode() ^ obj.intensity.GetHashCode();
            }
        }
        public static EqualityComparer DefaultComparer = new EqualityComparer();
    }

    [System.Serializable]
    public class EditGradient
    {
        public List<EditKeyframe> keyframes = new List<EditKeyframe>();

        public bool empty => keyframes?.Count == 0;
        public float duration => keyframes.Count == 0 ? 0 : keyframes.Max(k => k.time);
        public float firstTime => keyframes.Count == 0 ? 0 : keyframes.First().time;
        public float lastTime => keyframes.Count == 0 ? 0 : keyframes.Last().time;

        public EditGradient Duplicate()
        {
            var track = new EditGradient();
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

    }


}
