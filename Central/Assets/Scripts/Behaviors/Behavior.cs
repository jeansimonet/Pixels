using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Behaviors
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public class Behavior
    {
        public ushort rulesOffset;
        public ushort rulesCount;
    }
}
