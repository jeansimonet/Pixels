#include "animation_set.h"
#include "utils/utils.h"
#include "drivers_nrf/flash.h"
#include "drivers_nrf/scheduler.h"
#include "bluetooth/bluetooth_messages.h"
#include "bluetooth/bluetooth_message_service.h"
#include "bluetooth/bulk_data_transfer.h"
#include "malloc.h"
#include "assert.h"
#include "nrf_log.h"

#define ANIMATION_SET_VALID_KEY (0x600DF00D) // Good Food ;)
// We place animation set and animations in descending addresses
// So the animation set is at the top of the page
#define PALETTE_SIZE (COLOR_MAP_SIZE * 3)

#define ANIMATION_SET_ADDRESS (0x28000)

using namespace Utils;
using namespace DriversNRF;
using namespace Bluetooth;

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
		const uint8_t* palette;
		// Size is constant: PALETTE_SIZE

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

	#define ANIMATION_SET_DATA_ADDRESS (ANIMATION_SET_ADDRESS + sizeof(Data))

	// The animation set always points at a specific address in memory
	const Data* data = (const Data*)ANIMATION_SET_DATA_ADDRESS;

	void init()
	{
		MessageService::RegisterMessageHandler(Message::MessageType_TransferAnimSet, nullptr, ReceiveAnimationSetHandler);
	}

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
	int ComputeAnimationDataSize() {
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

	uint16_t getKeyframeCount() {
		assert(CheckValid());
		return data->keyFrameCount;
	}

	AnimationTrack getTrack(uint16_t trackIndex) {
		assert(CheckValid() && trackIndex < data->trackCount);
		return data->tracks[trackIndex];
	}

	AnimationTrack const * const getTracks(uint16_t tracksStartIndex) {
		assert(CheckValid() && tracksStartIndex < data->trackCount);
		return &(data->tracks[tracksStartIndex]);
	}

	uint16_t getTrackCount() {
		assert(CheckValid());
		return data->trackCount;
	}

	Animation getAnimation(uint16_t animIndex) {
		assert(CheckValid() && animIndex < data->animationCount);
		return data->animations[animIndex];
	}


	uint16_t getAnimationCount() {
		assert(CheckValid());
		return data->animationCount;
	}

	uint32_t getFlashByteSize(uint32_t totalAnimByteSize) {
		auto pageSize = Flash::getPageSize();
		return pageSize * ((totalAnimByteSize + pageSize - 1) / pageSize);
	}

	struct ProgrammingToken
	{
		uint32_t dataAddress;
		uint32_t buffersSize;
		uint32_t keyFrameCount;
		uint32_t trackCount;
		uint32_t animationCount;
	} programmingToken;


	void ReceiveAnimationSetHandler(void* context, const Message* msg) {

		NRF_LOG_INFO("Received Request to download new animation set");
		const MessageTransferAnimSet* message = (const MessageTransferAnimSet*)msg;

		uint32_t buffersSize =
			message->keyFrameCount * sizeof(RGBKeyframe) +
			message->trackCount * sizeof(AnimationTrack) +
			message->animationCount * sizeof(Animation) +
			PALETTE_SIZE;

		NRF_LOG_DEBUG("Animation Data to be received:");
		NRF_LOG_DEBUG("keyframes: %d * %d", message->keyFrameCount, sizeof(RGBKeyframe));
		NRF_LOG_DEBUG("Tracks: %d * %d", message->trackCount, sizeof(AnimationTrack));
		NRF_LOG_DEBUG("animations: %d * %d", message->animationCount, sizeof(Animation));
		NRF_LOG_DEBUG("palette: %d", PALETTE_SIZE);

		uint32_t totalSize = buffersSize + sizeof(Data);
		uint32_t flashSize = getFlashByteSize(totalSize);
		uint32_t pageAddress = ANIMATION_SET_ADDRESS;
		uint32_t dataAddress = ANIMATION_SET_DATA_ADDRESS;
		uint32_t pageCount = Flash::bytesToPages(flashSize);

		NRF_LOG_DEBUG("totalSize: 0x%04x", totalSize);
		NRF_LOG_DEBUG("flashSize: 0x%04x", flashSize);
		NRF_LOG_DEBUG("pageAddress: 0x%08x", pageAddress);
		NRF_LOG_DEBUG("dataAddress: 0x%08x", dataAddress);
		NRF_LOG_DEBUG("pageCount: %d", pageCount);

		// Store the address and size
		programmingToken.dataAddress = dataAddress;
		programmingToken.buffersSize = buffersSize;
		programmingToken.keyFrameCount = message->keyFrameCount;
		programmingToken.trackCount = message->trackCount;
		programmingToken.animationCount = message->animationCount;

		// Start by erasing the flash
		Flash::erase(pageAddress, pageCount,
			[](bool result, uint32_t address, uint16_t size) {
				NRF_LOG_DEBUG("done Erasing %d page", size);

				// Send Ack and receive data
				MessageService::SendMessage(Message::MessageType_TransferAnimSetAck);

				// Receive all the buffers directly to flash
				ReceiveBulkData::receiveToFlash(programmingToken.dataAddress, nullptr,
					[](void* context, bool success, uint16_t size) {
						NRF_LOG_DEBUG("Finished flashing animation data, flashing animation set itself");
						NRF_LOG_DEBUG("Buffer Size: 0x%04x, Programmed Size: 0x%04x", programmingToken.buffersSize, size);
						if (success) {
							// Program the animation set itself
							NRF_LOG_DEBUG("Setting up pointers");
							Data data;
							data.headMarker = ANIMATION_SET_VALID_KEY;
							data.palette = (const uint8_t*)programmingToken.dataAddress;
							data.keyframes = (const RGBKeyframe*)(programmingToken.dataAddress + PALETTE_SIZE);
							data.keyFrameCount = programmingToken.keyFrameCount;
							data.tracks = (const AnimationTrack*)(programmingToken.dataAddress + PALETTE_SIZE
																+ programmingToken.keyFrameCount * sizeof(RGBKeyframe));
							data.trackCount = programmingToken.trackCount;
							data.animations = (const Animation*)(programmingToken.dataAddress + PALETTE_SIZE
															+ programmingToken.keyFrameCount * sizeof(RGBKeyframe)
															+ programmingToken.trackCount * sizeof(AnimationTrack));
							data.animationCount = programmingToken.animationCount;
							data.tailMarker = ANIMATION_SET_VALID_KEY;
							NRF_LOG_DEBUG("Writing set");
							Flash::write(ANIMATION_SET_ADDRESS, &data, sizeof(Data),
								[](bool result, uint32_t address, uint16_t size) {
									NRF_LOG_INFO("Animation Set written to flash!");
								}
							);
						} else {
							NRF_LOG_ERROR("Error transfering animation data");
						}
					}
				);
			}
		);
	}

}
}
