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
#include "bluetooth/bluetooth_stack.h"

using namespace Modules;
using namespace Core;
using namespace DriversHW;
using namespace Config;
using namespace Bluetooth;

// This defines how frequently we try to read the accelerometer
#define TIMER2_RESOLUTION (100)	// ms
#define JERK_SCALE (1000)		// To make the jerk in the same range as the acceleration
#define MAX_ACC_CLIENTS 4

namespace Modules
{
namespace Accelerometer
{
	APP_TIMER_DEF(accelControllerTimer);

	int face;
	float confidence;
	float sigma;
	float3 smoothAcc;
	RollState rollState = RollState_Unknown;
	bool moving = false;

	// This small buffer stores about 1 second of Acceleration data
	Core::RingBuffer<AccelFrame, ACCEL_BUFFER_SIZE> buffer;

	DelegateArray<FrameDataClientMethod, MAX_ACC_CLIENTS> frameDataClients;
	DelegateArray<RollStateClientMethod, MAX_ACC_CLIENTS> rollStateClients;

	void updateState();

    void CalibrateHandler(void* context, const Message* msg);
	void CalibrateFaceHandler(void* context, const Message* msg);

    void init() {
        MessageService::RegisterMessageHandler(Message::MessageType_Calibrate, nullptr, CalibrateHandler);
        MessageService::RegisterMessageHandler(Message::MessageType_CalibrateFace, nullptr, CalibrateFaceHandler);

		face = 0;
		confidence = 0.0f;
		smoothAcc = float3::zero();

		LIS2DE12::read();
		AccelFrame newFrame;
		newFrame.acc = float3(LIS2DE12::cx, LIS2DE12::cy, LIS2DE12::cz);
		newFrame.time = Utils::millis();
		newFrame.jerk = float3::zero();
		newFrame.sigma = 0.0f;
		newFrame.smoothAcc = newFrame.acc;
		buffer.push(newFrame);

		start();
		NRF_LOG_INFO("Accelerometer initialized");
	}

	/// <summary>
	/// update is called from the timer
	/// </summary>
	void update(void* context) {
		auto settings = SettingsManager::getSettings();
		auto& lastFrame = buffer.last();

		LIS2DE12::read();

		AccelFrame newFrame;
		newFrame.acc = float3(LIS2DE12::cx, LIS2DE12::cy, LIS2DE12::cz);
		newFrame.time = Utils::millis();
		newFrame.jerk = ((newFrame.acc - lastFrame.acc) * 1000.0f) / (float)(newFrame.time - lastFrame.time);

		float jerkMag = newFrame.jerk.sqrMagnitude();
		if (jerkMag > 10.f) {
			jerkMag = 10.f;
		}
		sigma = sigma * settings->sigmaDecay + jerkMag * (1.0f - settings->sigmaDecay);
		newFrame.sigma = sigma;

		smoothAcc = smoothAcc * settings->accDecay + newFrame.acc * (1.0f - settings->accDecay);
		newFrame.smoothAcc = smoothAcc;
		newFrame.face = determineFace(smoothAcc, &newFrame.faceConfidence);

		buffer.push(newFrame);

		// Notify clients
		for (int i = 0; i < frameDataClients.Count(); ++i)
		{
			frameDataClients[i].handler(frameDataClients[i].token, newFrame);
		}

		bool startMoving = sigma > settings->startMovingThreshold;
		bool stopMoving = sigma < settings->stopMovingThreshold;
		bool onFace = newFrame.faceConfidence > settings->faceThreshold;
		bool zeroG = newFrame.acc.sqrMagnitude() < (settings->fallingThreshold * settings->fallingThreshold);
        bool shock = newFrame.acc.sqrMagnitude() > (settings->shockThreshold * settings->shockThreshold);

        RollState newRollState = rollState;
        switch (rollState) {
            case RollState_Unknown:
            case RollState_OnFace:
            case RollState_Crooked:
                // We start rolling if we detect enough motion
                if (startMoving) {
                    // We're at least being handled
                    newRollState = RollState_Handling;
                }
                break;
            case RollState_Handling:
				if (shock || zeroG || newFrame.face != face) {
					// Stuff is happening that we are most likely rolling now
					newRollState = RollState_Rolling;
				} else if (stopMoving) {
					// Just slid the dice around?
					if (stopMoving) {
						if (BoardManager::getBoard()->ledCount == 6) {
							// We may be at rest
							if (onFace) {
								// We're at rest
								newRollState = RollState_OnFace;
							} else {
								newRollState = RollState_Crooked;
							}
						} else {
							newRollState = RollState_OnFace;
						}
					}
				}
				break;
			case RollState_Rolling:
                // If we stop moving we may be on a face
                if (stopMoving) {
					if (BoardManager::getBoard()->ledCount == 6) {
						// We may be at rest
						if (onFace) {
							// We're at rest
							newRollState = RollState_OnFace;
						} else {
							newRollState = RollState_Crooked;
						}
					} else {
						newRollState = RollState_OnFace;
					}
                }
                break;
            default:
                break;
        }

		if (newFrame.face != face || newRollState != rollState) {
			if (newFrame.face != face) {
				face = newFrame.face;
				confidence = newFrame.faceConfidence;

				NRF_LOG_INFO("Face %d, confidence " NRF_LOG_FLOAT_MARKER, face, NRF_LOG_FLOAT(confidence));
			}

			if (newRollState != rollState) {
				NRF_LOG_INFO("State: %s", getRollStateString(newRollState));
				rollState = newRollState;
			}

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

	const char* getRollStateString(RollState state) {
		switch (state) {
			case RollState_Unknown:
			default:
				return "Unknown";
			case RollState_OnFace:
				return "OnFace";
			case RollState_Handling:
				return "Handling";
			case RollState_Rolling:
				return "Rolling";
			case RollState_Crooked:
				return "Crooked";
		}
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
			float3 nacc  = acc / accMag; // normalize
			float bestDot = -1000.0f;
			int bestFace = -1;
			for (int i = 0; i < faceCount; ++i) {
				float dot = float3::dot(nacc, normals[i]);
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

	void CalibrateFaceHandler(void* context, const Message* msg) {
		const MessageCalibrateFace* faceMsg = (const MessageCalibrateFace*)msg;
		uint8_t face = faceMsg->face;

		// Copy current calibration normals
		int normalCount = BoardManager::getBoard()->ledCount;
		float3 calibratedNormalsCopy[normalCount];
		memcpy(calibratedNormalsCopy, SettingsManager::getSettings()->faceNormals, normalCount * sizeof(float3));

		// Replace the face's normal with what we measured
		LIS2DE12::read();
		calibratedNormalsCopy[face] = float3(LIS2DE12::cx, LIS2DE12::cy, LIS2DE12::cz);

		// And flash the new normals
		SettingsManager::programNormals(calibratedNormalsCopy, normalCount);

		MessageService::NotifyUser("Face is calibrated.", true, false, 5, nullptr);
	}
}
}
