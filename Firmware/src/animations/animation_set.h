#pragma once

#include "animation.h"
#include "stdint.h"
#include "../bluetooth/bluetooth_messages.h"
#include "../bluetooth/bulk_data_transfer.h"

#define COLOR_MAP_SIZE (1 << 7) // 128 colors!
#define MAX_ANIMATIONS (64)

namespace Animations
{
	namespace AnimationSet
	{
		void init();
		bool CheckValid();
		int ComputeAnimationDataSize();

		uint32_t getColor(uint16_t colorIndex);
		RGBKeyframe getKeyframe(uint16_t keyFrameIndex);
		uint16_t getKeyframeCount();
		AnimationTrack getTrack(uint16_t trackIndex);
		AnimationTrack const * const getTracks(uint16_t tracksStartIndex);
		uint16_t getTrackCount();
		Animation getAnimation(uint16_t animIndex);
		uint16_t getAnimationCount();

		void ReceiveAnimationSetHandler(void* context, const Bluetooth::Message* msg);
	}
}

