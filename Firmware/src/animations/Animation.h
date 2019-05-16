// KeyframedLEDs.h

#ifndef _KEYFRAMEDLEDS_h
#define _KEYFRAMEDLEDS_h

#include <stdint.h>

#define MAX_KEYFRAMES (8)

/// <summary>
/// Stores a single keyframe of an LED animation
/// </summary>
struct RGBKeyframe
{
public:
	uint8_t time;		// 0 - 255 (normalized)
	uint8_t red;		// 0 - 255 (normalized)
	uint8_t green;
	uint8_t blue;

	uint32_t color() const;
};

/// <summary>
/// An animation track is essentially a scaled animation curve for a
/// specific LED. It defines how long the curve is stretched over and when it starts.
/// With 8 keyframes, on the die, a track should take 8 * 4 + 8 = 40 uint8_ts
/// </summary>
struct AnimationTrack
{
public:
	RGBKeyframe keyframes[MAX_KEYFRAMES];
	short startTime;	// ms
	short duration;		// ms
	short padding;
	uint8_t ledIndex;		// 0 - 20
	uint8_t count;

	uint32_t evaluateNormalized(int time) const;
	uint32_t evaluate(int time) const;
	void AddKeyframe(uint8_t time, uint8_t red, uint8_t green, uint8_t blue);
};

/// <summary>
/// A keyframe-based animation
/// </summary>
class Animation
{
private:
	short duration; // ms
	short count;
	AnimationTrack tracks[1]; // Actual size is determined by 'count', the data 'overflows' the end of the struct!

public:
	// Interface implementation
	void start() const;
	int updateLEDs(int time, int retIndices[], uint32_t retColors[]) const;
	int stop(int retIndices[]) const;
	int totalDuration() const;
	int Computeuint8_tSize() const;

	// Used for testing, manually creating animations
	bool SetTrack(const AnimationTrack& track, int index);
	int TrackCount() const;
	const AnimationTrack& GetTrack(int index) const;

	static int Computeuint8_tSizeForTracks(int trackCount);
	static Animation* AllocateAnimation(int trackCount);
};

#endif

