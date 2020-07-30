#pragma once

#include <stdint.h>

namespace Die
{
    void init();
	void initMainLogic();
	void initDebugLogic();

	uint64_t getDeviceID();
    void update();
}

