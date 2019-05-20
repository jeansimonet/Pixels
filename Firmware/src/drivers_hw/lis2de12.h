/******************************************************************************

Modified by Jean Simonet, Systemic Games

******************************************************************************/

/******************************************************************************
SparkFun_MMA8452Q.h
SparkFun_MMA8452Q Library Header File
Jim Lindblom @ SparkFun Electronics
Original Creation Date: June 3, 2014
https://github.com/sparkfun/MMA8452_Accelerometer

This file prototypes the MMA8452Q class, implemented in SFE_MMA8452Q.cpp. In
addition, it defines every register in the MMA8452Q.

Development environment specifics:
IDE: Arduino 1.0.5
Hardware Platform: Arduino Uno

**Updated for Arduino 1.6.4 5/2015**

This code is beerware; if you see me (or any other SparkFun employee) at the
local, and you've found our code helpful, please buy us a round!

Distributed as-is; no warranty is given.
******************************************************************************/

#pragma once

#include <stdint.h>

////////////////////////////////
// MMA8452Q Misc Declarations //
////////////////////////////////
enum LIS2DE12_Scale
{
	SCALE_2G = 0,
	SCALE_4G,
	SCALE_8G,
	SCALE_16G
}; // Possible full-scale settings

enum LIS2DE12_ODR
{
	ODR_PWR_DWN = 0,
	ODR_1,
	ODR_10,
	ODR_25,
	ODR_50,
	ODR_100,
	ODR_200,
	ODR_400,
	ODR_1620,
	ODR_5376,
}; // possible data rates

namespace DriversHW
{
	/// <summary>
	/// The accelerometer I2C devices
	/// </summary>
	namespace LIS2DE12
	{
		void init(LIS2DE12_Scale fsr = SCALE_8G, LIS2DE12_ODR odr = ODR_200);
		void read();
		uint8_t available();

		float convert(short value);

		void setScale(LIS2DE12_Scale fsr);
		void setODR(LIS2DE12_ODR odr);
		void standby();
		void active();

		void enableTransientInterrupt();
		void clearTransientInterrupt();
		void disableTransientInterrupt();

		void selfTest();
		void selfTestInterrupt();
	}
}

