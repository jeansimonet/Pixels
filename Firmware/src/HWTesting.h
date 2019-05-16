#ifndef _HWTESTING_h
#define _HWTESTING_h
#include "Arduino.h"

namespace Tests
{
	void TestDebug();
	void TestANDGate();
	void TestI2C();
	void TestAcc();
	void TestAccDice();
	void TestLED();
	void TestLEDSlow();
	void TestLEDPower();
	void TestLEDDice();
	void TestSleepForever();
	void TestSleepAwakeAcc();
	void TestSettings();
	void TestAnimationSet();
	void TestBattery();
	void TestCharging();
	void TestBatteryDischarge();

	void TestTimerSetup();
	void TestTimerUpdate();

	void TestAnimationsSetup();
	void TestAnimationsUpdate();

	void TestAllSystems();
	void TestMagnet();
	void TestConnections();
}

#endif

