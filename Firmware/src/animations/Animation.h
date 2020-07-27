#pragma once

#include <stdint.h>

#pragma pack(push, 1)

namespace Animations
{
	enum SpecialColor
	{
		SpecialColor_None = 0,
		SpecialColor_Face,
		SpecialColor_ColorWheel,
		SpecialColor_Heat_Current,
		SpecialColor_Heat_Start,
	};

	class IAnimationSpecialColorToken;

	/// <summary>
	/// Stores a single keyframe of an LED animation
	/// size: 2 bytes, split this way:
	/// - 9 bits: time 0 - 511 in 50th of a second (i.e )
	///   + 1    -> 0.02s
	///   + 500  -> 10s
	/// - 7 bits: color lookup (128 values)
	/// </summary>
	struct RGBKeyframe
	{
	public:
		uint16_t timeAndColor;

		uint16_t time() const; // unpack the time in ms
		uint32_t color(const IAnimationSpecialColorToken* token) const;// unpack the color using the lookup table from the animation set

		void setTimeAndColorIndex(uint16_t timeInMS, uint16_t colorIndex);
	};

	/// <summary>
	/// An animation track is essentially an animation curve for a specific LED.
	/// size: 4 bytes (+ the actual keyframe data).
	
	/// Note: Consider compressing into 2 bytes, for instance:
	/// - 12 bits for offset (to cover a max of 4096 keyframes in the buffer, or 8kB of data)
	/// - 4 bits for length (16 keyframes per track)
	/// Also consider storing a time offset...
	/// </summary>
	struct RGBTrack
	{
	public:
		uint16_t keyframesOffset; // offset into a global keyframe buffer
		uint8_t keyFrameCount;		// Keyframe count
		uint8_t padding;

		uint16_t getDuration() const;
		const RGBKeyframe& getKeyframe(uint16_t keyframeIndex) const;
		uint32_t evaluate(const IAnimationSpecialColorToken* token, int time) const;
	};

	/// <summary>
	/// LEDTrack is essentially an animation associated with an led
	/// size: 4 bytes (+ the actual keyframe data)
	/// </summary>
	struct LEDTrack
	{
	public:
		uint16_t trackOffset; // offset into a global keyframe buffer
		uint8_t ledIndex;	// 0 - 20
		uint8_t padding;

		const RGBTrack& getLEDTrack() const;
		uint32_t evaluate(const IAnimationSpecialColorToken* token, int time) const;
	};

	/// <summary>
	/// Defines the types of Animation Presets we have/support
	/// </summary>
	enum AnimationType : uint8_t
	{
		Animation_Unknown = 0,
		Animation_Simple,
		Animation_Rainbow,
		Animation_Keyframed,
	};

	/// <summary>
	/// Base struct for animation presets. All presets have a few properties in common.
	/// Presets are stored in flash, so do not have methods or vtables or anything like that.
	/// </summary>
	struct Animation
	{
		AnimationType type;
		uint8_t padding_type; // to keep duration 16-bit aligned
		uint16_t duration; // in ms
	};

	/// <summary>
	/// Animation instance data, refers to an animation preset but stores the instance data and
	/// (derived classes) implements logic for displaying the animation.
	/// </summary>
	class AnimationInstance
	{
	public:
		Animation const * animationPreset;
		int startTime; //ms
		uint8_t remapFace;
		bool loop;

	protected:
		AnimationInstance(const Animation* preset);

	public:
		virtual ~AnimationInstance();
		virtual void start(int _startTime, uint8_t _remapFace, bool _loop);
		virtual int animationSize() const = 0;
		virtual int updateLEDs(int ms, int retIndices[], uint32_t retColors[]) = 0;
		virtual int stop(int retIndices[]) = 0;
	};

	/// <summary>
	/// Special interface class to let RGB keyframes access (and blend) with special colors.
	/// </summary>
	class IAnimationSpecialColorToken
	{
	public:
		virtual uint32_t getColor(uint32_t colorIndex) const = 0;
	};

	Animations::AnimationInstance* createAnimationInstance(int animationIndex);
	Animations::AnimationInstance* createAnimationInstance(const Animations::Animation* preset);
	void destroyAnimationInstance(Animations::AnimationInstance* animationInstance);

}

#pragma pack(pop)
