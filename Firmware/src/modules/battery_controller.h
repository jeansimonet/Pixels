#ifndef BATTERY_CONTROLLER
#define BATTERY_CONTROLLER

namespace Modules
{
	/// <summary>
	/// Manages a set of running animations, talking to the LED controller
	/// to tell it what LEDs must have what intensity at what time.
	/// </summary>
	class BatteryController
	{
    public:
        void init();
    };
}

#endif //BATTERY_CONTROLLER