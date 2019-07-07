#include "accelerometer.h"

#include "drivers_hw/lis2de12.h"
#include "utils/utils.h"
#include "core/ring_buffer.h"
#include "config/board_config.h"
#include "app_timer.h"
#include "app_error.h"
#include "nrf_log.h"
#include "config/settings.h"

using namespace Modules;
using namespace Core;
using namespace DriversHW;
using namespace Config;

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
	float slowSigma;
	float fastSigma;

	// This small buffer stores about 1 second of Acceleration data
	Core::RingBuffer<AccelFrame, ACCEL_BUFFER_SIZE> buffer;

	DelegateArray<ClientMethod, MAX_ACC_CLIENTS> clients;

	void updateState();

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
		int newFace = determineFace(LIS2DE12::cx, LIS2DE12::cy, LIS2DE12::cz);
		if (newFace != face) {
			NRF_LOG_INFO("NewFace: %d", (newFace + 1));
			face = newFace;
		}

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

		float jerkX = LIS2DE12::convert(newFrame.jerkX);
		float jerkY = LIS2DE12::convert(newFrame.jerkY);
		float jerkZ = LIS2DE12::convert(newFrame.jerkZ);
		float jerk2 = jerkX * jerkX + jerkY * jerkY + jerkZ * jerkZ;
		auto settings = SettingsManager::getSettings();
		slowSigma = slowSigma * settings->sigmaDecaySlow + jerk2 * (1.0f - settings->sigmaDecaySlow);
		fastSigma = slowSigma * settings->sigmaDecayFast + jerk2 * (1.0f - settings->sigmaDecayFast);

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
	int determineFace(float x, float y, float z, float* outConfidence)
	{
		// Compare against face normals stored in board manager
		int faceCount = BoardManager::getBoard()->ledCount;
		auto& normals = BoardManager::getBoard()->faceNormals;
		float3 acc(x, y, z);
		acc.normalize();
		float bestDot = -1000.0f;
		int bestFace = -1;
		for (int i = 0; i < faceCount; ++i) {
			float dot = float3::dot(acc, normals[i]);
			if (dot > bestDot) {
				bestDot = dot;
				bestFace = i;
			}
		}
		if (outConfidence != nullptr) {
			*outConfidence = bestDot;
		}
		return bestFace;
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
