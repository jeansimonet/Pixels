// AnimationSet.h

#ifndef _ANIMATIONSET_h
#define _ANIMATIONSET_h

#include "arduino.h"
#include "Animation.h"
#include "BulkDataTransfer.h"

// Per readme file here: https://github.com/blieber/arduino-flash-queue
// Many examples in Simblee docs say up to 251 is available, but CAUTION - if use OTA 
// unfortunately this also uses some address space. According to 
// http://forum.rfduino.com/index.php?topic=1347.0 240-251 are used by the OTA bootloader. 
// Hence to be safe even in this example we only write up to 239.
#define HIGHEST_FLASH_PAGE (235)

// Defined after linking - lowest page after all program memory.
const int LowestFlashPageAvailable = PAGE_FROM_ADDRESS(&_etextrelocate) + 1;

#define MAX_ANIMATIONS (64)
#define ANIMATION_SET_START_PAGE (HIGHEST_FLASH_PAGE - 1)
#define PAGE_SIZE (1024) // bytes
#define ANIMATION_SET_VALID_KEY (0x600DF00D) // Good Food ;)
// We place animation set and animations in descending addresses
// So the animation set is at the top of the page
#define ANIMATION_SET_ADDRESS ((ANIMATION_SET_START_PAGE + 1) * PAGE_SIZE - sizeof(AnimationSet))

/// <summary>
/// The Animation set is the set of all die animations. It is mapped directly to flash!
/// </summary>
class AnimationSet
{
private:
	// Indicates whether there is valid data
	uint32_t headMarker;
	const Animation* animations[MAX_ANIMATIONS];
	uint32_t count;
	uint32_t tailMarker;

public:
	bool CheckValid() const;
	int ComputeAnimationTotalSize() const;
	uint32_t Count() const;
	const Animation* GetAnimation(int index) const;
	void DebugPrint() const;

	struct ProgrammingToken
	{
		// Temporarily stores animation pointers as we program them in flash
		const Animation* animationPtrInFlash[MAX_ANIMATIONS];
		int currentCount;
		uint32_t nextAnimFlashAddress;
	};

	static bool EraseAnimations(size_t totalAnimByteSize, ProgrammingToken& outToken);
	static bool TransferAnimation(const Animation* sourceAnim, ProgrammingToken& inOutToken);
	static bool TransferAnimationRaw(const void* rawData, size_t rawDataSize, ProgrammingToken& inOutToken);
	static bool TransferAnimationSet(const Animation** sourceAnims, uint32_t animCount);
	static bool ProgramDefaultAnimationSet(uint32_t color);
private:
	static void PrintError(int error);
};

// The animation set in flash memory
extern const AnimationSet* animationSet;

/// <summary>
/// This defines a state machine that can manage receiving a new anim set
/// over bluetooth from the phone and then burn that animation set in flash.
/// </summary>
class ReceiveAnimSetSM
{
private:
	enum State
	{
		State_ErasingFlash = 0,
		State_SendingAck,
		State_TransferAnim,
		State_SendingReadyForNextAnim,
		State_Failed,
		State_Done
	};

	short count;
	State currentState;

	AnimationSet::ProgrammingToken progToken;
	ReceiveBulkDataSM receiveBulkDataSM;

	typedef void(*FinishedCallback)(void* token);
	FinishedCallback FinishedCallbackHandler;
	void* FinishedCallbackToken;

private:
	void Finish();

public:
	ReceiveAnimSetSM();
	void Setup(short animCount, short totalAnimByteSize, void* token, FinishedCallback handler);
	void Update();
};

/// <summary>
/// This defines a state machine that can send the current animation set over
/// bluetooth to the phone. Typically so the phone can edit it and redownload it.
/// </summary>
class SendAnimSetSM
{
private:
	enum State
	{
		State_SendingSetup,
		State_WaitingForSetupAck,
		State_SetupAckReceived,
		State_SendingAnim,
		State_WaitingForReadyForNextAnim,
		State_ReceivedReadyForNextAnim,
		State_Failed,
		State_Done
	};

	short currentAnim;
	State currentState;

	// Temporarily stores animation pointers as we program them in flash
	SendBulkDataSM sendBulkDataSM;

	typedef void(*FinishedCallback)(void* token);
	FinishedCallback FinishedCallbackHandler;
	void* FinishedCallbackToken;

private:
	void Finish();

public:
	SendAnimSetSM();
	void Setup(void* token, FinishedCallback handler);
	void Update();
};


#endif

