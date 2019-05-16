#include "AnimationSet.h"
#include "Debug.h"
#include "Die.h"
#include "SimbleeBLE.h"
#include "BluetoothMessage.h"
#include "Utils.h"
#include "LEDs.h"

// The animation set always points at a specific address in memory
const AnimationSet* animationSet = (const AnimationSet*)ANIMATION_SET_ADDRESS;

/// <summary>
/// Checks whether the animation set in flash is valid or garbage data
/// </summary>
bool AnimationSet::CheckValid() const
{
	return headMarker == ANIMATION_SET_VALID_KEY && tailMarker == ANIMATION_SET_VALID_KEY;
}

/// <summary>
/// In order to help the receiving end, we can compute the total byte size of all the
/// animations in the set (excluding the set itself).
/// </summary>
int AnimationSet::ComputeAnimationTotalSize() const
{
	if (!CheckValid())
		return -1;

	int ret = 0;
	for (int i = 0; i < count; ++i)
	{
		ret += animations[i]->ComputeByteSize();
	}
	return ret;
}

/// <summary>
/// How many animations are in the set?
/// </summary>
uint32_t AnimationSet::Count() const
{
	if (CheckValid())
		return count;
	else
		return 0;
}

/// <summary>
/// Fetch an animation, if valid!
/// </summary>
const Animation* AnimationSet::GetAnimation(int index) const
{
	if (CheckValid() && index < count)
	{
		return animations[index];
	}
	else
	{
		return nullptr;
	}
}


bool AnimationSet::EraseAnimations(size_t totalAnimByteSize, ProgrammingToken& outToken)
{
	// How many pages will we need?
	int totalSize = totalAnimByteSize + sizeof(AnimationSet);
	int pageCount = (totalSize + PAGE_SIZE - 1) / PAGE_SIZE; // a page is 1k, and we want to round up!

	// Erase all necessary pages
	bool eraseSuccessful = true;
	for (int i = 0; i < pageCount; ++i)
	{
		int res = flashPageErase(ANIMATION_SET_START_PAGE - i);
		if (res != 0)
		{
			debugPrint("Not enough free pages (needed ");
			debugPrint(pageCount);
			debugPrint(" pages for ");
			debugPrint(totalSize);
			debugPrintln(" bytes of animation data)");
			eraseSuccessful = false;
			break;
		}
	}
	if (eraseSuccessful)
	{
		outToken.currentCount = 0;
		outToken.nextAnimFlashAddress = ANIMATION_SET_ADDRESS;
	}
	return eraseSuccessful;
}

bool AnimationSet::TransferAnimation(const Animation* sourceAnimation, ProgrammingToken& inOutToken)
{
	return TransferAnimationRaw(sourceAnimation, sourceAnimation->ComputeByteSize(), inOutToken);
}

bool AnimationSet::TransferAnimationRaw(const void* rawData, size_t rawDataSize, ProgrammingToken& inOutToken)
{
	// The reason we're subtracting here is that we place animation set and animations in descending addresses
	inOutToken.nextAnimFlashAddress -= rawDataSize;
	Animation* dst = (Animation*)inOutToken.nextAnimFlashAddress;
	int res = flashWriteBlock(dst, rawData, rawDataSize);
	if (res == 0)
	{
		// Remember the address of this new animation
		inOutToken.animationPtrInFlash[inOutToken.currentCount] = dst;
		inOutToken.currentCount++;
	}
	else
	{
		PrintError(res);
	}
	return res == 0;
}

bool AnimationSet::TransferAnimationSet(const Animation ** sourceAnims, uint32_t animCount)
{
	// We overwrite the members manually!
	uint32_t* progAnimationSetRaw = (uint32_t*)ANIMATION_SET_ADDRESS;
	int res = flashWrite(progAnimationSetRaw, ANIMATION_SET_VALID_KEY);
	if (res == 0)
	{
		progAnimationSetRaw += 1;
		res = flashWriteBlock(progAnimationSetRaw, sourceAnims, sizeof(Animation*) * animCount);
		if (res == 0)
		{
			progAnimationSetRaw += sizeof(Animation*) * MAX_ANIMATIONS / 4;
			res = flashWrite(progAnimationSetRaw, animCount);
			if (res == 0)
			{
				progAnimationSetRaw += 1;
				res = flashWrite(progAnimationSetRaw, ANIMATION_SET_VALID_KEY);
			}
		}
	}

	if (res != 0)
	{
		PrintError(res);
	}
	return res == 0;
}

bool AnimationSet::ProgramDefaultAnimationSet(uint32_t color)
{
	// We're going to program a few animations!
	int totalAnimSize = 0;
	Animation* faceAnims[12];
	for (int i = 0; i < 6; ++i)
	{
		Animation* anim = Animation::AllocateAnimation(i + 1); // face i has i+1 led
		//Animation* anim = Animation::AllocateAnimation(1);

		int totalTime = 400;
		int ledTime = totalTime / (i + 1);
		for (int j = 0; j <= i; ++j)
		//int j = 0;
		{
			AnimationTrack updown;
			updown.count = 0;
			updown.startTime = ledTime * j;	// ms
			updown.duration = totalTime;	// ms
			updown.ledIndex = LEDs::ledIndex(i,j);
			updown.AddKeyframe(0, 0, 0, 0);
			updown.AddKeyframe(128, Core::getRed(color), Core::getGreen(color), Core::getBlue(color));
			updown.AddKeyframe(255, 0, 0, 0);
			anim->SetTrack(updown, j);
		}

		faceAnims[i] = anim;
		totalAnimSize += anim->ComputeByteSize();
	}

	for (int i = 0; i < 6; ++i)
	{
		Animation* anim = Animation::AllocateAnimation(i + 1); // face i has i+1 led
															   //Animation* anim = Animation::AllocateAnimation(1);
		int totalTime = 1500;
		for (int j = 0; j <= i; ++j)
		{
			AnimationTrack updown;
			updown.count = 0;
			updown.startTime = 0;	// ms
			updown.duration = totalTime;	// ms
			updown.ledIndex = LEDs::ledIndex(i, j);
			updown.AddKeyframe(0, Core::getRed(color), Core::getGreen(color), Core::getBlue(color));
			updown.AddKeyframe(160, Core::getRed(color), Core::getGreen(color), Core::getBlue(color));
			updown.AddKeyframe(255, 0, 0, 0);
			anim->SetTrack(updown, j);
		}

		faceAnims[i+6] = anim;
		totalAnimSize += anim->ComputeByteSize();
	}

	AnimationSet::ProgrammingToken token;
	bool ret = EraseAnimations(totalAnimSize, token);
	if (ret)
	{
		for (int i = 0; ret && i < 12; ++i)
		{
			ret = TransferAnimation(faceAnims[i], token);
		}

		if (ret)
		{
			ret = AnimationSet::TransferAnimationSet(token.animationPtrInFlash, token.currentCount);
		}
	}

	// Clean up memory
	for (int i = 0; i < 12; ++i)
	{
		free(faceAnims[i]);
	}

	return ret;
}


void AnimationSet::PrintError(int error)
{
	// Print error message if any
	switch (error)
	{
	case 1:
		debugPrint("Animations could not be written, reserved page");
		break;
	case 2:
		debugPrint("Animations could not be written, sketch page");
		break;
	case 4:
		debugPrint("Bad data size");
		break;
	default:
		break;
	}
}

void AnimationSet::DebugPrint() const
{
	Serial.print("AnimationSet address; ");
	Serial.println((uint32_t)animationSet, HEX);
	Serial.print("Head key = ");
	Serial.print(animationSet->headMarker, HEX);
	Serial.println(" (should be 0x600DF00D)");
	Serial.print("Set contains ");
	Serial.print(animationSet->Count());
	Serial.println(" animations");

	for (int i = 0; i < animationSet->Count(); ++i)
	{
		auto anim = animationSet->GetAnimation(i);
		Serial.print("Anim ");
		Serial.print(i);
		Serial.print(" contains ");
		Serial.print(anim->TrackCount());
		Serial.println(" tracks");

		for (int j = 0; j < anim->TrackCount(); ++j)
		{
			auto& track = anim->GetTrack(j);
			Serial.print("Anim ");
			Serial.print(i);
			Serial.print(", track ");
			Serial.print(j);
			Serial.print(" has ");
			Serial.print(track.count);
			Serial.print(" keyframes, starts at ");
			Serial.print(track.startTime);
			Serial.print(" ms, lasts ");
			Serial.print(track.duration);
			Serial.print(" ms and controls LED ");
			Serial.println(track.ledIndex);

			for (int k = 0; k < track.count; ++k)
			{
				auto& keyframe = track.keyframes[k];
				Serial.print("(");
				Serial.print(keyframe.time);
				Serial.print("ms = ");
				Serial.print(keyframe.red);
				Serial.print(", ");
				Serial.print(keyframe.green);
				Serial.print(", ");
				Serial.print(keyframe.blue);
				Serial.print(") ");
			}
			Serial.println();
		}
	}

	Serial.print("Tail key = ");
	Serial.print(animationSet->tailMarker, HEX);
	Serial.println(" (should be 0x600DF00D)");

};

//-----------------------------------------------------------------------------


/// <summary>
/// Constructor
/// </summary>
ReceiveAnimSetSM::ReceiveAnimSetSM()
	: count(0)
	, currentState(State_Done)
	, FinishedCallbackHandler(nullptr)
	, FinishedCallbackToken(nullptr)
{
}

/// <summary>
/// Prepare to receive an animation set, erase the flash, etc...
/// </summary>
void ReceiveAnimSetSM::Setup(short animCount, short totalAnimByteSize, void* token, FinishedCallback handler)
{
	debugPrint("Receiving animation set ");
	debugPrint(animCount);
	debugPrint(" animations for a total of ");
	debugPrint(totalAnimByteSize);
	debugPrintln(" bytes");

	count = animCount;
	currentState = State_ErasingFlash;

	FinishedCallbackHandler = handler;
	FinishedCallbackToken = token;

	// How many pages will we need?
	int totalSize = totalAnimByteSize + sizeof(AnimationSet);
	int pageCount = (totalSize + 1023) / 1024; // a page is 1k, and we want to round up!

	// Erase all necessary pages
	if (AnimationSet::EraseAnimations(totalSize, progToken))
	{
		// Register for update so we can try to send ack messages
		die.RegisterUpdate(this, [](void* token)
		{
			((ReceiveAnimSetSM*)token)->Update();
		});
		currentState = State_SendingAck;
	}
	else
	{
		currentState = State_Done;
	}
}

/// <summary>
/// State machine update method, which we registered with the die in Setup()
/// </summary>
void ReceiveAnimSetSM::Update()
{
	switch (currentState)
	{
	case State_SendingAck:
		{
			debugPrintln("sending Ack");
			if (die.SendMessage(DieMessage::MessageType_TransferAnimSetAck))
			{
				// Prepare to receive animation bulk data
				currentState = State_TransferAnim;
				receiveBulkDataSM.Setup();
			}
			// Else we try again next update
		}
		break;
	case State_TransferAnim:
		{
			debugPrint("Is bulk transfer done?");
			// Is it done?
			switch (receiveBulkDataSM.GetState())
			{
			case BulkDataState_Complete:
				{
					debugPrint("yes, copy to flash");
					// The anim data is ready, copy it to flash!
					if (!AnimationSet::TransferAnimationRaw(receiveBulkDataSM.mallocData, receiveBulkDataSM.mallocSize, progToken))
					{
						receiveBulkDataSM.Finish();
						Finish();
					}
					else
					{
						// Clean up memory allocated by the bulk transfer
						receiveBulkDataSM.Finish();

						if (progToken.currentCount == count)
						{
							debugPrint("no more anims, programming animation set");
							// No more anims to receive, program AnimationSet in flash
							AnimationSet::TransferAnimationSet(progToken.animationPtrInFlash, count);

							// Clean up animation table too
							Finish();
						}
						else
						{
							currentState = State_SendingReadyForNextAnim;
						}
					}
				}
				break;
			case BulkDataState_Failed:
				debugPrint("Timeout transfering animation data");
				currentState = State_Failed;
				break;
			default:
				// Else keep waiting
				break;
			}
		}
		break;
	case State_SendingReadyForNextAnim:
		{
		debugPrint("Indicating we are ready for next anim.");
		if (die.SendMessage(DieMessage::MessageType_TransferAnimReadyForNextAnim))
			{
				// Prepare to receive next animation bulk data
				currentState = State_TransferAnim;
				receiveBulkDataSM.Setup();
			}
			// Else we try again next update
		}
		break;
	default:
		break;
	}
}


/// <summary>
/// Clean up after having received a new animation set
/// This mostly means feeing temporary memory
/// </summary>
void ReceiveAnimSetSM::Finish()
{
	count = 0;
	currentState = State_Done;
	die.UnregisterUpdateToken(this);

	if (FinishedCallbackHandler != nullptr)
	{
		FinishedCallbackHandler(FinishedCallbackToken);
		FinishedCallbackHandler = nullptr;
		FinishedCallbackToken = nullptr;
	}
}


//-----------------------------------------------------------------------------


/// <summary>
/// Constructor
/// </summary>
SendAnimSetSM::SendAnimSetSM()
	: currentAnim(0)
	, currentState(State_Done)
	, FinishedCallbackHandler(nullptr)
	, FinishedCallbackToken(nullptr)
{
}

/// <summary>
/// Prepare for sending the animation set data over bluetooth
/// </summary>
void SendAnimSetSM::Setup(void* token, FinishedCallback handler)
{
	if (animationSet->CheckValid())
	{
		currentAnim = 0;
		currentState = State_SendingSetup;

		FinishedCallbackHandler = handler;
		FinishedCallbackToken = token;

		die.RegisterUpdate(this, [](void* token)
		{
			((SendAnimSetSM*)token)->Update();
		});
	}
}

/// <summary>
/// State Machine update method, which we registered with the die in Setup()
/// </summary>
void SendAnimSetSM::Update()
{
	switch (currentState)
	{
	case State_SendingSetup:
		{
			DieMessageTransferAnimSet setupMsg;
			setupMsg.count = animationSet->Count();
			setupMsg.totalAnimationByteSize = animationSet->ComputeAnimationTotalSize();
			if (die.SendMessage(&setupMsg, sizeof(setupMsg)))
			{
				die.RegisterMessageHandler(DieMessage::MessageType_TransferAnimSetAck, this, [](void* token, DieMessage* msg)
				{
					((SendAnimSetSM*)token)->currentState = State_SetupAckReceived;
				});

				currentState = State_WaitingForSetupAck;
			}
			// Else try again next update
		}
		break;
	case State_SetupAckReceived:
		{
			// Unregister from the ack message
			die.UnregisterMessageHandler(DieMessage::MessageType_TransferAnimSetAck);

			// Start transfering the anims
			auto anim = animationSet->GetAnimation(currentAnim);
			sendBulkDataSM.Setup((byte*)anim, anim->ComputeByteSize());
			currentState = State_SendingAnim;
		}
		break;
	case State_SendingAnim:
		{
			// Is the transfer complete?
			switch (sendBulkDataSM.GetState())
			{
			case BulkDataState_Complete:
				{
					// Next anim!
					currentAnim++;
					if (currentAnim == animationSet->Count())
					{
						// We're done!
						Finish();
					}
					else
					{
						// Wait for a message indicating that the other side is ready for the next anim
						die.RegisterMessageHandler(DieMessage::MessageType_TransferAnimReadyForNextAnim, this, [](void* token, DieMessage* msg)
						{
							((SendAnimSetSM*)token)->currentState = State_ReceivedReadyForNextAnim;
						});

						currentState = State_WaitingForReadyForNextAnim;
					}
				}
				break;
			case BulkDataState_Failed:
				currentState = State_Failed;
				break;
			default:
				// Else wait some more
				break;
			}
		}
		break;
	case SendAnimSetSM::State_ReceivedReadyForNextAnim:
		{
			// Unregister from the ack message
			die.UnregisterMessageHandler(DieMessage::MessageType_TransferAnimReadyForNextAnim);

			// Start transfering the anims
			auto anim = animationSet->GetAnimation(currentAnim);
			sendBulkDataSM.Setup((byte*)anim, anim->ComputeByteSize());
			currentState = State_SendingAnim;
		}
		break;
	default:
		break;
	}
}

/// <summary>
/// Clean up after ourselves
/// </summary>
void SendAnimSetSM::Finish()
{
	currentAnim = 0;
	currentState = State_Done;
	die.UnregisterUpdateToken(this);

	if (FinishedCallbackHandler != nullptr)
	{
		FinishedCallbackHandler(FinishedCallbackToken);
		FinishedCallbackHandler = nullptr;
		FinishedCallbackToken = nullptr;
	}
}

