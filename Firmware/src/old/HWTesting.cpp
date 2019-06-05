#include <Arduino.h>
#include "HWTesting.h"
#include "Console.h"
#include "delay.h"
#include "nrf_delay.h"
#include "nrf_drv_twi.h"
#include "I2C.h"
#include "Accelerometer.h"
#include "Debug.h"
#include "Adafruit_DotStar.h"
#include "APA102LEDs.h"
// #include "Accelerometer.h"
// #include "Timer.h"
// #include "LEDs.h"
// #include "AnimController.h"
// #include "Settings.h"
// #include "AnimationSet.h"
#include "Rainbow.h"
#include "nrf_pwr_mgmt.h"

using namespace Devices;
using namespace Systems;

//extern Adafruit_DotStar strip;
#define NUMPIXELS	21
#define POWERPIN	9
#define BATTERY_ANALOG_PIN 2
#define DATAPIN		6
#define CLOCKPIN	5
#define CHARGING_PIN 22
#define MAGNET_PIN 6
#define accelPin 20
#define radioPin 31

#define ACC_BASE_ADDRESS 0x18


/* TWI instance. */
static const nrf_drv_twi_t m_twi = NRF_DRV_TWI_INSTANCE(0);

/**
 * @brief TWI initialization.
 */
void twi_init (void)
{
    const nrf_drv_twi_config_t twi_config = {
       .scl                = 15,
       .sda                = 14,
       .frequency          = NRF_DRV_TWI_FREQ_100K,
       .interrupt_priority = APP_IRQ_PRIORITY_HIGH,
       .clear_bus_init     = false
    };

    nrf_drv_twi_init(&m_twi, &twi_config, NULL, NULL);
    //APP_ERROR_CHECK(err_code);

    nrf_drv_twi_enable(&m_twi);
}

/// <summary>
/// Writes to the serial port
/// </summary>
void Tests::TestDebug()
{
	console.println("Testing Serial port");
	while (true)
	{
		int a = 10;
		float b = 20.0f;
		uint8_t c = 0x55;
		uint16_t d = 0xA5A5;
		console.println("Testing 1");
		console.print("Testing 2: ");
		console.println(a);
		console.print("Testing 3: ");
		console.println(b);
		console.print("Testing 4: ");
		console.println(c, HEX);
		console.println("Testing 5");
		console.print("Testing 6: ");
		console.println(a);
		console.print("Testing 7: ");
		console.println(b);
		console.print("Testing 8: ");
		console.println(c, HEX);
		console.print("Testing 9: 0x");
		console.println(d, HEX);
		console.println(" More text!");
		nrf_delay_ms(1000);
	}
}

void Tests::TestANDGate()
{
	console.println("Testing AND Gate");

}

/// <summary>
/// Repeatedly attempts to write to I2C devices, so you can check that the lines are toggling on and off!
/// </summary>
void Tests::TestI2C()
{
    twi_init();
	//wire.begin();
	while (true)
	{
		console.println("Trying to Read from I2C addresses...");
		bool detected_device = false;
		for (uint8_t address = 1; address <= 127; address++)
		{
		    uint8_t sample_data;

			// wire.beginTransmission(address);
			// wire.write(0x00);
			// uint8_t error = wire.endTransmission();			
			ret_code_t err_code = nrf_drv_twi_rx(&m_twi, address, &sample_data, sizeof(sample_data));
		    //if (error == 0)
			if (err_code == NRF_SUCCESS)
			{
				detected_device = true;
				console.print("TWI device detected at address 0x");
				console.println(address, HEX);
			}
		}

		if (!detected_device)
		{
			console.println("No device was found.");
		}
	}
}

/// <summary>
/// Attempts to read from the Accelerometer repeatedly
/// </summary>
void Tests::TestAcc()
{
	console.println("Initializing I2C...");
	// Initialize I2C
    twi_init();
	//Systems::wire.begin();

	while (true)
	{
		console.println("Trying to read from Accelerometer...");
		uint8_t whoami_add = 0x0F;
		ret_code_t ret = nrf_drv_twi_tx(&m_twi, ACC_BASE_ADDRESS, &whoami_add, sizeof(whoami_add), false);
		if (ret == NRF_SUCCESS)
		{
			uint8_t whoami_val = 0xAA;
			ret = nrf_drv_twi_rx(&m_twi, ACC_BASE_ADDRESS, &whoami_val, sizeof(whoami_val));
			if (ret == NRF_SUCCESS)
			{
				if (whoami_val != 0x33) // WHO_AM_I should always be 0x2A
				{
					console.print("Wrong device id, got 0x");
					console.print(whoami_val, HEX);
					console.println(", expected 0x33");
				}
				else
				{
					console.print("Ok, got 0x");
					console.println(whoami_val, HEX);

				}
			}
			else
			{
				console.println("Error reading register from ACC");
			}
		}
		else
		{
			console.println("Error writing register address to ACC");
		}
		nrf_delay_ms(1000);
	}
}


/// <summary>
/// Attempts to read from the Accelerometer repeatedly
/// </summary>
void Tests::TestAccDice()
{
	console.print("Initializing I2C...");
	wire.begin();
	console.println("Ok...");

	console.print("Initializing accelerometer");
	accelerometer.init();
	console.println("Ok");

	console.println("Trying to read from Accelerometer...");
	while (true)
	{
		uint8_t ctl1 = 0;
		wire.write(ACC_BASE_ADDRESS, CTRL_REG1, true);
		wire.read(ACC_BASE_ADDRESS, &ctl1, 1);

		ctl1 &= 0x0F; // Mask out data rate bits
		ctl1 |= (ODR_10 << 4);

		uint8_t write[2];
		write[0] = CTRL_REG1;
		write[1] = ctl1;
		wire.write(ACC_BASE_ADDRESS, write, 2);
		// wire.write(ACC_BASE_ADDRESS, CTRL_REG1, true);
		// wire.write(ACC_BASE_ADDRESS, ctl1, 1);

		uint8_t ctl1_check = 0;
		wire.write(ACC_BASE_ADDRESS, CTRL_REG1, true);
		wire.read(ACC_BASE_ADDRESS, &ctl1_check, 1);

		if (ctl1 == ctl1_check)
		{
			debugPrintln("Control Register Written Correctly");
		}
		else
		{
			debugPrintln("Control Register NOT Written");
		}
		

		accelerometer.read();
		console.print("x: ");
		console.print(accelerometer.x);
		console.print("  y: ");
		console.print(accelerometer.y);
		console.print("  z: ");
		console.println(accelerometer.z);
		nrf_delay_ms(500);
	}
}


/// <summary>
/// Drive the LEDs Repeatedly
/// </summary>
void Tests::TestLED()
{
	console.println("Trying to Control APA102 LEDs.");
	pinMode(POWERPIN, OUTPUT);
	digitalWrite(POWERPIN, 1);

	strip.begin();
	while (true)
	{
		Rainbow::rainbowAll(5, 5);
		digitalWrite(POWERPIN, 0);
		digitalWrite(DATAPIN, 0);
		digitalWrite(CLOCKPIN, 0);
		nrf_delay_ms(3000);
		digitalWrite(POWERPIN, 1);
	}
}

// /// <summary>
// /// Drive the LEDs Repeatedly
// /// </summary>
// void Tests::TestLEDSlow()
// {
// 	Serial.begin(9600);
// 	Serial.println("Trying to Control APA102 LEDs every 10s");
// 	//Serial.end();

// 	randomSeed(analogRead(4));

// 	Serial.print("Initializing I2C...");
// 	Systems::wire.begin();
// 	Serial.println("Ok");

// 	Serial.print("Initializing accelerometer...");
// 	accelerometer.init();
// 	Serial.println("Ok");

// 	// Set accelerometer interrupt pin as an input!
// 	pinMode(accelPin, INPUT_PULLUP);
// 	pinMode(CHARGING_PIN, INPUT_PULLUP);

// 	strip.begin();
// 	while (true)
// 	{
// 		// Setup interrupt on accelerometer
// 		Serial.print("Setting up accelerometer, and ");
// 		accelerometer.enableTransientInterrupt();

// 		// Prepare to wakeup on matching interrupt pin
// 		Simblee_pinWake(accelPin, LOW);

// 		// Sleep forever
// 		Serial.println("going to sleep for 10s...");
// 		SimbleeBLE_ULPDelay(SECONDS(30));

// 		// If we get here, we either got an accelerometer interrupt, or bluetooth message

// 		// Reset both pinwake flags
// 		bool accWoke = Simblee_pinWoke(accelPin);
// 		if (accWoke)
// 		{
// 			Simblee_resetPinWake(accelPin);
// 			Serial.println("Woken up by acc");
// 		}
// 		else
// 		{
// 			Serial.println("Time out!");
// 		}

// 		// Disable accelerometer interrupts
// 		accelerometer.clearTransientInterrupt();
// 		accelerometer.disableTransientInterrupt();

// 		// Disable pinWake
// 		Simblee_pinWake(accelPin, DISABLE);

// 		pinMode(POWERPIN, OUTPUT);
// 		digitalWrite(POWERPIN, LOW);

// 		// Output a random color on all leds
// 		Rainbow::rainbowCycle(1, 32);

// 		// Go to sleep
// 		digitalWrite(POWERPIN, 1);
// 		digitalWrite(DATAPIN, 0);
// 		digitalWrite(CLOCKPIN, 0);

// 		//if (accWoke)
// 		//{
// 		//	SimbleeBLE_ULPDelay(SECONDS(1));
// 		//}
// 	}
// }

// /// <summary>
// /// Drive the LEDs Repeatedly
// /// </summary>
// void Tests::TestLEDPower()
// {
// 	Serial.begin(9600);
// 	Serial.println("Trying to Control APA102 LEDs Power pin.");
// 	pinMode(POWERPIN, OUTPUT);
// 	while (true)
// 	{
// 		pinMode(POWERPIN, OUTPUT);
// 		digitalWrite(POWERPIN, 0);
// 		delay(1000);
// 		pinMode(POWERPIN, OUTPUT);
// 		digitalWrite(POWERPIN, 1);
// 		delay(1000);
// 		pinMode(POWERPIN, INPUT);
// 		delay(1000);
// 	}
// }

// void Tests::TestLEDDice()
// {
// 	Serial.begin(9600);
// 	Serial.print("Initializing LEDs...");
// 	leds.init();
// 	Serial.println("Ok");

// 	while (true)
// 	{
// 		Serial.println("Increasing brightness from black to white");
// 		for (int b = 0; b < 256; ++b)
// 		{
// 			leds.setAll(b | (b << 8) | (b << 16));
// 			delay(10);
// 		}
// 		for (int b = 255; b >= 0; --b)
// 		{
// 			leds.setAll(b | (b << 8) | (b << 16));
// 			delay(10);
// 		}
// 		Serial.println("Cycling colors");
// 		for (int k = 0; k < 5; ++k)
// 		{
// 			for (int j = 0; j<256; j++)
// 			{
// 				leds.setAll(Rainbow::Wheel(j));
// 				delay(5);
// 			}
// 		}
// 	}
// }

void Tests::TestSleepForever()
{
	console.println("Going to sleep forever now, bye!");

	sd_power_mode_set(NRF_POWER_MODE_LOWPWR);
}


// void Tests::TestSleepAwakeAcc()
// {
// 	Serial.begin(9600);
// 	Serial.print("Initializing I2C...");
// 	Systems::wire.begin();
// 	Serial.println("Ok");

// 	Serial.print("Initializing accelerometer...");
// 	accelerometer.init();
// 	Serial.println("Ok");

// 	// Set accelerometer interrupt pin as an input!
// 	pinMode(accelPin, INPUT_PULLUP);

// 	while (true)
// 	{
// 		// Setup interrupt on accelerometer
// 		Serial.print("Setting up accelerometer, and ");
// 		accelerometer.enableTransientInterrupt();

// 		// Prepare to wakeup on matching interrupt pin
// 		Simblee_pinWake(accelPin, LOW);

// 		// Sleep forever
// 		Serial.println("going to sleep...");
// 		Simblee_ULPDelay(INFINITE);

// 		// If we get here, we either got an accelerometer interrupt, or bluetooth message

// 		// Reset both pinwake flags
// 		if (Simblee_pinWoke(accelPin))
// 			Simblee_resetPinWake(accelPin);

// 		// Disable accelerometer interrupts
// 		accelerometer.clearTransientInterrupt();
// 		accelerometer.disableTransientInterrupt();

// 		// Disable pinWake
// 		Simblee_pinWake(accelPin, DISABLE);

// 		Serial.println("...And I'm back!");
// 		delay(1000);
// 		Serial.print("3...");
// 		delay(1000);
// 		Serial.print("2...");
// 		delay(1000);
// 		Serial.print("1...");
// 		delay(1000);
// 	}
// }

// void Tests::TestSettings()
// {
// 	Serial.begin(9600);

// 	Serial.print("Checking Settings...");
// 	bool ok = settings->CheckValid();
// 	if (ok)
// 	{
// 		Serial.println("Ok");
// 		Serial.print("Dice name: ");
// 		Serial.println(settings->name);
// 	}
// 	else
// 	{
// 		Serial.println("Not initialized");
// 	}

// 	Serial.print("Erasing settings flash page");
// 	if (Settings::EraseSettings())
// 	{
// 		Serial.println("Ok");
// 		Serial.print("Writing some settings...");
// 		Settings settingsToWrite;
// 		strncpy(settingsToWrite.name, "TestingSettings", 16);
// 		if (Settings::TransferSettings(&settingsToWrite))
// 		{
// 			Serial.println("Ok");
// 			Serial.println("Settings content");
// 			Serial.println(settings->headMarker, HEX);
// 			Serial.println(settings->name);
// 			Serial.println(settings->tailMarker, HEX);

// 			Serial.print("Checking settings again...");
// 			ok = settings->CheckValid();
// 			if (ok)
// 			{
// 				Serial.println("Ok");
// 				Serial.print("Dice name: ");
// 				Serial.println(settings->name);
// 			}
// 			else
// 			{
// 				Serial.println("Not initialized");
// 			}
// 		}
// 		else
// 		{
// 			Serial.println("Error writing settings");
// 		}
// 	}
// 	else
// 	{
// 		Serial.println("Error erasing flash");
// 	}
// }

// void Tests::TestAnimationSet()
// {
// 	auto printAnimationSet = []()
// 	{
// 		Serial.print("Set contains ");
// 		Serial.print(animationSet->Count());
// 		Serial.println(" animations");

// 		for (int i = 0; i < animationSet->Count(); ++i)
// 		{
// 			auto anim = animationSet->GetAnimation(i);
// 			Serial.print("Anim ");
// 			Serial.print(i);
// 			Serial.print(" contains ");
// 			Serial.print(anim->TrackCount());
// 			Serial.println(" tracks");

// 			for (int j = 0; j < anim->TrackCount(); ++j)
// 			{
// 				auto& track = anim->GetTrack(j);
// 				Serial.print("Anim ");
// 				Serial.print(i);
// 				Serial.print(", track ");
// 				Serial.print(j);
// 				Serial.print(" has ");
// 				Serial.print(track.count);
// 				Serial.print(" keyframes, starts at ");
// 				Serial.print(track.startTime);
// 				Serial.print(" ms, lasts ");
// 				Serial.print(track.duration);
// 				Serial.print(" ms and controls LED ");
// 				Serial.println(track.ledIndex);

// 				for (int k = 0; k < track.count; ++k)
// 				{
// 					auto& keyframe = track.keyframes[k];
// 					Serial.print("(");
// 					Serial.print(keyframe.time);
// 					Serial.print("ms = ");
// 					Serial.print(keyframe.red);
// 					Serial.print(", ");
// 					Serial.print(keyframe.green);
// 					Serial.print(", ");
// 					Serial.print(keyframe.blue);
// 					Serial.print(") ");
// 				}
// 				Serial.println();
// 			}
// 		}
// 	};

// 	Serial.begin(9600);

// 	Serial.print("Checking AnimationSet...");
// 	bool ok = animationSet->CheckValid();
// 	if (ok)
// 	{
// 		Serial.println("Ok");
// 		printAnimationSet();
// 	}
// 	else
// 	{
// 		Serial.println("Not initialized");
// 	}

// 	// We're going to program a few animations!
// 	// Create them
// 	AnimationTrack updown;
// 	updown.count = 0;
// 	updown.startTime = 0;	// ms
// 	updown.duration = 1000;	// ms
// 	updown.ledIndex = 0;
// 	updown.AddKeyframe(0,0,0,0);
// 	updown.AddKeyframe(128, 255, 255, 255);
// 	updown.AddKeyframe(255, 0, 0, 0);
	
// 	AnimationTrack updownRed;
// 	updownRed.count = 0;
// 	updownRed.startTime = 0;	// ms
// 	updownRed.duration = 333;	// ms
// 	updownRed.ledIndex = 1;
// 	updownRed.AddKeyframe(0, 0, 0, 0);
// 	updownRed.AddKeyframe(50, 255, 0, 0);
// 	updownRed.AddKeyframe(200, 255, 0, 0);
// 	updownRed.AddKeyframe(255, 0, 0, 0);

// 	AnimationTrack updownGreen;
// 	updownGreen.count = 0;
// 	updownGreen.startTime = 333;	// ms
// 	updownGreen.duration = 333;	// ms
// 	updownGreen.ledIndex = 2;
// 	updownGreen.AddKeyframe(0, 0, 0, 0);
// 	updownGreen.AddKeyframe(50, 0, 255, 0);
// 	updownGreen.AddKeyframe(200, 0, 255, 0);
// 	updownGreen.AddKeyframe(255, 0, 0, 0);

// 	AnimationTrack updownBlue;
// 	updownBlue.count = 0;
// 	updownBlue.startTime = 667;	// ms
// 	updownBlue.duration = 333;	// ms
// 	updownBlue.ledIndex = 3;
// 	updownBlue.AddKeyframe(0, 0, 0, 0);
// 	updownBlue.AddKeyframe(50, 0, 0, 255);
// 	updownBlue.AddKeyframe(200, 0, 0, 255);
// 	updownBlue.AddKeyframe(255, 0, 0, 0);

// 	Animation* anim1 = Animation::AllocateAnimation(1);
// 	anim1->SetTrack(updown, 0);

// 	Animation* anim2 = Animation::AllocateAnimation(3);
// 	anim2->SetTrack(updownRed, 0);
// 	anim2->SetTrack(updownGreen, 1);
// 	anim2->SetTrack(updownBlue, 2);

// 	int totalAnimSize = anim1->ComputeByteSize() + anim2->ComputeByteSize();

// 	Serial.print("Erasing animation flash pages...");
// 	AnimationSet::ProgrammingToken token;
// 	if (AnimationSet::EraseAnimations(totalAnimSize, token))
// 	{
// 		Serial.println("Ok");
// 		Serial.print("Writing 2 animation...");
// 		if (AnimationSet::TransferAnimation(anim1, token) && AnimationSet::TransferAnimation(anim2, token))
// 		{
// 			Serial.println("Ok");
// 			Serial.print("Writing animation set...");
// 			if (AnimationSet::TransferAnimationSet(token.animationPtrInFlash, token.currentCount))
// 			{
// 				Serial.println("Ok");

// 				// Clean up memory
// 				free(anim2);
// 				free(anim1);

// 				Serial.print("Checking AnimationSet again...");
// 				if (animationSet->CheckValid())
// 				{
// 					Serial.println("Ok");
// 					printAnimationSet();
// 				}
// 				else
// 				{
// 					Serial.println("Not initialized");
// 				}
// 			}
// 			else
// 			{
// 				Serial.println("Error writing animation set");
// 			}
// 		}
// 		else
// 		{
// 			Serial.println("Error writing animation");
// 		}
// 	}
// 	else
// 	{
// 		Serial.println("Error erasing flash");
// 	}
// }


// void Tests::TestTimerSetup()
// {
// 	Serial.begin(9600);

// 	Serial.print("Setting some callback...");
// 	Systems::timer.hook(100000, [](void* ignore) {Serial.println("Callback!"); }, nullptr);
// 	Serial.println("Ok");

// 	Serial.print("Initializing Timer...");
// 	Systems::timer.begin();
// 	Serial.println("Ok");
// }

// void Tests::TestTimerUpdate()
// {
// 	Systems::timer.update();
// }

// void Tests::TestAnimationsSetup()
// {
// 	Serial.begin(9600);

// 	Serial.print("Initializing LEDs...");
// 	leds.init();
// 	Serial.println("Ok");

// 	if (!animationSet->CheckValid())
// 	{
// 		Serial.println("No animation data, programming some");
// 		TestAnimationSet();
// 	}

// 	if (animationSet->CheckValid())
// 	{
// 		Serial.print("Initializing animation controller...");
// 		animController.begin();
// 		Serial.println("Ok");

// 		Serial.print("Initializing Timer...");
// 		Systems::timer.begin();
// 		Serial.println("Ok");

// 		// Kick off some animations
// 		animController.play(animationSet->GetAnimation(0));
// 		animController.play(animationSet->GetAnimation(1));
// 	}
// }

// void Tests::TestAnimationsUpdate()
// {
// 	Systems::timer.update();
// }


// void Tests::TestBattery()
// {
// 	Serial.begin(9600);

// 	Serial.println("Reading battery voltage...");
// 	while (true)
// 	{
// 		int value = analogRead(BATTERY_ANALOG_PIN);
// 		float voltage = value * 3.3f / 1023.0f; // use calibration value here!
// 		Serial.print(value);
// 		Serial.print(" units, ");
// 		Serial.print(voltage);
// 		Serial.println("v");
// 		delay(1000);
// 	}
// }

// void Tests::TestCharging()
// {
// 	Serial.begin(9600);

// 	Serial.println("Checking charging state...");

// 	pinMode(CHARGING_PIN, INPUT_PULLUP);

// 	bool currentlyCharging = digitalRead(CHARGING_PIN) == LOW;
// 	if (currentlyCharging)
// 		Serial.println("Charging");
// 	else
// 		Serial.println("Not charging");
// 	while (true)
// 	{
// 		bool newCharging = digitalRead(CHARGING_PIN) == LOW;
// 		if (newCharging != currentlyCharging)
// 		{
// 			currentlyCharging = newCharging;
// 			if (currentlyCharging)
// 				Serial.println("Charging");
// 			else
// 				Serial.println("Not charging");
// 		}
// 		delay(100);
// 	}
// }

// /// <summary>
// /// Drive the LEDs Repeatedly
// /// </summary>
// void Tests::TestBatteryDischarge()
// {
// 	Serial.begin(9600);
// 	Serial.println("Trying to Control APA102 LEDs and read battery voltage");
// 	pinMode(POWERPIN, OUTPUT);
// 	digitalWrite(POWERPIN, 0);

// 	strip.begin();
// 	while (true)
// 	{
// 		Rainbow::rainbowCycle(5);
// 		uint32_t value = analogRead(BATTERY_ANALOG_PIN);
// 		float voltage = value * 2 * 3.55f / 1023.0f; // use calibration value here!
// 		Serial.print(voltage, 2);
// 		Serial.println("v");
// 	}
// }

// void Tests::TestAllSystems()
// {
// 	Serial.begin(9600);
// 	Serial.println("Trying to Control APA102 LEDs.");

// 	// Turn leds on
// 	pinMode(POWERPIN, OUTPUT);
// 	digitalWrite(POWERPIN, 0);

// 	strip.begin();
// 	Rainbow::rainbowCycle(5);

// 	// Turn leds off
// 	pinMode(DATAPIN, OUTPUT);
// 	pinMode(CLOCKPIN, OUTPUT);
// 	pinMode(POWERPIN, OUTPUT);
// 	digitalWrite(DATAPIN, 0);
// 	digitalWrite(CLOCKPIN, 0);
// 	digitalWrite(POWERPIN, 1);

// 	Serial.println("Trying to read accelerometer.");

// 	// Initialize I2C
// 	Systems::wire.begin();

// 	for (int i = 0; i < 3; ++i)
// 	{
// 		Serial.println("Trying to read from Accelerometer...");
// 		Systems::wire.beginTransmission(0x1C);
// 		Systems::wire.write(WHO_AM_I);
// 		uint8_t ret = Systems::wire.endTransmission(false); //endTransmission but keep the connection active
// 		switch (ret)
// 		{
// 		case 4:
// 			Serial.println("Unknown error");
// 			break;
// 		case 3:
// 			Serial.println("NACK on Data");
// 			break;
// 		case 2:
// 			Serial.println("NACK on Address");
// 			break;
// 		case 1:
// 			Serial.println("Data too long");
// 			break;
// 		case 0:
// 		{
// 			Systems::wire.requestFrom((uint8_t)0x1C, (byte)1); // Ask for 1 byte, once done, bus is released by default

// 			int start = millis();
// 			bool timeout = false;
// 			while (!Systems::wire.available() && !timeout)
// 			{
// 				timeout = (millis() - start) > 1000;
// 			}

// 			if (timeout)
// 			{
// 				Serial.println("Timeout waiting for data");
// 			}
// 			else
// 			{
// 				byte c = Systems::wire.read(); //Return this one byte
// 				if (c != 0x2A) // WHO_AM_I should always be 0x2A
// 				{
// 					Serial.print("Wrong device id, got ");
// 					Serial.print(c, HEX);
// 					Serial.println(", expected 2A");
// 				}
// 				else
// 				{
// 					Serial.println("Ok");
// 				}
// 			}
// 		}
// 		break;
// 		}
// 	}

// 	// Testing acc interrupt
// 	Serial.print("Testing interrupt pin");
// 	Serial.print("Initializing accelerometer...");
// 	accelerometer.init();
// 	Serial.println("Ok");

// 	// Set accelerometer interrupt pin as an input!
// 	pinMode(accelPin, INPUT_PULLUP);

// 	for (int i = 0; i < 2; ++i)
// 	{
// 		// Setup interrupt on accelerometer
// 		Serial.print("Setting up accelerometer, and ");
// 		accelerometer.enableTransientInterrupt();

// 		// Prepare to wakeup on matching interrupt pin
// 		Simblee_pinWake(accelPin, LOW);

// 		// Sleep forever
// 		Serial.println("going to sleep...");
// 		Serial.println("Check that current draw is minimal!");
// 		Simblee_ULPDelay(INFINITE);

// 		// If we get here, we either got an accelerometer interrupt, or bluetooth message

// 		// Reset both pinwake flags
// 		if (Simblee_pinWoke(accelPin))
// 			Simblee_resetPinWake(accelPin);

// 		// Disable accelerometer interrupts
// 		accelerometer.clearTransientInterrupt();
// 		accelerometer.disableTransientInterrupt();

// 		// Disable pinWake
// 		Simblee_pinWake(accelPin, DISABLE);

// 		Serial.println("...And I'm back!");
// 		delay(1000);
// 		Serial.print("Zzzz...");
// 		delay(1000);
// 	}

// 	Serial.println("Checking charging state...");

// 	pinMode(CHARGING_PIN, INPUT_PULLUP);

// 	bool currentlyCharging = digitalRead(CHARGING_PIN) == LOW;
// 	if (currentlyCharging)
// 		Serial.println("Charging");
// 	else
// 		Serial.println("Not charging");

// 	Serial.println("Place on charger");
// 	while (!currentlyCharging)
// 	{
// 		bool newCharging = digitalRead(CHARGING_PIN) == LOW;
// 		if (newCharging != currentlyCharging)
// 		{
// 			currentlyCharging = newCharging;
// 			if (currentlyCharging)
// 				Serial.println("Charging");
// 			else
// 				Serial.println("Not charging");
// 		}
// 		delay(100);
// 	}
// 	Serial.println("Remove from charger");
// 	while (currentlyCharging)
// 	{
// 		bool newCharging = digitalRead(CHARGING_PIN) == LOW;
// 		if (newCharging != currentlyCharging)
// 		{
// 			currentlyCharging = newCharging;
// 			if (currentlyCharging)
// 				Serial.println("Charging");
// 			else
// 				Serial.println("Not charging");
// 		}
// 		delay(100);
// 	}


// 	Serial.println("Checking magnet state...");

// 	pinMode(MAGNET_PIN, INPUT_PULLUP);

// 	bool currentMagnet = digitalRead(MAGNET_PIN) == LOW;
// 	if (currentMagnet)
// 		Serial.println("MAGNET");
// 	else
// 		Serial.println("NO MAGNET");

// 	Serial.println("Place Magnet");
// 	while (!currentMagnet)
// 	{
// 		bool newMagnet = digitalRead(MAGNET_PIN) == LOW;
// 		if (newMagnet != currentMagnet)
// 		{
// 			currentMagnet = newMagnet;
// 			if (currentMagnet)
// 				Serial.println("MAGNET");
// 			else
// 				Serial.println("NO MAGNET");
// 		}
// 		delay(100);
// 	}
// 	Serial.println("Remove Magnet");
// 	while (currentMagnet)
// 	{
// 		bool newMagnet = digitalRead(MAGNET_PIN) == LOW;
// 		if (newMagnet != currentMagnet)
// 		{
// 			currentMagnet = newMagnet;
// 			if (currentMagnet)
// 				Serial.println("MAGNET");
// 			else
// 				Serial.println("NO MAGNET");
// 		}
// 		delay(100);
// 	}

// 	Serial.println("All Tests Passed!");

// }


// void Tests::TestMagnet()
// {
// 	Serial.begin(9600);
// 	Serial.println("Trying to Control APA102 LEDs.");

// 	pinMode(MAGNET_PIN, INPUT);

// 	bool currentlyCharging = digitalRead(MAGNET_PIN) == LOW;
// 	while(true)
// 	{
// 		bool newCharging = digitalRead(MAGNET_PIN) == LOW;
// 		if (newCharging != currentlyCharging)
// 		{
// 			currentlyCharging = newCharging;
// 			if (currentlyCharging)
// 				Serial.println("Magnet present");
// 			else
// 				Serial.println("Magnet not preset");
// 		}
// 	}

// }

// void Tests::TestConnections()
// {
// 	Serial.begin(9600);
// 	Serial.println("Toggling all pins");

// 	pinMode(POWERPIN, OUTPUT);
// 	pinMode(DATAPIN, OUTPUT);
// 	pinMode(CLOCKPIN, OUTPUT);
// 	pinMode(CHARGING_PIN, OUTPUT);
// 	pinMode(MAGNET_PIN, OUTPUT);
// 	pinMode(accelPin, OUTPUT);
// 	pinMode(SCLpin, OUTPUT);
// 	pinMode(SDApin, OUTPUT);

// 	while (true)
// 	{
// 		digitalWrite(POWERPIN, LOW);
// 		digitalWrite(DATAPIN, LOW);
// 		digitalWrite(CLOCKPIN, LOW);
// 		digitalWrite(CHARGING_PIN, LOW);
// 		digitalWrite(MAGNET_PIN, LOW);
// 		digitalWrite(accelPin, LOW);
// 		digitalWrite(SCLpin, LOW);
// 		digitalWrite(SDApin, LOW);
// 		delay(1);
// 		digitalWrite(POWERPIN, HIGH);
// 		digitalWrite(DATAPIN, HIGH);
// 		digitalWrite(CLOCKPIN, HIGH);
// 		digitalWrite(CHARGING_PIN, HIGH);
// 		digitalWrite(MAGNET_PIN, HIGH);
// 		digitalWrite(accelPin, HIGH);
// 		digitalWrite(SCLpin, HIGH);
// 		digitalWrite(SDApin, HIGH);
// 		delay(1);
// 	}
// }
