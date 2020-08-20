#include "animation.h"
#include "data_set/data_set.h"

#include "assert.h"
#include "../utils/utils.h"
#include "nrf_log.h"

#include "animation_simple.h"
#include "animation_keyframed.h"
#include "animation_rainbow.h"
#include "animation_gradientpattern.h"


// Define new and delete
void* operator new(size_t size) { return malloc(size); }
void operator delete(void* ptr) { free(ptr); }
void operator delete(void* ptr, unsigned int) { free(ptr); }


#define MAX_LEVEL (256)

using namespace Utils;

namespace Animations
{
	/// Dims the passed in color by the passed in intensity (normalized 0 - 255)
	/// </summary>
	uint32_t scaleColor(uint32_t refColor, uint8_t intensity)
	{
		uint8_t r = getRed(refColor);
		uint8_t g = getGreen(refColor);
		uint8_t b = getBlue(refColor);
		return toColor(r * intensity / MAX_LEVEL, g * intensity / MAX_LEVEL, b * intensity / MAX_LEVEL);
	}

	AnimationInstance::AnimationInstance(const Animation* preset) 
		: animationPreset(preset) {
	}

	AnimationInstance::~AnimationInstance() {
	}

	void AnimationInstance::start(int _startTime, uint8_t _remapFace, bool _loop) {
		startTime = _startTime;
		remapFace = _remapFace;
		loop = _loop;
	}

	AnimationInstance* createAnimationInstance(int animationIndex) {
		// Grab the preset data
		const Animation* preset = DataSet::getAnimation(animationIndex);
		return createAnimationInstance(preset);
	}

	AnimationInstance* createAnimationInstance(const Animation* preset) {
		AnimationInstance* ret = nullptr;
		switch (preset->type) {
			case Animation_Simple:
				// Maybe we'll pass an allocator at some point, this is the only place I've ever used a new in the firmware...
				ret = new AnimationInstanceSimple(static_cast<const AnimationSimple*>(preset));
				break;
			case Animation_Rainbow:
				ret = new AnimationInstanceRainbow(static_cast<const AnimationRainbow*>(preset));
				break;
			case Animation_Keyframed:
				ret = new AnimationInstanceKeyframed(static_cast<const AnimationKeyframed*>(preset));
				break;
			case Animation_GradientPattern:
				ret = new AnimationInstanceGradientPattern(static_cast<const AnimationGradientPattern*>(preset));
				break;
			default:
				NRF_LOG_ERROR("Unknown animation preset type");
				break;
		}
		return ret;
	}

	void destroyAnimationInstance(AnimationInstance* animationInstance) {
		// Eventually we might use an allocator
		delete animationInstance;
	}
}

