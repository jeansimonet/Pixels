#include "animation_set.h"
#include "utils/utils.h"
#include "drivers_nrf/flash.h"
#include "drivers_nrf/scheduler.h"
#include "drivers_nrf/timers.h"
#include "config/board_config.h"
#include "config/settings.h"
#include "bluetooth/bluetooth_messages.h"
#include "bluetooth/bluetooth_message_service.h"
#include "bluetooth/bulk_data_transfer.h"
#include "malloc.h"
#include "assert.h"
#include "nrf_log.h"
#include "nrf_delay.h"

#define ANIMATION_SET_VALID_KEY (0x600DF00D) // Good Food ;)
// We place animation set and animations in descending addresses
// So the animation set is at the top of the page
#define MAX_PALETTE_SIZE (COLOR_MAP_SIZE * 3)
#define SPECIAL_COLOR_INDEX (MAX_COLOR_MAP_SIZE - 1)

using namespace Utils;
using namespace DriversNRF;
using namespace Bluetooth;
using namespace Config;

namespace Animations
{
namespace AnimationSet
{
	struct Data
	{
		// Indicates whether there is valid data
		uint32_t headMarker;

		// The palette for all animations, stored in RGB RGB RGB etc...
		// Maximum 128 * 3 = 376 bytes
		const uint8_t* palette;
		uint32_t paletteSize; // In bytes (divide by 3 for colors)
		// Size is constant: PALETTE_SIZE

		// The animations we have
		const RGBKeyframe* keyframes; // pointer to the array of tracks
		uint32_t keyFrameCount;

		// The RGB tracks we have
		const RGBTrack* rgbTracks; // pointer to the array of tracks
		uint32_t rgbTrackCount;

		// The animations we have
		const AnimationTrack* tracks; // pointer to the array of tracks
		uint32_t trackCount;

		// The animations we have
		const Animation* animations;
		uint32_t animationCount;

		// Heat Animation
		uint32_t heatTrackIndex;

		// Indicates whether there is valid data
		uint32_t tailMarker;
	};

	// This is called for special colors
	getColorHandler handler = nullptr;

	void setGetColorHandler(getColorHandler the_handler) {
		handler = the_handler;
	}

	void unsetGetColorHandler(getColorHandler the_handler) {
		if (handler == the_handler) {
			handler = nullptr;
		} else {
			NRF_LOG_ERROR("Trying to unset wrong Color handler");
		}
	}

	uint32_t getAnimationSetAddress() {
		return SettingsManager::getSettingsEndAddress();
	}

	uint32_t getAnimationSetDataAddress() {
		return getAnimationSetAddress() + sizeof(Data);
	}

	// The animation set always points at a specific address in memory
	Data const * data = nullptr;

	void init(AnimationSetWrittenCallback callback)
	{
		static AnimationSetWrittenCallback _callback; // Don't initialize this static inline because it would only do it on first call!
		_callback = callback;
		data = (Data const *)SettingsManager::getSettingsEndAddress();

		// This gets called after the animation set has been initialized
		auto finishInit = [] (bool result) {
				MessageService::RegisterMessageHandler(Message::MessageType_TransferAnimSet, nullptr, ReceiveAnimationSetHandler);
				NRF_LOG_INFO("Animation Set initialized");
				auto callBackCopy = _callback;
				_callback = nullptr;
				if (callBackCopy != nullptr) {
					callBackCopy(result);
				}
			};

		//ProgramDefaultAnimationSet();
		if (!CheckValid()) {
			NRF_LOG_INFO("Animation Set not valid, programming default");
			ProgramDefaultAnimationSet(finishInit);
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
			sizeof(uint8_t) * data->paletteSize * 3 +
			sizeof(RGBKeyframe) * data->keyFrameCount +
			sizeof(RGBTrack) * data->trackCount +
			sizeof(Animation) * data-> animationCount;
	}

	uint32_t getColor(void* token, uint16_t colorIndex) {
		assert(CheckValid());

		if (colorIndex == SPECIAL_COLOR_INDEX && token != nullptr && handler != nullptr) {
			return handler(token, colorIndex);
		} else {
			return getPaletteColor(colorIndex);
		}
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

	const AnimationTrack& getTrack(uint16_t trackIndex) {
		assert(CheckValid() && trackIndex < data->trackCount);
		return data->tracks[trackIndex];
	}

	AnimationTrack const * const getTracks(uint16_t tracksStartIndex) {
		assert(CheckValid() && tracksStartIndex < data->trackCount);
		return &(data->tracks[tracksStartIndex]);
	}

	const Animation& getAnimation(uint16_t animIndex) {
		assert(CheckValid() && animIndex < data->animationCount);
		return data->animations[animIndex];
	}

	uint16_t getPaletteSize() {
		assert(CheckValid());
		return data->paletteSize;
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
		uint32_t paletteSize;
		uint32_t keyFrameCount;
		uint32_t rgbTrackCount;
		uint32_t trackCount;
		uint32_t animationCount;
		uint32_t heatTrackIndex;
	} programmingToken;


	Data* newData = nullptr;

	void ReceiveAnimationSetHandler(void* context, const Message* msg) {

		NRF_LOG_INFO("Received Request to download new animation set");
		const MessageTransferAnimSet* message = (const MessageTransferAnimSet*)msg;

		uint32_t buffersSize =
			message->keyFrameCount * sizeof(RGBKeyframe) +
			message->rgbTrackCount * sizeof(RGBTrack) +
			message->trackCount * sizeof(AnimationTrack) +
			message->animationCount * sizeof(Animation) +
			message->paletteSize * sizeof(uint8_t);

		NRF_LOG_DEBUG("Animation Data to be received:");
		NRF_LOG_DEBUG("Palette: %d * %d", message->paletteSize, sizeof(uint8_t));
		NRF_LOG_DEBUG("Keyframes: %d * %d", message->keyFrameCount, sizeof(RGBKeyframe));
		NRF_LOG_DEBUG("RGB Tracks: %d * %d", message->rgbTrackCount, sizeof(RGBTrack));
		NRF_LOG_DEBUG("Animation Tracks: %d * %d", message->trackCount, sizeof(AnimationTrack));
		NRF_LOG_DEBUG("Animations: %d * %d", message->animationCount, sizeof(Animation));

		uint32_t totalSize = buffersSize + sizeof(Data);
		uint32_t flashSize = getFlashByteSize(totalSize);
		uint32_t pageAddress = getAnimationSetAddress();
		uint32_t dataAddress = getAnimationSetDataAddress();
		uint32_t pageCount = Flash::bytesToPages(flashSize);

		NRF_LOG_DEBUG("totalSize: 0x%04x", totalSize);
		NRF_LOG_DEBUG("flashSize: 0x%04x", flashSize);
		NRF_LOG_DEBUG("pageAddress: 0x%08x", pageAddress);
		NRF_LOG_DEBUG("dataAddress: 0x%08x", dataAddress);
		NRF_LOG_DEBUG("pageCount: %d", pageCount);

		// Store the address and size
		programmingToken.dataAddress = dataAddress;
		programmingToken.buffersSize = buffersSize;
		programmingToken.paletteSize = message->paletteSize;
		programmingToken.keyFrameCount = message->keyFrameCount;
		programmingToken.rgbTrackCount = message->rgbTrackCount;
		programmingToken.trackCount = message->trackCount;
		programmingToken.animationCount = message->animationCount;
		programmingToken.heatTrackIndex = message->heatTrackIndex;

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
							newData = (Data*)malloc(sizeof(Data));
							newData->headMarker = ANIMATION_SET_VALID_KEY;
							uint32_t address = programmingToken.dataAddress;
							newData->palette = (const uint8_t*)address;
							newData->paletteSize = programmingToken.paletteSize;
							address += programmingToken.paletteSize * sizeof(uint8_t);

							newData->keyframes = (const RGBKeyframe*)address;
							newData->keyFrameCount = programmingToken.keyFrameCount;
							address += programmingToken.keyFrameCount * sizeof(RGBKeyframe);

							newData->rgbTracks = (const RGBTrack*)address;
							newData->rgbTrackCount = programmingToken.rgbTrackCount;
							address += programmingToken.rgbTrackCount * sizeof(RGBTrack);

							newData->tracks = (const AnimationTrack*)address;
							newData->trackCount = programmingToken.trackCount;
							address += programmingToken.trackCount * sizeof(AnimationTrack);

							newData->animations = (const Animation*)address;
							newData->animationCount = programmingToken.animationCount;

							newData->heatTrackIndex = programmingToken.heatTrackIndex;

							newData->tailMarker = ANIMATION_SET_VALID_KEY;
							NRF_LOG_DEBUG("Writing set");

							Flash::write(getAnimationSetAddress(), newData, sizeof(Data),
								[](bool result, uint32_t address, uint16_t size) {
									NRF_LOG_INFO("Animation Set written to flash!");
									free(newData);

									if (!CheckValid()) {
										NRF_LOG_ERROR("Animation data is not valid!");
									}
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

	void ProgramDefaultAnimationSet(AnimationSetWrittenCallback callback) {

		static AnimationSetWrittenCallback _setWrittenCallback;
		_setWrittenCallback = callback;

		int paletteSize = 12;
		static uint8_t* palette;
		palette = (uint8_t*)malloc(paletteSize * sizeof(uint8_t));
		palette[0] = 0;
		palette[1] = 0;
		palette[2] = 0;

		palette[3] = 128;
		palette[4] = 0;
		palette[5] = 0;

		palette[6] = 0;
		palette[7] = 128;
		palette[8] = 0;

		palette[9] = 0;
		palette[10] = 0;
		palette[11] = 128;

		// Create a few keyframes
		int keyFrameCount = 4 * 3;
		static RGBKeyframe* keyframes;
		keyframes = (RGBKeyframe*)malloc(keyFrameCount * sizeof(RGBKeyframe));
		keyframes[0].setTimeAndColorIndex(0, 0);
		keyframes[1].setTimeAndColorIndex(300, 1);
		keyframes[2].setTimeAndColorIndex(700, 1);
		keyframes[3].setTimeAndColorIndex(1000, 0);

		keyframes[4].setTimeAndColorIndex(0, 0);
		keyframes[5].setTimeAndColorIndex(300, 2);
		keyframes[6].setTimeAndColorIndex(700, 2);
		keyframes[7].setTimeAndColorIndex(1000, 0);

		keyframes[8].setTimeAndColorIndex(0, 0);
		keyframes[9].setTimeAndColorIndex(300, 3);
		keyframes[10].setTimeAndColorIndex(700, 3);
		keyframes[11].setTimeAndColorIndex(1000, 0);

		// Create tracks
		int ledCount = Config::BoardManager::getBoard()->ledCount;

		// One track per color
		int rgbTrackCount = 3;
		static RGBTrack* rgbTracks;
		rgbTracks = (RGBTrack*)malloc(rgbTrackCount * sizeof(RGBTrack));
		for (int c = 0; c < 3; ++c) {
			// Each anim is a single track per face
			int trackIndex = c;
			rgbTracks[trackIndex].keyFrameCount = 4;
			rgbTracks[trackIndex].keyframesOffset = c * 4;
		}

		// Create animation tracks
		int trackCount = ledCount * 3;
		static AnimationTrack* tracks;
		tracks = (AnimationTrack*)malloc(trackCount * sizeof(AnimationTrack));
		for (int c = 0; c < 3; ++c) {
			for (int l = 0; l < ledCount; ++l) {
				int trackIndex = c * ledCount + l;
				tracks[trackIndex].ledIndex = l;
				tracks[trackIndex].trackOffset = c;
			}
		}

		// Create animations
		int animCount = ledCount * 3 + 3;
		static Animation* animations;
		animations = (Animation*)malloc(animCount * sizeof(Animation));
		for (int c = 0; c < 3; ++c) {
			for (int a = 0; a < ledCount; ++a) {
				int animIndex = c * ledCount + a;
				animations[animIndex].tracksOffset = c * ledCount + a;
				animations[animIndex].trackCount = 1;
				animations[animIndex].duration = 1000;
				if (a == 0) {
					switch (c) {
						case 0:
							animations[animIndex].animationEvent = Animations::AnimationEvent_Rolling;
							break;
						case 1:
							animations[animIndex].animationEvent = Animations::AnimationEvent_OnFace_Default;
							break;
						case 2:
							animations[animIndex].animationEvent = Animations::AnimationEvent_Handling;
							break;
					}
				}
			}
		}

		// Create last 3 anims
		for (int c = 0; c < 3; ++c) {
			int animIndex = ledCount * 3 + c;
			animations[animIndex].tracksOffset = c * ledCount;
			animations[animIndex].trackCount = ledCount;
			animations[animIndex].duration = 1000;
			switch (c) {
				case 0:
					animations[animIndex].animationEvent = Animations::AnimationEvent_Disconnected;
					break;
				case 1:
					animations[animIndex].animationEvent = Animations::AnimationEvent_Hello;
					break;
				case 2:
					animations[animIndex].animationEvent = Animations::AnimationEvent_Connected;
					break;
			}
		}

		// Program all this into flash
		uint32_t buffersSize =
			keyFrameCount * sizeof(RGBKeyframe) +
			rgbTrackCount * sizeof(RGBTrack) +
			trackCount * sizeof(AnimationTrack) +
			animCount * sizeof(Animation) +
			paletteSize * sizeof(uint8_t);

		NRF_LOG_INFO("Programming default anim set");
		NRF_LOG_DEBUG("Keyframes: %d * %d", keyFrameCount, sizeof(RGBKeyframe));
		NRF_LOG_DEBUG("RGB Tracks: %d * %d", rgbTrackCount, sizeof(RGBTrack));
		NRF_LOG_DEBUG("Animation Tracks: %d * %d", trackCount, sizeof(AnimationTrack));
		NRF_LOG_DEBUG("Animations: %d * %d", animCount, sizeof(Animation));
		NRF_LOG_DEBUG("Palette: %d * %d", paletteSize, sizeof(uint8_t));

		uint32_t totalSize = buffersSize + sizeof(Data);
		uint32_t flashSize = getFlashByteSize(totalSize);
		uint32_t pageAddress = getAnimationSetAddress();
		uint32_t dataAddress = getAnimationSetDataAddress();
		uint32_t pageCount = Flash::bytesToPages(flashSize);

		uint32_t paletteAddress = dataAddress;
		uint32_t keyframesAddress = paletteAddress + paletteSize * sizeof(uint8_t);
		uint32_t rgbTracksAddress = keyframesAddress + keyFrameCount * sizeof(RGBKeyframe);
		uint32_t tracksAddress = rgbTracksAddress + rgbTrackCount * sizeof(RGBTrack);
		uint32_t animationsAddress = tracksAddress + trackCount * sizeof(AnimationTrack);

		static Data* newData; // Don't initialize this static inline because it would only do it on first call!
		newData = (Data*)malloc(sizeof(Data));
		newData->headMarker = ANIMATION_SET_VALID_KEY;
		newData->palette = (const uint8_t*)paletteAddress;
		newData->paletteSize = paletteSize;
		newData->keyframes = (const RGBKeyframe*)keyframesAddress;
		newData->keyFrameCount = keyFrameCount;
		newData->rgbTracks = (const RGBTrack*)rgbTracksAddress;
		newData->rgbTrackCount = rgbTrackCount;
		newData->tracks = (const AnimationTrack*)tracksAddress;
		newData->trackCount = trackCount;
		newData->animations = (const Animation*)animationsAddress;
		newData->animationCount = animCount;
		newData->heatTrackIndex = 0;
		newData->tailMarker = ANIMATION_SET_VALID_KEY;

		static auto finishProgram = [] (bool result) {
			// Cleanup
			free(newData);
			free(animations);
			free(tracks);
			free(rgbTracks);
			free(keyframes);
			free(palette);

			auto callBackCopy = _setWrittenCallback;
			_setWrittenCallback = nullptr;
			if (callBackCopy != nullptr) {
				callBackCopy(result);
			}
		};

		// Start by erasing the flash
		NRF_LOG_DEBUG("Erasing flash");
		Flash::erase(pageAddress, pageCount, [](bool result, uint32_t address, uint16_t size) {
			if (result) {
				// Then program the palette
				NRF_LOG_DEBUG("Writing palette");
				Flash::write((uint32_t)(const void*)newData->palette, palette, newData->paletteSize * sizeof(uint8_t), [] (bool result, uint32_t address, uint16_t size) {
					if (result) {
						NRF_LOG_DEBUG("Writing RGB keyframes");
						Flash::write((uint32_t)(const void*)newData->keyframes, keyframes, newData->keyFrameCount * sizeof(RGBKeyframe), [] (bool result, uint32_t address, uint16_t size) {
							if (result) {
								NRF_LOG_DEBUG("Writing RGB tracks");
								Flash::write((uint32_t)(const void*)newData->rgbTracks, rgbTracks, newData->rgbTrackCount * sizeof(RGBTrack), [] (bool result, uint32_t address, uint16_t size) {
									if (result) {
										NRF_LOG_DEBUG("Writing Animation tracks");
										Flash::write((uint32_t)(const void*)newData->tracks, tracks, newData->trackCount * sizeof(AnimationTrack), [] (bool result, uint32_t address, uint16_t size) {
											if (result) {
												NRF_LOG_DEBUG("Writing Animations");
												Flash::write((uint32_t)(const void*)newData->animations, animations, newData->animationCount * sizeof(Animation), [] (bool result, uint32_t address, uint16_t size) {
													if (result) {
														NRF_LOG_DEBUG("Writing anim set");
														Flash::write(getAnimationSetAddress(), newData, sizeof(Data), [] (bool result, uint32_t address, uint16_t size) {
															if (result) {
																NRF_LOG_INFO("Done");
																//printAnimationInfo();
															}
															finishProgram(result);
														});
													} else {
														finishProgram(false);
													}
												});
											} else {
												finishProgram(false);
											}
										});
									} else {
										finishProgram(false);
									}
								});
							} else {
								finishProgram(false);
							}
						});
					} else {
						finishProgram(false);
					}
				});
			} else {
				finishProgram(false);
			}
		});
	}

	void printAnimationInfo() {
		Timers::pause();
		auto board = BoardManager::getBoard();
		NRF_LOG_INFO("Palette size: %d bytes", getPaletteSize());
		for (int p = 0; p < getPaletteSize() / 3; ++p) {
			NRF_LOG_INFO("  Color %d: %08x", p, getPaletteColor(p));
		}
		NRF_LOG_INFO("Animation Count: %d", getAnimationCount());
		for (int a = 0; a < getAnimationCount(); ++a) {
			auto& anim = getAnimation(a);
			NRF_LOG_INFO("Anim %d:", a);
			NRF_LOG_INFO("  Track count: %d", anim.trackCount);
			for (int t = 0; t < anim.trackCount; ++t) {
				auto& track = anim.GetTrack(t);
				auto& rgbTrack = track.getTrack();
				NRF_LOG_INFO("  Track %d:", t);
				NRF_LOG_INFO("  Face %d:", track.ledIndex);
				NRF_LOG_INFO("  LED %d:", board->faceToLedLookup[track.ledIndex]);
				NRF_LOG_INFO("  Track Offset %d:", anim.tracksOffset + t);
				NRF_LOG_INFO("  RGBTrack Offset %d:", track.trackOffset);
				NRF_LOG_INFO("  Keyframe count: %d", rgbTrack.keyFrameCount);
				// for (int k = 0; k < rgbTrack.keyFrameCount; ++k) {
				// 	auto& keyframe = rgbTrack.getKeyframe(k);
				// 	int time = keyframe.time();
				// 	uint32_t color = keyframe.color(nullptr);
				// 	NRF_LOG_INFO("    Offset %d: %d -> %06x", (rgbTrack.keyframesOffset + k), time, color);
				// }
			}
		}
		Timers::resume();
	}

	const RGBTrack& getHeatTrack() {
		return data->rgbTracks[data->heatTrackIndex];
	}

}
}
