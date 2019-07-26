#include "hardware_test.h"

#include "drivers_hw/lis2de12.h"
#include "utils/utils.h"
#include "core/ring_buffer.h"
#include "config/board_config.h"
#include "config/settings.h"
#include "app_timer.h"
#include "app_error.h"
#include "nrf_log.h"
#include "config/settings.h"
#include "bluetooth/bluetooth_message_service.h"
#include "bluetooth/bluetooth_messages.h"
#include "nrf_gpio.h"
#include "../drivers_nrf/gpiote.h"
#include "drivers_nrf/gpiote.h"
#include "drivers_nrf/scheduler.h"
#include "drivers_nrf/timers.h"
#include "drivers_hw/battery.h"
#include "modules/anim_controller.h"
#include "modules/battery_controller.h"
#include "animations/animation_set.h"

using namespace Modules;
using namespace Core;
using namespace DriversHW;
using namespace DriversNRF;
using namespace Config;
using namespace Bluetooth;
using namespace Animations;


namespace Modules
{
namespace HardwareTest
{
    void HardwareTestHandler(void* context, const Message* msg);

    void init() {
        MessageService::RegisterMessageHandler(Message::MessageType_TestHardware, nullptr, HardwareTestHandler);
		NRF_LOG_INFO("Hardware Test initialized");
	}


    APP_TIMER_DEF(ledsTimer);
    APP_TIMER_DEF(chargingTimer);

    void HardwareTestHandler(void* context, const Message* msg) {
        NRF_LOG_INFO("Starting Hardware Test");

        // Reprogram default anim settings
        AnimationSet::ProgramDefaultAnimationSet();

        // Check Accelerometer WHOAMI
        if (LIS2DE12::checkWhoAMI()) {
            NRF_LOG_INFO("Good WHOAMI");
            // Good, move onto the next test
            MessageService::NotifyUser("Put the die down!", true, true, 30, [](bool okCancel)
            {
                if (okCancel) {
                    // Check that interrupt pin is low
                    if (LIS2DE12::checkIntPin()) {
                        NRF_LOG_INFO("Good int pin");
                        // Good, try interrupt
                        MessageService::NotifyUser("Now pick the die up!", false, false, 10, nullptr);

                        NRF_LOG_INFO("Setting up interrupt");

                        // Set interrupt pin
                        GPIOTE::enableInterrupt(
                            BoardManager::getBoard()->accInterruptPin,
                            NRF_GPIO_PIN_NOPULL,
                            NRF_GPIOTE_POLARITY_LOTOHI,
                            [](uint32_t pin, nrf_gpiote_polarity_t action)
                            {
                                GPIOTE::disableInterrupt(BoardManager::getBoard()->accInterruptPin);
                                
                                // Don't do a lot of work in interrupt context
                                Scheduler::push(nullptr, 0, [](void * p_event_data, uint16_t event_size)
                                {
                                    NRF_LOG_INFO("Interrupt triggered");
                                    // Acc seems to work well,

                                    // Turn all LEDs on repeatedly!
                                    Timers::createTimer(&ledsTimer, APP_TIMER_MODE_REPEATED, [](void* ctx)
                                    {
                                        AnimController::play(&AnimationSet::getAnimation(AnimationSet::getAnimationCount() - 3));
                                    });
                                    Timers::startTimer(ledsTimer, 1000, nullptr);
                                    AnimController::play(&AnimationSet::getAnimation(AnimationSet::getAnimationCount() - 3));

                                    MessageService::NotifyUser("Check all leds", true, true, 30, [](bool okCancel)
                                    {
                                        Timers::stopTimer(ledsTimer);

                                        // LEDs seem good, test charging
                                        if (okCancel) {
                                            char buffer[100]; buffer[0] = '\0';
                                            const char* stateString = BatteryController::getChargeStateString(BatteryController::getCurrentChargeState());
                                            float vbat = Battery::checkVBat();
                                            sprintf(buffer, "Battery %s, " SPRINTF_FLOAT_MARKER "V. place on charger!", stateString, SPRINTF_FLOAT(vbat));
                                            MessageService::NotifyUser(buffer, false, false, 30, nullptr);

                                            // Register a handler with the battery controller
                                            BatteryController::hook([](void* ignore, BatteryController::BatteryState newState) {
                                                if (newState == BatteryController::BatteryState_Charging) {

                                                    // Good! unhook from the controller now
                                                    BatteryController::unHookWithParam((void*)(0x12345678));
                                                    Timers::stopTimer(chargingTimer);

                                                    // Done!
                                                    MessageService::NotifyUser("Test complete!", true, false, 10, nullptr);
                                                }
                                            }, (void*)(0x12345678));

                                            // Turn all LEDs on repeatedly!
                                            Timers::createTimer(&chargingTimer, APP_TIMER_MODE_SINGLE_SHOT, [](void* ctx)
                                            {
                                                BatteryController::unHookWithParam((void*)(0x12345678));
                                                MessageService::NotifyUser("No charging detected!", true, false, 10, nullptr);

                                                // Done!
                                            });
                                            Timers::startTimer(chargingTimer, 30000, nullptr);
                                        }
                                    });
                                });
                            });

                        LIS2DE12::enableTransientInterrupt();
                    } else {
                        NRF_LOG_INFO("Bad int pin");
                        MessageService::NotifyUser("Bad Int. pin.", true, false, 10, nullptr);
                    }
                }
            });
        } else {
            NRF_LOG_INFO("Bad WHOAMI");
            MessageService::NotifyUser("Bad I2C conn.", true, false, 10, nullptr);
        }
    }

}
}
