// AccelController.h

#ifndef _ACCELCONTROLLER_h
#define _ACCELCONTROLLER_h

#include "Arduino.h"
#include "RingBuffer.h"
#include "float3.h"
#include "DelegateArray.h"

#define ACCEL_BUFFER_SIZE 100 // 10ms * 100 = 1 seconds of buffer
							  // 16 bytes * 128 = 2k of RAM
#define MAX_CLIENTS 4

/// <summary>
/// Small struct holding a single frame of accelerometer data
/// used for both face detection (not that kind) and telemetry
/// </summary>
struct AccelFrame
{
	short X;
	short Y;
	short Z;
	short jerkX;
	short jerkY;
	short jerkZ;
	unsigned long Time;

	Core::float3 getAcc() const;
	Core::float3 getJerk() const;
};

/// <summary>
/// The component in charge of maintaining the acceleraion readings,
/// and determining die motion state.
/// </summary>
class AccelerationController
{
private:
	typedef void(*ClientMethod)(void* param, const AccelFrame& accelFrame);

	int face;

	// This small buffer stores about 1 second of Acceleration data
	Core::RingBuffer<AccelFrame, ACCEL_BUFFER_SIZE> buffer;

	DelegateArray<ClientMethod, MAX_CLIENTS> clients;

private:
	// To be passed to the timer
	static void accelControllerUpdate(void* param);
	int determineFace(float x, float y, float z);

public:
	AccelerationController();
	void begin();
	void stop();

	void timerUpdate();
	int currentFace();

	const Core::RingBuffer<AccelFrame, ACCEL_BUFFER_SIZE>& getBuffer() const { return buffer; }

	// Notification management
	void hook(ClientMethod method, void* param);
	void unHook(ClientMethod client);
	void unHookWithParam(void* param);
};

extern AccelerationController accelController;

#endif

