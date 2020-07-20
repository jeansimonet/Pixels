#pragma once

#include "core/ring_buffer.h"
#include "core/float3.h"
#include "core/delegate_array.h"

#define ACCEL_BUFFER_SIZE 10 // 10ms * 10 = 100ms seconds of buffer
							  // 16 bytes * 128 = 2k of RAM

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
		// size is 36
		/// </summary>
		struct AccelFrame
		{
			Core::float3 acc;
			Core::float3 jerk;
			Core::float3 smoothAcc;
			float sigma;
			float faceConfidence;
			int face;
			uint32_t time;
		};

	    enum RollState : uint8_t
		{
			RollState_Unknown = 0,
			RollState_OnFace,
			RollState_Handling,
			RollState_Rolling,
			RollState_Crooked,
		};

		int determineFace(Core::float3 acc, float* outConfidence = nullptr);

		void init();
		void start();
		void stop();

		int currentFace();
		float currentFaceConfidence();
		RollState currentRollState();
		const char* getRollStateString(RollState state);

		// Notification management
		typedef void(*FrameDataClientMethod)(void* param, const AccelFrame& accelFrame);
		void hookFrameData(FrameDataClientMethod method, void* param);
		void unHookFrameData(FrameDataClientMethod client);
		void unHookFrameDataWithParam(void* param);

		typedef void(*RollStateClientMethod)(void* param, RollState newState, int newFace);
		void hookRollState(RollStateClientMethod method, void* param);
		void unHookRollState(RollStateClientMethod client);
		void unHookRollStateWithParam(void* param);
	}
}


