// SimpleThrowDetector.h

#ifndef _SIMPLETHROWDETECTOR_h
#define _SIMPLETHROWDETECTOR_h

#include "arduino.h"
#include "AccelController.h"

namespace Systems
{
	/// <summary>
	/// Attaches to the accel controller and monitors jerk.
	/// if the jerk is high enough after a little bit of filtering,
	/// poke Lazarus so the Die doesn't go to sleep.
	/// </summary>
	class SimpleThrowDetector
	{
	public:
		enum ThrowState
		{
			ThrowState_StartedRolling,
			ThrowState_RolledLongEnough,
			ThrowState_OnFace,
		};

		ThrowState GetCurrentState() const { return currentState; }
		int GetOnFaceFace() const { return onFaceFace; }

	private:
		float sigma;
		ThrowState currentState;
		int rollStartTime; // ms
		int onFaceFace;

	private:
		static void accelControllerCallback(void* ignore, const AccelFrame& frame);

	public:
		void begin();
		void onAccelFrame(const AccelFrame& frame);
	};

	extern SimpleThrowDetector simpleThrowDetector;
}

#endif

