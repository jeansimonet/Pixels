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

#include "Arduino.h"


///////////////////////////////////
// MMA8452Q Register Definitions //
///////////////////////////////////
enum MMA8452Q_Register {
	STATUS_MMA8452Q = 0x00,
	OUT_X_MSB = 0x01,
	OUT_X_LSB = 0x02,
	OUT_Y_MSB = 0x03,
	OUT_Y_LSB = 0x04,
	OUT_Z_MSB = 0x05,
	OUT_Z_LSB = 0x06,
	SYSMOD = 0x0B,
	INT_SOURCE = 0x0C,
	WHO_AM_I = 0x0D,
	XYZ_DATA_CFG = 0x0E,
	HP_FILTER_CUTOFF = 0x0F,
	PL_STATUS = 0x10,
	PL_CFG = 0x11,
	PL_COUNT = 0x12,
	PL_BF_ZCOMP = 0x13,
	P_L_THS_REG = 0x14,
	FF_MT_CFG = 0x15,
	FF_MT_SRC = 0x16,
	FF_MT_THS = 0x17,
	FF_MT_COUNT = 0x18,
	TRANSIENT_CFG = 0x1D,
	TRANSIENT_SRC = 0x1E,
	TRANSIENT_THS = 0x1F,
	TRANSIENT_COUNT = 0x20,
	PULSE_CFG = 0x21,
	PULSE_SRC = 0x22,
	PULSE_THSX = 0x23,
	PULSE_THSY = 0x24,
	PULSE_THSZ = 0x25,
	PULSE_TMLT = 0x26,
	PULSE_LTCY = 0x27,
	PULSE_WIND = 0x28,
	ASLP_COUNT = 0x29,
	CTRL_REG1 = 0x2A,
	CTRL_REG2 = 0x2B,
	CTRL_REG3 = 0x2C,
	CTRL_REG4 = 0x2D,
	CTRL_REG5 = 0x2E,
	OFF_X = 0x2F,
	OFF_Y = 0x30,
	OFF_Z = 0x31
};

////////////////////////////////
// MMA8452Q Misc Declarations //
////////////////////////////////
enum MMA8452Q_Scale { SCALE_2G = 2, SCALE_4G = 4, SCALE_8G = 8 }; // Possible full-scale settings
enum MMA8452Q_ODR { ODR_800, ODR_400, ODR_200, ODR_100, ODR_50, ODR_12, ODR_6, ODR_1 }; // possible data rates

// Possible portrait/landscape settings
#define PORTRAIT_U 0
#define PORTRAIT_D 1
#define LANDSCAPE_R 2
#define LANDSCAPE_L 3
#define LOCKOUT 0x40

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
		byte address;
		MMA8452Q_Scale scale;

	public:
		Accelerometer();
		byte init(MMA8452Q_Scale fsr = SCALE_8G, MMA8452Q_ODR odr = ODR_200);
		void read();
		byte available();
		byte readTap();
		byte readPL();

		float convert(short value);

		void enableTransientInterrupt();
		void clearTransientInterrupt();
		void disableTransientInterrupt();

	private:
		void standby();
		void active();
		void setupPL();
		void setupTap(byte xThs, byte yThs, byte zThs);
		void setScale(MMA8452Q_Scale fsr);
		void setODR(MMA8452Q_ODR odr);
		void writeRegister(MMA8452Q_Register reg, byte data);
		void writeRegisters(MMA8452Q_Register reg, byte *buffer, byte len);
		byte readRegister(MMA8452Q_Register reg);
		void readRegisters(MMA8452Q_Register reg, byte *buffer, byte len);
	};

	extern Accelerometer accelerometer;
}

#endif

