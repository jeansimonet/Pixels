#pragma once

#include <stdint.h>

#pragma pack(push, 1)

namespace Animations
{
    enum AnimationEvent
    {
		AnimationEvent_None = 0,
		AnimationEvent_Hello,
		AnimationEvent_Connected,
		AnimationEvent_Disconnected,
        AnimationEvent_LowBattery,
        AnimationEvent_ChargingStart,
        AnimationEvent_ChargingDone,
        AnimationEvent_ChargingError,
        AnimationEvent_Handling,
		AnimationEvent_Rolling,
		AnimationEvent_OnFace,
		AnimationEvent_Crooked,
		AnimationEvent_Battle_ShowTeam,
		AnimationEvent_Battle_FaceUp,
		AnimationEvent_Battle_WaitingForBattle,
		AnimationEvent_Battle_Duel,
		AnimationEvent_Battle_DuelWin,
		AnimationEvent_Battle_DuelLose,
		AnimationEvent_Battle_DuelDraw,
		AnimationEvent_Battle_TeamWin,
		AnimationEvent_Battle_TeamLoose,
		AnimationEvent_Battle_TeamDraw,
        // Etc...
        AnimationEvent_Count
    };

	enum SpecialColor
	{
		SpecialColor_None = 0,
		SpecialColor_Face,
		SpecialColor_Heat
	};

	const char* getEventName(AnimationEvent event);

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
		uint32_t color(void* token) const;// unpack the color using the lookup table from the animation set

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

		const RGBKeyframe& getKeyframe(uint16_t keyframeIndex) const;
		uint32_t evaluate(void* token, int time) const;
	};

	/// <summary>
	/// AnimationTrack track is essentially an animation associated with a track
	/// size: 4 bytes (+ the actual keyframe data)
	/// </summary>
	struct AnimationTrack
	{
	public:
		uint16_t trackOffset; // offset into a global keyframe buffer
		uint8_t ledIndex;	// 0 - 20
		uint8_t padding;

		const RGBTrack& getTrack() const;
		uint32_t evaluate(void* token, int time) const;
	};

	/// <summary>
	/// A keyframe-based animation
	/// size: 8 bytes (+ actual track and keyframe data)
	/// </summary>
	struct Animation
	{
	public:
		uint16_t duration; // in ms
		uint16_t tracksOffset; // offset into a global buffer of tracks
		uint16_t trackCount;
		uint8_t animationEvent; // is really AnimationEvent
		uint8_t specialColorType; // is really SpecialColor

	public:
		const AnimationTrack& GetTrack(int index) const;
		int updateLEDs(void* token, int time, int retIndices[], uint32_t retColors[]) const;
		int stop(int retIndices[]) const;
	};

}

#pragma pack(pop)
