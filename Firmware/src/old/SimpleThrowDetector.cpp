#include "SimpleThrowDetector.h"
#include "Accelerometer.h"
#include "Debug.h"
#include "Die.h"
#include "AnimController.h"
#include "Settings.h"

using namespace Systems;
using namespace Devices;

SimpleThrowDetector Systems::simpleThrowDetector;

void SimpleThrowDetector::begin()
{
	sigma = 0.0f;
	currentState = ThrowState_OnFace;
	rollStartTime = millis();
	accelController.hook(accelControllerCallback, nullptr);
}

void SimpleThrowDetector::accelControllerCallback(void* ignore, const AccelFrame& frame)
{
	simpleThrowDetector.onAccelFrame(frame);
}

void SimpleThrowDetector::onAccelFrame(const AccelFrame& frame)
{
	// Add the last frame
	float jerkX = accelerometer.convert(frame.jerkX);
	float jerkY = accelerometer.convert(frame.jerkY);
	float jerkZ = accelerometer.convert(frame.jerkZ);
	float jerk2 = jerkX * jerkX + jerkY * jerkY + jerkZ * jerkZ;
	//debugPrint("Sigma: ");
	//debugPrintln(sigma);

	switch (currentState)
	{
	case ThrowState_StartedRolling:
		sigma = sigma * settings->sigmaDecayStart + jerk2 * (1.0f - settings->sigmaDecayStart);
		if (sigma < settings->sigmaThresholdStart)
		{
			// Stopped rolling, so this is like a small bump, not enough
			currentState = ThrowState_OnFace;
		}
		else
		{
			if (millis() > rollStartTime + settings->minRollTime)
			{
				// Ok, it's been long enough
				currentState = ThrowState_RolledLongEnough;
			}
			// Keep waiting
		}
		break;
	case ThrowState_RolledLongEnough:
		sigma = sigma * settings->sigmaDecayStop + jerk2 * (1.0f - settings->sigmaDecayStop);
		if (sigma < settings->sigmaThresholdEnd)
		{
			currentState = ThrowState_OnFace;
			onFaceFace = accelController.currentFace();
			if (abs(accelerometer.convert(frame.X)) > settings->faceThreshold ||
				abs(accelerometer.convert(frame.Y)) > settings->faceThreshold ||
				abs(accelerometer.convert(frame.Z)) > settings->faceThreshold)
			{
				// We stopped moving
				// Play an anim, and switch state
				int animIndex = onFaceFace + 6; // hardcoded for now
				animController.play(animationSet->GetAnimation(animIndex));
				die.playAnimation(animIndex);
			}
		}
		break;
	case ThrowState_OnFace:
		sigma = sigma * settings->sigmaDecayStart + jerk2 * (1.0f - settings->sigmaDecayStart);
		if (sigma >= settings->sigmaThresholdStart)
		{
			// We started moving, count time
			rollStartTime = millis();
			currentState = ThrowState_StartedRolling;
		}
		// Else nothing
		break;
	}
}



