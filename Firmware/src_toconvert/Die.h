// Die.h

#ifndef _DIE_h
#define _DIE_h

#include "Arduino.h"
#include "BluetoothMessage.h"
#include "AccelController.h"
#include "AnimController.h"
#include "Settings.h"
#include "IStateEstimator.h"
#include "AnimationSet.h"
#include "DelegateArray.h"

#define UPDATE_MAX_COUNT 8

class Animation;

/// <summary>
/// This defines our global die object!
/// It coordinates all the systems and devices we need to work with
/// </summary>
class Die
{
private:
	int currentFace;
	StateEstimate currentState;
	IStateEstimator* stateEstimators[DieState_Count];

	// Our bluetooth message handlers
	typedef void (*DieMessageHandler)(void* token, DieMessage* message);

	struct HandlerAndToken
	{
		DieMessageHandler handler;
		void* token;
	};
	HandlerAndToken messageHandlers[DieMessage::MessageType_Count];

	// Update message handlers
	typedef void(*DieUpdateHandler)(void* token);
	DelegateArray<DieUpdateHandler, UPDATE_MAX_COUNT> updateHandlers;

	SendAnimSetSM sendAnimSetSM;
	ReceiveAnimSetSM receiveAnimSetSM;
	SendSettingsSM sendSettingsSM;
	ReceiveSettingsSM receiveSettingsSM;

public:
	Die();
	void init();
	void onConnect();
	void onDisconnect();
	void onReceive(char *data, int len);
	bool SendMessage(DieMessage::MessageType msgType);
	bool SendMessage(const DieMessage* msg, int msgSize);
	void update();

	void RegisterMessageHandler(DieMessage::MessageType msgType, void* token, DieMessageHandler handler);
	void UnregisterMessageHandler(DieMessage::MessageType msgType);

	void RegisterUpdate(void* token, DieUpdateHandler handler);
	void UnregisterUpdateHandler(DieUpdateHandler handler);
	void UnregisterUpdateToken(void* token);

	void playAnimation(int animIndex);

private:
	void updateFaceAnimation();
	void PauseModules();
	void ResumeModules();

#if defined(_CONSOLE)
	void processConsole();
#endif

	// Message handlers
	void OnPlayAnim(DieMessage* msg);
	void OnRequestState(DieMessage* msg);
	void OnRequestAnimSet(DieMessage* msg);
	void OnUpdateAnimSet(DieMessage* msg);
	void OnRequestSettings(DieMessage* msg);
	void OnUpdateSettings(DieMessage* msg);
	void OnRequestTelemetry(DieMessage* msg);
	void OnProgramDefaultAnimSet(DieMessage* msg);
	void OnRequestDefaultAnimSetColor(DieMessage* msg);
	void OnRenameDie(DieMessage* msg);
	void OnFlash(DieMessage* msg);

	void RenameDie(const char* newName);
};

// The global die!
extern Die die;

#endif

