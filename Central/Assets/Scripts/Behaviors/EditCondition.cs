using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Behaviors
{
    [System.Serializable]
    public abstract class EditCondition
        : EditObject
    {
        [JsonIgnore]
        public abstract ConditionType type { get; }
        public abstract Condition ToCondition(EditDataSet editSet, DataSet set);
        public abstract EditCondition Duplicate();

        public static EditCondition Create(ConditionType type)
        {
            switch (type)
            {
                case ConditionType.Handling:
                    return new EditConditionHandling();
                case ConditionType.Rolling:
                    return new EditConditionRolling();
                case ConditionType.Crooked:
                    return new EditConditionCrooked();
                case ConditionType.FaceCompare:
                    return new EditConditionFaceCompare();
                case ConditionType.HelloGoodbye:
                    return new EditConditionHelloGoodbye();
                case ConditionType.ConnectionState:
                    return new EditConditionConnectionState();
                case ConditionType.BatteryState:
                    return new EditConditionBatteryState();
                case ConditionType.Idle:
                    return new EditConditionIdle();
                default:
                    throw new System.Exception("Unknown condition type");
            }
        }

        public static System.Type GetConditionType(ConditionType type)
        {
            switch (type)
            {
                case ConditionType.Handling:
                    return typeof(EditConditionHandling);
                case ConditionType.Rolling:
                    return typeof(EditConditionRolling);
                case ConditionType.Crooked:
                    return typeof(EditConditionCrooked);
                case ConditionType.FaceCompare:
                    return typeof(EditConditionFaceCompare);
                case ConditionType.HelloGoodbye:
                    return typeof(EditConditionHelloGoodbye);
                case ConditionType.ConnectionState:
                    return typeof(EditConditionConnectionState);
                case ConditionType.BatteryState:
                    return typeof(EditConditionBatteryState);
                case ConditionType.Idle:
                    return typeof(EditConditionIdle);
                default:
                    throw new System.Exception("Unknown condition type");
            }
        }
    }

    public class EditConditionConverter
        : JsonConverter<EditCondition>
    {
        public override void WriteJson(JsonWriter writer, EditCondition value, JsonSerializer serializer)
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

        public override EditCondition ReadJson(JsonReader reader, System.Type objectType, EditCondition existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (hasExistingValue)
                throw(new System.NotImplementedException());

            using (new IgnoreThisConverter(serializer, this))
            {
                JObject jsonObject = JObject.Load(reader);
                var type = jsonObject["type"].ToObject<ConditionType>();
                var ret = (EditCondition)jsonObject["data"].ToObject(EditCondition.GetConditionType(type), serializer);
                return ret;
            }
        }
    }

    /// <summary>
    /// Condition that triggers when the die is being handled
    /// </summary>
    [System.Serializable]
    public class EditConditionIdle
        : EditCondition
    {
        public override ConditionType type { get { return ConditionType.Idle; } }
        [Slider, FloatRange(0.5f, 30.0f, 0.5f), Units("sec")]
        public float period = 10.0f;
        public override Condition ToCondition(EditDataSet editSet, DataSet set)
        {
            var ret = new ConditionIdle();
            ret.repeatPeriodMs = (ushort)Mathf.RoundToInt(period * 1000.0f);
            return ret;
        }
        public override EditCondition Duplicate()
        {
            return new EditConditionIdle() { period = period };
        }
        public override string ToString()
        {
            return "die is idle";
        }
    };

    /// <summary>
    /// Condition that triggers when the die is being handled
    /// </summary>
    [System.Serializable]
    public class EditConditionHandling
        : EditCondition
    {
        public override ConditionType type { get { return ConditionType.Handling; } }
        public override Condition ToCondition(EditDataSet editSet, DataSet set)
        {
            return new ConditionHandling();
        }
        public override EditCondition Duplicate()
        {
            return new EditConditionHandling();
        }
        public override string ToString()
        {
            return "die is picked up";
        }
    };

    /// <summary>
    /// Condition that triggers when the die is being rolled
    /// </summary>
    [System.Serializable]
    public class EditConditionRolling
        : EditCondition
    {
        public override ConditionType type { get { return ConditionType.Rolling; } }
        [Slider, FloatRange(0.5f, 5.0f, 0.1f), Units("sec")]
        public float recheckAfter = 1.0f;
        public override Condition ToCondition(EditDataSet editSet, DataSet set)
        {
            var ret = new ConditionRolling();
            ret.repeatPeriodMs = (ushort)Mathf.RoundToInt(recheckAfter * 1000.0f);
            return ret;
        }
        public override EditCondition Duplicate()
        {
            return new EditConditionRolling() { recheckAfter = recheckAfter };
        }
        public override string ToString()
        {
            return "die is rolling";
        }
    };

    /// <summary>
    /// Condition that triggers when the die has landed by is crooked
    /// </summary>
    [System.Serializable]
    public class EditConditionCrooked
        : EditCondition
    {
        public override ConditionType type { get { return ConditionType.Crooked; } }
        public override Condition ToCondition(EditDataSet editSet, DataSet set)
        {
            return new ConditionCrooked();
        }
        public override EditCondition Duplicate()
        {
            return new EditConditionCrooked();
        }
        public override string ToString()
        {
            return "die is crooked";
        }
    };

    /// <summary>
    /// Condition that triggers when the die has landed on a face
    /// </summary>
    [System.Serializable]
    public class EditConditionFaceCompare
        : EditCondition
    {
        [Bitfield, Name("Comparison")]
        public ConditionFaceCompare_Flags flags;
        [FaceIndex, IntRange(0, 19), Name("Than")]
        public int faceIndex;

        public override ConditionType type { get { return ConditionType.FaceCompare; } }
        public override Condition ToCondition(EditDataSet editSet, DataSet set)
        {
            return new ConditionFaceCompare()
            {
                faceIndex = (byte)this.faceIndex,
                flags = this.flags
            };
        }
        public override EditCondition Duplicate()
        {
            return new EditConditionFaceCompare()
            {
                faceIndex = this.faceIndex,
                flags = this.flags
            };
        }
        public override string ToString()
        {
            if (flags != 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("roll is ");
                if (flags == (ConditionFaceCompare_Flags.Less | ConditionFaceCompare_Flags.Equal | ConditionFaceCompare_Flags.Greater))
                {
                    builder.Append("any");
                }
                else if (flags == (ConditionFaceCompare_Flags.Less | ConditionFaceCompare_Flags.Greater))
                {
                    builder.Append("not equal to ");
                    builder.Append(faceIndex + 1);
                }
                else if ((flags & ConditionFaceCompare_Flags.Less) != 0)
                {
                    builder.Append("less");
                    if ((flags & ConditionFaceCompare_Flags.Equal) != 0)
                    {
                        builder.Append(" or equal");
                    }
                    builder.Append(" to ");
                    builder.Append(faceIndex + 1);
                }
                else if ((flags & ConditionFaceCompare_Flags.Greater) != 0)
                {
                    builder.Append("greater");
                    if ((flags & ConditionFaceCompare_Flags.Equal) != 0)
                    {
                        builder.Append(" or equal");
                    }
                    builder.Append(" to ");
                    builder.Append(faceIndex + 1);
                }
                else if (flags == ConditionFaceCompare_Flags.Equal)
                {
                    builder.Append("equal to ");
                    builder.Append(faceIndex + 1);
                }
                return builder.ToString();
            }
            else
            {
                return "No condition";
            }
        }
    };

    /// <summary>
    /// Condition that triggers on a life state event
    /// </sumary>
    [System.Serializable]
    public class EditConditionHelloGoodbye
        : EditCondition
    {
        [Bitfield, Name("Hello / Goodbye")]
        public ConditionHelloGoodbye_Flags flags;

        public override ConditionType type { get { return ConditionType.HelloGoodbye; } }
        public override Condition ToCondition(EditDataSet editSet, DataSet set)
        {
            return new ConditionHelloGoodbye()
            {
                flags = this.flags
            };
        }
        public override EditCondition Duplicate()
        {
            return new EditConditionHelloGoodbye()
            {
                flags = this.flags
            };
        }
        public override string ToString()
        {
            if (flags != 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("die is ");
                string or = "";
                if ((flags & ConditionHelloGoodbye_Flags.Hello) != 0)
                {
                    builder.Append("waking up");
                    or = " or ";
                }
                if ((flags & ConditionHelloGoodbye_Flags.Goodbye) != 0)
                {
                    builder.Append(or + "going to sleep");
                }
                return builder.ToString();
            }
            else
            {
                return "No condition";
            }
        }
    };

    /// <summary>
    /// Condition that triggers on connection events
    /// </sumary>
    [System.Serializable]
    public class EditConditionConnectionState
        : EditCondition
    {
        [Bitfield, Name("Connection Event")]
        public ConditionConnectionState_Flags flags;

        public override ConditionType type { get { return ConditionType.ConnectionState; } }
        public override Condition ToCondition(EditDataSet editSet, DataSet set)
        {
            return new ConditionConnectionState()
            {
                flags = this.flags
            };
        }
        public override EditCondition Duplicate()
        {
            return new EditConditionConnectionState()
            {
                flags = this.flags
            };
        }
        public override string ToString()
        {
            if (flags != 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("die is ");
                string or = "";
                if ((flags & ConditionConnectionState_Flags.Connected) != 0)
                {
                    builder.Append("connected");
                    or = " or ";
                }
                if ((flags & ConditionConnectionState_Flags.Disconnected) != 0)
                {
                    builder.Append(or + "disconnect");
                }
                return builder.ToString();
            }
            else
            {
                return "No condition";
            }
        }
    };

    /// <summary>
    /// Condition that triggers on battery state events
    /// </sumary>
    [System.Serializable]
    public class EditConditionBatteryState
        : EditCondition
    {
        [Bitfield, Name("Battery State")]
        public ConditionBatteryState_Flags flags;
        [Slider, FloatRange(5.0f, 60.0f, 1.0f), Units("sec")]
        public float recheckAfter = 1.0f;

        public override ConditionType type { get { return ConditionType.BatteryState; } }
        public override Condition ToCondition(EditDataSet editSet, DataSet set)
        {
            return new ConditionBatteryState()
            {
                flags = this.flags,
                repeatPeriodMs = (ushort)Mathf.RoundToInt(recheckAfter * 1000.0f)
            };
        }
        public override EditCondition Duplicate()
        {
            return new EditConditionBatteryState()
            {
                flags = this.flags,
                recheckAfter = this.recheckAfter
            };
        }
        public override string ToString()
        {
            if (flags != 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("battery is ");
                string or = "";
                if ((flags & ConditionBatteryState_Flags.Ok) != 0)
                {
                    builder.Append("ok");
                    or = " or ";
                }
                if ((flags & ConditionBatteryState_Flags.Low) != 0)
                {
                    builder.Append(or + "low");
                    or = " or ";
                }
                if ((flags & ConditionBatteryState_Flags.Charging) != 0)
                {
                    builder.Append(or + "charing");
                    or = " or ";
                }
                if ((flags & ConditionBatteryState_Flags.Done) != 0)
                {
                    builder.Append(or + "done charging");
                }
                return builder.ToString();
            }
            else
            {
                return "No condition";
            }
        }
    };

}
