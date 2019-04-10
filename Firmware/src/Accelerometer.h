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

#ifndef _DICE_ACCEL_h
#define _DICE_ACCEL_h

#include <stdint.h>

///////////////////////////////////
// LIS2DE12 Register Definitions //
///////////////////////////////////
enum LIS2DE12_Register {
	STATUS_REG_AUX = 0x07,
	OUT_TEMP_L = 0x0C,
	OUT_TEMP_H = 0x0D,
	WHO_AM_I = 0x0F,
	CTRL_REG0 = 0x1E,
	TEMP_CFG_REG = 0x1F,
	CTRL_REG1 = 0x20,
	CTRL_REG2 = 0x21,
	CTRL_REG3 = 0x22,
	CTRL_REG4 = 0x23,
	CTRL_REG5 = 0x24,
	CTRL_REG6 = 0x25,
	REFERENCE = 0x26,
	STATUS_REG = 0x27,
	FIFO_READ_START = 0x28,
	OUT_X_H = 0x29,
	OUT_Y_H = 0x2B,
	OUT_Z_H = 0x2D,
	FIFO_CTRL_REG = 0x2E,
	FIFO_SRC_REG = 0x2F,
	INT1_CFG = 0x30,
	INT1_SRC = 0x31,
	INT1_THS = 0x32,
	INT1_DURATION = 0x33,
	INT2_CFG = 0x34,
	INT2_SRC = 0x35,
	INT2_THS = 0x36,
	INT2_DURATION = 0x37,
	CLICK_CFG = 0x38,
	CLICK_SRC = 0x39,
	CLICK_THS = 0x3A,
	TIME_LIMIT = 0x3B,
	TIME_LATENCY = 0x3C,
	TIME_WINDOW = 0x3D,
	ACT_THS = 0x3E,
	ACT_DUR = 0x3F,
};

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

namespace Devices
{
	/// <summary>
	/// The accelerometer I2C devices
	/// </summary>
	class Accelerometer
	{
	public:
		short x, y, z;
		float cx, cy, cz;
	private:
		uint8_t address;
		LIS2DE12_Scale scale;

	public:
		Accelerometer();
		uint8_t init(LIS2DE12_Scale fsr = SCALE_8G, LIS2DE12_ODR odr = ODR_200);
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

	private:
		void writeRegister(LIS2DE12_Register reg, uint8_t data);
		uint8_t readRegister(LIS2DE12_Register reg);
		void readRegisters(LIS2DE12_Register reg, uint8_t *buffer, uint8_t len);
	};

	extern Accelerometer accelerometer;
}

#endif

