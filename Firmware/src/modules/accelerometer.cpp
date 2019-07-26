#include "accelerometer.h"

#include "drivers_hw/lis2de12.h"
#include "utils/utils.h"
#include "core/ring_buffer.h"
#include "config/board_config.h"
#include "app_timer.h"
#include "app_error.h"
#include "nrf_log.h"
#include "config/settings.h"
#include "bluetooth/bluetooth_message_service.h"
#include "bluetooth/bluetooth_messages.h"

using namespace Modules;
using namespace Core;
using namespace DriversHW;
using namespace Config;
using namespace Bluetooth;

// This defines how frequently we try to read the accelerometer
#define TIMER2_RESOLUTION (10)	// ms
#define JERK_SCALE (1000)		// To make the jerk in the same range as the acceleration
#define MAX_ACC_CLIENTS 4

namespace Modules
{
namespace Accelerometer
{
	APP_TIMER_DEF(accelControllerTimer);

	int face;
	float confidence;
	float slowSigma;
	float fastSigma;
	RollState rollState = RollState_Unknown;

	// This small buffer stores about 1 second of Acceleration data
	Core::RingBuffer<AccelFrame, ACCEL_BUFFER_SIZE> buffer;

	DelegateArray<FrameDataClientMethod, MAX_ACC_CLIENTS> frameDataClients;
	DelegateArray<RollStateClientMethod, MAX_ACC_CLIENTS> rollStateClients;

	void updateState();

    void CalibrateHandler(void* context, const Message* msg);

    void init() {
        MessageService::RegisterMessageHandler(Message::MessageType_Calibrate, nullptr, CalibrateHandler);

		face = 0;
		start();
		NRF_LOG_INFO("Accelerometer initialized");
	}

	/// <summary>
	/// update is called from the timer
	/// </summary>
	void update(void* context) {
		LIS2DE12::read();

		AccelFrame newFrame;
		newFrame.acc = float3(LIS2DE12::cx, LIS2DE12::cy, LIS2DE12::cz);
		newFrame.time = Utils::millis();

		// Compute delta!
		auto& lastFrame = buffer.last();
		// NRF_LOG_INFO("lastFrame: %d: %d,%d,%d", lastFrame.Time, lastFrame.X, lastFrame.Y, lastFrame.Z);
		// NRF_LOG_INFO("newFrame: %d: %d,%d,%d", newFrame.Time, newFrame.X, newFrame.Y, newFrame.Z);

		float3 delta = newFrame.acc - lastFrame.acc;

		// deltaTime should be roughly 10ms because that's how frequently we asked to be updated!
		short deltaTime = (short)(newFrame.time - lastFrame.time); 

		// Compute jerk
		// deltas are stored in the same unit (over time) as accelerometer readings
		// i.e. if readings are 8g scaled to a signed 12 bit integer (which they are)
		// then jerk is 8g/s scaled to a signed 12 bit integer
		newFrame.jerk = delta / deltaTime;

		float jerk2 = newFrame.jerk.x * newFrame.jerk.x + newFrame.jerk.y * newFrame.jerk.y + newFrame.jerk.z * newFrame.jerk.z;
		auto settings = SettingsManager::getSettings();
		slowSigma = slowSigma * settings->sigmaDecaySlow + jerk2 * (1.0f - settings->sigmaDecaySlow);
		fastSigma = fastSigma * settings->sigmaDecayFast + jerk2 * (1.0f - settings->sigmaDecayFast);
		newFrame.slowSigma = slowSigma;
		newFrame.fastSigma = fastSigma;

		newFrame.face = determineFace(newFrame.acc, &newFrame.faceConfidence);

		buffer.push(newFrame);

		// Notify clients
		for (int i = 0; i < frameDataClients.Count(); ++i)
		{
			frameDataClients[i].handler(frameDataClients[i].token, newFrame);
		}

		bool movingFast = newFrame.fastSigma > settings->fastMovingThreshold;
		bool movingSlow = newFrame.slowSigma > settings->slowMovingThreshold;
		bool onFace = newFrame.faceConfidence > settings->faceThreshold;
		bool zeroG = newFrame.acc.sqrMagnitude() < (settings->fallingThreshold * settings->fallingThreshold);
        bool shock = newFrame.acc.sqrMagnitude() > (settings->shockThreshold * settings->shockThreshold);

        RollState newRollState = rollState;
        switch (rollState) {
            case RollState_Unknown:
            case RollState_OnFace:
            case RollState_Crooked:
                // We start rolling if we detect enough motion
                if (movingSlow) {
                    // We're at least being handled
                    newRollState = RollState_Handling;
                }
                break;
            case RollState_Handling:
				if (shock || zeroG || newFrame.face != face) {
					// Stuff is happening that we are most likely rolling now
					newRollState = RollState_Rolling;
				}
				break;
			case RollState_Rolling:
                // If we stop moving we may be on a face
                if (!movingFast) {
                    // We may be at rest
                    if (onFace) {
                        // We're at rest
                        newRollState = RollState_OnFace;
                    } else {
                        newRollState = RollState_Crooked;
                    }
                }
                break;
            default:
                break;
        }

		if (newFrame.face != face) {
			face = newFrame.face;
			confidence = newFrame.faceConfidence;
		}

        if (newRollState != rollState) {
            rollState = newRollState;
            NRF_LOG_INFO("State: %d, Face %d", rollState, currentFace);

			// Notify clients
			for (int i = 0; i < rollStateClients.Count(); ++i)
			{
				rollStateClients[i].handler(rollStateClients[i].token, rollState, face);
			}
        }
	}

	/// <summary>
	/// Initialize the acceleration system
	/// </summary>
	void start()
	{
		LIS2DE12::read();
		float3 acc(LIS2DE12::cx, LIS2DE12::cy, LIS2DE12::cz);
		face = determineFace(acc, &confidence);

		// Determine what state we're in to begin with
		auto settings = SettingsManager::getSettings();
		bool onFace = confidence > settings->faceThreshold;
        if (onFace) {
            rollState = RollState_OnFace;
        } else {
            rollState = RollState_Crooked;
        }

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
	int currentFace() {
		return face;
	}

	float currentFaceConfidence() {
		return confidence;
	}

	RollState currentRollState() {
		return rollState;
	}

	/// <summary>
	/// Crudely compares accelerometer readings passed in to determine the current face up
	/// </summary>
	/// <returns>The face number, starting at 0</returns>
	int determineFace(float3 acc, float* outConfidence)
	{
		// Compare against face normals stored in board manager
		int faceCount = BoardManager::getBoard()->ledCount;
		auto settings = SettingsManager::getSettings();
		auto& normals = settings->faceNormals;
		float accMag = acc.magnitude();
		if (accMag < settings->fallingThreshold) {
			if (outConfidence != nullptr) {
				*outConfidence = 0.0f;
			}
			return face;
		} else {
			acc  = acc / sqrt(accMag); // normalize
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
	}

	/// <summary>
	/// Method used by clients to request timer callbacks when accelerometer readings are in
	/// </summary>
	void hookFrameData(Accelerometer::FrameDataClientMethod callback, void* parameter)
	{
		if (!frameDataClients.Register(parameter, callback))
		{
			NRF_LOG_ERROR("Too many accelerometer hooks registered.");
		}
	}

	/// <summary>
	/// Method used by clients to stop getting accelerometer reading callbacks
	/// </summary>
	void unHookFrameData(Accelerometer::FrameDataClientMethod callback)
	{
		frameDataClients.UnregisterWithHandler(callback);
	}

	/// <summary>
	/// Method used by clients to stop getting accelerometer reading callbacks
	/// </summary>
	void unHookFrameDataWithParam(void* param)
	{
		frameDataClients.UnregisterWithToken(param);
	}

	void hookRollState(RollStateClientMethod method, void* param)
	{
		if (!rollStateClients.Register(param, method))
		{
			NRF_LOG_ERROR("Too many accelerometer hooks registered.");
		}
	}

	void unHookRollState(RollStateClientMethod client)
	{
		rollStateClients.UnregisterWithHandler(client);
	}

	void unHookRollStateWithParam(void* param)
	{
		rollStateClients.UnregisterWithToken(param);
	}

	struct CalibrationNormals
	{
		float3 face1;
		float3 face5;
	};
	CalibrationNormals* measuredNormals = nullptr;

    void CalibrateHandler(void* context, const Message* msg) {
		// Start calibration!
		measuredNormals = (CalibrationNormals*)malloc(sizeof(CalibrationNormals));

		// Ask user to place die on face 1
		MessageService::NotifyUser("Place face 1 up", true, true, 30, [] (bool okCancel)
		{
			if (okCancel) {
				// Die is on face 1
				// Read the normals
				LIS2DE12::read();
				measuredNormals->face1 = float3(LIS2DE12::cx, LIS2DE12::cy, LIS2DE12::cz);

				// Place on face 5
				MessageService::NotifyUser("Place face 5 up", true, true, 30, [] (bool okCancel)
				{
					if (okCancel) {
						// Die is on face 5
						// Read the normals
						LIS2DE12::read();
						measuredNormals->face5 = float3(LIS2DE12::cx, LIS2DE12::cy, LIS2DE12::cz);

						// Now we can calibrate
						int normalCount = BoardManager::getBoard()->ledCount;
						float3 canonNormalsCopy[normalCount];
						memcpy(canonNormalsCopy, BoardManager::getBoard()->faceNormals, normalCount * sizeof(float3));

						Utils::CalibrateNormals(0, measuredNormals->face1, 4, measuredNormals->face5, canonNormalsCopy, normalCount);

						// And flash the new normals
						SettingsManager::programNormals(canonNormalsCopy, normalCount);

						MessageService::NotifyUser("Die is calibrated.", true, false, 30, nullptr);
					}
				});
			}
		});
	}
}
}
