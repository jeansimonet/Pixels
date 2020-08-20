#pragma once

#include "animations/Animation.h"

namespace Animations
{
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
    /// Stores a single keyframe of an LED animation
    /// size: 2 bytes, split this way:
    /// - 9 bits: time 0 - 511 in 50th of a second (i.e )
    ///   + 1    -> 0.02s
    ///   + 500  -> 10s
    /// - 7 bits: intensity (0 - 127)
    /// </summary>
    struct Keyframe
    {
		uint16_t timeAndIntensity;

        uint16_t time() const;
        uint8_t intensity() const;

        void setTimeAndIntensity(uint16_t timeInMS, uint8_t intensity);
    };
}