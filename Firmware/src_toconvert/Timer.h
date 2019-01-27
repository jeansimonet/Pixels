// Dice_Timer.h

#ifndef _DICE_TIMER_h
#define _DICE_TIMER_h

#include "Arduino.h"
#include "DiceQueue.h"

namespace Systems
{
	#define MAX_CLIENTS (4) 
	#define MAX_QUEUED_CALLS (8)

	/// <summary>
	/// The timer class allows systems to request a callback on
	/// a periodic interval.
	/// </summary>
	struct Timer
	{
	public:
		typedef void(*ClientMethod)(void* param);

		void hook(int resolutionInMicroSeconds, ClientMethod client, void* param);
		void unHook(ClientMethod client);

		struct Client
		{
			ClientMethod callback;
			void* param;
			int ticks;
		};

	private:
		Client clients[MAX_CLIENTS];
		int count;

		Core::Queue<byte, MAX_QUEUED_CALLS> calls;

	private:
		static void timer2Interrupt();
		void interrupt();

	public:
		Timer();
		void begin();
		void stop();
		void update();
	};

	extern Timer timer;
}

#endif

