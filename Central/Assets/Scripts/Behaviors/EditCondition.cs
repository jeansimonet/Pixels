using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Behaviors
{
    [System.Serializable]
    public abstract class EditCondition
    {
        public abstract ConditionType type { get; }
        public abstract Condition ToCondition(EditDataSet editSet, DataSet set);
        public abstract string ToJson();
        public abstract void FromJson(string Json);
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
                default:
                    throw new System.Exception("Unknown condition type");
            }
        }
    }

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
        public override string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public override void FromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
        public override EditCondition Duplicate()
        {
            return new EditConditionHandling();
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
        public override Condition ToCondition(EditDataSet editSet, DataSet set)
        {
            return new ConditionRolling();
        }
        public override string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public override void FromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
        public override EditCondition Duplicate()
        {
            return new EditConditionRolling();
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
        public override string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public override void FromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
        public override EditCondition Duplicate()
        {
            return new EditConditionCrooked();
        }
    };

    /// <summary>
    /// Condition that triggers when the die has landed on a face
    /// </summary>
    [System.Serializable]
    public class EditConditionFaceCompare
        : EditCondition
    {
        public byte faceIndex;
        public ConditionFaceCompare_Flags flags;

        public override ConditionType type { get { return ConditionType.FaceCompare; } }
        public override Condition ToCondition(EditDataSet editSet, DataSet set)
        {
            return new ConditionFaceCompare()
            {
                faceIndex = this.faceIndex,
                flags = this.flags
            };
        }
        public override string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public override void FromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
        public override EditCondition Duplicate()
        {
            return new EditConditionFaceCompare()
            {
                faceIndex = this.faceIndex,
                flags = this.flags
            };
        }
    };

    /// <summary>
    /// Condition that triggers on a life state event
    /// </sumary>
    [System.Serializable]
    public class EditConditionHelloGoodbye
        : EditCondition
    {
        public ConditionHelloGoodbye_Flags flags;

        public override ConditionType type { get { return ConditionType.HelloGoodbye; } }
        public override Condition ToCondition(EditDataSet editSet, DataSet set)
        {
            return new ConditionHelloGoodbye()
            {
                flags = this.flags
            };
        }
        public override string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public override void FromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
        public override EditCondition Duplicate()
        {
            return new EditConditionHelloGoodbye()
            {
                flags = this.flags
            };
        }
    };

    /// <summary>
    /// Condition that triggers on connection events
    /// </sumary>
    [System.Serializable]
    public class EditConditionConnectionState
        : EditCondition
    {
        public ConditionConnectionState_Flags flags;

        public override ConditionType type { get { return ConditionType.ConnectionState; } }
        public override Condition ToCondition(EditDataSet editSet, DataSet set)
        {
            return new ConditionConnectionState()
            {
                flags = this.flags
            };
        }
        public override string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public override void FromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
        public override EditCondition Duplicate()
        {
            return new EditConditionConnectionState()
            {
                flags = this.flags
            };
        }
    };

    /// <summary>
    /// Condition that triggers on battery state events
    /// </sumary>
    [System.Serializable]
    public class EditConditionBatteryState
        : EditCondition
    {
        public ConditionBatteryState_Flags flags;

        public override ConditionType type { get { return ConditionType.BatteryState; } }
        public override Condition ToCondition(EditDataSet editSet, DataSet set)
        {
            return new ConditionBatteryState()
            {
                flags = this.flags
            };
        }
        public override string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public override void FromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
        public override EditCondition Duplicate()
        {
            return new EditConditionBatteryState()
            {
                flags = this.flags
            };
        }
    };

}
