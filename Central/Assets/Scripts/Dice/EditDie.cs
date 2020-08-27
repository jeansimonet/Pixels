using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dice
{
    [System.Serializable]
    public class EditDie
    {
        public string name;
        public System.UInt64 deviceId;
        public int faceCount; // Which kind of dice this is
        public DesignAndColor designAndColor; // Physical look
        public uint dataSetHash;
    }
}