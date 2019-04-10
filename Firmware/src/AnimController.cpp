#include "AnimController.h"
#include "Animation.h"
#include "LEDs.h"
#include "nrf_delay.h"
#include "app_timer.h"
#include "Utils.h"

using namespace Devices;

#define TIMER2_RESOLUTION 33 //ms

AnimController animController;
_APP_TIMER_DEF(animControllerTimer);

/// <summary>
/// Constructor
/// </summary>
AnimController::AnimInstance::AnimInstance()
	: animation(nullptr)
	, startTime(0)
{
}

/// <summary>
/// Constructor
/// </summary>
AnimController::AnimController()
	: count(0)
{
}

/// <summary>
/// Kick off the animation controller, registering it with the Timer system
/// </summary>
void AnimController::begin()
{
	ret_code_t ret_code = app_timer_create(&animControllerTimer, APP_TIMER_MODE_REPEATED, AnimController::animationControllerUpdate);
	APP_ERROR_CHECK(ret_code);

	ret_code = app_timer_start(animControllerTimer, APP_TIMER_TICKS(TIMER2_RESOLUTION), NULL);
	APP_ERROR_CHECK(ret_code);
}

/// <summary>
/// Called by the timer system to update current animations
/// </summary>
void AnimController::update()
{
	update(Core::millis());
}

/// <summary>
/// Update all currently running animations, and performing housekeeping when necessary
/// </summary>
/// <param name="ms">Current global time in milliseconds</param>
void AnimController::update(int ms)
{
	for (int i = 0; i < count; ++i)
	{
		auto& anim = animations[i];
		int animTime = ms - anim.startTime;
		if (animTime > anim.animation->totalDuration())
		{
			// The animation is over, get rid of it!
			removeAtIndex(i);

			// Decrement loop counter since we just replaced the current anim
			i--;
		}
		else
		{
			// Update the leds
			int ledIndices[LED_COUNT];
			uint32_t colors[LED_COUNT];
			int ledCount = anim.animation->updateLEDs(animTime, ledIndices, colors);
			leds.setLEDs(ledIndices, colors, ledCount);
		}
	}
}

// To be passed to the timer
void AnimController::animationControllerUpdate(void* param)
{
	((AnimController*)param)->update();
}

/// <summary>
/// Stop updating animations
/// </summary>
void AnimController::stop()
{
	auto ret_code = app_timer_stop(animControllerTimer);
	APP_ERROR_CHECK(ret_code);
}

/// <summary>
/// Add an animation to the list of running animations
/// </summary>
void AnimController::play(const Animation* anim)
{
	int prevAnimIndex = 0;
	for (; prevAnimIndex < count; ++prevAnimIndex)
	{
		if (animations[prevAnimIndex].animation == anim)
		{
			break;
		}
	}

	int ms = Core::millis();
	if (prevAnimIndex < count)
	{
		// Replace a previous animation
		stopAtIndex(prevAnimIndex);

		animations[prevAnimIndex].startTime = ms;
		animations[prevAnimIndex].animation->start();
	}
	else if (count < MAX_ANIMS)
	{
		// Add a new animation
		animations[count].animation = anim;
		animations[count].startTime = ms;
		animations[count].animation->start();
		count++;
	}
	// Else there is no more room
}

/// <summary>
/// Forcibly stop a currently running animation
/// </summary>
void AnimController::stop(const Animation* anim)
{
	int prevAnimIndex = 0;
	for (; prevAnimIndex < count; ++prevAnimIndex)
	{
		if (animations[prevAnimIndex].animation == anim)
		{
			break;
		}
	}

	if (prevAnimIndex < count)
	{
		removeAtIndex(prevAnimIndex);
	}
	// Else the animation isn't playing
}

/// <summary>
/// Stop all currently running animations
/// </summary>
void AnimController::stopAll()
{
	for (int i = 0; i < count; ++i)
	{
		animations[i].animation = nullptr;
		animations[i].startTime = 0;
	}
	count = 0;
	leds.clearAll();
}

/// <summary>
/// Helper function to clear anim LED turned on by a current animation
/// </summary>
void AnimController::stopAtIndex(int animIndex)
{
	// Found the animation, start by killing the leds it controls
	int ledIndices[LED_COUNT];
	uint32_t zeros[LED_COUNT] = { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 };
	int ledCount = animations[animIndex].animation->stop(ledIndices);
	leds.setLEDs(ledIndices, zeros, ledCount);
}

/// <summary>
/// Helper method: Stop the animation at the given index. Used by Stop(IAnimation*)
/// </summary>
void AnimController::removeAtIndex(int animIndex)
{
	stopAtIndex(animIndex);

	// Shift the other animations
	for (; animIndex < count - 1; ++animIndex)
	{
		animations[animIndex] = animations[animIndex + 1];
	}

	// Reduce the count
	count--;
}