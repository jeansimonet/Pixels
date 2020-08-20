using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Presets
{
    [System.Serializable]
    public class EditDie
    {
        public string name;
        public System.UInt64 deviceId;
        public int faceCount; // Which kind of dice this is
        public DiceVariants.DesignAndColor designAndColor; // Physical look
        public uint dataSetHash;
    }
}