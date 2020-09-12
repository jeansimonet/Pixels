using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Behaviors
{
    /// <summary>
    /// The different types of action we support. Yes, yes, it's only one right now :)
    /// </summary>
    public enum ActionType : byte
    {
        Unknown = 0,
        PlayAnimation,
        PlayAudioClip,
    };

    /// <summary>
    /// Base interface for Actions. Stores the actual type so that we can cast the data
    /// to the proper derived type and access the parameters.
    /// </summary>
    public interface Action
    {
        ActionType type {get; set; }
    };

    /// <summary>
    /// Action to play an animation, really! 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public class ActionPlayAnimation
        : Action
    {
        public ActionType type { get; set; } = ActionType.PlayAnimation;
        public byte animIndex;
        public byte faceIndex;
        public byte loopCount;
    };


    /// <summary>
    /// Action to play a sound! 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public class ActionPlayAudioClip
        : Action
    {
        public ActionType type { get; set; } = ActionType.PlayAudioClip;
        public byte paddingType;
        public ushort clipId;
    };
}
