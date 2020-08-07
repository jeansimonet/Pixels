using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;


namespace Animations
{
	/// <summary>
	/// Defines the types of Animation Presets we have/support
	/// </summary>
    [EnumRange((int)AnimationType.Simple, (int)AnimationType.Keyframed)]
	public enum AnimationType : byte
	{
		Unknown = 0,
		Simple,
		Rainbow,
		Keyframed,
	};

	/// <summary>
	/// Base class for animation presets. All presets have a few properties in common.
	/// Presets are stored in flash, so do not have methods or vtables or anything like that.
	/// </summary>
	public interface Animation
	{
		AnimationType type { get; set; }
		byte padding_type { get; set; } // to keep duration 16-bit aligned
		ushort duration { get; set; } // in ms
        AnimationInstance CreateInstance();
	};

	/// <summary>
	/// Animation instance data, refers to an animation preset but stores the instance data and
	/// (derived classes) implements logic for displaying the animation.
	/// </summary>
	public abstract class AnimationInstance
	{
		public Animation animationPreset;
		public int startTime; //ms
		public byte remapFace;
		public bool loop;

        protected DataSet set;

		public AnimationInstance(Animation animation)
        {
            animationPreset = animation;
        }

		public virtual void start(DataSet _set, int _startTime, byte _remapFace, bool _loop)
        {
            set = _set;
            startTime = _startTime;
            remapFace = _remapFace;
            loop = _loop;
        }

		public abstract int updateLEDs(DataSet set, int ms, int[] retIndices, uint[] retColors);
		public abstract int stop(DataSet set, int[] retIndices);
	};
}
