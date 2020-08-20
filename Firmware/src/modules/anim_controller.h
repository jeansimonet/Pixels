#pragma once

#include "stdint.h"
#include "animations/Animation.h"

namespace Modules
{
	/// <summary>
	/// Manages a set of running animations, talking to the LED controller
	/// to tell it what LEDs must have what intensity at what time.
	/// </summary>
	namespace AnimController
	{
		void stopAtIndex(int animIndex);
		void removeAtIndex(int animIndex);
		void update();
		void update(int ms);

		void init();
		void stop();
		void start();

		void play(int animIndex, uint8_t remapFace = 0, bool loop = false);
		void stop(int animIndex, uint8_t remapFace = 0);
		void stopAll();

		int getCurrentRainbowOffset();
		float getCurrentHeat();
	}
}

