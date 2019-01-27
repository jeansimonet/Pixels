/******************************************************************************

Modified by Jean Simonet, Systemic Games

******************************************************************************/

/******************************************************************************
SparkFun_MMA8452Q.cpp
SparkFun_MMA8452Q Library Source File
Jim Lindblom @ SparkFun Electronics
Original Creation Date: June 3, 2014
https://github.com/sparkfun/MMA8452_Accelerometer

This file implements all functions of the MMA8452Q class. Functions here range
from higher level stuff, like reading/writing MMA8452Q registers to low-level,
hardware I2C reads and writes.

Development environment specifics:
IDE: Arduino 1.0.5
Hardware Platform: Arduino Uno

**Updated for Arduino 1.6.4 5/2015**

This code is beerware; if you see me (or any other SparkFun employee) at the
local, and you've found our code helpful, please buy us a round!

Distributed as-is; no warranty is given.
******************************************************************************/

#include <Arduino.h>
#include "Accelerometer.h"
#include "I2C.h"

using namespace Devices;
using namespace Systems;

Accelerometer Devices::accelerometer;

#define DEV_ADDRESS 0x1C

// CONSTRUCTOR
Accelerometer::Accelerometer()
{
	address = DEV_ADDRESS; // Store address into private variable
}

/// <summary>
///	This function initializes the MMA8452Q. It sets up the scale (either 2, 4,
///	or 8g), output data rate, portrait/landscape detection and tap detection.
///	It also checks the WHO_AM_I register to make sure we can communicate with
///	the sensor. Returns a 0 if communication failed, 1 if successful.
/// </summary>
/// <param name="fsr"></param>
/// <param name="odr"></param>
/// <returns></returns>
byte Accelerometer::init(MMA8452Q_Scale fsr, MMA8452Q_ODR odr)
{
	scale = fsr; // Haul fsr into our class variable, scale

	byte c = readRegister(WHO_AM_I);  // Read WHO_AM_I register

	if (c != 0x2A) // WHO_AM_I should always be 0x2A
	{
		return 0;
	}

	standby();  // Must be in standby to change registers

	setScale(scale);  // Set up accelerometer scale
	setODR(odr);  // Set up output data rate
	setupPL();  // Set up portrait/landscape detection
				// Multiply parameter by 0.0625g to calculate threshold.
	setupTap(0x80, 0x80, 0x08); // Disable x, y, set z to 0.5g

	active();  // Set to active to start reading

	return 1;
}

/// <summary>
/// READ ACCELERATION DATA
///  This function will read the acceleration values from the MMA8452Q. After
///	reading, it will update two triplets of variables:
///		* int's x, y, and z will store the signed 12-bit values read out
///		  of the acceleromter.
///		* floats cx, cy, and cz will store the calculated acceleration from
///		  those 12-bit values. These variables are in units of g's.
/// </summary>
void Accelerometer::read()
{
	byte rawData[6];  // x/y/z accel register data stored here

	readRegisters(OUT_X_MSB, rawData, 6);  // Read the six raw data registers into data array

	x = ((short)(rawData[0] << 8 | rawData[1])) >> 4;
	y = ((short)(rawData[2] << 8 | rawData[3])) >> 4;
	z = ((short)(rawData[4] << 8 | rawData[5])) >> 4;
	cx = (float)x / (float)(1 << 11) * (float)(scale);
	cy = (float)y / (float)(1 << 11) * (float)(scale);
	cz = (float)z / (float)(1 << 11) * (float)(scale);
}

/// <summary>
/// Helper that converts a raw reading value to a float
/// </summary>
/// <param name="value"></param>
/// <returns></returns>
float Accelerometer::convert(short value)
{
	return (float)value / (float)(1 << 11) * (float)(scale);
}

/// <summary>
/// CHECK IF NEW DATA IS AVAILABLE
///	This function checks the status of the MMA8452Q to see if new data is availble.
///	returns 0 if no new data is present, or a 1 if new data is available.
/// </summary>
byte Accelerometer::available()
{
	return (readRegister(STATUS_MMA8452Q) & 0x08) >> 3;
}

/// <summary>
/// SET FULL-SCALE RANGE
///	This function sets the full-scale range of the x, y, and z axis accelerometers.
/// </summary>
/// <param name="fsr">
///	Possible values for the fsr variable are SCALE_2G, SCALE_4G, or SCALE_8G.
/// </param>
void Accelerometer::setScale(MMA8452Q_Scale fsr)
{
	// Must be in standby mode to make changes!!!
	byte cfg = readRegister(XYZ_DATA_CFG);
	cfg &= 0xFC; // Mask out scale bits
	cfg |= (fsr >> 2);  // Neat trick, see page 22. 00 = 2G, 01 = 4A, 10 = 8G
	writeRegister(XYZ_DATA_CFG, cfg);
}

/// <summary>
/// SET THE OUTPUT DATA RATE
///	This function sets the output data rate of the MMA8452Q.
/// </summary>
/// <param name="odr">
///	Possible values for the odr parameter are: ODR_800, ODR_400, ODR_200, 
///	ODR_100, ODR_50, ODR_12, ODR_6, or ODR_1
/// </param>
void Accelerometer::setODR(MMA8452Q_ODR odr)
{
	// Must be in standby mode to make changes!!!
	byte ctrl = readRegister(CTRL_REG1);
	ctrl &= 0xC7; // Mask out data rate bits
	ctrl |= (odr << 3);
	writeRegister(CTRL_REG1, ctrl);
}

/// <summary>
/// SET UP TAP DETECTION
///	This function can set up tap detection on the x, y, and/or z axes.
///	The xThs, yThs, and zThs parameters serve two functions:
///		1. Enable tap detection on an axis. If the 7th bit is SET (0x80)
///			tap detection on that axis will be DISABLED.
///		2. Set tap g's threshold. The lower 7 bits will set the tap threshold
///			on that axis.
/// </summary>
void Accelerometer::setupTap(byte xThs, byte yThs, byte zThs)
{
	// Set up single and double tap - 5 steps:
	// for more info check out this app note:
	// http://cache.freescale.com/files/sensors/doc/app_note/AN4072.pdf
	// Set the threshold - minimum required acceleration to cause a tap.
	byte temp = 0;
	if (!(xThs & 0x80)) // If top bit ISN'T set
	{
		temp |= 0x3; // Enable taps on x
		writeRegister(PULSE_THSX, xThs);  // x thresh
	}
	if (!(yThs & 0x80))
	{
		temp |= 0xC; // Enable taps on y
		writeRegister(PULSE_THSY, yThs);  // y thresh
	}
	if (!(zThs & 0x80))
	{
		temp |= 0x30; // Enable taps on z
		writeRegister(PULSE_THSZ, zThs);  // z thresh
	}
	// Set up single and/or double tap detection on each axis individually.
	writeRegister(PULSE_CFG, temp | 0x40);
	// Set the time limit - the maximum time that a tap can be above the thresh
	writeRegister(PULSE_TMLT, 0x30);  // 30ms time limit at 800Hz odr
									  // Set the pulse latency - the minimum required time between pulses
	writeRegister(PULSE_LTCY, 0xA0);  // 200ms (at 800Hz odr) between taps min
									  // Set the second pulse window - maximum allowed time between end of
									  //	latency and start of second pulse
	writeRegister(PULSE_WIND, 0xFF);  // 5. 318ms (max value) between taps max
}

/// <summary>
/// READ TAP STATUS
///	This function returns any taps read by the MMA8452Q. If the function 
///	returns no new taps were detected. Otherwise the function will return the
///	lower 7 bits of the PULSE_SRC register.
/// </summary>
byte Accelerometer::readTap()
{
	byte tapStat = readRegister(PULSE_SRC);
	if (tapStat & 0x80) // Read EA bit to check if a interrupt was generated
	{
		return tapStat & 0x7F;
	}
	else
		return 0;
}

/// <summary>
/// SET UP PORTRAIT/LANDSCAPE DETECTION
///	This function sets up portrait and landscape detection.
/// </summary>
void Accelerometer::setupPL()
{
	// Must be in standby mode to make changes!!!
	// For more info check out this app note:
	//	http://cache.freescale.com/files/sensors/doc/app_note/AN4068.pdf
	// 1. Enable P/L
	writeRegister(PL_CFG, readRegister(PL_CFG) | 0x40); // Set PL_EN (enable)
														// 2. Set the debounce rate
	writeRegister(PL_COUNT, 0x50);  // Debounce counter at 100ms (at 800 hz)
}

/// <summary>
/// READ PORTRAIT/LANDSCAPE STATUS
///	This function reads the portrait/landscape status register of the MMA8452Q.
///	It will return either PORTRAIT_U, PORTRAIT_D, LANDSCAPE_R, LANDSCAPE_L,
///	or LOCKOUT. LOCKOUT indicates that the sensor is in neither p or ls.
/// </summary>
byte Accelerometer::readPL()
{
	byte plStat = readRegister(PL_STATUS);

	if (plStat & 0x40) // Z-tilt lockout
		return LOCKOUT;
	else // Otherwise return LAPO status
		return (plStat & 0x6) >> 1;
}

/// <summary>
/// ENABLE INTERRUPT ON TRANSIENT MOTION DETECTION
/// This function sets up the MMA8452Q to trigger an interrupt on pin 1
/// when it detects any motion (lowest detectable threshold).
/// </summary>
void Accelerometer::enableTransientInterrupt()
{
	standby();

	// Tell the accelerometer that we want transient interrupts!
	writeRegister(TRANSIENT_CFG, 0b00011110); // enable latch, xyz and hi-pass filter

	// Setup the threshold
	writeRegister(TRANSIENT_THS, 16); // Minimum threshold

	// Set detection count
	writeRegister(TRANSIENT_COUNT, 1); // Shortest detection period

	// Route the transient interrupt to interrupt pin 1
	writeRegister(CTRL_REG5, 0b00100000);

	// Enable the transient interrupt
	writeRegister(CTRL_REG4, 0b00100000);

	active();
}

/// <summary>
/// CLEARS TRANSIENT INTERRUPT
/// This function will 'aknowledge' the transient interrupt from the device
/// </summary>
void Accelerometer::clearTransientInterrupt()
{
	standby();
	uint8_t dontCare = readRegister(TRANSIENT_SRC);
	// maybe log to console->..
	active();
}

/// <summary>
/// DISABLE TRANSIENT INTERRUPT
/// </summary>
void Accelerometer::disableTransientInterrupt()
{
	standby();

	writeRegister(TRANSIENT_CFG, 0b000000000);

	active();
}



/// <summary>
/// SET STANDBY MODE
///	Sets the MMA8452 to standby mode. It must be in standby to change most register settings
/// </summary>
void Accelerometer::standby()
{
	byte c = readRegister(CTRL_REG1);
	writeRegister(CTRL_REG1, c & ~(0x01)); //Clear the active bit to go into standby
}

/// <summary>
/// SET ACTIVE MODE
///	Sets the MMA8452 to active mode. Needs to be in this mode to output data
/// </summary>
void Accelerometer::active()
{
	byte c = readRegister(CTRL_REG1);
	writeRegister(CTRL_REG1, c | 0x01); //Set the active bit to begin detection
}

/// <summary>
/// WRITE A SINGLE REGISTER
/// 	Write a single byte of data to a register in the MMA8452Q.
/// </summary>
void Accelerometer::writeRegister(MMA8452Q_Register reg, byte data)
{
	writeRegisters(reg, &data, 1);
}

/// <summary>
/// WRITE MULTIPLE REGISTERS
///	Write an array of "len" bytes ("buffer"), starting at register "reg", and
///	auto-incrmenting to the next.
/// </summary>
void Accelerometer::writeRegisters(MMA8452Q_Register reg, byte *buffer, byte len)
{
	wire.beginTransmission(address);
	wire.write(reg);
	for (int x = 0; x < len; x++)
		wire.write(buffer[x]);
	wire.endTransmission(); //Stop transmitting
}

/// <summary>
/// READ A SINGLE REGISTER
///	Read a byte from the MMA8452Q register "reg".
/// </summary>
byte Accelerometer::readRegister(MMA8452Q_Register reg)
{
	wire.beginTransmission(address);
	wire.write(reg);
	wire.endTransmission(false); //endTransmission but keep the connection active

	wire.requestFrom(address, (byte)1); //Ask for 1 byte, once done, bus is released by default

	while (!wire.available()); //Wait for the data to come back

	return wire.read(); //Return this one byte
}

/// <summary>
/// READ MULTIPLE REGISTERS
///	Read "len" bytes from the MMA8452Q, starting at register "reg". Bytes are stored
///	in "buffer" on exit.
/// </summary>
void Accelerometer::readRegisters(MMA8452Q_Register reg, byte *buffer, byte len)
{
	wire.beginTransmission(address);
	wire.write(reg);
	wire.endTransmission(false); //endTransmission but keep the connection active

	wire.requestFrom(address, len); //Ask for bytes, once done, bus is released by default

	while (wire.available() < len); //Hang out until we get the # of bytes we expect

	for (int x = 0; x < len; x++)
		buffer[x] = wire.read();
}
