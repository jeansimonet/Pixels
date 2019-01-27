#include "Animation.h"
#include "LEDs.h"
#include "Utils.h"

#define MAX_LEVEL (256)

using namespace Core;

/// <summary>
/// Dims the passed in color by the passed in intensity (normalized 0 - 255)
/// </summary>
uint32_t scaleColor(uint32_t refColor, byte intensity)
{
	byte r = getRed(refColor);
	byte g = getGreen(refColor);
	byte b = getBlue(refColor);
	return toColor(r * intensity / MAX_LEVEL, g * intensity / MAX_LEVEL, b * intensity / MAX_LEVEL);
}

/// <summary>
/// Returns the keyframe's color in uint32_t type!
/// </summary>
uint32_t RGBKeyframe::color() const
{
	return toColor(red, green, blue);
}

/// <summary>
/// Interpolate between keyframes of an animation curve
/// </summary>
/// <param name="time">The normalized time (0 - 255)</param>
/// <returns>The normalized intensity (0 - 255)</returns>
uint32_t AnimationTrack::evaluateNormalized(int time) const
{
	if (count == 0)
		return 0;

	// Find the first keyframe
	int nextIndex = 0;
	while (nextIndex < count && keyframes[nextIndex].time < time)
	{
		nextIndex++;
	}

	if (nextIndex == 0)
	{
		// The first keyframe is already after the requested time, clamp to first value
		return keyframes[nextIndex].color();
	}
	else if (nextIndex == count)
	{
		// The last keyframe is still before the requested time, clamp to the last value
		return keyframes[nextIndex - 1].color();
	}
	else
	{
		// Grab the prev and next keyframes
		auto& nextKeyframe = keyframes[nextIndex];
		auto& prevKeyframe = keyframes[nextIndex - 1];

		// Compute the interpolation parameter
		// To stick to integer math, we'll scale the values
		int scaler = 1024;
		int scaledPercent = (time - prevKeyframe.time) * scaler / (nextKeyframe.time - prevKeyframe.time);
		int scaledRed = prevKeyframe.red * (scaler - scaledPercent) + nextKeyframe.red * scaledPercent;
		int scaledGreen = prevKeyframe.green * (scaler - scaledPercent) + nextKeyframe.green * scaledPercent;
		int scaledBlue = prevKeyframe.blue * (scaler - scaledPercent) + nextKeyframe.blue * scaledPercent;
		return toColor(scaledRed / scaler, scaledGreen / scaler, scaledBlue / scaler);
	}
}

/// <summary>
/// Evaluate an animation track's intensity for a given time, in milliseconds.
/// Values outside the track's range are clamped to first or last keyframe value.
/// </summary>
/// <returns>A normalized intensity (0-255)</returns>
uint32_t AnimationTrack::evaluate(int time) const
{
	uint32_t ret = 0;
	if (time < startTime)
	{
		ret = evaluateNormalized(0);
	}
	else if (time >= startTime + duration)
	{
		ret = evaluateNormalized(256);
	}
	else
	{
		int scaler = MAX_LEVEL;
		int scaledTime = (time - startTime) * scaler / duration;
		ret = evaluateNormalized(scaledTime);
	}

	// Scale the return value
	return ret;
}

void AnimationTrack::AddKeyframe(byte time, byte red, byte green, byte blue)
{
	if (count < MAX_KEYFRAMES)
	{
		keyframes[count].time = time;
		keyframes[count].red = red;
		keyframes[count].green = green;
		keyframes[count].blue = blue;
		count++;
	}
}

/// <summary>
/// Kick off the animation
/// </summary>
void Animation::start() const
{
	// Nothing to do here!
}

/// <summary>
/// Computes the list of LEDs that need to be on, and what their intensities should be
/// based on the different tracks of this animation.
/// </summary>
/// <param name="time">The animation time (in milliseconds)</param>
/// <param name="retIndices">the return list of LED indices to fill, max size should be at least 21, the total number of leds</param>
/// <param name="retColors">the return list of LED color to fill, max size should be at least 21, the total number of leds</param>
/// <returns>The number of leds/intensities added to the return array</returns>
int Animation::updateLEDs(int time, int retIndices[], uint32_t retColors[]) const
{
	for (int i = 0; i < count; ++i)
	{
		retIndices[i] = tracks[i].ledIndex;
		retColors[i] = tracks[i].evaluate(time);
	}
	return count;
}

/// <summary>
/// Clear all LEDs controlled by this animation, for instance when
/// the anim gets interrupted.
/// </summary>
int Animation::stop(int retIndices[]) const
{
	for (int i = 0; i < count; ++i)
	{
		retIndices[i] = tracks[i].ledIndex;
	}
	return count;
}

/// <summary>
/// Returns the duration of this animation
/// </summary>
int Animation::totalDuration() const
{
	return duration;
}

/// <summary>
/// Computes how much space this animation actually takes
/// </summary>
int Animation::ComputeByteSize() const
{
	return ComputeByteSizeForTracks(count);
}

/// <summary>
/// Helper to add tracks to an animation. Used for testing
/// </summary>
bool Animation::SetTrack(const AnimationTrack& track, int index)
{
	if (index < count)
	{
		tracks[index] = track;
		short trackEnd = track.startTime + track.duration;
		if (duration < trackEnd)
		{
			duration = trackEnd;
		}
	}
}

int Animation::TrackCount() const
{
	return count;
}

/// <summary>
/// Returns a track
/// </summary>
const AnimationTrack& Animation::GetTrack(int index) const
{
	return tracks[index];
}


/// <summary>
/// Computes how much space an animation would actually take, given an expected number of tracks
/// </summary>
int Animation::ComputeByteSizeForTracks(int trackCount)
{
	return sizeof(Animation) + sizeof(AnimationTrack) * (trackCount - 1);
}

/// <summary>
/// Allocates a new animation, client must free it!
/// </summary>
Animation* Animation::AllocateAnimation(int trackCount)
{
	Animation* ret = (Animation*)malloc(ComputeByteSizeForTracks(trackCount));
	ret->duration = 0;
	ret->count = trackCount;
}

