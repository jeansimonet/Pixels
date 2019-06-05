#include "anim_controller.h"
#include "animations/animation.h"
#include "drivers_nrf/timers.h"
#include "utils/utils.h"
#include "config/board_config.h"
#include "drivers_hw/apa102.h"
#include "app_error.h"

using namespace Animations;
using namespace Modules;
using namespace DriversNRF;
using namespace DriversHW;

#define MAX_ANIMS (20)
#define TIMER2_RESOLUTION 33 //ms

namespace Modules
{
namespace AnimController
{
	/// <summary>
	/// Internal helper struct used to store a running animation instance
	/// </summary>
	struct AnimInstance
	{
		Animation const * animation;
		int startTime; //ms

		AnimInstance()
			: animation(nullptr)
			, startTime(0)
		{
		}
	};

	// Our currently running animations
	AnimInstance animations[MAX_ANIMS];
	int animationCount;

	APP_TIMER_DEF(animControllerTimer);
	// To be passed to the timer
	void animationControllerUpdate(void* param)
	{
		update(Utils::millis());
	}

	/// <summary>
	/// Kick off the animation controller, registering it with the Timer system
	/// </summary>
	void init()
	{
		animationCount = 0;
		Timers::createTimer(&animControllerTimer, APP_TIMER_MODE_REPEATED, animationControllerUpdate);
		Timers::startTimer(animControllerTimer, TIMER2_RESOLUTION, NULL);
	}

	/// <summary>
	/// Update all currently running animations, and performing housekeeping when necessary
	/// </summary>
	/// <param name="ms">Current global time in milliseconds</param>
	void update(int ms)
	{
		for (int i = 0; i < animationCount; ++i)
		{
			auto& anim = animations[i];
			int animTime = ms - anim.startTime;
			if (animTime > anim.animation->duration)
			{
				// The animation is over, get rid of it!
				removeAtIndex(i);

				// Decrement loop counter since we just replaced the current anim
				i--;
			}
			else
			{
				// Update the leds
				int ledIndices[MAX_LED_COUNT];
				uint32_t colors[MAX_LED_COUNT];
				int ledCount = anim.animation->updateLEDs(animTime, ledIndices, colors);
				APA102::setPixelColors(ledIndices, colors, ledCount);
			}
		}
		APA102::show();
	}

	/// <summary>
	/// Stop updating animations
	/// </summary>
	void stop()
	{
		Timers::stopTimer(animControllerTimer);
	}

	/// <summary>
	/// Add an animation to the list of running animations
	/// </summary>
	void play(const Animation* anim)
	{
		int prevAnimIndex = 0;
		for (; prevAnimIndex < animationCount; ++prevAnimIndex)
		{
			if (animations[prevAnimIndex].animation == anim)
			{
				break;
			}
		}

		int ms = Utils::millis();
		if (prevAnimIndex < animationCount)
		{
			// Replace a previous animation
			stopAtIndex(prevAnimIndex);
			animations[prevAnimIndex].startTime = ms;
		}
		else if (animationCount < MAX_ANIMS)
		{
			// Add a new animation
			animations[animationCount].animation = anim;
			animations[animationCount].startTime = ms;
			animationCount++;
		}
		// Else there is no more room
	}

	/// <summary>
	/// Forcibly stop a currently running animation
	/// </summary>
	void stop(const Animation* anim)
	{
		int prevAnimIndex = 0;
		for (; prevAnimIndex < animationCount; ++prevAnimIndex)
		{
			if (animations[prevAnimIndex].animation == anim)
			{
				break;
			}
		}

		if (prevAnimIndex < animationCount)
		{
			removeAtIndex(prevAnimIndex);
		}
		// Else the animation isn't playing
	}

	/// <summary>
	/// Stop all currently running animations
	/// </summary>
	void stopAll()
	{
		for (int i = 0; i < animationCount; ++i)
		{
			animations[i].animation = nullptr;
			animations[i].startTime = 0;
		}
		animationCount = 0;
		APA102::clear();
		APA102::show();
	}

	/// <summary>
	/// Helper function to clear anim LED turned on by a current animation
	/// </summary>
	void stopAtIndex(int animIndex)
	{
		// Found the animation, start by killing the leds it controls
		int ledIndices[MAX_LED_COUNT];
		uint32_t zeros[MAX_LED_COUNT];
		memset(zeros, 0, sizeof(uint32_t) * MAX_LED_COUNT);
		int ledCount = animations[animIndex].animation->stop(ledIndices);
		APA102::setPixelColors(ledIndices, zeros, ledCount);
		APA102::show();
	}

	/// <summary>
	/// Helper method: Stop the animation at the given index. Used by Stop(IAnimation*)
	/// </summary>
	void removeAtIndex(int animIndex)
	{
		stopAtIndex(animIndex);

		// Shift the other animations
		for (; animIndex < animationCount - 1; ++animIndex)
		{
			animations[animIndex] = animations[animIndex + 1];
		}

		// Reduce the count
		animationCount--;
	}

}
}

