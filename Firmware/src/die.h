#pragma once

namespace Die
{
    void init();
	void initMainLogic();
	void initDebugLogic();

	// Event handlers
	void onChargingNeeded();
	void onChargingComplete();
	void onChargingInterrupted();
	void onChargingStarted();

    void update();
}

