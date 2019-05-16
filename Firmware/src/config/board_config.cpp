#include "board_config.h"
#include "drivers_nrf/a2d.h"
#include "nrf_gpio.h"
#include "nrf_saadc.h"
#include "nrf_log.h"

#define BOARD_DETECT_DRIVE_PIN 25
#define BOARD_DETECT_RESISTOR 100000 // 100k

namespace Config
{
namespace BoardManager
{
    static const Board DevBoard = {

        // Measuring board type
        .boardResistorValue = -1,

        // Talking to LEDs
        .ledDataPin =  6,
        .ledClockPin = 5,
        .ledPowerPin = 9,

        // I2C Pins for accelerometer
        .i2cDataPin = 14,
        .i2cClockPin = 15,
        .accInterruptPin = 16,

        // Power Management pins
        .chargingStatePin = 1,
        .vledFaultPin = 0,
        .vbatSensePin = NRF_SAADC_INPUT_AIN2,

        // Magnet pin
        .magnetPin = 10,

        // LED config
        .ledCount = 21,
        .faceToLedLookup = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 },
        .faceNormal = {
            {0, 0, 1},  // This is not the correct data
            {0, 0, -1}, // This is not the correct data
            {0, 1, 0},  // This is not the correct data
            {0, -1, 0}, // This is not the correct data
            {1, 0, 0},  // This is not the correct data
            {-1, 0, 0},  // This is not the correct data
            {0, 0, 1},  // This is not the correct data
            {0, 0, -1}, // This is not the correct data
            {0, 1, 0},  // This is not the correct data
            {0, -1, 0}, // This is not the correct data
            {1, 0, 0},  // This is not the correct data
            {-1, 0, 0},  // This is not the correct data
            {0, 0, 1},  // This is not the correct data
            {0, 0, -1}, // This is not the correct data
            {0, 1, 0},  // This is not the correct data
            {0, -1, 0}, // This is not the correct data
            {1, 0, 0},  // This is not the correct data
            {-1, 0, 0},  // This is not the correct data
            {0, -1, 0}, // This is not the correct data
            {1, 0, 0},  // This is not the correct data
            {-1, 0, 0}  // This is not the correct data
        } 
    };

    static const Board D6Board = {

        // Measuring board type
        .boardResistorValue = 47000, // 47k Resistor

        // Talking to LEDs
        .ledDataPin =  1,
        .ledClockPin = 4,
        .ledPowerPin = 0,

        // I2C Pins for accelerometer
        .i2cDataPin = 12,
        .i2cClockPin = 14,
        .accInterruptPin = 15,

        // Power Management pins
        .chargingStatePin = 10,
        .vledFaultPin = 9,
        .vbatSensePin = NRF_SAADC_INPUT_AIN3,

        // Magnet pin
        .magnetPin = 6,

        // LED config
        .ledCount = 6,
        .faceToLedLookup = { 0, 1, 2, 3, 4, 5, }, // This is not the correct data
        .faceNormal = {
            {0, 0, 1},  // This is not the correct data
            {0, 0, -1}, // This is not the correct data
            {0, 1, 0},  // This is not the correct data
            {0, -1, 0}, // This is not the correct data
            {1, 0, 0},  // This is not the correct data
            {-1, 0, 0}  // This is not the correct data
        } 
    };

    static const Board D20Board = {

        // Measuring board type
        .boardResistorValue = 20000, // 20k Resistor

        // Talking to LEDs
        .ledDataPin =  1,
        .ledClockPin = 4,
        .ledPowerPin = 0,

        // I2C Pins for accelerometer
        .i2cDataPin = 12,
        .i2cClockPin = 14,
        .accInterruptPin = 15,

        // Power Management pins
        .chargingStatePin = 10,
        .vledFaultPin = 9,
        .vbatSensePin = NRF_SAADC_INPUT_AIN3,

        // Magnet pin
        .magnetPin = 6,

        // LED config
        .ledCount = 20,
        .faceToLedLookup = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, // This is not the correct data
        .faceNormal = {
            {0, 0, 1},  // This is not the correct data
            {0, 0, -1}, // This is not the correct data
            {0, 1, 0},  // This is not the correct data
            {0, -1, 0}, // This is not the correct data
            {1, 0, 0},  // This is not the correct data
            {-1, 0, 0}, // This is not the correct data
            {0, 0, 1},  // This is not the correct data
            {0, 0, -1}, // This is not the correct data
            {0, 1, 0},  // This is not the correct data
            {0, -1, 0}, // This is not the correct data
            {1, 0, 0},  // This is not the correct data
            {-1, 0, 0}, // This is not the correct data
            {0, 0, 1},  // This is not the correct data
            {0, 0, -1}, // This is not the correct data
            {0, 1, 0},  // This is not the correct data
            {0, -1, 0}, // This is not the correct data
            {1, 0, 0},  // This is not the correct data
            {-1, 0, 0}, // This is not the correct data
            {1, 0, 0},  // This is not the correct data
            {-1, 0, 0}  // This is not the correct data
        }
    };

    // The board we're currently using
    static const Board* currentBoard = nullptr;

    void init() {
        // Sample adc board pin
        nrf_gpio_cfg_output(BOARD_DETECT_DRIVE_PIN);
        nrf_gpio_pin_set(BOARD_DETECT_DRIVE_PIN);

        float vboard = DriversNRF::A2D::readVBoard();

        // Now that we're done reading, we can turn off the drive pin
        nrf_gpio_cfg_input(BOARD_DETECT_DRIVE_PIN, NRF_GPIO_PIN_NOPULL);

        // Do some computation to figure out which variant we're working with!
        // D20v3 board uses 20k over 100k voltage divider
        // i.e. the voltage read should be 3.3V * 20k / 120k = 0.55V
        // The D6v2 board uses 47k over 100k, i.e. 1.05V
        // The D20v2 board should read 0 (unconnected)
        // So we can allow a decent
        const float vdd = 3.2f; // supply voltage 3.2V
        const float tolerance = 0.2f; // +- 0.2V
        float D20BoardVoltage = (vdd * D20Board.boardResistorValue) / (float)(100000 + D20Board.boardResistorValue);
        float D6BoardVoltage = (vdd * D6Board.boardResistorValue) / (float)(100000 + D6Board.boardResistorValue);
        if (vboard >= D20BoardVoltage - tolerance && vboard <= D20BoardVoltage + tolerance) {
            currentBoard = &D20Board;
            NRF_LOG_INFO("Board is D20v3, boardIdVoltage=" NRF_LOG_FLOAT_MARKER, NRF_LOG_FLOAT(vboard));
        } else if (vboard >= D6BoardVoltage - tolerance && vboard <= D6BoardVoltage + tolerance) {
            currentBoard = &D6Board;
            NRF_LOG_INFO("Board is D6v2, boardIdVoltage=" NRF_LOG_FLOAT_MARKER, NRF_LOG_FLOAT(vboard));
        } else {
            NRF_LOG_WARNING("Could not identify the board, assuming Dev Board, boardIdVoltage=" NRF_LOG_FLOAT_MARKER, NRF_LOG_FLOAT(vboard));
            currentBoard = &DevBoard;
        }
    }

    const Board* getBoard() {
        return currentBoard;
    }
}
}
