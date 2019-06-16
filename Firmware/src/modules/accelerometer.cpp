#include "accelerometer.h"

#include "drivers_hw/lis2de12.h"
#include "utils/utils.h"
#include "app_timer.h"
#include "core/ring_buffer.h"
#include "nrf_log.h"

using namespace Modules;
using namespace Core;
using namespace DriversHW;

// This defines how frequently we try to read the accelerometer
#define TIMER2_RESOLUTION (500)	// ms
#define JERK_SCALE (1000)		// To make the jerk in the same range as the acceleration

namespace Modules
{
namespace Accelerometer
{
	/// <summary>
	/// Retrieves the acceleration value stored in the frame of data
	/// </summary>
	float3 AccelFrame::getAcc() const
	{
		return float3(LIS2DE12::convert(X), LIS2DE12::convert(Y), LIS2DE12::convert(Z));
	}

	/// <summary>
	/// Retrieves the jerk
	/// </summary>
	float3 AccelFrame::getJerk() const
	{
		return float3(LIS2DE12::convert(jerkX), LIS2DE12::convert(jerkY), LIS2DE12::convert(jerkZ));
	}

	_APP_TIMER_DEF(accelControllerTimer);

	int face;

	// This small buffer stores about 1 second of Acceleration data
	Core::RingBuffer<AccelFrame, ACCEL_BUFFER_SIZE> buffer;

	DelegateArray<ClientMethod, MAX_ACC_CLIENTS> clients;

	void init()	{
		face = 0;
		start();
		NRF_LOG_INFO("Accelerometer initialized");
	}

	/// <summary>
	/// update is called from the timer
	/// </summary>
	void update(void* context) {
		LIS2DE12::read();
		face = determineFace(LIS2DE12::cx, LIS2DE12::cy, LIS2DE12::cz);

		AccelFrame newFrame;
		newFrame.X = LIS2DE12::x;
		newFrame.Y = LIS2DE12::y;
		newFrame.Z = LIS2DE12::z;
		newFrame.Time = Utils::millis();

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

	/// <summary>
	/// Initialize the acceleration system
	/// </summary>
	void start()
	{
		LIS2DE12::read();
		face = determineFace(LIS2DE12::cx, LIS2DE12::cy, LIS2DE12::cz);

		ret_code_t ret_code = app_timer_create(&accelControllerTimer, APP_TIMER_MODE_REPEATED, Accelerometer::update);
		APP_ERROR_CHECK(ret_code);

		ret_code = app_timer_start(accelControllerTimer, APP_TIMER_TICKS(TIMER2_RESOLUTION), NULL);
		APP_ERROR_CHECK(ret_code);
	}

	/// <summary>
	/// Stop getting updated from the timer
	/// </summary>
	void stop()
	{
		ret_code_t ret_code = app_timer_stop(accelControllerTimer);
		APP_ERROR_CHECK(ret_code);
	}

	/// <summary>
	/// Returns the currently stored up face!
	/// </summary>
	int currentFace()
	{
		return face;
	}

	/// <summary>
	/// Crudely compares accelerometer readings passed in to determine the current face up
	/// </summary>
	/// <returns>The face number, starting at 0</returns>
	int determineFace(float x, float y, float z)
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
	void hook(Accelerometer::ClientMethod callback, void* parameter)
	{
		if (!clients.Register(parameter, callback))
		{
			NRF_LOG_ERROR("Too many accelerometer hooks registered.");
		}
	}

	/// <summary>
	/// Method used by clients to stop getting accelerometer reading callbacks
	/// </summary>
	void unHook(Accelerometer::ClientMethod callback)
	{
		clients.UnregisterWithHandler(callback);
	}

	/// <summary>
	/// Method used by clients to stop getting accelerometer reading callbacks
	/// </summary>
	void unHookWithParam(void* param)
	{
		clients.UnregisterWithToken(param);
	}


}
}
