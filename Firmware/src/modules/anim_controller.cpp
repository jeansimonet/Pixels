#include "anim_controller.h"
#include "animations/animation.h"
#include "data_set/data_set.h"
#include "drivers_nrf/timers.h"
#include "drivers_nrf/power_manager.h"
#include "utils/utils.h"
#include "utils/rainbow.h"
#include "config/board_config.h"
#include "config/settings.h"
#include "config/dice_variants.h"
#include "drivers_hw/apa102.h"
#include "app_error.h"
#include "nrf_log.h"
#include "accelerometer.h"

using namespace Animations;
using namespace Modules;
using namespace Config;
using namespace DriversNRF;
using namespace DriversHW;

#define MAX_ANIMS (20)
#define TIMER2_RESOLUTION 33 //ms

namespace Modules
{
namespace AnimController
{
	// Our currently running animations
	Animations::AnimationInstance* animations[MAX_ANIMS];
	int animationCount;

	// FIXME!!!
	int currentRainbowIndex = 0;
	const int rainbowScale = 1; 
	float heat = 0.0f;

	uint32_t getColorForAnim(void* token, uint32_t colorIndex);
	void onAccelFrame(void* param, const Accelerometer::AccelFrame& accelFrame);
	uint8_t animIndexToLEDIndex(int animFaceIndex, int remapFace);

	void onSettingsProgrammingEvent(void* context, SettingsManager::ProgrammingEventType evt);
	void onDatasetProgrammingEvent(void* context, DataSet::ProgrammingEventType evt);

	APP_TIMER_DEF(animControllerTimer);
	// To be passed to the timer
	void animationControllerUpdate(void* param)
	{
		update(Utils::millis());
	}

	/// <summary>
	/// Kick off the animation controller, registering it with the Timer system
	/// </summary>
	void init()
	{
		DataSet::hookProgrammingEvent(onDatasetProgrammingEvent, nullptr);
		SettingsManager::hookProgrammingEvent(onSettingsProgrammingEvent, nullptr);

		heat = 0.0f;
		currentRainbowIndex = 0;

		animationCount = 0;
		Timers::createTimer(&animControllerTimer, APP_TIMER_MODE_REPEATED, animationControllerUpdate);
		start();
		NRF_LOG_INFO("Anim Controller Initialized");
	}

	/// <summary>
	/// Update all currently running animations, and performing housekeeping when necessary
	/// </summary>
	/// <param name="ms">Current global time in milliseconds</param>
	void update(int ms)
	{
		auto s = SettingsManager::getSettings();
		auto b = BoardManager::getBoard();
		auto l = DiceVariants::getLayout(b->ledCount, s->faceLayoutLookupIndex);
		int c = b->ledCount;

		// Update heat value (cool down)
		heat *= s->coolDownRate;
		if (heat < 0.0f) {
			heat = 0.0f;
		}

		if (animationCount > 0) {
	        PowerManager::feed();

			// clear the global color array
			uint32_t allColors[MAX_LED_COUNT];
			for (int j = 0; j < c; ++j) {
				allColors[j] = 0;
			}

			for (int i = 0; i < animationCount; ++i)
			{
				auto anim = animations[i];
				int animTime = ms - anim->startTime;
				if (anim->loop && animTime > anim->animationPreset->duration) {
					// Yes, update anim start time so next if statement updates the animation
					anim->startTime += anim->animationPreset->duration;
					animTime = ms - anim->startTime;
				}

				if (animTime > anim->animationPreset->duration)
				{
					// The animation is over, get rid of it!
					Animations::destroyAnimationInstance(anim);

					// Shift the other animations
					for (int j = i; j < animationCount - 1; ++j)
					{
						animations[j] = animations[j + 1];
					}

					// Reduce the count
					animationCount--;

					// Decrement loop counter since we just replaced the current anim
					i--;
				}
				else
				{
					int canonIndices[MAX_LED_COUNT * 4]; // Allow up to 4 tracks to target the same LED
					int ledIndices[MAX_LED_COUNT * 4];
					uint32_t colors[MAX_LED_COUNT * 4];

					// Update the leds
					int animTrackCount = anim->updateLEDs(ms, canonIndices, colors);

					// Gamma correct and map face index to led index
					//NRF_LOG_INFO("track_count = %d", animTrackCount);
					for (int j = 0; j < animTrackCount; ++j) {
						//colors[j] = Utils::gamma(colors[j]);

						// The transformation is:
						// animFaceIndex (what face the animation says it wants to light up)
						//	-> rotatedAnimFaceIndex (based on remapFace and remapping table, i.e. what actual
						//	   face should light up to "retarget" the animation around the current up face)
						//		-> ledIndex (based on pcb face to led mapping, i.e. to account for the internal rotation
						//		   of the PCB and the fact that the LEDs are not accessed in the same order as the number of the faces)
						int rotatedAnimFaceIndex = l->faceRemap[anim->remapFace * c + canonIndices[j]];
						ledIndices[j] = s->faceToLEDLookup[rotatedAnimFaceIndex];
					}

					// Update color array
					for (int j = 0; j < animTrackCount; ++j) {
						
						// Combine colors if necessary
						//NRF_LOG_INFO("index: %d -> %08x", ledIndices[j], colors[j]);
						allColors[ledIndices[j]] = Utils::addColors(allColors[ledIndices[j]], colors[j]);
					}
				}
			}
			
			// And light up!
			APA102::setPixelColors(allColors);
			APA102::show();
		}
	}

	/// <summary>
	/// Stop updating animations
	/// </summary>
	void stop()
	{
		Accelerometer::unHookFrameData(onAccelFrame);
		Timers::stopTimer(animControllerTimer);
		// Clear all data
		stopAll();
		NRF_LOG_INFO("Stopped anim controller");
	}

	void start()
	{
		Accelerometer::hookFrameData(onAccelFrame, nullptr);
		NRF_LOG_INFO("Starting anim controller");
		Timers::startTimer(animControllerTimer, TIMER2_RESOLUTION, NULL);
	}

	/// <summary>
	/// Add an animation to the list of running animations
	/// </summary>
	void play(int animIndex, uint8_t remapFace, bool loop)
	{
		#if (NRF_LOG_DEFAULT_LEVEL == 4)
		NRF_LOG_DEBUG("Playing Anim!");
		NRF_LOG_DEBUG("  Track count: %d", anim->trackCount);
		for (int t = 0; t < anim->trackCount; ++t) {
			auto& track = anim->GetTrack(t);
			NRF_LOG_DEBUG("  Track %d:", t);
			NRF_LOG_DEBUG("  Track Offset %d:", anim->tracksOffset + t);
			NRF_LOG_DEBUG("  LED index %d:", track.ledIndex);
			NRF_LOG_DEBUG("  RGB Track Offset %d:", track.trackOffset);
			auto& rgbTrack = track.getLEDTrack();
			NRF_LOG_DEBUG("  RGB Keyframe count: %d", rgbTrack.keyFrameCount);
			for (int k = 0; k < rgbTrack.keyFrameCount; ++k) {
				auto& keyframe = rgbTrack.getRGBKeyframe(k);
				int time = keyframe.time();
				uint32_t color = keyframe.color(0);
				NRF_LOG_DEBUG("    Offset %d: %d -> %06x", (rgbTrack.keyframesOffset + k), time, color);
			}
		}
		#endif

		// Find the preset for this animation Index
		auto animationPreset = DataSet::getAnimation(animIndex);

		// Is there already an animation for this?
		int prevAnimIndex = 0;
		for (; prevAnimIndex < animationCount; ++prevAnimIndex)
		{
			auto prevAnim = animations[prevAnimIndex];
			if (prevAnim->animationPreset == animationPreset && prevAnim->remapFace == remapFace)
			{
				break;
			}
		}

		int ms = Utils::millis();
		if (prevAnimIndex < animationCount)
		{
			// Replace a previous animation
			stopAtIndex(prevAnimIndex);
			animations[prevAnimIndex]->startTime = ms;
		}
		else if (animationCount < MAX_ANIMS)
		{
			// Add a new animation
			animations[animationCount] = Animations::createAnimationInstance(animationPreset, DataSet::getAnimationBits());
			animations[animationCount]->start(ms, remapFace, loop);
			animationCount++;
		}
		// Else there is no more room
	}

	/// <summary>
	/// Forcibly stop a currently running animation
	/// </summary>
	void stop(int animIndex, uint8_t remapFace)
	{
		// Find the preset for this animation Index
		auto animationPreset = DataSet::getAnimation(animIndex);

		// Find the animation with that preset and remap face
		int prevAnimIndex = 0;
		AnimationInstance* prevAnimInstance = nullptr;
		for (; prevAnimIndex < animationCount; ++prevAnimIndex)
		{
			auto instance = animations[prevAnimIndex];
			if (instance->animationPreset == animationPreset && (remapFace == 255 || instance->remapFace == remapFace))
			{
				prevAnimInstance = instance;
				break;
			}
		}

		if (prevAnimIndex < animationCount)
		{
			removeAtIndex(prevAnimIndex);

			// Delete the instance
			Animations::destroyAnimationInstance(prevAnimInstance);
		}
		// Else the animation isn't playing
	}

	/// <summary>
	/// Stop all currently running animations
	/// </summary>
	void stopAll()
	{
		for (int i = 0; i < animationCount; ++i)
		{
			// Delete the instance
			Animations::destroyAnimationInstance(animations[i]);
		}
		animationCount = 0;
		APA102::clear();
		APA102::show();
	}

	/// <summary>
	/// Helper function to clear anim LED turned on by a current animation
	/// </summary>
	void stopAtIndex(int animIndex)
	{
		auto s = SettingsManager::getSettings();
		auto b = BoardManager::getBoard();
		auto l = DiceVariants::getLayout(b->ledCount, s->faceLayoutLookupIndex);
		int c = b->ledCount;

		// Found the animation, start by killing the leds it controls
		int canonIndices[MAX_LED_COUNT];
		int ledIndices[MAX_LED_COUNT];
		uint32_t zeros[MAX_LED_COUNT];
		memset(zeros, 0, sizeof(uint32_t) * MAX_LED_COUNT);
		auto anim = animations[animIndex];
		int ledCount = anim->stop(canonIndices);
		for (int i = 0; i < ledCount; ++i) {
			// The transformation is:
			// animFaceIndex (what face the animation says it wants to light up)
			//	-> rotatedAnimFaceIndex (based on remapFace and remapping table, i.e. what actual
			//	   face should light up to "retarget" the animation around the current up face)
			//		-> ledIndex (based on pcb face to led mapping, i.e. to account for the internal rotation
			//		   of the PCB and the fact that the LEDs are not accessed in the same order as the number of the faces)
			int rotatedAnimFaceIndex = l->faceRemap[anim->remapFace * c + canonIndices[i]];
			ledIndices[i] = s->faceToLEDLookup[rotatedAnimFaceIndex];
		}
		APA102::setPixelColors(ledIndices, zeros, ledCount);
		APA102::show();
	}

	/// <summary>
	/// Helper method: Stop the animation at the given index. Used by Stop(IAnimation*)
	/// </summary>
	void removeAtIndex(int animIndex)
	{
		stopAtIndex(animIndex);

		// Shift the other animations
		for (; animIndex < animationCount - 1; ++animIndex)
		{
			animations[animIndex] = animations[animIndex + 1];
		}

		// Reduce the count
		animationCount--;
	}

	void onAccelFrame(void* param, const Accelerometer::AccelFrame& accelFrame) {
		auto sqrMag = accelFrame.jerk.sqrMagnitude();
		if (sqrMag > 0.0f) {
			currentRainbowIndex++;
			heat += sqrt(sqrMag) * SettingsManager::getSettings()->heatUpRate;
			if (heat > 1.0f) {
				heat = 1.0f;
			}
		}
	}

	uint8_t animIndexToLEDIndex(int animFaceIndex, int remapFace) {
		// The transformation is:
		// animFaceIndex (what face the animation says it wants to light up)
		//	-> rotatedAnimFaceIndex (based on remapFace and remapping table, i.e. what actual
		//	   face should light up to "retarget" the animation around the current up face)
		//		-> ledIndex (based on pcb face to led mapping, i.e. to account for the internal rotation
		//		   of the PCB and the fact that the LEDs are not accessed in the same order as the number of the faces)

		auto s = SettingsManager::getSettings();
		auto b = BoardManager::getBoard();
		auto l = DiceVariants::getLayout(b->ledCount, s->faceLayoutLookupIndex);
		int c = b->ledCount;

		int rotatedAnimFaceIndex = l->faceRemap[remapFace * c + animFaceIndex];
		return s->faceToLEDLookup[rotatedAnimFaceIndex];
	}

	int getCurrentRainbowOffset() {
		return currentRainbowIndex / rainbowScale;
	}
	float getCurrentHeat() {
		return heat;
	}
	

	void onSettingsProgrammingEvent(void* context, SettingsManager::ProgrammingEventType evt){
		if (evt == SettingsManager::ProgrammingEventType_Begin) {
			stop();
		} else {
			start();
		}
	}

	void onDatasetProgrammingEvent(void* context, DataSet::ProgrammingEventType evt){
		if (evt == DataSet::ProgrammingEventType_Begin) {
			stop();
		} else {
			start();
		}
	}

}
}

