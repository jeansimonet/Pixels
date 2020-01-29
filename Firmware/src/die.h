#pragma once

namespace Die
{
	void init();
	void initMainLogic();

	// Event handlers
	void onChargingNeeded();
	void onChargingComplete();
	void onChargingInterrupted();
	void onChargingStarted();

	void update();
}
