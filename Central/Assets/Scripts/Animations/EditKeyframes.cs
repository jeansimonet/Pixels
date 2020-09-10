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

        public RGBKeyframe ToRGBKeyframe(EditDataSet editSet, DataSet.AnimationBits bits)
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

        public Keyframe ToKeyframe(EditDataSet editSet, DataSet.AnimationBits bits)
        {
            Keyframe ret = new Keyframe();

            // Get the intensity from the color and scale
            ret.setTimeAndIntensity((ushort)(time * 1000), (byte)(ColorUtils.desaturate(color) * 255.0f));
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
    /// Simple list of keyframes for a led
    /// </summary>
    [System.Serializable]
    public class EditPattern
    {
        public string name = "LED Pattern";
        public List<EditRGBGradient> gradients = new List<EditRGBGradient>();
        public float duration => gradients.Count > 0 ? gradients.Max(g => g.duration) : 1.0f;

        public EditPattern Duplicate()
        {
            var track = new EditPattern();
            track.name = name;
            track.gradients = new List<EditRGBGradient>();
            foreach (var g in gradients)
            {
                track.gradients.Add(g.Duplicate());
            }
            return track;
        }

        public RGBTrack[] ToRGBTracks(EditDataSet editSet, DataSet.AnimationBits bits)
        {
            RGBTrack[] ret = new RGBTrack[gradients.Count];
            for (int i = 0; i < gradients.Count; ++i)
            {
                RGBTrack t = new RGBTrack();
                t.keyframesOffset = (ushort)bits.rgbKeyframes.Count;
                t.keyFrameCount = (byte)gradients[i].keyframes.Count;
                t.ledMask = 0;
                t.ledMask = (uint)(1 << i);

                // Add the keyframes
                foreach (var editKeyframe in gradients[i].keyframes)
                {
                    var kf = editKeyframe.ToRGBKeyframe(editSet, bits);
                    bits.rgbKeyframes.Add(kf);
                }
                ret[i] = t;
            }

            return ret;
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
                    var kf = editKeyframe.ToKeyframe(editSet, bits);
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
                var gradient = new EditRGBGradient() { keyframes = keyframes };
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
                            Color prevColor = currentGradient.keyframes[i - 1].color;
                            Color nextColor = currentGradient.keyframes[i].color;
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

        public Texture2D ToGreyscaleTexture()
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
                            float prevIntensity = ColorUtils.desaturate(currentGradient.keyframes[i - 1].color);
                            float nextIntensity = ColorUtils.desaturate(currentGradient.keyframes[i].color);
                            Color prevColor = new Color(prevIntensity, prevIntensity, prevIntensity);
                            Color nextColor = new Color(nextIntensity, nextIntensity, nextIntensity);
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
