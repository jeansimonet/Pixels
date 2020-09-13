#pragma once

#include <stdint.h>

namespace Die
{
    enum TopLevelState
    {
        TopLevel_Unknown = 0,
        TopLevel_SoloPlay,      // Playing animations as a result of landing on faces
        TopLevel_BattlePlay,    // Some kind of battle play
        TopLevel_Animator,      // LED Animator
        TopLevel_LowPower,      // Die is low on power
        TopLevel_Attract,
    };

    void init();
	void initMainLogic();
	void initDebugLogic();

	uint32_t getDeviceID();
    void update();

	TopLevelState getCurrentState();
}

