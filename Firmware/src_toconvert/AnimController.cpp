#include "AnimController.h"
#include "Animation.h"
#include "LEDs.h"
#include "Timer.h"
#include "Console.h"

using namespace Systems;
using namespace Devices;

//#define TIMER2_RESOLUTION (33333) // 33.333 ms = 30 Hz
#define TIMER2_RESOLUTION (100000) // 33.333 ms = 30 Hz

AnimController animController;

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
	timer.hook(TIMER2_RESOLUTION, animationControllerUpdate, this); // 33.333 ms = 30 Hz
}

/// <summary>
/// Called by the timer system to update current animations
/// </summary>
int AnimController::update()
{
	update(millis());
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
	timer.unHook(animationControllerUpdate);
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

	int ms = millis();
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