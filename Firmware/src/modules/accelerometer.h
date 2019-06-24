#pragma once

#include "core/ring_buffer.h"
#include "core/float3.h"
#include "core/delegate_array.h"

#define ACCEL_BUFFER_SIZE 100 // 10ms * 100 = 1 seconds of buffer
							  // 16 bytes * 128 = 2k of RAM
#define MAX_ACC_CLIENTS 4

namespace Modules
{
	/// <summary>
	/// The component in charge of maintaining the acceleraion readings,
	/// and determining die motion state.
	/// </summary>
	namespace Accelerometer
	{
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

		typedef void(*ClientMethod)(void* param, const AccelFrame& accelFrame);

		int determineFace(float x, float y, float z, float* outConfidence = nullptr);

		void init();
		void start();
		void stop();

		int currentFace();

		//const Core::RingBuffer<AccelFrame, ACCEL_BUFFER_SIZE>& getBuffer() { return buffer; }

		// Notification management
		void hook(ClientMethod method, void* param);
		void unHook(ClientMethod client);
		void unHookWithParam(void* param);
	}
}


