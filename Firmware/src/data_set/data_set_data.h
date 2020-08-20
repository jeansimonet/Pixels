#pragma once
#include "animations/animation.h"
#include "behaviors/condition.h"
#include "behaviors/action.h"
#include "behaviors/behavior.h"
#include "data_animation_bits.h"

#define ANIMATION_SET_VALID_KEY (0x600DF00D) // Good Food ;)
#define ANIMATION_SET_VERSION 1

using namespace Animations;

namespace DataSet
{
	struct Data
	{
		// Indicates whether there is valid data
		uint32_t headMarker;
		uint32_t version;

		AnimationBits animationBits;

        // The animations. Because animations can be one of multiple classes (simple inheritance system)
        // The dataset stores an offset into the animations buffer for each entry. The first member of
        // The animation base class is a type enum indicating what it actually is.
		const uint16_t* animationOffsets; // offsets to actual animation from the animation pointer below
		uint32_t animationCount;
		const Animations::Animation* animations; // The animations we have, 4-byte aligned, so may need some padding
		uint32_t animationsSize; // In bytes

        // The conditionss. Because conditions can be one of multiple classes (simple inheritance system)
        // The dataset stores an offset into the conditions buffer for each entry. The first member of
        // The condition base class is a type enum indicating what it actually is.
		const uint16_t* conditionsOffsets; // offsets to actual conditions from the conditions pointer below
		uint32_t conditionCount; // The conditions we have, 4-byte aligned, so may need some padding
		const Behaviors::Condition* conditions;
		uint32_t conditionsSize; // In bytes

        // The actions. Because actions can be one of multiple classes (simple inheritance system)
        // The dataset stores an offset into the actions buffer for each entry. The first member of
        // The action base class is a type enum indicating what it actually is.
		const uint16_t* actionsOffsets; // offsets to actual actions from the actions pointer below
		uint32_t actionCount; // The actions we have, 4-byte aligned, so may need some padding
		const Behaviors::Action* actions;
		uint32_t actionsSize; // In bytes

        // Rules are pairs or conditions and actions
        const Behaviors::Rule* rules; // pointer to array of rules, behaviors index into it!
        uint32_t ruleCount;

        // Behaviors, or collection of condition->action pairs
		const Behaviors::Behavior* behaviors;
		uint32_t behaviorsCount;

		// Indicates whether there is valid data
		uint32_t tailMarker;
	};

}