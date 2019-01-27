/* Modified by Jean Simonet - Systemic Games */

/* Lazarus.h - Library for onBLE functions to break INFINITE waits in loop()
   by Thomas Olson Consulting
   20140806.01
*/

#ifndef Lazarus_h
#define Lazarus_h

namespace Systems
{
	class Lazarus
	{
	private:
		int lastMillis;

	private:
		void sleepUntilInterrupt();

	public:
		bool sleeping;

		Lazarus();
		void init();
		void onRadio();
		void poke();
		void update();
		void stop();
	};

	extern Lazarus lazarus;
}

#endif
