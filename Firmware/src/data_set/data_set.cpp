#include "data_set.h"
#include "data_set_data.h"
#include "utils/utils.h"
#include "drivers_nrf/flash.h"
#include "drivers_nrf/scheduler.h"
#include "drivers_nrf/timers.h"
#include "modules/accelerometer.h"
#include "config/board_config.h"
#include "config/settings.h"
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
	uint32_t getDataSetAddress() {
		return SettingsManager::getSettingsEndAddress();
	}

	uint32_t getDataSetDataAddress() {
		return getDataSetAddress() + sizeof(Data);
	}

	// The animation set always points at a specific address in memory
	Data const * data = nullptr;

	void init(DataSetWrittenCallback callback)
	{
		static DataSetWrittenCallback _callback; // Don't initialize this static inline because it would only do it on first call!
		_callback = callback;
		data = (Data const *)SettingsManager::getSettingsEndAddress();

		// This gets called after the animation set has been initialized
		auto finishInit = [] (bool result) {
				MessageService::RegisterMessageHandler(Message::MessageType_TransferAnimSet, nullptr, ReceiveDataSetHandler);
				NRF_LOG_INFO("Animation Set initialized");
				auto callBackCopy = _callback;
				_callback = nullptr;
				if (callBackCopy != nullptr) {
					callBackCopy(result);
				}
			};

		//ProgramDefaultDataSet();
		if (!CheckValid()) {
			NRF_LOG_INFO("Animation Set not valid, programming default");
			ProgramDefaultDataSet(finishInit);
		} else {
			finishInit(true);
		}
		//printAnimationInfo();
	}

	/// <summary>
	/// Checks whether the animation set in flash is valid or garbage data
	/// </summary>
	bool CheckValid()
	{
		return data->headMarker == ANIMATION_SET_VALID_KEY &&
			data->version == ANIMATION_SET_VERSION &&
			data->tailMarker == ANIMATION_SET_VALID_KEY;
	}

	uint32_t getPaletteColor(uint16_t colorIndex) {
		if (colorIndex < (data->paletteSize / 3)) {
			return toColor(
					data->palette[colorIndex * 3 + 0],
					data->palette[colorIndex * 3 + 1],
					data->palette[colorIndex * 3 + 2]);
		} else {
			return 0xFFFFFFFF;
		}
	}

	uint16_t getPaletteSize() {
		assert(CheckValid());
		return data->paletteSize;
	}

	const RGBKeyframe& getKeyframe(uint16_t keyFrameIndex) {
		assert(CheckValid() && keyFrameIndex < data->keyFrameCount);
		return data->keyframes[keyFrameIndex];
	}

	uint16_t getKeyframeCount() {
		assert(CheckValid());
		return data->keyFrameCount;
	}

	const RGBTrack& getRGBTrack(uint16_t trackIndex) {
		assert(CheckValid() && trackIndex < data->trackCount);
		return data->rgbTracks[trackIndex];
	}

	uint16_t getRGBTrackCount() {
		assert(CheckValid());
		return data->rgbTrackCount;
	}

	const RGBTrack& getHeatTrack() {
		return data->rgbTracks[data->heatTrackIndex];
	}

	const LEDTrack& getLEDTrack(uint16_t trackIndex) {
		assert(CheckValid() && trackIndex < data->trackCount);
		return data->tracks[trackIndex];
	}

	LEDTrack const * const getLEDTracks(uint16_t tracksStartIndex) {
		assert(CheckValid() && tracksStartIndex < data->trackCount);
		return &(data->tracks[tracksStartIndex]);
	}

	uint16_t getLEDTrackCount() {
		assert(CheckValid());
		return data->trackCount;
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

	int getCurrentBehaviorIndex() {
		assert(CheckValid());
		return data->currentBehaviorIndex;
	}

	// Behaviors
	const Behavior* getBehavior(int behaviorIndex) {
		assert(CheckValid());
		return &data->behaviors[behaviorIndex];
	}

	uint16_t getBehaviorCount() {
		assert(CheckValid());
		return data->behaviorsCount;
	}


	struct DataAndBufferSize
	{
		Data newData;
		uint32_t bufferSize;
	};
	DataAndBufferSize* dabs;

	void ReceiveDataSetHandler(void* context, const Message* msg) {

		NRF_LOG_INFO("Received Request to download new animation set");
		const MessageTransferAnimSet* message = (const MessageTransferAnimSet*)msg;

		Accelerometer::stop();

		dabs = (DataAndBufferSize*)malloc(sizeof(DataAndBufferSize));

		dabs->bufferSize =
			message->paletteSize * sizeof(uint8_t) +
			message->keyFrameCount * sizeof(RGBKeyframe) +
			message->rgbTrackCount * sizeof(RGBTrack) +
			message->trackCount * sizeof(LEDTrack) +
			Utils::roundUpTo4(sizeof(uint16_t) * message->animationCount) + // round up to multiple of 4
			message->animationSize +
			Utils::roundUpTo4(sizeof(uint16_t) * message->conditionCount) + // round up to multiple of 4
			message->conditionSize +
			Utils::roundUpTo4(sizeof(uint16_t) * message->actionCount) + // round up to multiple of 4
			message->actionSize +
			message->ruleCount * sizeof(Rule) +
			message->behaviorCount * sizeof(Behavior);

		NRF_LOG_DEBUG("Animation Data to be received:");
		NRF_LOG_DEBUG("Palette: %d * %d", message->paletteSize, sizeof(uint8_t));
		NRF_LOG_DEBUG("Keyframes: %d * %d", message->keyFrameCount, sizeof(RGBKeyframe));
		NRF_LOG_DEBUG("RGB Tracks: %d * %d", message->rgbTrackCount, sizeof(RGBTrack));
		NRF_LOG_DEBUG("Animation Tracks: %d * %d", message->trackCount, sizeof(LEDTrack));
		NRF_LOG_DEBUG("Animation Offsets: %d * %d", message->animationCount, sizeof(uint16_t));
		NRF_LOG_DEBUG("Animations: %d", message->animationSize);
		NRF_LOG_DEBUG("Conditions Offsets: %d * %d", message->conditionCount, sizeof(uint16_t));
		NRF_LOG_DEBUG("Conditions: %d", message->conditionSize);
		NRF_LOG_DEBUG("Actions Offsets: %d * %d", message->actionCount, sizeof(uint16_t));
		NRF_LOG_DEBUG("Actions: %d", message->actionSize);
		NRF_LOG_DEBUG("Rules: %d * %d", message->ruleCount, sizeof(Rule));
		NRF_LOG_DEBUG("Behaviors: %d * %d", message->behaviorCount, sizeof(Behavior));

		uint32_t totalSize = dabs->bufferSize + sizeof(Data);
		uint32_t flashSize = Flash::getFlashByteSize(totalSize);
		uint32_t pageAddress = getDataSetAddress();
		uint32_t dataAddress = getDataSetDataAddress();
		uint32_t pageCount = Flash::bytesToPages(flashSize);

		NRF_LOG_DEBUG("totalSize: 0x%04x", totalSize);
		NRF_LOG_DEBUG("flashSize: 0x%04x", flashSize);
		NRF_LOG_DEBUG("pageAddress: 0x%08x", pageAddress);
		NRF_LOG_DEBUG("dataAddress: 0x%08x", dataAddress);
		NRF_LOG_DEBUG("pageCount: %d", pageCount);

		// Store the address and size
		NRF_LOG_DEBUG("Setting up pointers");
		dabs->newData.headMarker = ANIMATION_SET_VALID_KEY;
		dabs->newData.version = ANIMATION_SET_VERSION;
		uint32_t address = dataAddress;
		dabs->newData.palette = (const uint8_t*)address;
		dabs->newData.paletteSize = message->paletteSize;
		address += message->paletteSize * sizeof(uint8_t);

		dabs->newData.keyframes = (const RGBKeyframe*)address;
		dabs->newData.keyFrameCount = message->keyFrameCount;
		address += message->keyFrameCount * sizeof(RGBKeyframe);

		dabs->newData.rgbTracks = (const RGBTrack*)address;
		dabs->newData.rgbTrackCount = message->rgbTrackCount;
		address += message->rgbTrackCount * sizeof(RGBTrack);

		dabs->newData.tracks = (const LEDTrack*)address;
		dabs->newData.trackCount = message->trackCount;
		address += message->trackCount * sizeof(LEDTrack);

		dabs->newData.animationOffsets = (const uint16_t*)address;
		dabs->newData.animationCount = message->animationCount;
		address += Utils::roundUpTo4(message->animationCount * sizeof(uint16_t)); // round to multiple of 4
		dabs->newData.animations = (const Animation*)address;
		address += message->animationSize;

		dabs->newData.conditionsOffsets = (const uint16_t*)address;
		dabs->newData.conditionCount = message->conditionCount;
		address += Utils::roundUpTo4(message->conditionCount * sizeof(uint16_t)); // round to multiple of 4
		dabs->newData.conditions = (const Condition*)address;
		address += message->conditionSize;

		dabs->newData.actionsOffsets = (const uint16_t*)address;
		dabs->newData.actionCount = message->actionCount;
		address += Utils::roundUpTo4(message->actionCount * sizeof(uint16_t)); // round to multiple of 4
		dabs->newData.actions = (const Action*)address;
		address += message->actionSize;

		dabs->newData.ruleCount = message->ruleCount;
		address += message->ruleCount * sizeof(Rule);

		dabs->newData.behaviorsCount= message->behaviorCount;
		address += message->behaviorCount * sizeof(Behavior);
		dabs->newData.currentBehaviorIndex = message->currentBehaviorIndex;

		dabs->newData.heatTrackIndex = message->heatTrackIndex;

		dabs->newData.tailMarker = ANIMATION_SET_VALID_KEY;

		// Start by erasing the flash
		Flash::erase(pageAddress, pageCount,
			[](bool result, uint32_t address, uint16_t size) {
				NRF_LOG_DEBUG("done Erasing %d page", size);

				// Send Ack and receive data
				MessageService::SendMessage(Message::MessageType_TransferAnimSetAck);

				// Receive all the buffers directly to flash
				ReceiveBulkData::receiveToFlash(getDataSetDataAddress(), nullptr,
					[](void* context, bool success, uint16_t size) {
						NRF_LOG_DEBUG("Finished flashing animation data, flashing animation set itself");
						NRF_LOG_DEBUG("Buffer Size: 0x%04x, Programmed Size: 0x%04x", dabs->bufferSize, size);
						if (success) {
							// Program the animation set itself
							NRF_LOG_DEBUG("Writing set");

							Flash::write(getDataSetAddress(), &dabs->newData, sizeof(Data),
								[](bool result, uint32_t address, uint16_t size) {
									NRF_LOG_INFO("Data Set written to flash!");
									free(dabs);

									if (!CheckValid()) {
										NRF_LOG_ERROR("Dateset is not valid, reprogramming defaults!");
										ProgramDefaultDataSet(nullptr);
									}
									Accelerometer::start();
								}
							);
						} else {
							NRF_LOG_ERROR("Error transfering animation data");
							Accelerometer::start();
						}
					}
				);
			}
		);
	}

	void printAnimationInfo() {
		Timers::pause();
		NRF_LOG_INFO("Palette size: %d bytes", getPaletteSize());
		for (int p = 0; p < getPaletteSize() / 3; ++p) {
			NRF_LOG_INFO("  Color %d: %08x", p, getPaletteColor(p));
		}
		NRF_LOG_INFO("Animation Count: %d", getAnimationCount());
		for (int a = 0; a < getAnimationCount(); ++a) {
			NRF_LOG_INFO("Anim %d, offset %d", a, data->animationOffsets[a]);
			// NRF_LOG_INFO("  Track count: %d", anim.trackCount);
			// for (int t = 0; t < anim.trackCount; ++t) {
			// 	auto& track = anim.GetTrack(t);
			// 	auto& rgbTrack = track.getLEDTrack();
			// 	NRF_LOG_INFO("  Track %d:", t);
			// 	NRF_LOG_INFO("  Face %d:", track.ledIndex);
			// 	NRF_LOG_INFO("  Track Offset %d:", anim.tracksOffset + t);
			// 	NRF_LOG_INFO("  RGBTrack Offset %d:", track.trackOffset);
			// 	NRF_LOG_INFO("  Keyframe count: %d", rgbTrack.keyFrameCount);
			// 	// for (int k = 0; k < rgbTrack.keyFrameCount; ++k) {
			// 	// 	auto& keyframe = rgbTrack.getKeyframe(k);
			// 	// 	int time = keyframe.time();
			// 	// 	uint32_t color = keyframe.color(nullptr);
			// 	// 	NRF_LOG_INFO("    Offset %d: %d -> %06x", (rgbTrack.keyframesOffset + k), time, color);
			// 	// }
			// }
		}
		Timers::resume();
	}
}
