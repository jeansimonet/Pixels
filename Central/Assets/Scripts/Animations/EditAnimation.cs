using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Animations
{
    /// <summary>
    /// An animation is a list of tracks!
    /// </summary>
    [System.Serializable]
    public abstract class EditAnimation
        : EditObject
    {
        public string name;
		public abstract float duration { get; set; }
        public PreviewSettings defaultPreviewSettings = new PreviewSettings() { design = Dice.DesignAndColor.V5_Grey };

        [JsonIgnore]
        public abstract AnimationType type { get; }

        public abstract Animation ToAnimation(EditDataSet editSet, DataSet.AnimationBits bits);
        public abstract EditAnimation Duplicate();
        public virtual void ReplacePattern(Animations.EditPattern oldPattern, Animations.EditPattern newPattern)
        {
            // Base does nothing
        }
        public virtual void DeletePattern(Animations.EditPattern pattern)
        {
            // Base does nothing
        }
        public virtual bool DependsOnPattern(Animations.EditPattern pattern)
        {
            // Base does not
            return false;
        }
        public virtual void ReplaceRGBPattern(Animations.EditRGBPattern oldPattern, Animations.EditRGBPattern newPattern)
        {
            // Base does nothing
        }
        public virtual void DeleteRGBPattern(Animations.EditRGBPattern pattern)
        {
            // Base does nothing
        }
        public virtual bool DependsOnRGBPattern(Animations.EditRGBPattern pattern)
        {
            // Base does not
            return false;
        }

        public static EditAnimation Create(AnimationType type)
        {
            switch (type)
            {
                case AnimationType.Simple:
                    return new EditAnimationSimple();
                case AnimationType.Keyframed:
                    return new EditAnimationKeyframed();
                case AnimationType.Rainbow:
                    return new EditAnimationRainbow();
                case AnimationType.GradientPattern:
                    return new EditAnimationGradientPattern();
                default:
                    throw new System.Exception("Unknown animation type");
            }
        }

        public static System.Type GetAnimationType(AnimationType type)
        {
            switch (type)
            {
                case AnimationType.Simple:
                    return typeof(EditAnimationSimple);
                case AnimationType.Keyframed:
                    return typeof(EditAnimationKeyframed);
                case AnimationType.Rainbow:
                    return typeof(EditAnimationRainbow);
                case AnimationType.GradientPattern:
                    return typeof(EditAnimationGradientPattern);
                default:
                    throw new System.Exception("Unknown animation type");
            }
        }
    }

    public class EditAnimationConverter
        : JsonConverter<EditAnimation>
    {
        public override void WriteJson(JsonWriter writer, EditAnimation value, JsonSerializer serializer)
        {
            using (new IgnoreThisConverter(serializer, this))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                serializer.Serialize(writer, value.type);
                writer.WritePropertyName("data");
                serializer.Serialize(writer, value);
                writer.WriteEndObject();
            }
        }

        public override EditAnimation ReadJson(JsonReader reader, System.Type objectType, EditAnimation existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (hasExistingValue)
                throw(new System.NotImplementedException());

            using (new IgnoreThisConverter(serializer, this))
            {
                JObject editAnimObject = JObject.Load(reader);
                var type = editAnimObject["type"].ToObject<Animations.AnimationType>();
                var ret = (EditAnimation)editAnimObject["data"].ToObject(EditAnimation.GetAnimationType(type), serializer);
                return ret;
            }
        }
    }

}
