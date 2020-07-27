#include "animation_keyframed.h"
#include "data_set/data_set.h"
#include "assert.h"

// FIXME!!!
#include "modules/anim_controller.h"
#include "utils/rainbow.h"
#include "config/board_config.h"

namespace Animations
{
	/// <summary>
	/// constructor for keyframe-based animation instances
	/// Needs to have an associated preset passed in
	/// </summary>
	AnimationInstanceKeyframed::AnimationInstanceKeyframed(const AnimationKeyframed* preset)
		: AnimationInstance(preset) {
	}

	/// <summary>
	/// destructor
	/// </summary>
	AnimationInstanceKeyframed::~AnimationInstanceKeyframed() {
	}

	/// <summary>
	/// Small helper to return the expected size of the preset data
	/// </summary>
	int AnimationInstanceKeyframed::animationSize() const {
		return sizeof(AnimationKeyframed);
	}

	/// <summary>
	/// (re)Initializes the instance to animate leds. This can be called on a reused instance.
	/// </summary>
	void AnimationInstanceKeyframed::start(int _startTime, uint8_t _remapFace, bool _loop) {
		AnimationInstance::start(_startTime, _remapFace, _loop);

		switch (getPreset()->specialColorType) {
			case SpecialColor_Face:
				// Store a color based on the face
				specialColorPayload = Rainbow::faceWheel(_remapFace, Config::BoardManager::getBoard()->ledCount);
				break;
			case SpecialColor_ColorWheel:
				// Store the face index
				specialColorPayload = _remapFace;
				break;
			case SpecialColor_Heat_Start:
				{
					// Use the global heat value
					auto& trk = DataSet::getHeatTrack();
					// FIXME!!! Need a separate heat module
					int heatMs = int(Modules::AnimController::getCurrentHeat() * trk.getDuration());
					specialColorPayload = trk.evaluate(nullptr, heatMs);
				}
				break;
			default:
				// Other cases don't need any per-instance payload
				specialColorPayload = 0;
				break;
		}
	}

	/// <summary>
	/// Computes the list of LEDs that need to be on, and what their intensities should be
	/// based on the different tracks of this animation.
	/// </summary>
	/// <param name="ms">The animation time (in milliseconds)</param>
	/// <param name="retIndices">the return list of LED indices to fill, max size should be at least 21, the max number of leds</param>
	/// <param name="retColors">the return list of LED color to fill, max size should be at least 21, the max number of leds</param>
	/// <returns>The number of leds/intensities added to the return array</returns>
	int AnimationInstanceKeyframed::updateLEDs(int ms, int retIndices[], uint32_t retColors[])
	{
		int time = ms - startTime;
		auto preset = getPreset();
		const LEDTrack * tracks = DataSet::getLEDTracks(preset->tracksOffset);
		for (int i = 0; i < preset->trackCount; ++i)
		{
			const RGBTrack& rgbTrack = tracks[i].getLEDTrack();
			retIndices[i] = tracks[i].ledIndex;
			retColors[i] = rgbTrack.evaluate(this, time);
		}
		return preset->trackCount;
	}

	/// <summary>
	/// Clear all LEDs controlled by this animation, for instance when the anim gets interrupted.
	/// </summary>
	int AnimationInstanceKeyframed::stop(int retIndices[]) {
		auto preset = getPreset();
		const LEDTrack * tracks = DataSet::getLEDTracks(preset->tracksOffset);
		for (int i = 0; i < preset->trackCount; ++i)
		{
			retIndices[i] = tracks[i].ledIndex;
		}
		return preset->trackCount;
	}

	/// <summary>
	/// Small helper to get the correct type preset data pointer stored in the instance
	/// </summary
	const AnimationKeyframed* AnimationInstanceKeyframed::getPreset() const {
		return static_cast<const AnimationKeyframed*>(animationPreset);
	}

	/// <summary>
	/// Returns a track
	/// </summary>
	const LEDTrack& AnimationInstanceKeyframed::GetTrack(int index) const	{
		auto preset = getPreset();
		assert(index < preset->trackCount);
		return DataSet::getLEDTrack(preset->tracksOffset + index);
	}

	/// <summary>
	/// returns a color RGB value for a given color index, taking into account special color indices
	/// </summary>
	uint32_t AnimationInstanceKeyframed::getColor(uint32_t colorIndex) const {
		auto preset = getPreset();
		switch (preset->specialColorType) {
			case SpecialColor_Face:
			case SpecialColor_Heat_Start:
				// The payload is the color
				return specialColorPayload;
			case SpecialColor_ColorWheel:
				{
					// Use the global rainbow
					int index = Modules::AnimController::getCurrentRainbowOffset();
					if (index < 0) {
						index += 256;
					}
					return Rainbow::wheel((uint8_t)index);
				}
			case SpecialColor_Heat_Current:
				{
					auto& trk = DataSet::getHeatTrack();
					int heatMs = int(Modules::AnimController::getCurrentHeat() * trk.getDuration());
					return trk.evaluate(nullptr, heatMs);
				}
			default:
				return DataSet::getPaletteColor(colorIndex);
		}
	}

}