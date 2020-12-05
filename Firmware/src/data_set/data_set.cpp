#include "data_set.h"
#include "data_set_data.h"
#include "utils/utils.h"
#include "drivers_nrf/flash.h"
#include "drivers_nrf/scheduler.h"
#include "drivers_nrf/timers.h"
#include "drivers_nrf/watchdog.h"
#include "drivers_nrf/power_manager.h"
#include "modules/accelerometer.h"
#include "config/board_config.h"
#include "config/settings.h"
#include "data_animation_bits.h"
#include "bluetooth/bluetooth_messages.h"
#include "bluetooth/bluetooth_message_service.h"
#include "bluetooth/bulk_data_transfer.h"
#include "malloc.h"
#include "assert.h"
#include "nrf_log.h"
#include "nrf_delay.h"


using namespace Utils;
using namespace DriversNRF;
using namespace Bluetooth;
using namespace Config;
using namespace Modules;
using namespace Animations;
using namespace Behaviors;


namespace DataSet
{
	uint32_t computeDataSetSize();
	uint32_t computeDataSetHash();

	// The animation set always points at a specific address in memory
	Data const * data = nullptr;

	// A simple hash value of the dataset data
	uint32_t size = 0;
	uint32_t hash = 0;

	uint32_t availableDataSize() {
		return Flash::getFlashEndAddress() - Flash::getDataSetDataAddress();
	}

	uint32_t dataSize() {
		return size;
	}

	uint32_t dataHash() {
		return hash;
	}

	void init(DataSetWrittenCallback callback) {

		static DataSetWrittenCallback _callback; // Don't initialize this static inline because it would only do it on first call!
		_callback = callback;
		data = (Data const *)Flash::getDataSetAddress();

		// This gets called after the animation set has been initialized
		auto finishInit = [] (bool result) {

				size = computeDataSetSize();
				hash = computeDataSetHash();

				PowerManager::clearClearSettingsAndDataSet();

				MessageService::RegisterMessageHandler(Message::MessageType_TransferAnimSet, nullptr, ReceiveDataSetHandler);
				NRF_LOG_INFO("DataSet initialized, size=0x%x, hash=0x%08x", size, hash);
				auto callBackCopy = _callback;
				_callback = nullptr;
				if (callBackCopy != nullptr) {
					callBackCopy(result);
				}
			};

		//ProgramDefaultDataSet();
		if (PowerManager::getClearSettingsAndDataSet()) {
			NRF_LOG_INFO("Watchdog indicates dataset might be bad, programming default");
			ProgramDefaultDataSet(*SettingsManager::getSettings(), finishInit);
		} else if (!CheckValid()) {
			NRF_LOG_INFO("Animation Set not valid, programming default");
			ProgramDefaultDataSet(*SettingsManager::getSettings(), finishInit);
		} else {
			finishInit(true);
		}
		//printAnimationInfo();
	}

	/// <summary>
	/// Checks whether the animation set in flash is valid or garbage data
	/// </summary>
	bool CheckValid() {
		return data->headMarker == ANIMATION_SET_VALID_KEY &&
			data->version == ANIMATION_SET_VERSION &&
			data->tailMarker == ANIMATION_SET_VALID_KEY;
	}

	const AnimationBits* getAnimationBits() {
		return &(data->animationBits);
	}

	const Animation* getAnimation(int animationIndex) {
		// Grab the preset data
		uint32_t animationAddress = (uint32_t)(const void*)data->animations + data->animationOffsets[animationIndex];
		return (const Animation *)animationAddress;
	}

	uint16_t getAnimationCount() {
		assert(CheckValid());
		return data->animationCount;
	}

	const Condition* getCondition(int conditionIndex) {
		assert(CheckValid());
		uint32_t conditionAddress = (uint32_t)(const void*)data->conditions + data->conditionsOffsets[conditionIndex];
		return (const Condition*)conditionAddress;
	}

	uint16_t getConditionCount() {
		assert(CheckValid());
		return data->conditionCount;
	}

	const Action* getAction(int actionIndex) {
		assert(CheckValid());
		uint32_t actionAddress = (uint32_t)(const void*)data->actions + data->actionsOffsets[actionIndex];
		return (const Action*)actionAddress;
	}

	uint16_t getActionCount() {
		assert(CheckValid());
		return data->actionCount;
	}

	const Rule* getRule(int ruleIndex) {
		assert(CheckValid());
		return &data->rules[ruleIndex];
	}

	uint16_t getRuleCount() {
		assert(CheckValid());
		return data->ruleCount;
	}

	// Behaviors
	const Behavior* getBehavior() {
		assert(CheckValid());
		return data->behavior;
	}

	void ReceiveDataSetHandler(void* context, const Message* msg) {
		NRF_LOG_INFO("Received Request to download new animation set");
		const MessageTransferAnimSet* message = (const MessageTransferAnimSet*)msg;

		NRF_LOG_DEBUG("Animation Data to be received:");
		NRF_LOG_DEBUG("Palette: %d * %d", message->paletteSize, sizeof(uint8_t));
		NRF_LOG_DEBUG("RGB Keyframes: %d * %d", message->rgbKeyFrameCount, sizeof(RGBKeyframe));
		NRF_LOG_DEBUG("RGB Tracks: %d * %d", message->rgbTrackCount, sizeof(RGBTrack));
		NRF_LOG_DEBUG("Keyframes: %d * %d", message->keyFrameCount, sizeof(Keyframe));
		NRF_LOG_DEBUG("Tracks: %d * %d", message->trackCount, sizeof(Track));
		NRF_LOG_DEBUG("Animation Offsets: %d * %d", message->animationCount, sizeof(uint16_t));
		NRF_LOG_DEBUG("Animations: %d", message->animationSize);
		NRF_LOG_DEBUG("Conditions Offsets: %d * %d", message->conditionCount, sizeof(uint16_t));
		NRF_LOG_DEBUG("Conditions: %d", message->conditionSize);
		NRF_LOG_DEBUG("Actions Offsets: %d * %d", message->actionCount, sizeof(uint16_t));
		NRF_LOG_DEBUG("Actions: %d", message->actionSize);
		NRF_LOG_DEBUG("Rules: %d * %d", message->ruleCount, sizeof(Rule));
		NRF_LOG_DEBUG("Behavior: %d", sizeof(Behavior));

		// Store the address and size
		NRF_LOG_DEBUG("Setting up pointers");
		Data newData  __attribute__ ((aligned (4)));
		newData.headMarker = ANIMATION_SET_VALID_KEY;
		newData.version = ANIMATION_SET_VERSION;

		uint32_t address = Flash::getDataSetDataAddress();
		newData.animationBits.palette = (const uint8_t*)address;
		newData.animationBits.paletteSize = message->paletteSize;
		address += Utils::roundUpTo4(message->paletteSize * sizeof(uint8_t));

		newData.animationBits.rgbKeyframes = (const RGBKeyframe*)address;
		newData.animationBits.rgbKeyFrameCount = message->rgbKeyFrameCount;
		address += message->rgbKeyFrameCount * sizeof(RGBKeyframe);

		newData.animationBits.rgbTracks = (const RGBTrack*)address;
		newData.animationBits.rgbTrackCount = message->rgbTrackCount;
		address += message->rgbTrackCount * sizeof(RGBTrack);

		newData.animationBits.keyframes = (const Keyframe*)address;
		newData.animationBits.keyFrameCount = message->keyFrameCount;
		address += message->keyFrameCount * sizeof(Keyframe);

		newData.animationBits.tracks = (const Track*)address;
		newData.animationBits.trackCount = message->trackCount;
		address += message->trackCount * sizeof(Track);

		newData.animationOffsets = (const uint16_t*)address;
		newData.animationCount = message->animationCount;
		address += Utils::roundUpTo4(message->animationCount * sizeof(uint16_t)); // round to multiple of 4
		newData.animations = (const Animation*)address;
		newData.animationsSize = message->animationSize;
		address += message->animationSize;

		newData.conditionsOffsets = (const uint16_t*)address;
		newData.conditionCount = message->conditionCount;
		address += Utils::roundUpTo4(message->conditionCount * sizeof(uint16_t)); // round to multiple of 4
		newData.conditions = (const Condition*)address;
		newData.conditionsSize = message->conditionSize;
		address += message->conditionSize;

		newData.actionsOffsets = (const uint16_t*)address;
		newData.actionCount = message->actionCount;
		address += Utils::roundUpTo4(message->actionCount * sizeof(uint16_t)); // round to multiple of 4
		newData.actions = (const Action*)address;
		newData.actionsSize = message->actionSize;
		address += message->actionSize;

		newData.rules = (const Rule*)address;
		newData.ruleCount = message->ruleCount;
		address += message->ruleCount * sizeof(Rule);

		newData.behavior = (const Behavior*)address;
		address += sizeof(Behavior);

		newData.tailMarker = ANIMATION_SET_VALID_KEY;

		static auto receiveToFlash = [](Flash::ProgramFlashFuncCallback callback) {
			MessageTransferAnimSetAck ack;
			ack.result = 1;
			MessageService::SendMessage(&ack);

			// Transfer data
			Bluetooth::ReceiveBulkData::receiveToFlash(Flash::getDataSetDataAddress(), nullptr, callback);
		};

		static auto onProgramFinished = [](bool result) {
			size = computeDataSetSize();
			hash = computeDataSetHash();
			//printAnimationInfo();
			NRF_LOG_INFO("Dataset size=0x%x, hash=0x%08x", size, hash);
			//NRF_LOG_INFO("Data addr: 0x%08x, data: 0x%08x", Flash::getDataSetAddress(), Flash::getDataSetDataAddress());
			MessageService::SendMessage(Message::MessageType_TransferAnimSetFinished);
		};

		if (!Flash::programFlash(newData, *SettingsManager::getSettings(), receiveToFlash, onProgramFinished)) {
			// Don't send data please
			MessageTransferAnimSetAck ack;
			ack.result = 0;
			MessageService::SendMessage(&ack);
		}
	}

	uint32_t computeDataSetDataSize(const Data* newData) {
		return
			newData->animationBits.paletteSize * sizeof(uint8_t) +
			newData->animationBits.rgbKeyFrameCount * sizeof(RGBKeyframe) +
			newData->animationBits.rgbTrackCount * sizeof(RGBTrack) +
			newData->animationBits.keyFrameCount * sizeof(Keyframe) +
			newData->animationBits.trackCount * sizeof(Track) +
			Utils::roundUpTo4(sizeof(uint16_t) * newData->animationCount) + // round up to multiple of 4
			newData->animationsSize +
			Utils::roundUpTo4(sizeof(uint16_t) * newData->conditionCount) + // round up to multiple of 4
			newData->conditionsSize +
			Utils::roundUpTo4(sizeof(uint16_t) * newData->actionCount) + // round up to multiple of 4
			newData->actionsSize +
			newData->ruleCount * sizeof(Rule) +
			sizeof(Behavior);
	}


	void printAnimationInfo() {
		Timers::pause();
		NRF_LOG_INFO("Palette: %d * %d", data->animationBits.paletteSize, sizeof(uint8_t));
		NRF_LOG_INFO("RGB Keyframes: %d * %d", data->animationBits.rgbKeyFrameCount, sizeof(RGBKeyframe));
		NRF_LOG_INFO("RGB Tracks: %d * %d", data->animationBits.rgbTrackCount, sizeof(RGBTrack));
		NRF_LOG_INFO("Keyframes: %d * %d", data->animationBits.keyFrameCount, sizeof(Keyframe));
		NRF_LOG_INFO("Tracks: %d * %d", data->animationBits.trackCount, sizeof(Track));
		NRF_LOG_INFO("Animation Offsets: %d * %d", data->animationCount, sizeof(uint16_t));
		NRF_LOG_INFO("Animations: %d", data->animationsSize);
		NRF_LOG_INFO("Conditions Offsets: %d * %d", data->conditionCount, sizeof(uint16_t));
		NRF_LOG_INFO("Conditions: %d", data->conditionsSize);
		NRF_LOG_INFO("Actions Offsets: %d * %d", data->actionCount, sizeof(uint16_t));
		NRF_LOG_INFO("Actions: %d", data->actionsSize);
		NRF_LOG_INFO("Rules: %d * %d", data->ruleCount, sizeof(Rule));
		NRF_LOG_INFO("Behaviors: %d", sizeof(Behavior));
		Timers::resume();
	}

	uint32_t computeDataSetSize() {
		// Compute the size of the needed buffer to store all that data!
		return computeDataSetDataSize(data);
	}

	uint32_t computeDataSetHash() {
		return Utils::computeHash((const uint8_t*)Flash::getDataSetDataAddress(), size);
	}

}
