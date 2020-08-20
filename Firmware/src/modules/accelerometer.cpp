#include "accelerometer.h"

#include "drivers_hw/lis2de12.h"
#include "utils/utils.h"
#include "core/ring_buffer.h"
#include "config/board_config.h"
#include "config/settings.h"
#include "config/dice_variants.h"
#include "app_timer.h"
#include "app_error.h"
#include "nrf_log.h"
#include "config/settings.h"
#include "bluetooth/bluetooth_message_service.h"
#include "bluetooth/bluetooth_messages.h"
#include "bluetooth/bluetooth_stack.h"
#include "drivers_hw/apa102.h"


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
	float3 handleStateNormal; // The normal when we entered the handled state, so we can determine if we've moved enough
	bool paused;

	// This small buffer stores about 1 second of Acceleration data
	Core::RingBuffer<AccelFrame, ACCEL_BUFFER_SIZE> buffer;

	DelegateArray<FrameDataClientMethod, MAX_ACC_CLIENTS> frameDataClients;
	DelegateArray<RollStateClientMethod, MAX_ACC_CLIENTS> rollStateClients;

	void updateState();
	void pauseNotifications();
	void resumeNotifications();

    void CalibrateHandler(void* context, const Message* msg);
	void CalibrateFaceHandler(void* context, const Message* msg);
	void onSettingsProgrammingEvent(void* context, SettingsManager::ProgrammingEventType evt);

	void update(void* context);

    void init() {
        MessageService::RegisterMessageHandler(Message::MessageType_Calibrate, nullptr, CalibrateHandler);
        MessageService::RegisterMessageHandler(Message::MessageType_CalibrateFace, nullptr, CalibrateFaceHandler);

		SettingsManager::hookProgrammingEvent(onSettingsProgrammingEvent, nullptr);

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

		// Create the accelerometer timer
		ret_code_t ret_code = app_timer_create(&accelControllerTimer, APP_TIMER_MODE_REPEATED, update);
		APP_ERROR_CHECK(ret_code);

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
		newFrame.face = determineFace(newFrame.acc, &newFrame.faceConfidence);

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
					handleStateNormal = newFrame.acc.normalized();
                }
                break;
            case RollState_Handling:
				// Did we move ennough?
				{
					bool rotatedEnough = float3::dot(newFrame.acc.normalized(), handleStateNormal) < 0.5f;
					if (shock || zeroG || rotatedEnough) {
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

			// Debugging
			//BLE_LOG_INFO("Face Normal: %d, %d, %d", (int)(newFrame.acc.x * 100), (int)(newFrame.acc.y * 100), (int)(newFrame.acc.z * 100));

			if (newFrame.face != face) {
				face = newFrame.face;
				confidence = newFrame.faceConfidence;

				//NRF_LOG_INFO("Face %d, confidence " NRF_LOG_FLOAT_MARKER, face, NRF_LOG_FLOAT(confidence));
			}

			if (newRollState != rollState) {
				//NRF_LOG_INFO("State: %s", getRollStateString(newRollState));
				rollState = newRollState;
			}

			// Notify clients
			if (!paused) {
				for (int i = 0; i < rollStateClients.Count(); ++i) {
					rollStateClients[i].handler(rollStateClients[i].token, rollState, face);
				}
			}
		}
	}

	/// <summary>
	/// Initialize the acceleration system
	/// </summary>
	void start()
	{
		NRF_LOG_INFO("Starting accelerometer");
		// Set initial value
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

		ret_code_t ret_code = app_timer_start(accelControllerTimer, APP_TIMER_TICKS(TIMER2_RESOLUTION), NULL);
		APP_ERROR_CHECK(ret_code);
	}

	/// <summary>
	/// Stop getting updated from the timer
	/// </summary>
	void stop()
	{
		ret_code_t ret_code = app_timer_stop(accelControllerTimer);
		APP_ERROR_CHECK(ret_code);
		NRF_LOG_INFO("Stopped accelerometer");
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
		float3 face10;
		float3 led0;
		float3 led1;

		// Set to true if connection is lost to stop calibration
		bool calibrationInterrupted;

		CalibrationNormals() : calibrationInterrupted(false) {}
	};
	CalibrationNormals* measuredNormals = nullptr;
	void onConnectionLost(void* param, bool connected)
	{
		if (!connected)
			measuredNormals->calibrationInterrupted = true;
	}

    void CalibrateHandler(void* context, const Message* msg) {

		// Turn off state change notifications
		stop();
		Bluetooth::Stack::hook(onConnectionLost, nullptr);

		// Helper to restart accelerometer and clean up
		static auto restart = []() {
			Bluetooth::Stack::unHook(onConnectionLost);
			start();
		};

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

				// Debugging
				//BLE_LOG_INFO("Face 1 Normal: %d, %d, %d", (int)(measuredNormals->face1.x * 100), (int)(measuredNormals->face1.y * 100), (int)(measuredNormals->face1.z * 100));

				// Place on face 5
				MessageService::NotifyUser("Place face 5 up", true, true, 30, [] (bool okCancel)
				{
					if (okCancel) {
						// Die is on face 5
						// Read the normals
						LIS2DE12::read();
						measuredNormals->face5 = float3(LIS2DE12::cx, LIS2DE12::cy, LIS2DE12::cz);

						// Debugging
						//BLE_LOG_INFO("Face 5 Normal: %d, %d, %d", (int)(measuredNormals->face5.x * 100), (int)(measuredNormals->face5.y * 100), (int)(measuredNormals->face5.z * 100));

						// Place on face 10
						MessageService::NotifyUser("Place face 10 up", true, true, 30, [] (bool okCancel)
						{
							if (okCancel) {
								// Die is on face 10
								// Read the normals
								LIS2DE12::read();
								measuredNormals->face10 = float3(LIS2DE12::cx, LIS2DE12::cy, LIS2DE12::cz);

								// Debugging
								//BLE_LOG_INFO("Face 10 Normal: %d, %d, %d", (int)(measuredNormals->face10.x * 100), (int)(measuredNormals->face10.y * 100), (int)(measuredNormals->face10.z * 100));
								APA102::setPixelColor(0, 0x00FF00);
								APA102::show();

								// Place on led index 0
								MessageService::NotifyUser("Place lit face up", true, true, 30, [] (bool okCancel)
								{
									APA102::setPixelColor(0, 0x000000);
									APA102::show();
									if (okCancel) {
										// Read the normals
										LIS2DE12::read();
										measuredNormals->led0 = float3(LIS2DE12::cx, LIS2DE12::cy, LIS2DE12::cz);


										APA102::setPixelColor(9, 0x00FF00);
										APA102::show();
										// Place on led index 1
										MessageService::NotifyUser("Place lit face up", true, true, 30, [] (bool okCancel)
										{
											APA102::setPixelColor(9, 0x000000);
											APA102::show();
											if (okCancel) {
												// Read the normals
												LIS2DE12::read();
												measuredNormals->led1 = float3(LIS2DE12::cx, LIS2DE12::cy, LIS2DE12::cz);

												// Now we can calibrate

												// From the 3 measured normals we can calibrate the accelerometer
												auto b = BoardManager::getBoard();
												auto l = DiceVariants::getLayouts(b->ledCount);

												float3 newNormals[b->ledCount];
												int layoutVersionIndex = Utils::CalibrateNormals(
													0, measuredNormals->face1,
													4, measuredNormals->face5,
													9, measuredNormals->face10,
													l, newNormals, b->ledCount);

												// Now figure out the base remapping based on how the electronics is rotated inside the dice
												auto ll = l->layouts[layoutVersionIndex];

												uint8_t newFaceToLEDLookup[b->ledCount];
												if (Utils::CalibrateInternalRotation(
													0, measuredNormals->led0,
													9, measuredNormals->led1,
													newNormals, ll, newFaceToLEDLookup, b->ledCount)) {

													// And flash the new normals
													SettingsManager::programCalibrationData(newNormals, layoutVersionIndex, newFaceToLEDLookup, b->ledCount, [] (bool result) {

														// Notify user that we're done, yay!!!
														MessageService::NotifyUser("Calibrated!", false, false, 30, nullptr);

														// Restart notifications
														restart();
													});
												} else {
													// Notify user
													MessageService::NotifyUser("Calibration failed.", false, false, 30, nullptr);

													// Restart notifications
													restart();
												}
											} else {
												// Process cancelled, restart notifications
												restart();
											}
										});
									} else {
										// Process cancelled, restart notifications
										restart();
									}
								});
							} else {
								// Process cancelled, restart notifications
								restart();
							}
						});
					} else {
						// Process cancelled, restart notifications
						restart();
					}
				});

			} else {
				// Process cancelled, restart notifications
				restart();
			}
		});
	}

	void CalibrateFaceHandler(void* context, const Message* msg) {
		const MessageCalibrateFace* faceMsg = (const MessageCalibrateFace*)msg;
		uint8_t face = faceMsg->face;

		// Copy current calibration data
		int normalCount = BoardManager::getBoard()->ledCount;
		float3 calibratedNormalsCopy[normalCount];
		memcpy(calibratedNormalsCopy, SettingsManager::getSettings()->faceNormals, normalCount * sizeof(float3));
		uint8_t ftlCopy[normalCount];
		memcpy(ftlCopy, SettingsManager::getSettings()->faceToLEDLookup, normalCount * sizeof(uint8_t));

		// Replace the face's normal with what we measured
		LIS2DE12::read();
		calibratedNormalsCopy[face] = float3(LIS2DE12::cx, LIS2DE12::cy, LIS2DE12::cz);

		// And flash the new normals
		int fli = SettingsManager::getSettings()->faceLayoutLookupIndex;
		SettingsManager::programCalibrationData(calibratedNormalsCopy, fli, ftlCopy, normalCount, [] (bool result) {
			MessageService::NotifyUser("Face is calibrated.", true, false, 5, nullptr);
		});
	}

	void onSettingsProgrammingEvent(void* context, SettingsManager::ProgrammingEventType evt){
		if (evt == SettingsManager::ProgrammingEventType_Begin) {
			stop();
		} else {
			start();
		}
	}

}
}
