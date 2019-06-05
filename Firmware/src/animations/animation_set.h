#pragma once

#include "animation.h"
#include "stdint.h"
#include "../bluetooth/bulk_data_transfer.h"

#define COLOR_MAP_SIZE (1 << 9) // 128 colors!
#define MAX_ANIMATIONS (64)

namespace Animations
{
	namespace AnimationSet
	{
		bool CheckValid();
		int ComputeAnimationTotalSize();

		struct ProgrammingToken
		{
			// Temporarily stores animation pointers as we program them in flash
			const Animation* animationPtrInFlash[MAX_ANIMATIONS];
			int currentCount;
			uint32_t nextAnimFlashAddress;
		};

		uint32_t getColor(uint16_t colorIndex);
		RGBKeyframe getKeyframe(uint16_t keyFrameIndex);
		AnimationTrack getTrack(uint16_t trackIndex);
		AnimationTrack const * const getTracks(uint16_t tracksStartIndex);
		Animation getAnimation(uint16_t animIndex);
		uint16_t getAnimationCount();

		// bool EraseAnimations(uint32_t totalAnimByteSize, ProgrammingToken& outToken);
		// bool TransferPalette(const uint8_t* sourcePalette, ProgrammingToken& inOutToken);
		// bool TransferAnimation(const Animation* sourceAnim, ProgrammingToken& inOutToken);
		// bool TransferAnimationRaw(const void* rawData, uint32_t rawDataSize, ProgrammingToken& inOutToken);
		// bool TransferAnimationSet(const Animation** sourceAnims, uint32_t animCount);
		// bool ProgramDefaultAnimationSet(uint32_t color);
	}

	// /// <summary>
	// /// This defines a state machine that can manage receiving a new anim set
	// /// over bluetooth from the phone and then burn that animation set in flash.
	// /// </summary>
	// class ReceiveAnimSetSM
	// {
	// private:
	// 	enum State
	// 	{
	// 		State_ErasingFlash = 0,
	// 		State_SendingAck,
	// 		State_TransferAnim,
	// 		State_SendingReadyForNextAnim,
	// 		State_Failed,
	// 		State_Done
	// 	};

	// 	short count;
	// 	State currentState;

	// 	AnimationSet::ProgrammingToken progToken;
	// 	ReceiveBulkDataSM receiveBulkDataSM;

	// 	typedef void(*FinishedCallback)(void* token);
	// 	FinishedCallback FinishedCallbackHandler;
	// 	void* FinishedCallbackToken;

	// private:
	// 	void Finish();

	// public:
	// 	ReceiveAnimSetSM();
	// 	void Setup(short animCount, short totalAnimByteSize, void* token, FinishedCallback handler);
	// 	void Update();
	// };

	// /// <summary>
	// /// This defines a state machine that can send the current animation set over
	// /// bluetooth to the phone. Typically so the phone can edit it and redownload it.
	// /// </summary>
	// class SendAnimSetSM
	// {
	// private:
	// 	enum State
	// 	{
	// 		State_SendingSetup,
	// 		State_WaitingForSetupAck,
	// 		State_SetupAckReceived,
	// 		State_SendingAnim,
	// 		State_WaitingForReadyForNextAnim,
	// 		State_ReceivedReadyForNextAnim,
	// 		State_Failed,
	// 		State_Done
	// 	};

	// 	short currentAnim;
	// 	State currentState;

	// 	// Temporarily stores animation pointers as we program them in flash
	// 	SendBulkDataSM sendBulkDataSM;

	// 	typedef void(*FinishedCallback)(void* token);
	// 	FinishedCallback FinishedCallbackHandler;
	// 	void* FinishedCallbackToken;

	// private:
	// 	void Finish();

	// public:
	// 	SendAnimSetSM();
	// 	void Setup(void* token, FinishedCallback handler);
	// 	void Update();
	// };

}

