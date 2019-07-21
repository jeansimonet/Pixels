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
#include "modules/anim_controller.h"
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

    void HardwareTestHandler(void* context, const Message* msg) {
        NRF_LOG_INFO("Starting Hardware Test");

        // Reprogram default anim settings
        AnimationSet::ProgramDefaultAnimationSet();

        // Check Accelerometer WHOAMI
        if (LIS2DE12::checkWhoAMI()) {
            NRF_LOG_INFO("Good WHOAMI");
            // Good, move onto the next test
            MessageService::NotifyUser("Put die down!", []()
            {
                // Check that interrupt pin is low
                if (LIS2DE12::checkIntPin()) {
                    NRF_LOG_INFO("Good int pin");
                    // Good, try interrupt
                    MessageService::NotifyUser("Pick it up!", []()
                    {
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

                                    MessageService::NotifyUser("Check all leds", []()
                                    {
                                        NRF_LOG_INFO("Done");
                                        Timers::stopTimer(ledsTimer);

                                        // LEDs seem good, test charging

                                    });
                                });
                            });

                        LIS2DE12::enableTransientInterrupt();
                    });
                } else {
                    NRF_LOG_INFO("Bad int pin");
                    MessageService::NotifyUser("Bad Int. pin.", [](){});
                }
            });
        } else {
            NRF_LOG_INFO("Bad WHOAMI");
            MessageService::NotifyUser("Bad I2C conn.", [](){});
        }
    }

}
}
