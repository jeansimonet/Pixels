#include "AccelController.h"
#include "Timer.h"
#include "Accelerometer.h"
#include "Debug.h"

using namespace Systems;
using namespace Devices;
using namespace Core;

// This defines how frequently we try to read the accelerometer
#define TIMER2_RESOLUTION (10000)	// 10ms
#define JERK_SCALE (1000)			// To make the jerk in the same range as the acceleration

AccelerationController accelController;

/// <summary>
/// Retrieves the acceleration value stored in the frame of data
/// </summary>
float3 AccelFrame::getAcc() const
{
	return float3(accelerometer.convert(X), accelerometer.convert(Y), accelerometer.convert(Z));
}

/// <summary>
/// Retrieves the jerk
/// </summary>
float3 AccelFrame::getJerk() const
{
	return float3(accelerometer.convert(jerkX), accelerometer.convert(jerkY), accelerometer.convert(jerkZ));
}

/// <summary>
/// Concstructor
/// </summary>
AccelerationController::AccelerationController()
	: face(0)
{
	auto& lastFrame = buffer.last();
}

/// <summary>
/// update is called from the timer
/// </summary>
void AccelerationController::timerUpdate()
{
	accelerometer.read();
	face = determineFace(accelerometer.cx, accelerometer.cy, accelerometer.cz);

	AccelFrame newFrame;
	newFrame.X = accelerometer.x;
	newFrame.Y = accelerometer.y;
	newFrame.Z = accelerometer.z;
	newFrame.Time = millis();

	// Compute delta!
	auto& lastFrame = buffer.last();

	short deltaX = newFrame.X - lastFrame.X;
	short deltaY = newFrame.Y - lastFrame.Y;
	short deltaZ = newFrame.Z - lastFrame.Z;

	// deltaTime should be roughly 10ms because that's how frequently we asked to be updated!
	short deltaTime = (short)(newFrame.Time - lastFrame.Time); 

	// Compute jerk
	// deltas are stored in the same unit (over time) as accelerometer readings
	// i.e. if readings are 8g scaled to a signed 12 bit integer (which they are)
	// then jerk is 8g/s scaled to a signed 12 bit integer
	newFrame.jerkX = deltaX * JERK_SCALE / deltaTime;
	newFrame.jerkY = deltaY * JERK_SCALE / deltaTime;
	newFrame.jerkZ = deltaZ * JERK_SCALE / deltaTime;

	//debugPrint("new frame jerk: ");
	//debugPrint(newFrame.jerkX);
	//debugPrint(", ");
	//debugPrint(newFrame.jerkY);
	//debugPrint(", ");
	//debugPrintln(newFrame.jerkZ);

	buffer.push(newFrame);

	// Notify clients
	for (int i = 0; i < clients.Count(); ++i)
	{
		clients[i].handler(clients[i].token, newFrame);
	}
}

// To be passed to the timer
void AccelerationController::accelControllerUpdate(void* param)
{
	((AccelerationController*)param)->timerUpdate();
}

/// <summary>
/// Initialize the acceleration system
/// </summary>
void AccelerationController::begin()
{
	accelerometer.read();
	face = determineFace(accelerometer.cx, accelerometer.cy, accelerometer.cz);
	timer.hook(TIMER2_RESOLUTION, AccelerationController::accelControllerUpdate, this);
}

/// <summary>
/// Stop getting updated from the timer
/// </summary>
void AccelerationController::stop()
{
	timer.unHook(AccelerationController::accelControllerUpdate);
}

/// <summary>
/// Returns the currently stored up face!
/// </summary>
int AccelerationController::currentFace()
{
	return face;
}

/// <summary>
/// Crudely compares accelerometer readings passed in to determine the current face up
/// </summary>
/// <returns>The face number, starting at 0</returns>
int AccelerationController::determineFace(float x, float y, float z)
{
	if (abs(x) > abs(y))
	{
		if (abs(x) > abs(z))
		{
			// X is greatest direction
			if (x > 0)
			{
				return 1;
			}
			else
			{
				return 4;
			}
		}
		else
		{
			// Z is greatest direction
			if (z > 0)
			{
				return 0;
			}
			else
			{
				return 5;
			}
		}
	}
	else
	{
		if (abs(z) > abs(y))
		{
			// Z is greatest direction
			if (z > 0)
			{
				return 0;
			}
			else
			{
				return 5;
			}
		}
		else
		{
			// Y is greatest direction
			if (y > 0)
			{
				return 2;
			}
			else
			{
				return 3;
			}
		}
	}
}

/// <summary>
/// Method used by clients to request timer callbacks when accelerometer readings are in
/// </summary>
void AccelerationController::hook(AccelerationController::ClientMethod callback, void* parameter)
{
	if (!clients.Register(parameter, callback))
	{
		debugPrintln("Too many accelerometer hooks registered.");
	}
}

/// <summary>
/// Method used by clients to stop getting accelerometer reading callbacks
/// </summary>
void AccelerationController::unHook(AccelerationController::ClientMethod callback)
{
	clients.UnregisterWithHandler(callback);
}

/// <summary>
/// Method used by clients to stop getting accelerometer reading callbacks
/// </summary>
void AccelerationController::unHookWithParam(void* param)
{
	clients.UnregisterWithToken(param);
}

