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

        public RGBKeyframe ToKeyframe(EditDataSet editSet, DataSet.AnimationBits bits)
        {
            RGBKeyframe ret = new RGBKeyframe();

            // Add the color to the palette if not already there, otherwise grab the color index
            int colorIndex = bits.palette.IndexOf(color);
            if (colorIndex == -1)
            {
                colorIndex = bits.palette.Count;
                bits.palette.Add(color);
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

        public Keyframe ToKeyframe(EditDataSet editSet)
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

    /// <summary>
    /// Simple list of keyframes for a led
    /// </summary>
    [System.Serializable]
    public class EditPattern
    {
        public List<EditGradient> gradients = new List<EditGradient>();
        public float duration => gradients.Count > 0 ? gradients.Max(g => g.duration) : 1.0f;

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

        public Texture2D ToTexture()
        {
            Texture2D ret = null;
            int width = Mathf.RoundToInt(duration / 0.02f);
            int height = gradients.Count;
            if (width > 0 && height > 0)
            {
                ret = new Texture2D(width, height, TextureFormat.ARGB32, false);
                ret.filterMode = FilterMode.Point;
                ret.wrapMode = TextureWrapMode.Clamp;
                
                Color[] pixels = ret.GetPixels();
                for (int i = 0; i < pixels.Length; ++i)
                {
                    pixels[i] = Color.black;
                }
                for (int j = 0; j < gradients.Count; ++j)
                {
                    var currentGradient = gradients[j];
                    int x = 0, lastMax = 0;
                    for (int i = 1; i < currentGradient.keyframes.Count; ++i)
                    {
                        int max = Mathf.RoundToInt(currentGradient.keyframes[i].time / 0.02f);
                        for (; x < max; ++x)
                        {
                            Color prevColor = new Color(currentGradient.keyframes[i - 1].intensity, currentGradient.keyframes[i - 1].intensity, currentGradient.keyframes[i - 1].intensity);
                            Color nextColor = new Color(currentGradient.keyframes[i].intensity, currentGradient.keyframes[i].intensity, currentGradient.keyframes[i].intensity);
                            pixels[j * ret.width + x] = Color.Lerp(prevColor, nextColor, ((float)x - lastMax) / (max - lastMax));
                        }
                        lastMax = max;
                    }
                }
                ret.SetPixels(pixels);
                ret.Apply(false);
            }
            return ret;
        }
    }


}
