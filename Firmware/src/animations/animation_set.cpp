#include "animation_set.h"
#include "utils/utils.h"

#include "malloc.h"
#include "assert.h"

#define ANIMATION_SET_VALID_KEY (0x600DF00D) // Good Food ;)
// We place animation set and animations in descending addresses
// So the animation set is at the top of the page
#define ANIMATION_SET_ADDRESS (0)

using namespace Utils;

namespace Animations
{
namespace AnimationSet
{
	struct Data
	{
		// Indicates whether there is valid data
		uint32_t headMarker;

		// The palette for all animations, stored in RGB RGB RGB etc...
		// 128 * 3 = 376 bytes
		uint8_t palette[COLOR_MAP_SIZE * 3];

		// The animations we have
		const RGBKeyframe* keyframes; // pointer to the array of tracks
		uint32_t keyFrameCount;

		// The animations we have
		const AnimationTrack* tracks; // pointer to the array of tracks
		uint32_t trackCount;

		// The animations we have
		const Animation* animations;
		uint32_t animationCount;

		// Indicates whether there is valid data
		uint32_t tailMarker;
	};

	// The animation set always points at a specific address in memory
	const Data* data = (const Data*)ANIMATION_SET_ADDRESS;

	/// <summary>
	/// Checks whether the animation set in flash is valid or garbage data
	/// </summary>
	bool CheckValid()
	{
		return data->headMarker == ANIMATION_SET_VALID_KEY && data->tailMarker == ANIMATION_SET_VALID_KEY;
	}

	/// <summary>
	/// In order to help the receiving end, we can compute the total byte size of all the
	/// animations in the set (excluding the set itself).
	/// </summary>
	int ComputeAnimationTotalSize() {
		if (!CheckValid())
			return -1;

		return 
			sizeof(uint8_t) * COLOR_MAP_SIZE * 3 +
			sizeof(RGBKeyframe) * data->keyFrameCount +
			sizeof(AnimationTrack) * data->trackCount +
			sizeof(Animation) * data-> animationCount;
	}

	uint32_t getColor(uint16_t colorIndex) {
		assert(CheckValid() && colorIndex < COLOR_MAP_SIZE);
		return toColor(
				data->palette[colorIndex * 3 + 0],
				data->palette[colorIndex * 3 + 1],
				data->palette[colorIndex * 3 + 2]);
	}

	RGBKeyframe getKeyframe(uint16_t keyFrameIndex) {
		assert(CheckValid() && keyFrameIndex < data->keyFrameCount);
		return data->keyframes[keyFrameIndex];
	}

	AnimationTrack getTrack(uint16_t trackIndex) {
		assert(CheckValid() && trackIndex < data->trackCount);
		return data->tracks[trackIndex];
	}

	AnimationTrack const * const getTracks(uint16_t tracksStartIndex) {
		assert(CheckValid() && tracksStartIndex < data->trackCount);
		return &(data->tracks[tracksStartIndex]);
	}

	Animation getAnimation(uint16_t animIndex) {
		assert(CheckValid() && animIndex < data->animationCount);
		return data->animations[animIndex];
	}


	uint16_t getAnimationCount() {
		assert(CheckValid());
			return data->animationCount;
	}

	// bool EraseAnimations(uint32_t totalAnimByteSize, ProgrammingToken& outToken)
	// {
	// 	// How many pages will we need?
	// 	int totalSize = totalAnimByteSize + sizeof(Data);
	// 	int pageCount = (totalSize + PAGE_SIZE - 1) / PAGE_SIZE; // a page is 1k, and we want to round up!

	// 	// Erase all necessary pages
	// 	bool eraseSuccessful = true;
	// 	for (int i = 0; i < pageCount; ++i)
	// 	{
	// 		int res = flashPageErase(ANIMATION_SET_START_PAGE - i);
	// 		if (res != 0)
	// 		{
	// 			debugPrint("Not enough free pages (needed ");
	// 			debugPrint(pageCount);
	// 			debugPrint(" pages for ");
	// 			debugPrint(totalSize);
	// 			debugPrintln(" bytes of animation data)");
	// 			eraseSuccessful = false;
	// 			break;
	// 		}
	// 	}
	// 	if (eraseSuccessful)
	// 	{
	// 		outToken.currentCount = 0;
	// 		outToken.nextAnimFlashAddress = ANIMATION_SET_ADDRESS;
	// 	}
	// 	return eraseSuccessful;
	// }

	// bool TransferAnimation(const Animation* sourceAnimation, ProgrammingToken& inOutToken)
	// {
	// 	return TransferAnimationRaw(sourceAnimation, sourceAnimation->computeByteSize(), inOutToken);
	// }

	// bool TransferAnimationRaw(const void* rawData, uint32_t rawDataSize, ProgrammingToken& inOutToken)
	// {
	// 	// The reason we're subtracting here is that we place animation set and animations in descending addresses
	// 	inOutToken.nextAnimFlashAddress -= rawDataSize;
	// 	Animation* dst = (Animation*)inOutToken.nextAnimFlashAddress;
	// 	int res = flashWriteBlock(dst, rawData, rawDataSize);
	// 	if (res == 0)
	// 	{
	// 		// Remember the address of this new animation
	// 		inOutToken.animationPtrInFlash[inOutToken.currentCount] = dst;
	// 		inOutToken.currentCount++;
	// 	}
	// 	else
	// 	{
	// 		PrintError(res);
	// 	}
	// 	return res == 0;
	// }

	// bool TransferAnimationSet(const Animation ** sourceAnims, uint32_t animCount)
	// {
	// 	// We overwrite the members manually!
	// 	uint32_t* progAnimationSetRaw = (uint32_t*)ANIMATION_SET_ADDRESS;
	// 	int res = flashWrite(progAnimationSetRaw, ANIMATION_SET_VALID_KEY);
	// 	if (res == 0)
	// 	{
	// 		progAnimationSetRaw += 1;
	// 		res = flashWriteBlock(progAnimationSetRaw, sourceAnims, sizeof(Animation*) * animCount);
	// 		if (res == 0)
	// 		{
	// 			progAnimationSetRaw += sizeof(Animation*) * MAX_ANIMATIONS / 4;
	// 			res = flashWrite(progAnimationSetRaw, animCount);
	// 			if (res == 0)
	// 			{
	// 				progAnimationSetRaw += 1;
	// 				res = flashWrite(progAnimationSetRaw, ANIMATION_SET_VALID_KEY);
	// 			}
	// 		}
	// 	}

	// 	if (res != 0)
	// 	{
	// 		PrintError(res);
	// 	}
	// 	return res == 0;
	// }

	// bool ProgramDefaultAnimationSet(uint32_t color)
	// {
	// 	// We're going to program a few animations!
	// 	int totalAnimSize = 0;
	// 	Animation* faceAnims[12];
	// 	for (int i = 0; i < 6; ++i)
	// 	{
	// 		Animation* anim = Animation::AllocateAnimation(i + 1); // face i has i+1 led
	// 		//Animation* anim = Animation::AllocateAnimation(1);

	// 		int totalTime = 400;
	// 		int ledTime = totalTime / (i + 1);
	// 		for (int j = 0; j <= i; ++j)
	// 		//int j = 0;
	// 		{
	// 			AnimationTrack updown;
	// 			updown.count = 0;
	// 			updown.startTime = ledTime * j;	// ms
	// 			updown.duration = totalTime;	// ms
	// 			updown.ledIndex = LEDs::ledIndex(i,j);
	// 			updown.AddKeyframe(0, 0, 0, 0);
	// 			updown.AddKeyframe(128, Core::getRed(color), Core::getGreen(color), Core::getBlue(color));
	// 			updown.AddKeyframe(255, 0, 0, 0);
	// 			anim->SetTrack(updown, j);
	// 		}

	// 		faceAnims[i] = anim;
	// 		totalAnimSize += anim->ComputeByteSize();
	// 	}

	// 	for (int i = 0; i < 6; ++i)
	// 	{
	// 		Animation* anim = Animation::AllocateAnimation(i + 1); // face i has i+1 led
	// 															//Animation* anim = Animation::AllocateAnimation(1);
	// 		int totalTime = 1500;
	// 		for (int j = 0; j <= i; ++j)
	// 		{
	// 			AnimationTrack updown;
	// 			updown.count = 0;
	// 			updown.startTime = 0;	// ms
	// 			updown.duration = totalTime;	// ms
	// 			updown.ledIndex = LEDs::ledIndex(i, j);
	// 			updown.AddKeyframe(0, Core::getRed(color), Core::getGreen(color), Core::getBlue(color));
	// 			updown.AddKeyframe(160, Core::getRed(color), Core::getGreen(color), Core::getBlue(color));
	// 			updown.AddKeyframe(255, 0, 0, 0);
	// 			anim->SetTrack(updown, j);
	// 		}

	// 		faceAnims[i+6] = anim;
	// 		totalAnimSize += anim->computeByteSize();
	// 	}

	// 	ProgrammingToken token;
	// 	bool ret = EraseAnimations(totalAnimSize, token);
	// 	if (ret)
	// 	{
	// 		for (int i = 0; ret && i < 12; ++i)
	// 		{
	// 			ret = TransferAnimation(faceAnims[i], token);
	// 		}

	// 		if (ret)
	// 		{
	// 			ret = TransferAnimationSet(token.animationPtrInFlash, token.currentCount);
	// 		}
	// 	}

	// 	// Clean up memory
	// 	for (int i = 0; i < 12; ++i)
	// 	{
	// 		free(faceAnims[i]);
	// 	}

	// 	return ret;
	// }
}
}



// //-----------------------------------------------------------------------------


// /// <summary>
// /// Constructor
// /// </summary>
// ReceiveAnimSetSM::ReceiveAnimSetSM()
// 	: count(0)
// 	, currentState(State_Done)
// 	, FinishedCallbackHandler(nullptr)
// 	, FinishedCallbackToken(nullptr)
// {
// }

// /// <summary>
// /// Prepare to receive an animation set, erase the flash, etc...
// /// </summary>
// void ReceiveAnimSetSM::Setup(short animCount, short totalAnimByteSize, void* token, FinishedCallback handler)
// {
// 	debugPrint("Receiving animation set ");
// 	debugPrint(animCount);
// 	debugPrint(" animations for a total of ");
// 	debugPrint(totalAnimByteSize);
// 	debugPrintln(" bytes");

// 	count = animCount;
// 	currentState = State_ErasingFlash;

// 	FinishedCallbackHandler = handler;
// 	FinishedCallbackToken = token;

// 	// How many pages will we need?
// 	int totalSize = totalAnimByteSize + sizeof(AnimationSet);
// 	int pageCount = (totalSize + 1023) / 1024; // a page is 1k, and we want to round up!

// 	// Erase all necessary pages
// 	if (AnimationSet::EraseAnimations(totalSize, progToken))
// 	{
// 		// Register for update so we can try to send ack messages
// 		die.RegisterUpdate(this, [](void* token)
// 		{
// 			((ReceiveAnimSetSM*)token)->Update();
// 		});
// 		currentState = State_SendingAck;
// 	}
// 	else
// 	{
// 		currentState = State_Done;
// 	}
// }

// /// <summary>
// /// State machine update method, which we registered with the die in Setup()
// /// </summary>
// void ReceiveAnimSetSM::Update()
// {
// 	switch (currentState)
// 	{
// 	case State_SendingAck:
// 		{
// 			debugPrintln("sending Ack");
// 			if (die.SendMessage(DieMessage::MessageType_TransferAnimSetAck))
// 			{
// 				// Prepare to receive animation bulk data
// 				currentState = State_TransferAnim;
// 				receiveBulkDataSM.Setup();
// 			}
// 			// Else we try again next update
// 		}
// 		break;
// 	case State_TransferAnim:
// 		{
// 			debugPrint("Is bulk transfer done?");
// 			// Is it done?
// 			switch (receiveBulkDataSM.GetState())
// 			{
// 			case BulkDataState_Complete:
// 				{
// 					debugPrint("yes, copy to flash");
// 					// The anim data is ready, copy it to flash!
// 					if (!AnimationSet::TransferAnimationRaw(receiveBulkDataSM.mallocData, receiveBulkDataSM.mallocSize, progToken))
// 					{
// 						receiveBulkDataSM.Finish();
// 						Finish();
// 					}
// 					else
// 					{
// 						// Clean up memory allocated by the bulk transfer
// 						receiveBulkDataSM.Finish();

// 						if (progToken.currentCount == count)
// 						{
// 							debugPrint("no more anims, programming animation set");
// 							// No more anims to receive, program AnimationSet in flash
// 							AnimationSet::TransferAnimationSet(progToken.animationPtrInFlash, count);

// 							// Clean up animation table too
// 							Finish();
// 						}
// 						else
// 						{
// 							currentState = State_SendingReadyForNextAnim;
// 						}
// 					}
// 				}
// 				break;
// 			case BulkDataState_Failed:
// 				debugPrint("Timeout transfering animation data");
// 				currentState = State_Failed;
// 				break;
// 			default:
// 				// Else keep waiting
// 				break;
// 			}
// 		}
// 		break;
// 	case State_SendingReadyForNextAnim:
// 		{
// 		debugPrint("Indicating we are ready for next anim.");
// 		if (die.SendMessage(DieMessage::MessageType_TransferAnimReadyForNextAnim))
// 			{
// 				// Prepare to receive next animation bulk data
// 				currentState = State_TransferAnim;
// 				receiveBulkDataSM.Setup();
// 			}
// 			// Else we try again next update
// 		}
// 		break;
// 	default:
// 		break;
// 	}
// }


// /// <summary>
// /// Clean up after having received a new animation set
// /// This mostly means feeing temporary memory
// /// </summary>
// void ReceiveAnimSetSM::Finish()
// {
// 	count = 0;
// 	currentState = State_Done;
// 	die.UnregisterUpdateToken(this);

// 	if (FinishedCallbackHandler != nullptr)
// 	{
// 		FinishedCallbackHandler(FinishedCallbackToken);
// 		FinishedCallbackHandler = nullptr;
// 		FinishedCallbackToken = nullptr;
// 	}
// }


// //-----------------------------------------------------------------------------


// /// <summary>
// /// Constructor
// /// </summary>
// SendAnimSetSM::SendAnimSetSM()
// 	: currentAnim(0)
// 	, currentState(State_Done)
// 	, FinishedCallbackHandler(nullptr)
// 	, FinishedCallbackToken(nullptr)
// {
// }

// /// <summary>
// /// Prepare for sending the animation set data over bluetooth
// /// </summary>
// void SendAnimSetSM::Setup(void* token, FinishedCallback handler)
// {
// 	if (animationSet->CheckValid())
// 	{
// 		currentAnim = 0;
// 		currentState = State_SendingSetup;

// 		FinishedCallbackHandler = handler;
// 		FinishedCallbackToken = token;

// 		die.RegisterUpdate(this, [](void* token)
// 		{
// 			((SendAnimSetSM*)token)->Update();
// 		});
// 	}
// }

// /// <summary>
// /// State Machine update method, which we registered with the die in Setup()
// /// </summary>
// void SendAnimSetSM::Update()
// {
// 	switch (currentState)
// 	{
// 	case State_SendingSetup:
// 		{
// 			DieMessageTransferAnimSet setupMsg;
// 			setupMsg.count = animationSet->Count();
// 			setupMsg.totalAnimationByteSize = animationSet->ComputeAnimationTotalSize();
// 			if (die.SendMessage(&setupMsg, sizeof(setupMsg)))
// 			{
// 				die.RegisterMessageHandler(DieMessage::MessageType_TransferAnimSetAck, this, [](void* token, DieMessage* msg)
// 				{
// 					((SendAnimSetSM*)token)->currentState = State_SetupAckReceived;
// 				});

// 				currentState = State_WaitingForSetupAck;
// 			}
// 			// Else try again next update
// 		}
// 		break;
// 	case State_SetupAckReceived:
// 		{
// 			// Unregister from the ack message
// 			die.UnregisterMessageHandler(DieMessage::MessageType_TransferAnimSetAck);

// 			// Start transfering the anims
// 			auto anim = animationSet->GetAnimation(currentAnim);
// 			sendBulkDataSM.Setup((byte*)anim, anim->ComputeByteSize());
// 			currentState = State_SendingAnim;
// 		}
// 		break;
// 	case State_SendingAnim:
// 		{
// 			// Is the transfer complete?
// 			switch (sendBulkDataSM.GetState())
// 			{
// 			case BulkDataState_Complete:
// 				{
// 					// Next anim!
// 					currentAnim++;
// 					if (currentAnim == animationSet->Count())
// 					{
// 						// We're done!
// 						Finish();
// 					}
// 					else
// 					{
// 						// Wait for a message indicating that the other side is ready for the next anim
// 						die.RegisterMessageHandler(DieMessage::MessageType_TransferAnimReadyForNextAnim, this, [](void* token, DieMessage* msg)
// 						{
// 							((SendAnimSetSM*)token)->currentState = State_ReceivedReadyForNextAnim;
// 						});

// 						currentState = State_WaitingForReadyForNextAnim;
// 					}
// 				}
// 				break;
// 			case BulkDataState_Failed:
// 				currentState = State_Failed;
// 				break;
// 			default:
// 				// Else wait some more
// 				break;
// 			}
// 		}
// 		break;
// 	case SendAnimSetSM::State_ReceivedReadyForNextAnim:
// 		{
// 			// Unregister from the ack message
// 			die.UnregisterMessageHandler(DieMessage::MessageType_TransferAnimReadyForNextAnim);

// 			// Start transfering the anims
// 			auto anim = animationSet->GetAnimation(currentAnim);
// 			sendBulkDataSM.Setup((byte*)anim, anim->ComputeByteSize());
// 			currentState = State_SendingAnim;
// 		}
// 		break;
// 	default:
// 		break;
// 	}
// }

// /// <summary>
// /// Clean up after ourselves
// /// </summary>
// void SendAnimSetSM::Finish()
// {
// 	currentAnim = 0;
// 	currentState = State_Done;
// 	die.UnregisterUpdateToken(this);

// 	if (FinishedCallbackHandler != nullptr)
// 	{
// 		FinishedCallbackHandler(FinishedCallbackToken);
// 		FinishedCallbackHandler = nullptr;
// 		FinishedCallbackToken = nullptr;
// 	}
// }

