#ifndef BATTERY_CONTROLLER
#define BATTERY_CONTROLLER

namespace Modules
{
	/// <summary>
	/// Manages a set of running animations, talking to the LED controller
	/// to tell it what LEDs must have what intensity at what time.
	/// </summary>
	namespace BatteryController
	{
        void init();

		enum BatteryState
		{
			BatteryState_Unknown,
			BatteryState_Ok,
			BatteryState_Low,
			BatteryState_Charging,
			BatteryState_Done
		};

		BatteryState getCurrentChargeState();
		float getCurrentLevel();

		const char* getChargeStateString(BatteryState state);

		typedef void(*BatteryStateChangeHandler)(void* param, BatteryState newState);
		void hook(BatteryStateChangeHandler method, void* param);
		void unHook(BatteryStateChangeHandler client);
		void unHookWithParam(void* param);

		typedef void(*BatteryLevelChangeHandler)(void* param, float level);
		void hookLevel(BatteryLevelChangeHandler method, void* param);
		void unHookLevel(BatteryLevelChangeHandler method);
		void unHookLevelWithParam(void* param);
    }
}

#endif //BATTERY_CONTROLLER