using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Behaviors
{
    /// <summary>
    /// Base interface for Actions. Stores the actual type so that we can cast the data
    /// to the proper derived type and access the parameters.
    /// </summary>
    [System.Serializable]
    public abstract class EditAction
        : EditObject
    {
        [JsonIgnore]
        public abstract ActionType type { get; }
        public abstract Action ToAction(EditDataSet editSet, DataSet set);
        public abstract EditAction Duplicate();
        public virtual void ReplaceAnimation(Animations.EditAnimation oldAnimation, Animations.EditAnimation newAnimation)
        {
            // Base does nothing
        }
        public virtual void DeleteAnimation(Animations.EditAnimation animation)
        {
            // Base does nothing
        }
        public virtual bool DependsOnAnimation(Animations.EditAnimation animation)
        {
            return false;
        }
        public virtual bool DependsOnAudioClip(AudioClips.EditAudioClip clip)
        {
            return false;
        }
        public virtual IEnumerable<Animations.EditAnimation> CollectAnimations()
        {
            yield break;
        }
        public virtual IEnumerable<AudioClips.EditAudioClip> CollectAudioClips()
        {
            yield break;
        }

        public static EditAction Create(ActionType type)
        {
            switch (type)
            {
                case ActionType.PlayAnimation:
                    return new EditActionPlayAnimation();
                case ActionType.PlayAudioClip:
                    return new EditActionPlayAudioClip();
                default:
                    throw new System.Exception("Unknown condition type");
            }
        }

        public static System.Type GetActionType(ActionType type)
        {
            switch (type)
            {
                case ActionType.PlayAnimation:
                    return typeof(EditActionPlayAnimation);
                case ActionType.PlayAudioClip:
                    return typeof(EditActionPlayAudioClip);
                default:
                    throw new System.Exception("Unknown condition type");
            }
        }
    };

    public class EditActionConverter
        : JsonConverter<EditAction>
    {
        public override void WriteJson(JsonWriter writer, EditAction value, JsonSerializer serializer)
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

        public override EditAction ReadJson(JsonReader reader, System.Type objectType, EditAction existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (hasExistingValue)
                throw(new System.NotImplementedException());

            using (new IgnoreThisConverter(serializer, this))
            {
                JObject editAnimObject = JObject.Load(reader);
                var type = editAnimObject["type"].ToObject<Behaviors.ActionType>();
                var ret = (EditAction)editAnimObject["data"].ToObject(EditAction.GetActionType(type), serializer);
                return ret;
            }
        }
    }

    /// <summary>
    /// Action to play an animation, really! 
    /// </summary>
    [System.Serializable]
    public class EditActionPlayAnimation
        : EditAction
    {
        [Name("Lighting Pattern")]
        public Animations.EditAnimation animation;
        [PlaybackFace, Name("Play on Face")]
        public int faceIndex = -1;
        [IntSlider, IntRange(1, 10), Name("Repeat Count")]
        public int loopCount = 1;

        public override ActionType type { get { return ActionType.PlayAnimation; } }
        public override Action ToAction(EditDataSet editSet, DataSet set)
        {
            return new ActionPlayAnimation()
            {
                animIndex = (byte)editSet.animations.IndexOf(animation),
                faceIndex = (byte)this.faceIndex,
                loopCount = (byte)this.loopCount
            };
        }

        /// <summary>
        /// Specialized converter
        /// </sumary>
        public class Converter
            : JsonConverter<EditActionPlayAnimation>
        {
            AppDataSet dataSet;
            public Converter(AppDataSet dataSet)
            {
                this.dataSet = dataSet;
            }
            public override void WriteJson(JsonWriter writer, EditActionPlayAnimation value, JsonSerializer serializer)
            {
                using (new IgnoreThisConverter(serializer, this))
                {
                    writer.WriteStartObject();
                    var animationIndex = dataSet.animations.IndexOf(value.animation);
                    writer.WritePropertyName("animationIndex");
                    serializer.Serialize(writer, animationIndex);
                    writer.WritePropertyName("faceIndex");
                    serializer.Serialize(writer, value.faceIndex);
                    writer.WritePropertyName("loopCount");
                    serializer.Serialize(writer, value.loopCount);
                    writer.WriteEndObject();
                }
            }

            public override EditActionPlayAnimation ReadJson(JsonReader reader, System.Type objectType, EditActionPlayAnimation existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (hasExistingValue)
                    throw(new System.NotImplementedException());

                using (new IgnoreThisConverter(serializer, this))
                {
                    JObject jsonObject = JObject.Load(reader);
                    var ret = new EditActionPlayAnimation();
                    int animationIndex = jsonObject["animationIndex"].Value<int>();
                    if (animationIndex >= 0 && animationIndex < dataSet.animations.Count)
                        ret.animation = dataSet.animations[animationIndex];
                    else
                        ret.animation = null;
                    ret.faceIndex = jsonObject["faceIndex"].Value<int>();
                    ret.loopCount = jsonObject["loopCount"].Value<int>();
                    return ret;
                }
            }
        }

        public override EditAction Duplicate()
        {
            return new EditActionPlayAnimation()
            {
                animation = this.animation,
                faceIndex = this.faceIndex,
                loopCount = this.loopCount
            };
        }
        public override void ReplaceAnimation(Animations.EditAnimation oldAnimation, Animations.EditAnimation newAnimation)
        {
            if (animation == oldAnimation)
            {
                animation = newAnimation;
            }
        }

        public override void DeleteAnimation(Animations.EditAnimation animation)
        {
            if (this.animation == animation)
            {
                this.animation = null;
            }
        }

        public override bool DependsOnAnimation(Animations.EditAnimation animation)
        {
            return this.animation == animation;
        }

        public override IEnumerable<Animations.EditAnimation> CollectAnimations()
        {
            yield return animation;
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (animation != null)
            {
                builder.Append("play \"" + animation.name + "\"");
                if (loopCount > 1)
                {
                    builder.Append("x");
                    builder.Append(loopCount);
                }
                if (faceIndex != -1)
                {
                    builder.Append(" on face ");
                    builder.Append(faceIndex + 1);
                }
            }
            else
            {
                builder.Append("- Please select a Pattern -");
            }
            return builder.ToString();
        }
    };

    /// <summary>
    /// Action to play an animation, really! 
    /// </summary>
    [System.Serializable]
    public class EditActionPlayAudioClip
        : EditAction
    {
        [Name("Audio Clip")]
        public AudioClips.EditAudioClip clip;
        public override ActionType type { get { return ActionType.PlayAudioClip; } }
        public override Action ToAction(EditDataSet editSet, DataSet set)
        {
            return new ActionPlayAudioClip()
            {
                clipId = (byte)clip.id,
            };
        }

        /// <summary>
        /// Specialized converter
        /// </sumary>
        public class Converter
            : JsonConverter<EditActionPlayAudioClip>
        {
            AppDataSet dataSet;
            public Converter(AppDataSet dataSet)
            {
                this.dataSet = dataSet;
            }
            public override void WriteJson(JsonWriter writer, EditActionPlayAudioClip value, JsonSerializer serializer)
            {
                using (new IgnoreThisConverter(serializer, this))
                {
                    writer.WriteStartObject();
                    var clipIndex = dataSet.audioClips.IndexOf(value.clip);
                    writer.WritePropertyName("audioClipIndex");
                    serializer.Serialize(writer, clipIndex);
                    writer.WriteEndObject();
                }
            }

            public override EditActionPlayAudioClip ReadJson(JsonReader reader, System.Type objectType, EditActionPlayAudioClip existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (hasExistingValue)
                    throw(new System.NotImplementedException());

                using (new IgnoreThisConverter(serializer, this))
                {
                    JObject jsonObject = JObject.Load(reader);
                    var ret = new EditActionPlayAudioClip();
                    int clipIndex = jsonObject["audioClipIndex"].Value<int>();
                    if (clipIndex >= 0 && clipIndex < dataSet.audioClips.Count)
                        ret.clip = dataSet.audioClips[clipIndex];
                    else
                        ret.clip = null;
                    return ret;
                }
            }
        }

        public override EditAction Duplicate()
        {
            return new EditActionPlayAudioClip()
            {
                clip = this.clip,
            };
        }

        public override bool DependsOnAudioClip(AudioClips.EditAudioClip clip)
        {
            return this.clip == clip;
        }

        public override IEnumerable<AudioClips.EditAudioClip> CollectAudioClips()
        {
            if (this.clip != null)
            {
                yield return this.clip;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (clip != null)
            {
                builder.Append("play \"" + clip.name + "\"");
            }
            else
            {
                builder.Append("- Please select an Audio Clip -");
            }
            return builder.ToString();
        }
    };
}