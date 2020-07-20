#include "a2d.h"
#include "nrf_drv_saadc.h"
#include "config/board_config.h"
#include "log.h"
#include "timers.h"
#include "power_manager.h"

#define BOARD_DETECT_SENSE_PIN NRF_SAADC_INPUT_AIN4

namespace DriversNRF
{
namespace A2D
{
    bool supportsVCoil = false;
    bool supportsVLED = false;

    nrf_saadc_channel_config_t channel_config_conf;
    nrf_saadc_channel_config_t channel_config_batt;
    nrf_saadc_channel_config_t channel_config_5v;
    nrf_saadc_channel_config_t channel_config_vled;

    void saadc_callback(nrfx_saadc_evt_t const * p_event) {
        // Do nothing!
    }
    void init() {
        ret_code_t err_code;

        err_code = nrf_drv_saadc_init(NULL, saadc_callback);
        APP_ERROR_CHECK(err_code);

        channel_config_conf =
        {
            .resistor_p = NRF_SAADC_RESISTOR_DISABLED,
            .resistor_n = NRF_SAADC_RESISTOR_DISABLED,
            .gain       = NRF_SAADC_GAIN1_6,
            .reference  = NRF_SAADC_REFERENCE_INTERNAL,
            .acq_time   = NRF_SAADC_ACQTIME_40US,
            .mode       = NRF_SAADC_MODE_SINGLE_ENDED,
            .burst      = NRF_SAADC_BURST_DISABLED,
            .pin_p      = (nrf_saadc_input_t)(BOARD_DETECT_SENSE_PIN),
            .pin_n      = NRF_SAADC_INPUT_DISABLED
        };

        supportsVCoil = false;
        supportsVLED = false;

        NRF_LOG_INFO("A2D Initialized, vBoard=" NRF_LOG_FLOAT_MARKER, NRF_LOG_FLOAT(readVBoard()));

        #if DICE_SELFTEST && A2D_SELFTEST
        selfTest();
        #endif
    }

    int16_t readConfigPin() {
        ret_code_t err_code = nrf_drv_saadc_channel_init(0, &channel_config_conf);
        APP_ERROR_CHECK(err_code);

        int16_t ret;
        err_code = nrf_drv_saadc_sample_convert(0, &ret);
        if (err_code != NRF_SUCCESS) {
            ret = -1;
        }

        nrf_drv_saadc_channel_uninit(0);

        return ret;
    }

    void initBoardPins() {
        // For the battery, we're going to need to change the default config
        channel_config_batt =
        {
            .resistor_p = NRF_SAADC_RESISTOR_DISABLED,
            .resistor_n = NRF_SAADC_RESISTOR_DISABLED,
            .gain       = NRF_SAADC_GAIN1_6,
            .reference  = NRF_SAADC_REFERENCE_INTERNAL,
            .acq_time   = NRF_SAADC_ACQTIME_40US,
            .mode       = NRF_SAADC_MODE_SINGLE_ENDED,
            .burst      = NRF_SAADC_BURST_DISABLED,
            .pin_p      = (nrf_saadc_input_t)(Config::BoardManager::getBoard()->vbatSensePin),
            .pin_n      = NRF_SAADC_INPUT_DISABLED
        };

        auto csPin = (nrf_saadc_input_t)(Config::BoardManager::getBoard()->coilSensePin);
        if (csPin != NRF_SAADC_INPUT_DISABLED) {
            channel_config_5v =
            {
                .resistor_p = NRF_SAADC_RESISTOR_DISABLED,
                .resistor_n = NRF_SAADC_RESISTOR_DISABLED,
                .gain       = NRF_SAADC_GAIN1_6,
                .reference  = NRF_SAADC_REFERENCE_INTERNAL,
                .acq_time   = NRF_SAADC_ACQTIME_40US,
                .mode       = NRF_SAADC_MODE_SINGLE_ENDED,
                .burst      = NRF_SAADC_BURST_DISABLED,
                .pin_p      = csPin,
                .pin_n      = NRF_SAADC_INPUT_DISABLED
            };

            supportsVCoil = true;
        }

        auto vledPin = (nrf_saadc_input_t)(Config::BoardManager::getBoard()->vledSensePin);
        if (vledPin != NRF_SAADC_INPUT_DISABLED) {
            channel_config_vled =
            {
                .resistor_p = NRF_SAADC_RESISTOR_DISABLED,
                .resistor_n = NRF_SAADC_RESISTOR_DISABLED,
                .gain       = NRF_SAADC_GAIN1_6,
                .reference  = NRF_SAADC_REFERENCE_INTERNAL,
                .acq_time   = NRF_SAADC_ACQTIME_40US,
                .mode       = NRF_SAADC_MODE_SINGLE_ENDED,
                .burst      = NRF_SAADC_BURST_DISABLED,
                .pin_p      = vledPin,
                .pin_n      = NRF_SAADC_INPUT_DISABLED
            };

            supportsVLED = true;
        }

        NRF_LOG_INFO("A2D Pins Initialized");

        #if DICE_SELFTEST && A2D_SELFTEST_BATT
        selfTestBatt();
        #endif
    }

    int16_t readBatteryPin() {
        ret_code_t err_code = nrf_drv_saadc_channel_init(0, &channel_config_batt);
        APP_ERROR_CHECK(err_code);

        int16_t ret;
        err_code = nrf_drv_saadc_sample_convert(0, &ret);
        if (err_code != NRF_SUCCESS) {
            ret = -1;
        }

        nrf_drv_saadc_channel_uninit(0);
        return ret;
    }

    int16_t read5VPin() {
        int16_t ret;
        if (supportsVCoil) {
            ret_code_t err_code = nrf_drv_saadc_channel_init(0, &channel_config_5v);
            APP_ERROR_CHECK(err_code);

            err_code = nrf_drv_saadc_sample_convert(0, &ret);
            if (err_code != NRF_SUCCESS) {
                ret = -1;
            }

            nrf_drv_saadc_channel_uninit(0);
        } else {
            ret = -1;
        }


        return ret;
    }

    int16_t readVLEDPin() {
        int16_t ret;
        if (supportsVLED) {
            ret_code_t err_code = nrf_drv_saadc_channel_init(0, &channel_config_vled);
            APP_ERROR_CHECK(err_code);

            err_code = nrf_drv_saadc_sample_convert(0, &ret);
            if (err_code != NRF_SUCCESS) {
                ret = -1;
            }

            nrf_drv_saadc_channel_uninit(0);
        } else {
            ret = -1;
        }
        return ret;
    }

    float readVBat() {

        // Digital value read is [V(p) - V(n)] * Gain / Reference * 2^(Resolution - m)
        // In our case:
        // - V(n) = 0
        // - Gain = 1/6
        // - Reference = 0.6V
        // - Resolution = 10
        // - m = 0
        // val = V(p) * 2^12 / (6 * 0.6)
        // => V(p) = val * 3.6 / 2^10
        // => V(p) = val * 0.003515625

        int16_t val = readBatteryPin();
        if (val != -1) {
            return (float)val * 0.003515625f;
        } else {
            return 0.0f;
        }
    }
    
    float read5V() {

        // Digital value read is [V(p) - V(n)] * Gain / Reference * 2^(Resolution - m)
        // In our case:
        // - V(n) = 0
        // - Gain = 1/6
        // - Reference = 0.6V
        // - Resolution = 10
        // - m = 0
        // val = V(p) * 2^12 / (6 * 0.6)
        // => V(p) = val * 3.6 / 2^10
        // => V(p) = val * 0.003515625

        int16_t val = read5VPin();
        if (val != -1) {
            return (float)val * 0.003515625f;
        } else {
            return 0.0f;
        }
    }
    
    float readVLED() {

        // Digital value read is [V(p) - V(n)] * Gain / Reference * 2^(Resolution - m)
        // In our case:
        // - V(n) = 0
        // - Gain = 1/6
        // - Reference = 0.6V
        // - Resolution = 10
        // - m = 0
        // val = V(p) * 2^12 / (6 * 0.6)
        // => V(p) = val * 3.6 / 2^10
        // => V(p) = val * 0.003515625

        int16_t val = readVLEDPin();
        if (val != -1) {
            return (float)val * 0.003515625f;
        } else {
            return 0.0f;
        }
    }
    
    float readVBoard() {
        // Digital value read is [V(p) - V(n)] * Gain / Reference * 2^(Resolution - m)
        // In our case:
        // - V(n) = 0
        // - Gain = 1/6
        // - Reference = 0.6V
        // - Resolution = 10
        // - m = 0
        // val = V(p) * 2^12 / (6 * 0.6)
        // => V(p) = val * 3.6 / 2^10
        // => V(p) = val * 0.003515625

        int16_t val = readConfigPin();
        if (val != -1) {
            return (float)val * 0.003515625f;
        } else {
            return 0.0f;
        }
    }

#if DICE_SELFTEST && A2D_SELFTEST
    APP_TIMER_DEF(readBoardIdTimer);
    void printBoardIdValue(void* context) {
        float boardIdVoltage = readVBoard();
        NRF_LOG_INFO("boardIdVoltage=" NRF_LOG_FLOAT_MARKER, NRF_LOG_FLOAT(boardIdVoltage));
    }

    void selfTest() {
        Timers::createTimer(&readBoardIdTimer, APP_TIMER_MODE_REPEATED, printBoardIdValue);
        NRF_LOG_INFO("Reading board Id Repeatedly, press any key to abort");
        Log::process();

        Timers::startTimer(readBoardIdTimer, 200, nullptr);
        while (!Log::hasKey()) {
            Log::process();
            PowerManager::feed();
            PowerManager::update();
        }
		Log::getKey();
        NRF_LOG_INFO("Finished reading board id!");
        Timers::stopTimer(readBoardIdTimer);
    }
#endif // SELFTEST

#if DICE_SELFTEST && A2D_SELFTEST_BATT
    APP_TIMER_DEF(readVBatTimer);
    void printVBat(void* context) {
        float vbat = readVBat();
        NRF_LOG_INFO("VBat=" NRF_LOG_FLOAT_MARKER, NRF_LOG_FLOAT(vbat));
    }

    void selfTestBatt() {
        Timers::createTimer(&readVBatTimer, APP_TIMER_MODE_REPEATED, printVBat);
        NRF_LOG_INFO("Reading vbat Repeatedly, press any key to abort");
        Log::process();

        Timers::startTimer(readVBatTimer, 200, nullptr);
        while (!Log::hasKey()) {
            Log::process();
            PowerManager::feed();
            PowerManager::update();
        }
		Log::getKey();
        NRF_LOG_INFO("Finished reading vbat!");
        Timers::stopTimer(readVBatTimer);
    }
#endif // SELFTEST
}
}

