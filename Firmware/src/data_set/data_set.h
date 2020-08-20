#pragma once

#include "animations/keyframes.h"
#include "animations/animation.h"
#include "animations/animation_keyframed.h"
#include "animations/animation_gradientpattern.h"
#include "behaviors/condition.h"
#include "behaviors/action.h"
#include "behaviors/behavior.h"
#include "stdint.h"
#include "bluetooth/bluetooth_messages.h"
#include "bluetooth/bulk_data_transfer.h"

#define MAX_COLOR_MAP_SIZE (1 << 7) // 128 colors!
#define SPECIAL_COLOR_INDEX (MAX_COLOR_MAP_SIZE - 1)
#define MAX_ANIMATIONS (64)

namespace DataSet
{
	typedef void (*DataSetWrittenCallback)(bool success);

	void init(DataSetWrittenCallback callback);
	bool CheckValid();

	// Size Hash
	uint32_t dataSize();
	uint32_t dataHash();

	const Animations::RGBTrack& getHeatTrack();

	// Animation bits contain palette and keyframes for animations
	const AnimationBits* getAnimationBits();

	// Animations
	const Animations::Animation* getAnimation(int animationIndex);
	uint16_t getAnimationCount();

	// Conditions
	const Behaviors::Condition* getCondition(int conditionIndex);
	uint16_t getConditionCount();

	// Actions
	const Behaviors::Action* getAction(int actionIndex);
	uint16_t getActionCount();

	// Rules
	const Behaviors::Rule* getRule(int ruleIndex);
	uint16_t getRuleCount();

	// Behaviors
	const Behaviors::Behavior* getBehavior(int behaviorIndex);
	uint16_t getBehaviorCount();

	uint32_t getDataSetAddress();
	uint32_t getDataSetDataAddress();

	void ProgramDefaultDataSet(DataSetWrittenCallback callback);
	void ReceiveDataSetHandler(void* context, const Bluetooth::Message* msg);
	void printAnimationInfo();

	enum ProgrammingEventType
	{
		ProgrammingEventType_Begin = 0,
		ProgrammingEventType_End
	};

	typedef void (*ProgrammingEventMethod)(void* param, ProgrammingEventType evt);
	void hookProgrammingEvent(ProgrammingEventMethod client, void* param);
	void unhookProgrammingEvent(ProgrammingEventMethod client);
}

