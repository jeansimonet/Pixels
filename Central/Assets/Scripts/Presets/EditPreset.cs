using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Presets
{
    [System.Serializable]
    public class EditDieAssignment
        : EditObject
    {
        public EditDie die;
        public Behaviors.EditBehavior behavior;
    }

    class EditDieAssignmentConverter
        : JsonConverter<EditDieAssignment>
    {
        AppDataSet dataSet;
        public EditDieAssignmentConverter(AppDataSet dataSet)
        {
            this.dataSet = dataSet;
        }

        public override void WriteJson(JsonWriter writer, EditDieAssignment value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("deviceId");
            if (value.die != null)
                serializer.Serialize(writer, value.die.deviceId);
            else
                serializer.Serialize(writer, (ulong)0);
            writer.WritePropertyName("behaviorIndex");
            serializer.Serialize(writer, dataSet.behaviors.IndexOf(value.behavior));
            writer.WriteEndObject();
        }

        public override EditDieAssignment ReadJson(JsonReader reader, System.Type objectType, EditDieAssignment existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (hasExistingValue)
                throw new System.NotImplementedException();

            var ret = new EditDieAssignment();    
            JObject jsonObject = JObject.Load(reader);
            System.UInt64 deviceId = jsonObject["deviceId"].ToObject<System.UInt64>();
            ret.die = dataSet.dice.Find(d => d.deviceId == deviceId);
            int behaviorIndex = jsonObject["behaviorIndex"].Value<int>();
            if (behaviorIndex >= 0 && behaviorIndex < dataSet.behaviors.Count)
                ret.behavior = dataSet.behaviors[behaviorIndex];
            else
                ret.behavior = null;
            return ret;
        }
    }

    [System.Serializable]
    public class EditPreset
        : EditObject
    {
        public string name;
        public List<EditDieAssignment> dieAssignments = new List<EditDieAssignment>();

        public bool CheckDependency(EditDie die)
        {
            return dieAssignments.Any(ass => ass.die == die);
        }
    }
}

