using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dice
{
    [System.Serializable]
    public class EditDie
    {
        public string name;
        public System.UInt64 deviceId;
        public int faceCount; // Which kind of dice this is
        public DesignAndColor designAndColor; // Physical look

        [JsonIgnore]
        public Behaviors.EditBehavior currentBehavior;
        public int currentBehaviorIndex;

        public delegate void DieFoundLostEvent(EditDie editDie);
        [JsonIgnore]
        public DieFoundLostEvent onDieFound;
        [JsonIgnore]
        public DieFoundLostEvent onDieWillBeLost;

        [JsonIgnore]
        public Die die
        {
            get { return _die; }
            set
            {
                if (_die != null)
                {
                    onDieWillBeLost?.Invoke(this);
                }
                _die = value;
                if (_die != null)
                {
                    // We should check die information (name, design, hash)
                    onDieFound?.Invoke(this);
                }
            }
        }
        [JsonIgnore]
        Die _die;

        public void OnBeforeSerialize()
        {
            currentBehaviorIndex = AppDataSet.Instance.behaviors.IndexOf(currentBehavior);
        }

        public void OnAfterDeserialize()
        {
            if (currentBehaviorIndex >= 0 && currentBehaviorIndex < AppDataSet.Instance.behaviors.Count)
                currentBehavior = AppDataSet.Instance.behaviors[currentBehaviorIndex];
            else
                currentBehavior = null;
        }
    }
}