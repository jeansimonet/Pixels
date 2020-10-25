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
#include "config/settings.h"

#define MAX_COLOR_MAP_SIZE (1 << 7) // 128 colors!
#define SPECIAL_COLOR_INDEX (MAX_COLOR_MAP_SIZE - 1)
#define MAX_ANIMATIONS (64)

namespace DataSet
{
	struct Data;

	typedef void (*DataSetWrittenCallback)(bool success);

	void init(DataSetWrittenCallback callback);
	bool CheckValid();

	uint32_t availableDataSize();

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
	const Behaviors::Behavior* getBehavior();

	uint32_t computeDataSetDataSize(const Data* newData);

	void ProgramDefaultDataSet(const Config::Settings& settingsPackAlong, DataSetWrittenCallback callback);
	void ReceiveDataSetHandler(void* context, const Bluetooth::Message* msg);

	void printAnimationInfo();
}

