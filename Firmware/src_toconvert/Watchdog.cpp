// 
// 
// 

#include "Watchdog.h"
#include "Die.h"
#include "Debug.h"
#include "SimbleeBLE.h"

using namespace Systems;

Watchdog Systems::watchdog;

#define CHARGING_PIN 22
#define MAGNET_PIN 6
#define WATCHDOG_TIMEOUT 30 // seconds

Watchdog::Watchdog()
{

}

void Watchdog::sleepUntilInterrupt(uint32_t pin)
{
	// Turn off bluetooth
	//SimbleeBLE.end();

	// Prepare to wakeup on matching interrupt pin
	Simblee_pinWake(pin, HIGH);

	// Sleep until someboby pokes us!
	Simblee_ULPDelay(INFINITE);

	// If we get here, we either got an accelerometer interrupt, or bluetooth message
	// Reset both pinwake flags
	if (Simblee_pinWoke(pin))
	{
		Simblee_resetPinWake(pin);
		debugPrintln("Watchdog: waking up");
	}

	///SimbleeBLE.begin();
}

void Watchdog::init()
{
	// Setup hardware watchdog
	NRF_WDT->CONFIG = (WDT_CONFIG_HALT_Pause << WDT_CONFIG_HALT_Pos) | (WDT_CONFIG_SLEEP_Pause << WDT_CONFIG_SLEEP_Pos);
	NRF_WDT->CRV = WATCHDOG_TIMEOUT * 32768; // timout in seconds
	NRF_WDT->RREN |= WDT_RREN_RR0_Msk; //Enable reload register 0
	NRF_WDT->TASKS_START = 1;

	pinMode(MAGNET_PIN, INPUT);
	//pinMode(CHARGING_PIN, INPUT_PULLUP);

	if (digitalRead(MAGNET_PIN) == LOW)
	{
		// We just woke up and the magnet is present, this means we should sleep!
		debugPrintln("Magnet present on init, sleeping...");
		sleepUntilInterrupt(MAGNET_PIN);
	}

	//if (digitalRead(CHARGING_PIN) == LOW)
	//{
	//	// We just woke up and the magnet is present, this means we should sleep!
	//	debugPrintln("Charging, sleeping...");
	//	sleepUntilInterrupt(CHARGING_PIN);
	//}

	// Request to be updated every 'frame'
	die.RegisterUpdate(this, [](void* token) {((Watchdog*)token)->update(); });
}

bool prevmagnet = false;

void Watchdog::update()
{
	// Reset watchdog counter
	NRF_WDT->RR[0] = WDT_RR_RR_Reload; //Reload watchdog register 0

	bool magnet = digitalRead(MAGNET_PIN) == LOW;
	if (magnet)
		Simblee_systemReset();
}

void Watchdog::stop()
{
}

// pass the pin number so you can use the same interrupt handler for all pins
// return 0 to reset wake_detect bit (and not exit Simblee_ULPDelay)
// return 1 to leave wake_detect bit alone (and exit Simblee_ULPDelay)
void Watchdog::MagnetInterrupt()
{
	Simblee_systemReset();
}
