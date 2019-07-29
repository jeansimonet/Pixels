#pragma once

#include "animation.h"
#include "stdint.h"
#include "../bluetooth/bluetooth_messages.h"
#include "../bluetooth/bulk_data_transfer.h"

#define MAX_COLOR_MAP_SIZE (1 << 7) // 128 colors!
#define SPECIAL_COLOR_INDEX (MAX_COLOR_MAP_SIZE - 1)
#define MAX_ANIMATIONS (64)

namespace Animations
{
	namespace AnimationSet
	{
		void init();
		bool CheckValid();
		int ComputeAnimationDataSize();

		uint32_t getPaletteColor(uint16_t colorIndex);
		uint32_t getColor(void* token, uint16_t colorIndex);
		const RGBKeyframe& getKeyframe(uint16_t keyFrameIndex);
		uint16_t getKeyframeCount();
		const RGBTrack& getRGBTrack(uint16_t trackIndex);
		uint16_t getRGBTrackCount();
		const AnimationTrack& getTrack(uint16_t trackIndex);
		AnimationTrack const * const getTracks(uint16_t tracksStartIndex);
		const Animation& getAnimation(uint16_t animIndex);
		uint16_t getAnimationCount();
		void ProgramDefaultAnimationSet();
		void printAnimationInfo();

		void ReceiveAnimationSetHandler(void* context, const Bluetooth::Message* msg);

		typedef uint32_t (*getColorHandler)(void* token, uint32_t colorIndex);
		void setGetColorHandler(getColorHandler handler);
		void unsetGetColorHandler(getColorHandler handler);
	}
}

