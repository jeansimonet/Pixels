// LEDAnimationController.h

#ifndef _LEDANIMATIONCONTROLLER_h
#define _LEDANIMATIONCONTROLLER_h

#include "Arduino.h"

class Animation;

#define MAX_ANIMS (6)

/// <summary>
/// Manages a set of running animations, talking to the LED controller
/// to tell it what LEDs must have what intensity at what time.
/// </summary>
class AnimController
{
private:
	/// <summary>
	/// Internal helper struct used to store a running animation instance
	/// </summary>
	struct AnimInstance
	{
		const Animation* animation;
		int startTime; //ms

		AnimInstance();
	};

	// Our currently running animations
	AnimInstance animations[MAX_ANIMS];
	int count;

private:
	void stopAtIndex(int animIndex);
	void removeAtIndex(int animIndex);
	int update();
	void update(int ms);

	// To be passed to the timer
	static void animationControllerUpdate(void* param);

public:
	AnimController();
	void begin();
	void stop();
	void play(const Animation* anim);
	void stop(const Animation* anim);
	void stopAll();
};

extern AnimController animController;

#endif

