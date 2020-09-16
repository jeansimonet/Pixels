using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public RGBTrack ToTrack(EditDataSet editSet, DataSet.AnimationBits bits)
        {
            RGBTrack ret = new RGBTrack();
            ret.keyframesOffset = (ushort)bits.rgbKeyframes.Count;
            ret.keyFrameCount = (byte)gradient.keyframes.Count;
            ret.ledMask = 0;
            foreach (int index in ledIndices)
            {
                ret.ledMask |= (uint)(1 << index);
            }

            // Add the keyframes
            foreach (var editKeyframe in gradient.keyframes)
            {
                var kf = editKeyframe.ToRGBKeyframe(editSet, bits);
                bits.rgbKeyframes.Add(kf);
            }

            return ret;
        }
    }

    [System.Serializable]
    public class EditAnimationKeyframed
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
        [RGBPattern, Name("LED Pattern")]
		public EditPattern pattern = new EditPattern();
        [Name("Traveling Order")]
        public bool flowOrder = false;

        [Slider, FloatRange(-0.5f, 0.5f), Name("Hue Adjustment")]
        public float hueAdjust = 0.0f;

        public override AnimationType type => AnimationType.Keyframed;

        public override Animation ToAnimation(EditDataSet editSet, DataSet.AnimationBits bits)
        {
            var ret = new AnimationKeyframed();
		    ret.duration = (ushort)(duration * 1000); // stored in milliseconds
            ret.speedMultiplier256 = (ushort)(this.speedMultiplier * 256.0f);
		    ret.tracksOffset = (ushort)bits.rgbTracks.Count;
            // Copy the pattern so we can adjust the hue of the keyframes
            var patternCopy = pattern.Duplicate();
            foreach (var t in patternCopy.gradients)
            {
                foreach (var k in t.keyframes)
                {
                    float h, s, v;
                    Color.RGBToHSV(k.color, out h, out s, out v);
                    h = Mathf.Repeat(h + hueAdjust, 1.0f);
                    k.color = Color.HSVToRGB(h, s, v);
                }
            }
            var tracks = patternCopy.ToRGBTracks(editSet, bits);
		    ret.trackCount = (ushort)tracks.Length;
            ret.flowOrder = flowOrder ? (byte)1 : (byte)0;
            bits.rgbTracks.AddRange(tracks);
            return ret;
        }

        public override EditAnimation Duplicate()
        {
            EditAnimationKeyframed ret = new EditAnimationKeyframed();
            ret.name = this.name;
            ret.pattern = this.pattern.Duplicate();
            ret.flowOrder = this.flowOrder;
            ret.speedMultiplier = this.speedMultiplier;
		    ret.duration = this.duration;
            ret.hueAdjust = this.hueAdjust;
            return ret;
        }

        public override void ReplacePattern(Animations.EditPattern oldPattern, Animations.EditPattern newPattern)
        {
            if (pattern == oldPattern)
            {
                pattern = newPattern;
            }
        }
        public override void DeletePattern(Animations.EditPattern pattern)
        {
            if (this.pattern == pattern)
            {
                this.pattern = null;
            }
        }
        public override bool DependsOnPattern(Animations.EditPattern pattern)
        {
            return this.pattern == pattern;
        }

        /// <summary>
        /// Specialized converter
        /// </sumary>
        public class Converter
            : JsonConverter<EditAnimationKeyframed>
        {
            AppDataSet dataSet;
            public Converter(AppDataSet dataSet)
            {
                this.dataSet = dataSet;
            }
            public override void WriteJson(JsonWriter writer, EditAnimationKeyframed value, JsonSerializer serializer)
            {
                using (new IgnoreThisConverter(serializer, this))
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("name");
                    serializer.Serialize(writer, value.name);
                    var patternIndex = dataSet.patterns.IndexOf(value.pattern);
                    writer.WritePropertyName("patternIndex");
                    serializer.Serialize(writer, patternIndex);
                    writer.WritePropertyName("speedMultiplier");
                    serializer.Serialize(writer, value.speedMultiplier);
                    writer.WritePropertyName("duration");
                    serializer.Serialize(writer, value.duration);
                    writer.WritePropertyName("hueAdjust");
                    serializer.Serialize(writer, value.hueAdjust);
                    writer.WriteEndObject();
                }
            }

            public override EditAnimationKeyframed ReadJson(JsonReader reader, System.Type objectType, EditAnimationKeyframed existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (hasExistingValue)
                    throw(new System.NotImplementedException());

                using (new IgnoreThisConverter(serializer, this))
                {
                    JObject jsonObject = JObject.Load(reader);
                    var ret = new EditAnimationKeyframed();
                    ret.name = jsonObject["name"].Value<string>();
                    int patternIndex = jsonObject.ContainsKey("patternIndex") ? jsonObject["patternIndex"].Value<int>() : -1;
                    if (patternIndex >= 0 && patternIndex < dataSet.patterns.Count)
                        ret.pattern = dataSet.patterns[patternIndex];
                    else
                        ret.pattern = AppDataSet.Instance.AddNewDefaultPattern();
                    ret.speedMultiplier = jsonObject["speedMultiplier"].Value<float>();
                    ret.duration = jsonObject["duration"].Value<float>();
                    ret.hueAdjust = jsonObject["hueAdjust"].Value<float>();
                    return ret;
                }
            }
        }
    }
}