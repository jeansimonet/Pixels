#include <SimbleeBLE.h>

#include "Die.h"
#include "Timer.h"
#include "I2C.h"
#include "Debug.h"
#include "Lazarus.h"

#include "Accelerometer.h"
#include "LEDs.h"

#include "BluetoothMessage.h"
#include "AccelController.h"
#include "Settings.h"
#include "AnimController.h"
#include "Animation.h"
#include "AnimationSet.h"

#include "Utils.h"
#include "Telemetry.h"
#include "JerkMonitor.h"

#include "EstimatorOnFace.h"
#include "Rainbow.h"

#include "Watchdog.h"
#include "SimpleThrowDetector.h"

using namespace Core;
using namespace Systems;
using namespace Devices;

Die::Die()
{
	currentFace = 0;
	memset(messageHandlers, 0, sizeof(Die::HandlerAndToken) * DieMessage::MessageType_Count);
}

EstimatorOnFace estimatorOnFace;

void Die::init()
{
#if defined(_CONSOLE)
	console.begin();
#endif

	// Setup Watchdog first
	watchdog.init();

	// Initialize random colors
	randomSeed(analogRead(3));
	random(0xFF);
	random(0xFF);

	// For info, print out the highest page number
	debugPrint("Lowest page available: ");
	debugPrintln(LowestFlashPageAvailable);

	// put your setup code here, to run once:
	//setup I2C on the pins of your choice
	debugPrint("LED init...");
	leds.init();
	debugPrintln("ok");

	// Flash all the LEDs once, to make sure they work!
	leds.setAll(0x000F04);
	delay(300);
	leds.clearAll();

	// Then turn an led on the 6th face after major init code
	leds.setLED(5, 0, 0xFFFF00);
	debugPrint("Wire init...");
	wire.begin();
	debugPrintln("ok");
	//leds.init(); // Depends on I2C
	leds.setLED(5, 0, 0x00FF00);

	debugPrint("Accelerometer init...");
	leds.setLED(5, 1, 0xFFFF00);
	accelerometer.init();
	debugPrintln("ok");
	leds.setLED(5, 1, 0x00FF00);

	leds.setLED(5, 2, 0xFFFF00);
	debugPrint("Checking Settings...");
	if (settings->CheckValid())
		debugPrintln("ok");
	else
	{
		debugPrint("invalid, setting defaults");
		Settings::ProgramDefaults();
	}
	debugPrint("Checking AnimationSet...");
	if (animationSet->CheckValid())
		debugPrintln("ok");
	else
	{
		debugPrint("invalid, setting default anims...");
		byte hue = random(0xFF);
		uint32_t color = Rainbow::Wheel(hue);
		AnimationSet::ProgramDefaultAnimationSet(color);
		if (animationSet->CheckValid())
			debugPrintln("ok");
		else
			debugPrintln("failed");
	}
	leds.setLED(5, 2, 0x00FF00);

	leds.setLED(5, 3, 0xFFFF00);
	debugPrint("BLE init...");

	// Hook default message handlers
	RegisterMessageHandler(DieMessage::MessageType_PlayAnim, this, [](void* tok, DieMessage* msg) {((Die*)tok)->OnPlayAnim(msg); });
	RegisterMessageHandler(DieMessage::MessageType_RequestState, this, [](void* tok, DieMessage* msg) {((Die*)tok)->OnRequestState(msg); });
	RegisterMessageHandler(DieMessage::MessageType_RequestAnimSet, this, [](void* tok, DieMessage* msg) {((Die*)tok)->OnRequestAnimSet(msg); });
	RegisterMessageHandler(DieMessage::MessageType_TransferAnimSet, this, [](void* tok, DieMessage* msg) {((Die*)tok)->OnUpdateAnimSet(msg); });
	RegisterMessageHandler(DieMessage::MessageType_RequestSettings, this, [](void* tok, DieMessage* msg) {((Die*)tok)->OnRequestSettings(msg); });
	RegisterMessageHandler(DieMessage::MessageType_TransferSettings, this, [](void* tok, DieMessage* msg) {((Die*)tok)->OnUpdateSettings(msg); });
	RegisterMessageHandler(DieMessage::MessageType_RequestTelemetry, this, [](void* tok, DieMessage* msg) {((Die*)tok)->OnRequestTelemetry(msg); });
	RegisterMessageHandler(DieMessage::MessateType_ProgramDefaultAnimSet, this, [](void* tok, DieMessage* msg) {((Die*)tok)->OnProgramDefaultAnimSet(msg); });
	RegisterMessageHandler(DieMessage::MessageType_Rename, this, [](void* tok, DieMessage* msg) {((Die*)tok)->OnRenameDie(msg); });
	RegisterMessageHandler(DieMessage::MessageType_Flash, this, [](void* tok, DieMessage* msg) {((Die*)tok)->OnFlash(msg); });
	RegisterMessageHandler(DieMessage::MessageType_RequestDefaultAnimSetColor, this, [](void* tok, DieMessage* msg) {((Die*)tok)->OnRequestDefaultAnimSetColor(msg); });

	// start the BLE stack
	SimbleeBLE.end();
	const char* name = settings->CheckValid() ? settings->name : "ElectronicDie";
	SimbleeBLE.advertisementData = name;
	SimbleeBLE.deviceName = name;
	SimbleeBLE.txPowerLevel = 4;
	SimbleeBLE.begin();
	debugPrintln("ok");
	leds.setLED(5, 3, 0x00FF00);

	leds.setLED(5, 4, 0xFFFF00);
	debugPrint("Modules init...");
	animController.begin(); // Talks to controller
	accelController.begin();
	lazarus.init();
	jerkMonitor.begin();
	simpleThrowDetector.begin();
	debugPrintln("ok");
	leds.setLED(5, 4, 0x00FF00);

	leds.setLED(5, 5, 0xFFFF00);
	//estimatorOnFace.begin();
	//stateEstimators[DieState_OnFace] = &estimatorOnFace;

	leds.setLED(5, 5, 0x00FF00);
	delay(300);
	leds.clearAll();

	debugPrint("Dice ");
	debugPrint(name);
	debugPrintln(" active");

	timer.begin(); // Kicks off the timers!
}

void Die::onConnect()
{
	// Insert code
	debugPrintln("Connected!");

	// Wake up if necessary!
	lazarus.onRadio();
}

void Die::onDisconnect()
{
	// Insert code here
	debugPrintln("Disconnected!");
}

void Die::onReceive(char *data, int len)
{
	// Wake up if necessary
	lazarus.onRadio();

	debugPrint("Received ");
	debugPrint(len);
	debugPrint(" bytes: ");
	for (int i = 0; i < len; ++i)
	{
		debugPrint(data[i], HEX);
		debugPrint(" ");
	}
	debugPrintln();

	if (len >= sizeof(DieMessage))
	{
		auto msg = reinterpret_cast<DieMessage*>(data);
		auto handler = messageHandlers[(int)msg->type];
#if defined(_CONSOLE)
		debugPrint("Received ");
		debugPrint(DieMessage::GetMessageTypeString(msg->type));
		debugPrint("(");
		debugPrint(msg->type);
		debugPrintln(")");
#endif
		if (handler.handler != nullptr)
		{
			handler.handler(handler.token, msg);
		}
	}
}

bool Die::SendMessage(DieMessage::MessageType msgType)
{
	DieMessage msg(msgType);
	return SimbleeBLE.send(reinterpret_cast<const char*>(&msg), sizeof(msg));
}

bool Die::SendMessage(const DieMessage* msg, int msgSize)
{
	return SimbleeBLE.send(reinterpret_cast<const char*>(msg), msgSize);
}


void Die::update()
{
	if (lazarus.sleeping)
	{
		debugPrintln("Still sleeping but update is on!");
	}

	// Update systems that need it!
#if defined(_CONSOLE)
	processConsole();
#endif

	updateFaceAnimation();

	// Update handlers
	for (int i = 0; i < updateHandlers.Count(); ++i)
	{
		updateHandlers[i].handler(updateHandlers[i].token);
	}

	//// Ask the estimators which state we should be in...
	//for (int i = 0; i < DieState_Count; ++i)
	//{
	//	currentState.estimates[i] = stateEstimators[i]->GetEstimate();
	//}
}

void Die::updateFaceAnimation()
{
	int newFace = accelController.currentFace();
	if (newFace != currentFace)
	{
		// Don't update any internal value unless we can send the data to the central
		// otherwise things get out of sync
		if (!SimbleeBLE_radioActive)
		{
			currentFace = newFace;
			debugPrint("Detected face number ");
			debugPrintln(currentFace);

			// Toggle leds
			//animController.stopAll();
			if (simpleThrowDetector.GetCurrentState() != SimpleThrowDetector::ThrowState_OnFace ||
				currentFace != simpleThrowDetector.GetOnFaceFace())
			{
				playAnimation(currentFace);
			}
			// Send face message
			DieMessageState faceMessage;
			faceMessage.state = currentFace + 1;
			SimbleeBLE.send(reinterpret_cast<const char*>(&faceMessage), sizeof(DieMessageState));
		}
	}
}

void Die::PauseModules()
{
	timer.stop();
	lazarus.stop();
	leds.stop();
}

void Die::ResumeModules()
{
	lazarus.init();
	timer.begin();
	leds.init();
}


#if defined(_CONSOLE)
/// <summary>
/// Processes any input on the serial port, if any
/// </summary>
void Die::processConsole()
{
	if (console.available() > 0)
	{
		char buffer[32];
		int len = console.readBytesUntil('\n', buffer, 32);
		console.processCommand(buffer, len);
	}
}
#endif

void Die::RegisterMessageHandler(DieMessage::MessageType msgType, void* token, DieMessageHandler handler)
{
	if (messageHandlers[msgType].handler != nullptr)
	{
		debugPrint("Handler for message ");
		debugPrint(msgType);
		debugPrintln(" already set");
	}
	else
	{
		messageHandlers[msgType].handler = handler;
		messageHandlers[msgType].token = token;
	}
}

void Die::UnregisterMessageHandler(DieMessage::MessageType msgType)
{
	messageHandlers[msgType].handler = nullptr;
	messageHandlers[msgType].token = nullptr;
}

void Die::RegisterUpdate(void* token, DieUpdateHandler handler)
{
	if (!updateHandlers.Register(token, handler))
	{
		debugPrint("Too many update handlers");
	}
}

void Die::UnregisterUpdateHandler(DieUpdateHandler handler)
{
	updateHandlers.UnregisterWithHandler(handler);
}

void Die::UnregisterUpdateToken(void* token)
{
	updateHandlers.UnregisterWithToken(token);
}

void Die::OnPlayAnim(DieMessage* msg)
{
	auto playAnimMsg = static_cast<DieMessagePlayAnim*>(msg);
	playAnimation(playAnimMsg->animation);
}

void Die::OnRequestState(DieMessage* msg)
{
	// Send face message
	DieMessageState faceMessage;
	faceMessage.state = currentFace;
	SimbleeBLE.send(reinterpret_cast<const char*>(&faceMessage), sizeof(DieMessageState));
}

void Die::OnRequestAnimSet(DieMessage* msg)
{
	PauseModules();

	// This will setup the data transfers and unregister itself when done
	sendAnimSetSM.Setup(this, [](void* tok) {((Die*)tok)->ResumeModules(); });
}

void Die::OnUpdateAnimSet(DieMessage* msg)
{
	PauseModules();

	auto updateAnimSetMsg = (DieMessageTransferAnimSet*)msg;
	// This will setup the data transfers and unregister itself when done
	receiveAnimSetSM.Setup(updateAnimSetMsg->count, updateAnimSetMsg->totalAnimationByteSize,
		this, [](void* tok) {((Die*)tok)->ResumeModules(); });
}

void Die::OnRequestSettings(DieMessage* msg)
{
	PauseModules();

	// This will setup the data transfers and unregister itself when done
	sendSettingsSM.Setup(this, [](void* tok) {((Die*)tok)->ResumeModules(); });
}

void Die::OnUpdateSettings(DieMessage* msg)
{
	PauseModules();

	// This will setup the data transfers and unregister itself when done
	receiveSettingsSM.Setup(this, [](void* tok) {((Die*)tok)->ResumeModules(); });
}

void Die::OnRequestTelemetry(DieMessage* msg)
{
	auto telemMsg = (DieMessageRequestTelemetry*)msg;
	debugPrint("Receive request telemetry message: ");
	debugPrintln(telemMsg->telemetry);

	DieMessageRequestTelemetry sample;
	sample.telemetry = true;

	if (telemMsg->telemetry)
	{
		// Turn telemetry on
		telemetry.begin();
	}
	else
	{
		// Turn telemetry off
		telemetry.stop();
	}
}

void Die::playAnimation(int animIndex)
{
	if (animationSet->CheckValid())
	{
		animController.play(animationSet->GetAnimation(animIndex));
	}
	else
	{
		debugPrintln("Not playing animation because animation set is invalid");
	}
}

void Die::OnProgramDefaultAnimSet(DieMessage* msg)
{
	bool sleep = lazarus.sleeping;
	if (!sleep)
	{
		// Stop everything
		PauseModules();
	}

	auto animSetMsg = (DieMessageProgramDefaultAnimSet*)msg;
	AnimationSet::ProgramDefaultAnimationSet(animSetMsg->color);

	while (!SendMessage(DieMessage::MessateType_ProgramDefaultAnimSetFinished))
		delay(10);

	if (!sleep)
	{
		// Resume 
		ResumeModules();
	}

	// Flash entire die once
	for (int i = 0; i < 6; ++i)
	{
		animController.play(animationSet->GetAnimation(i + 6));
	}
}

void Die::OnRequestDefaultAnimSetColor(DieMessage* msg)
{
	// A bit of a hack, go fetch the color of the first anim's first track
	if (animationSet->CheckValid())
	{
		debugPrintln("Sending back default anim set color");
		auto keyframe = animationSet->GetAnimation(0)->GetTrack(0).keyframes[1];
		DieMessageDefaultAnimSetColor response;
		response.color = Core::toColor(keyframe.red, keyframe.green, keyframe.blue);
		while (!SendMessage(&response, sizeof(DieMessageDefaultAnimSetColor)))
			delay(10);
	}
}

void Die::OnRenameDie(DieMessage* msg)
{
	bool sleep = lazarus.sleeping;
	if (!sleep)
	{
		// Stop everything
		PauseModules();
	}

	auto animSetMsg = (DieMessageRename*)msg;
	debugPrint("Renaming die to ");
	debugPrint(animSetMsg->newName);
	debugPrint("...");

	RenameDie(animSetMsg->newName);

	if (!sleep)
	{
		// Resume 
		ResumeModules();
	}
}

void Die::RenameDie(const char* newName)
{
	Settings settingsToWrite;
	strncpy(settingsToWrite.name, newName, 16);
	if (Settings::EraseSettings())
	{
		if (Settings::TransferSettings(&settingsToWrite))
		{
			debugPrintln("Done");
		}
		else
		{
			debugPrintln("Error writing settings while trying to rename die");
		}
	}
	else
	{
		debugPrintln("Error erasing flash to rename die");
	}

	// Acknowledge the anim
	while (!SendMessage(DieMessage::MessageType_RenameFinished))
		delay(10);

	// Restart BLE with the new name
	SimbleeBLE.end();
	SimbleeBLE.advertisementData = settings->name;
	SimbleeBLE.deviceName = settings->name;
	SimbleeBLE.begin();
}

void Die::OnFlash(DieMessage* msg)
{
	bool sleep = lazarus.sleeping;
	if (!sleep)
	{
		// Stop everything
		PauseModules();
	}

	// Rainbow!
	leds.init();

	auto flashMsg = static_cast<DieMessageFlash*>(msg);
	switch (flashMsg->animIndex)
	{
	case 1:
		Rainbow::rainbowAll(2, 1);
		break;
	default:
		Rainbow::rainbowCycle(5);
		break;
	}

	leds.stop();

	// Acknowledge the anim
	while (!SendMessage(DieMessage::MessageType_FlashFinished))
		delay(10);

	if (!sleep)
	{
		// Resume everything
		ResumeModules();
	}
}



