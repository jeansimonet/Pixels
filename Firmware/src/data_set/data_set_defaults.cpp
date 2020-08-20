#include "data_set.h"
#include "data_set_data.h"
#include "config/board_config.h"
#include "animations/animation_simple.h"
#include "behaviors/action.h"
#include "behaviors/behavior.h"
#include "behaviors/condition.h"
#include "utils/utils.h"
#include "drivers_nrf/flash.h"
#include "nrf_log.h"

using namespace Utils;
using namespace DriversNRF;
using namespace Config;
using namespace Modules;
using namespace Animations;
using namespace Behaviors;

//#define USE_BINARY_BUFFER_IMAGE

namespace DataSet
{
#ifdef USE_BINARY_BUFFER_IMAGE
    uint8_t defaultDataSetData[] __attribute__ ((aligned (4))) = {
        0x00, 0x00, 0x0C, 0x00, 0x18, 0x00, 0x24, 0x00,
        0x30, 0x00, 0x3C, 0x00, 0x01, 0xA6, 0xE8, 0x03,
        0x00, 0xA9, 0xB6, 0xBE, 0x00, 0x00, 0xFF, 0x00,
        0x01, 0xB0, 0xE8, 0x03, 0x00, 0xCE, 0x13, 0x3F,
        0x00, 0xFF, 0x00, 0x00, 0x01, 0xBA, 0xE8, 0x03,
        0x00, 0xEB, 0x13, 0x3F, 0xFF, 0x00, 0x00, 0x00,
        0x01, 0xA9, 0xE8, 0x03, 0x01, 0xCC, 0x13, 0xBF,
        0x00, 0x00, 0xFF, 0x00, 0x01, 0x00, 0xE8, 0x03,
        0x01, 0x00, 0x00, 0x80, 0x00, 0xFF, 0x00, 0x00,
        0x01, 0xCE, 0xE8, 0x03, 0x01, 0xCA, 0x13, 0x3F,
        0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00,
        0x08, 0x00, 0x0C, 0x00, 0x01, 0x04, 0x00, 0x01,
        0x01, 0x05, 0x00, 0x01, 0x01, 0x00, 0xFF, 0x01,
        0x01, 0x00, 0xFF, 0x01, 0x00, 0x00, 0x04, 0x00,
        0x08, 0x00, 0x0C, 0x00, 0x01, 0x01, 0x00, 0x00,
        0x06, 0x03, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00,
        0x04, 0x00, 0x06, 0x01, 0x00, 0x00, 0x00, 0x00,
        0x01, 0x00, 0x01, 0x00, 0x02, 0x00, 0x02, 0x00,
        0x03, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00,
    };

    
    uint8_t defaultDataSet[] __attribute__ ((aligned (4))) = {
        0x0D, 0xF0, 0x0D, 0x60, 0x01, 0x00, 0x00, 0x00,
        0x74, 0xB0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x74, 0xB0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x74, 0xB0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x74, 0xB0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x74, 0xB0, 0x02, 0x00, 0x06, 0x00, 0x00, 0x00,
        0x80, 0xB0, 0x02, 0x00, 0x48, 0x00, 0x00, 0x00,
        0xE0, 0xB0, 0x02, 0x00, 0x04, 0x00, 0x00, 0x00,
        0xE8, 0xB0, 0x02, 0x00, 0x10, 0x00, 0x00, 0x00,
        0xC8, 0xB0, 0x02, 0x00, 0x04, 0x00, 0x00, 0x00,
        0xD0, 0xB0, 0x02, 0x00, 0x10, 0x00, 0x00, 0x00,
        0xF8, 0xB0, 0x02, 0x00, 0x04, 0x00, 0x00, 0x00,
        0x08, 0xB1, 0x02, 0x00, 0x01, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x02, 0x3E, 0x00, 0x00, 0x00, 0x00,
        0x0D, 0xF0, 0x0D, 0x60,
    };

#endif

	void ProgramDefaultDataSet(DataSetWrittenCallback callback) {

		static DataSetWrittenCallback _setWrittenCallback;
		_setWrittenCallback = callback;

        static void* writeBuffer;
        static uint32_t bufferSize;
		static Data* newData; 

#ifdef USE_BINARY_BUFFER_IMAGE
        writeBuffer = defaultDataSetData;
        bufferSize = sizeof(defaultDataSetData);
        newData = (Data*)defaultDataSet;
#else

		int paletteSize = 0;
		int rgbKeyframeCount = 0;
        int rgbTrackCount = 0;
		int keyframeCount = 0;
        int trackCount = 0;
		int animCount = 3 + 3;
        int animOffsetSize = Utils::roundUpTo4(animCount * sizeof(uint16_t));
        int animSize = sizeof(AnimationSimple) * animCount;
        int actionCount = 4;
        int actionOffsetSize = Utils::roundUpTo4(actionCount * sizeof(uint16_t));
        int actionSize = sizeof(ActionPlayAnimation) * actionCount;
        int conditionCount = 4;
        int conditionOffsetSize = Utils::roundUpTo4(conditionCount * sizeof(uint16_t));
        uint32_t conditionsSize =
            sizeof(ConditionHelloGoodbye) + 
            sizeof(ConditionConnectionState) +
            sizeof(ConditionRolling) +
            sizeof(ConditionFaceCompare);
        int ruleCount = 4;
        int behaviorCount = 1;

		// Compute the size of the needed buffer to store all that data!
		bufferSize =
            paletteSize * sizeof(uint8_t) +
			rgbKeyframeCount * sizeof(RGBKeyframe) +
			rgbTrackCount * sizeof(RGBTrack) +
			keyframeCount * sizeof(Keyframe) +
			trackCount * sizeof(Track) +
			animOffsetSize + animSize +
            actionOffsetSize + actionSize +
            conditionOffsetSize + conditionsSize + 
            ruleCount * sizeof(Rule) +
            behaviorCount * sizeof(Behavior);

		uint32_t dataAddress = getDataSetDataAddress();

		// NRF_LOG_INFO("Total size: %d", totalSize);
		// NRF_LOG_INFO("Flash size: %d", flashSize);
		// NRF_LOG_INFO("pageAddress size: %d", pageAddress);
		// NRF_LOG_INFO("dataAddress: %d", dataAddress);
		// NRF_LOG_INFO("page Count: %d", pageCount);

        // Allocate a buffer for all the data we're about to create
        // We'll write the data in the buffer and then program it into flash!
        writeBuffer = malloc(bufferSize);
        uint32_t writeBufferAddress = (uint32_t)writeBuffer;

        // Allocate a new data object
        // We need to fill it with pointers as if the data it points to is located in flash already.
        // That means we have to compute addresses by hand, we can't just point to the data buffer We
        // just created above. Instead we make the pointers point to where the data WILL be.
		newData = (Data*)malloc(sizeof(Data));
		
        int currentOffset = 0;
        newData->headMarker = ANIMATION_SET_VALID_KEY;
		newData->version = ANIMATION_SET_VERSION;

		newData->palette = (const uint8_t*)(dataAddress + currentOffset);
        //auto writePalette = (const uint8_t*)(writeBufferAddress + currentOffset);
        currentOffset += paletteSize * sizeof(uint8_t);
		newData->paletteSize = paletteSize;

		newData->rgbKeyframes = (const RGBKeyframe*)(dataAddress + currentOffset);
        //auto writeKeyframes = (RGBKeyframe*)(writeBufferAddress + currentOffset);
        currentOffset += rgbKeyframeCount * sizeof(RGBKeyframe);
		newData->rgbKeyFrameCount = rgbKeyframeCount;

		newData->rgbTracks = (const RGBTrack*)(dataAddress + currentOffset);
        //auto writeRGBTracks = (RGBTrack*)(writeBufferAddress + currentOffset);
		currentOffset += rgbTrackCount * sizeof(RGBTrack);
        newData->rgbTrackCount = rgbTrackCount;

		newData->keyframes = (const Keyframe*)(dataAddress + currentOffset);
        //auto writeKeyframes = (Keyframe*)(writeBufferAddress + currentOffset);
        currentOffset += keyframeCount * sizeof(Keyframe);
		newData->keyFrameCount = keyframeCount;

		newData->tracks = (const Track*)(dataAddress + currentOffset);
        //auto writeRGBTracks = (Track*)(writeBufferAddress + currentOffset);
		currentOffset += trackCount * sizeof(Track);
        newData->trackCount = trackCount;

		newData->animationOffsets = (const uint16_t*)(dataAddress + currentOffset);
        auto writeAnimationOffsets = (uint16_t*)(writeBufferAddress + currentOffset);
        currentOffset += animOffsetSize;
		newData->animationCount = animCount;

		newData->animations = (const Animation*)(dataAddress + currentOffset);
        auto writeAnimations = (AnimationSimple*)(writeBufferAddress + currentOffset);
        currentOffset += animSize;
		newData->animationsSize = animSize;
		
        newData->actionsOffsets = (const uint16_t*)(dataAddress + currentOffset);
        auto writeActionsOffsets = (uint16_t*)(writeBufferAddress + currentOffset);
        currentOffset += actionOffsetSize;
		newData->actionCount = actionCount;
		
        newData->actions = (const Action*)(dataAddress + currentOffset);
        auto writeActions = (ActionPlayAnimation*)(writeBufferAddress + currentOffset);
        currentOffset += actionSize;
		newData->actionsSize = actionSize;
		
        newData->conditionsOffsets = (const uint16_t*)(dataAddress + currentOffset);
        auto writeConditionsOffsets = (uint16_t*)(writeBufferAddress + currentOffset);
        currentOffset += conditionOffsetSize;
		newData->conditionCount = conditionCount;
		
        newData->conditions = (const Condition*)(dataAddress + currentOffset);
        auto writeConditions = (Condition*)(writeBufferAddress + currentOffset);
        currentOffset += conditionsSize;
		newData->conditionsSize = conditionsSize;
        
        newData->rules = (const Rule*)(dataAddress + currentOffset);
        auto writeRules = (Rule*)(writeBufferAddress + currentOffset);
        currentOffset += ruleCount * sizeof(Rule);
        newData->ruleCount = ruleCount;
		
        newData->behaviors = (const Behavior*)(dataAddress + currentOffset);
        auto writeBehaviors = (Behavior*)(writeBufferAddress + currentOffset);
        currentOffset += behaviorCount * sizeof(Behavior);
		newData->behaviorsCount = 1;
        
		newData->heatTrackIndex = 0;
		newData->tailMarker = ANIMATION_SET_VALID_KEY;

		// Create animations
		for (int c = 0; c < 3; ++c) {
            writeAnimations[c].type = Animation_Simple;
            writeAnimations[c].duration = 1000;
		    writeAnimations[c].faceMask = 0x80000;
            writeAnimations[c].count = 1;
            writeAnimations[c].fade = 255;
            writeAnimations[c].color = 0xFF0000 >> (c * 8);
		}

		for (int c = 0; c < 3; ++c) {
            writeAnimations[3 + c].type = Animation_Simple;
            writeAnimations[3 + c].duration = 1000;
		    writeAnimations[3 + c].faceMask = 0xFFFFF;
            writeAnimations[3 + c].count = 2;
            writeAnimations[3 + c].fade = 255;
            writeAnimations[3 + c].color = 0xFF0000 >> (c * 8);
		}

		// Create offsets
		for (int i = 0; i < animCount; ++i) {
			writeAnimationOffsets[i] = i * sizeof(AnimationSimple);
		}

        // Create conditions
        uint32_t address = reinterpret_cast<uint32_t>(writeConditions);
        uint16_t offset = 0;

        // Add Hello condition (index 0)
        ConditionHelloGoodbye* hello = reinterpret_cast<ConditionHelloGoodbye*>(address);
        hello->type = Condition_HelloGoodbye;
        hello->flags = ConditionHelloGoodbye_Hello;
        writeConditionsOffsets[0] = offset;
        offset += sizeof(ConditionHelloGoodbye);
        address += sizeof(ConditionHelloGoodbye);
        // And matching action
        writeActions[0].type = Action_PlayAnimation;
        writeActions[0].animIndex = 4; // All LEDs green
        writeActions[0].faceIndex = 0; // doesn't matter
        writeActions[0].loopCount = 1;

        // Add New Connection condition (index 1)
        ConditionConnectionState* connected = reinterpret_cast<ConditionConnectionState*>(address);
        connected->type = Condition_ConnectionState;
        connected->flags = ConditionConnectionState_Connected | ConditionConnectionState_Disconnected;
        writeConditionsOffsets[1] = offset;
        offset += sizeof(ConditionConnectionState);
        address += sizeof(ConditionConnectionState);
        // And matching action
        writeActions[1].type = Action_PlayAnimation;
        writeActions[1].animIndex = 5; // All LEDs blue
        writeActions[1].faceIndex = 0; // doesn't matter
        writeActions[1].loopCount = 1;

        // Add Rolling condition (index 2)
        ConditionRolling* rolling = reinterpret_cast<ConditionRolling*>(address);
        rolling->type = Condition_Rolling;
        writeConditionsOffsets[2] = offset;
        offset += sizeof(ConditionRolling);
        address += sizeof(ConditionRolling);
        // And matching action
        writeActions[2].type = Action_PlayAnimation;
        writeActions[2].animIndex = 0; // face led red
        writeActions[2].faceIndex = FACE_INDEX_CURRENT_FACE;
        writeActions[2].loopCount = 1;

        // Add OnFace condition (index 3)
        ConditionFaceCompare* face = reinterpret_cast<ConditionFaceCompare*>(address);
        face->type = Condition_FaceCompare;
        face->flags = ConditionFaceCompare_Equal | ConditionFaceCompare_Greater;
        face->faceIndex = 0;
        writeConditionsOffsets[3] = offset;
        offset += sizeof(ConditionFaceCompare);
        address += sizeof(ConditionFaceCompare);
        // And matching action
        writeActions[3].type = Action_PlayAnimation;
        writeActions[3].animIndex = 0; // face led green
        writeActions[3].faceIndex = FACE_INDEX_CURRENT_FACE;
        writeActions[3].loopCount = 1;

        // Create action offsets
		for (int i = 0; i < actionCount; ++i) {
            writeActionsOffsets[i] = i * sizeof(ActionPlayAnimation);
		}

        // Add Rules
        for (int i = 0; i < ruleCount; ++i) {
            writeRules[i].condition = i;
            writeRules[i].action = i;
        }

        // Add Behavior
        writeBehaviors[0].rulesOffset = 0;
        writeBehaviors[0].rulesCount = 4;

#endif

		uint32_t totalSize = bufferSize + sizeof(Data);
		uint32_t flashSize = Flash::getFlashByteSize(totalSize);
		uint32_t pageAddress = getDataSetAddress();
		uint32_t pageCount = Flash::bytesToPages(flashSize);

		static auto finishProgram = [] (bool result) {
			// Cleanup
			free(newData);
			free(writeBuffer);

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
				NRF_LOG_DEBUG("Writing data");
				Flash::write(getDataSetDataAddress(), writeBuffer, bufferSize, [] (bool result, uint32_t address, uint16_t size) {
					if (result) {
                        NRF_LOG_DEBUG("Writing anim set");
                        Flash::write(getDataSetAddress(), newData, sizeof(Data), [] (bool result, uint32_t address, uint16_t size) {
                            if (result) {
                                NRF_LOG_DEBUG("Done");
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
	}

}