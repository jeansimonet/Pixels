using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Animations
{
    [System.Serializable]
    public class EditAnimationGradientPattern
        : EditAnimation
    {
        public float speedMultiplier = 1.0f;
        [Slider, FloatRange(0.1f, 20.0f, 0.1f), Units("sec")]
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
        [GreyscalePattern, Name("LED Pattern")]
		public EditPattern pattern = new EditPattern();
        [Gradient]
        public EditRGBGradient gradient = new EditRGBGradient();
        [Name("Override color based on face")]
        public bool overrideWithFace = false;

        public override AnimationType type => AnimationType.GradientPattern;

        public override Animation ToAnimation(EditDataSet editSet, DataSet.AnimationBits bits)
        {
            var ret = new AnimationGradientPattern();
		    ret.duration = (ushort)(duration * 1000); // stored in milliseconds
            ret.speedMultiplier256 = (ushort)(this.speedMultiplier * 256.0f);
		    ret.tracksOffset = (ushort)editSet.getPatternTrackOffset(pattern);
		    ret.trackCount = (ushort)pattern.gradients.Count;

            // Add gradient
            ret.gradientTrackOffset = (ushort)bits.rgbTracks.Count;
            var tempTrack = new EditRGBTrack() { gradient = gradient };
            var gradientTrack = tempTrack.ToTrack(editSet, bits);
            bits.rgbTracks.Add(gradientTrack);
            ret.overrideWithFace = (byte)(overrideWithFace ? 1 : 0);

            return ret;
        }

        public override EditAnimation Duplicate()
        {
            EditAnimationGradientPattern ret = new EditAnimationGradientPattern();
            ret.name = this.name;
            ret.pattern = this.pattern;
            ret.speedMultiplier = this.speedMultiplier;
		    ret.duration = this.duration;
            ret.gradient = gradient.Duplicate();
            ret.overrideWithFace = overrideWithFace;
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
        public override bool DependsOnPattern(Animations.EditPattern pattern, out bool asRGB)
        {
            asRGB = false;
            return this.pattern == pattern;
        }

        /// <summary>
        /// Specialized converter
        /// </sumary>
        public class Converter
            : JsonConverter<EditAnimationGradientPattern>
        {
            AppDataSet dataSet;
            public Converter(AppDataSet dataSet)
            {
                this.dataSet = dataSet;
            }
            public override void WriteJson(JsonWriter writer, EditAnimationGradientPattern value, JsonSerializer serializer)
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
                    writer.WritePropertyName("gradient");
                    serializer.Serialize(writer, value.gradient);
                    writer.WriteEndObject();
                }
            }

            public override EditAnimationGradientPattern ReadJson(JsonReader reader, System.Type objectType, EditAnimationGradientPattern existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (hasExistingValue)
                    throw(new System.NotImplementedException());

                using (new IgnoreThisConverter(serializer, this))
                {
                    JObject jsonObject = JObject.Load(reader);
                    var ret = new EditAnimationGradientPattern();
                    ret.name = jsonObject["name"].Value<string>();
                    int patternIndex = jsonObject.ContainsKey("patternIndex") ? jsonObject["patternIndex"].Value<int>() : -1;
                    if (patternIndex >= 0 && patternIndex < dataSet.patterns.Count)
                        ret.pattern = dataSet.patterns[patternIndex];
                    else
                        ret.pattern = AppDataSet.Instance.AddNewDefaultPattern();
                    ret.speedMultiplier = jsonObject["speedMultiplier"].Value<float>();
                    ret.duration = jsonObject["duration"].Value<float>();
                    ret.gradient = jsonObject["gradient"].ToObject<EditRGBGradient>();
                    return ret;
                }
            }
        }


    }
}