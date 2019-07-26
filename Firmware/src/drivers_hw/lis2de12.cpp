/******************************************************************************

Modified by Jean Simonet, Systemic Games

******************************************************************************/

/******************************************************************************
SparkFun_MMA8452Q.cpp
SparkFun_MMA8452Q Library Source File
Jim Lindblom @ SparkFun Electronics
Original Creation Date: June 3, 2014
https://github.com/sparkfun/MMA8452_LIS2DE12

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

#include "lis2de12.h"
#include "drivers_nrf/i2c.h"
#include "nrf_log.h"
#include "../drivers_nrf/log.h"
#include "../drivers_nrf/power_manager.h"
#include "../drivers_nrf/timers.h"
#include "nrf_gpio.h"
#include "../config/board_config.h"
#include "../drivers_nrf/gpiote.h"

using namespace DriversNRF;
using namespace Config;

#define DEV_ADDRESS 0x18

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

namespace DriversHW
{
namespace LIS2DE12
{
	short x, y, z;
	float cx, cy, cz;

	LIS2DE12_Scale scale;

	void writeRegister(LIS2DE12_Register reg, uint8_t data);
	uint8_t readRegister(LIS2DE12_Register reg);
	void readRegisters(LIS2DE12_Register reg, uint8_t *buffer, uint8_t len);

	/// <summary>
	///	This function initializes the LIS2DE12. It sets up the scale (either 2, 4,
	///	or 8g), output data rate, portrait/landscape detection and tap detection.
	///	It also checks the WHO_AM_I register to make sure we can communicate with
	///	the sensor. Returns a 0 if communication failed, 1 if successful.
	/// </summary>
	/// <param name="fsr"></param>
	/// <param name="odr"></param>
	/// <returns></returns>
	void init(LIS2DE12_Scale fsr, LIS2DE12_ODR odr)
	{
		scale = fsr; // Haul fsr into our class variable, scale

		uint8_t c = readRegister(WHO_AM_I);  // Read WHO_AM_I register

		if (c != 0x33) // WHO_AM_I should always be 0x2A
		{
			NRF_LOG_ERROR("Bad WHOAMI");
			return;
		}

		standby();  // Must be in standby to change registers

		setScale(scale);  // Set up accelerometer scale
		setODR(odr);  // Set up output data rate
		active();  // Set to active to start reading

		// Make sure our interrupts are cleared to begin with!
		disableTransientInterrupt();
		clearTransientInterrupt();

		if (!checkIntPin())
		{
			NRF_LOG_ERROR("Bad interrupt Pin");
			return;
		}

		#if DICE_SELFTEST && LIS2DE12_SELFTEST
		selfTest();
		#endif
		#if DICE_SELFTEST && LIS2DE12_SELFTEST_INT
		selfTestInterrupt();
		#endif
		NRF_LOG_INFO("LIS2DE12 Initialized");
	}

	// Helper method to convert register readings to signed integers
	short  twosComplement(uint8_t registerValue) {
		// If a positive value, return it
		if ((registerValue & 0x80) == 0) {
			return registerValue;
		} else {
			// Otherwise perform the 2's complement math on the value
			uint8_t comp = ~(registerValue - 0x01);
			return (short)comp * -1;
		}
	}

	float getScaleMult() {
		float scaleMult = -1.0f;
		switch (scale) {
			case SCALE_2G:
				scaleMult = 2.0f;
				break;
			case SCALE_4G:
				scaleMult = 4.0f;
				break;
			case SCALE_8G:
				scaleMult = 8.0f;
				break;
			case SCALE_16G:
				scaleMult = 16.0f;
				break;
		}
		return scaleMult;
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
	void read()
	{
		x = twosComplement(readRegister(OUT_X_H));
		y = twosComplement(readRegister(OUT_Y_H));
		z = twosComplement(readRegister(OUT_Z_H));
		float scaleMult = getScaleMult();
		cx = (float)x / (float)(1 << 7) * scaleMult;
		cy = (float)y / (float)(1 << 7) * scaleMult;
		cz = (float)z / (float)(1 << 7) * scaleMult;
	}

	float convert(short value)
	{
		int a = twosComplement(readRegister(OUT_X_H));
		float scaleMult = getScaleMult();
		return (float)a / (float)(1 << 7) * scaleMult;
	}

	/// <summary>
	/// CHECK IF NEW DATA IS AVAILABLE
	///	This function checks the status of the MMA8452Q to see if new data is availble.
	///	returns 0 if no new data is present, or a 1 if new data is available.
	/// </summary>
	uint8_t available()
	{
		return (readRegister(FIFO_SRC_REG) & 0x1F);
	}

	/// <summary>
	/// SET FULL-SCALE RANGE
	///	This function sets the full-scale range of the x, y, and z axis accelerometers.
	/// </summary>
	/// <param name="fsr">
	///	Possible values for the fsr variable are SCALE_2G, SCALE_4G, or SCALE_8G.
	/// </param>
	void setScale(LIS2DE12_Scale fsr)
	{
		// Must be in standby mode to make changes!!!
		uint8_t cfg = readRegister(CTRL_REG4);
		cfg &= 0b11001111; // Mask out scale bits
		cfg |= (fsr << 4);
		writeRegister(CTRL_REG4, cfg);
	}

	/// <summary>
	/// SET THE OUTPUT DATA RATE
	/// </summary>
	void setODR(LIS2DE12_ODR odr)
	{
		// Must be in standby mode to make changes!!!
		uint8_t ctrl = readRegister(CTRL_REG1);
		ctrl &= 0x0F; // Mask out data rate bits
		ctrl |= (odr << 4);
		writeRegister(CTRL_REG1, ctrl);
	}

	/// <summary>
	/// ENABLE INTERRUPT ON TRANSIENT MOTION DETECTION
	/// This function sets up the MMA8452Q to trigger an interrupt on pin 1
	/// when it detects any motion (lowest detectable threshold).
	/// </summary>
	void enableTransientInterrupt()
	{
		standby();

		// Enable OR of acceleration interrupt on any axis
		writeRegister(INT1_CFG, 0b00101010);

		// Setup the high-pass filter
		//writeRegister(CTRL_REG2, 0b00110001);
		writeRegister(CTRL_REG2, 0b00000000);

		// Setup the threshold
		writeRegister(INT1_THS, 32);

		// Setup the duration to minimum
		writeRegister(INT1_DURATION, 1);

		// Enable interrupt on xyz axes
		writeRegister(CTRL_REG3, 0b01000000);

		active();
	}

	/// <summary>
	/// CLEARS TRANSIENT INTERRUPT
	/// This function will 'aknowledge' the transient interrupt from the device
	/// </summary>
	void clearTransientInterrupt()
	{
		standby();
		readRegister(INT1_SRC);
		// maybe log to console->..
		active();
	}

	/// <summary>
	/// DISABLE TRANSIENT INTERRUPT
	/// </summary>
	void disableTransientInterrupt()
	{
		standby();
		// Disable interrupt on xyz axes
		writeRegister(CTRL_REG3, 0b00000000);
		active();
	}

	/// <summary>
	/// SET STANDBY MODE
	///	Sets the MMA8452 to standby mode. It must be in standby to change most register settings
	/// </summary>
	void standby()
	{
		uint8_t c = readRegister(CTRL_REG1);
		writeRegister(CTRL_REG1, c & ~(0x08)); //Clear the active bit to go into standby
	}

	/// <summary>
	/// SET ACTIVE MODE
	///	Sets the MMA8452 to active mode. Needs to be in this mode to output data
	/// </summary>
	void active()
	{
		uint8_t c = readRegister(CTRL_REG1);
		writeRegister(CTRL_REG1, c | 0x08); //Set the active bit to begin detection
	}

	void powerDown() {
		writeRegister(CTRL_REG1, 0b00001000); //Set the active bit to begin detection
	}

	/// <summary>
	/// WRITE A SINGLE REGISTER
	/// 	Write a single uint8_t of data to a register in the MMA8452Q.
	/// </summary>
	void writeRegister(LIS2DE12_Register reg, uint8_t data)
	{
		uint8_t write[2];
		write[0] = reg;
		write[1] = data;
		I2C::write(DEV_ADDRESS, write, 2);
	}

	/// <summary>
	/// READ A SINGLE REGISTER
	///	Read a uint8_t from the MMA8452Q register "reg".
	/// </summary>
	uint8_t readRegister(LIS2DE12_Register reg)
	{
		I2C::write(DEV_ADDRESS, reg, true);
		uint8_t ret = 0;
		I2C::read(DEV_ADDRESS, &ret, 1);
		return ret;
	}

	/// <summary>
	/// READ MULTIPLE REGISTERS
	///	Read "len" bytes from the MMA8452Q, starting at register "reg". Bytes are stored
	///	in "buffer" on exit.
	/// </summary>
	void readRegisters(LIS2DE12_Register reg, uint8_t *buffer, uint8_t len)
	{
		I2C::write(DEV_ADDRESS & 0x8000, reg, true);
		I2C::read(DEV_ADDRESS, buffer, len);
	}

	bool checkWhoAMI() {
		uint8_t c = readRegister(WHO_AM_I);  // Read WHO_AM_I register
		return c == 0x33;
	}

	bool checkIntPin() {
        nrf_gpio_cfg_input(BoardManager::getBoard()->accInterruptPin, NRF_GPIO_PIN_NOPULL);
        bool ret = nrf_gpio_pin_read(BoardManager::getBoard()->accInterruptPin) == 0;
        nrf_gpio_cfg_default(BoardManager::getBoard()->accInterruptPin);
		return ret;
	}

	#if DICE_SELFTEST && LIS2DE12_SELFTEST
    APP_TIMER_DEF(readAccTimer);
    void readAcc(void* context) {
		read();
        NRF_LOG_INFO("x=%d, cx=" NRF_LOG_FLOAT_MARKER, x, NRF_LOG_FLOAT(cx));
        NRF_LOG_INFO("y=%d, cy=" NRF_LOG_FLOAT_MARKER, y, NRF_LOG_FLOAT(cy));
        NRF_LOG_INFO("z=%d, cz=" NRF_LOG_FLOAT_MARKER, z, NRF_LOG_FLOAT(cz));
    }

    void selfTest() {
        Timers::createTimer(&readAccTimer, APP_TIMER_MODE_REPEATED, readAcc);
        NRF_LOG_INFO("Reading Acc, press any key to abort");
        Log::process();

        Timers::startTimer(readAccTimer, 1000, nullptr);
        while (!Log::hasKey()) {
            Log::process();
			PowerManager::feed();
            PowerManager::update();
        }
		Log::getKey();
        NRF_LOG_INFO("Stopping to read acc!");
        Timers::stopTimer(readAccTimer);
        Log::process();
    }
	#endif

	#if DICE_SELFTEST && LIS2DE12_SELFTEST_INT
	bool interruptTriggered = false;
	void accInterruptHandler(uint32_t pin, nrf_gpiote_polarity_t action) {
		// pin and action don't matter
		interruptTriggered = true;
	}

    void selfTestInterrupt() {
        NRF_LOG_INFO("Setting accelerator to trigger interrupt");

		// Set interrupt pin
		GPIOTE::enableInterrupt(
			BoardManager::getBoard()->accInterruptPin,
			NRF_GPIO_PIN_NOPULL,
			NRF_GPIOTE_POLARITY_LOTOHI,
			accInterruptHandler);

		enableTransientInterrupt();
        Log::process();
        while (!interruptTriggered) {
            Log::process();
			PowerManager::feed();
            PowerManager::update();
        }
        NRF_LOG_INFO("Interrupt triggered!");
        Log::process();
    }
	#endif

}
}

