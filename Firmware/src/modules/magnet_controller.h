#ifndef MAGNET_CONTROLLER
#define MAGNET_CONTROLLER

namespace Modules
{
	/// <summary>
	/// Manages a set of running animations, talking to the LED controller
	/// to tell it what LEDs must have what intensity at what time.
	/// </summary>
	class MagnetController
	{
    public:
        void init();
    };
}

#endif // MAGNET_CONTROLLER