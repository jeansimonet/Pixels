#include "board_config.h"
#include "drivers_nrf/a2d.h"
#include "nrf_gpio.h"
#include "nrf_saadc.h"
#include "nrf_log.h"
#include "settings.h"

#define BOARD_DETECT_DRIVE_PIN 25
#define BOARD_DETECT_RESISTOR 100000 // 100k

namespace Config
{
namespace BoardManager
{
    static const Board DevBoard = {

        // Measuring board type
        .boardResistorValues = {-1, -1},

        // Talking to LEDs
        .ledDataPin =  6,
        .ledClockPin = 5,
        .ledPowerPin = 9,

        // I2C Pins for accelerometer
        .i2cDataPin = 14,
        .i2cClockPin = 15,
        .accInterruptPin = 16,

        // Power Management pins
        .chargingStatePin = 0xFFFFFFFF,
        .coilSensePin = NRF_SAADC_INPUT_DISABLED,
        .vbatSensePin = NRF_SAADC_INPUT_AIN2,
        .vledSensePin = NRF_SAADC_INPUT_DISABLED,

        // Magnet pin
        .magnetPin = 0xFFFFFFFF,

        // LED config
        .ledCount = 21,
    };

    static const Board D6Board = {

        // Measuring board type
        .boardResistorValues = {47000, 47000}, // 47k Resistor

        // Talking to LEDs
        .ledDataPin =  1,
        .ledClockPin = 4,
        .ledPowerPin = 0,

        // I2C Pins for accelerometer
        .i2cDataPin = 12,
        .i2cClockPin = 14,
        .accInterruptPin = 15,

        // Power Management pins
        .chargingStatePin = 0xFFFFFFFF,
        .coilSensePin = NRF_SAADC_INPUT_DISABLED,
        .vbatSensePin = NRF_SAADC_INPUT_AIN3,
        .vledSensePin = NRF_SAADC_INPUT_DISABLED,

        // Magnet pin
        .magnetPin = 0xFFFFFFFF,

        // LED config
        .ledCount = 6,
    };

    static const Board D20Board = {

        // Measuring board type
        .boardResistorValues = {20000, 20000}, // 20k Resistor

        // Talking to LEDs
        .ledDataPin =  1,
        .ledClockPin = 4,
        .ledPowerPin = 0,

        // I2C Pins for accelerometer
        .i2cDataPin = 12,
        .i2cClockPin = 14,
        .accInterruptPin = 15,

        // Power Management pins
        .chargingStatePin = 0xFFFFFFFF,
        .coilSensePin = NRF_SAADC_INPUT_DISABLED,
        .vbatSensePin = NRF_SAADC_INPUT_AIN3,
        .vledSensePin = NRF_SAADC_INPUT_DISABLED,

        // Magnet pin
        .magnetPin = 0xFFFFFFFF,

        // LED config
        .ledCount = 20,
    };

    static const Board D20BoardV5 = {

        // Measuring board type
        .boardResistorValues = { 33000, 56000 }, // 33k or 56k Resistor

        // Talking to LEDs
        .ledDataPin =  0,
        .ledClockPin = 1,
        .ledPowerPin = 10,

        // I2C Pins for accelerometer
        .i2cDataPin = 12,
        .i2cClockPin = 14,
        .accInterruptPin = 15,

        // Power Management pins
        .chargingStatePin = 6,
        .coilSensePin = NRF_SAADC_INPUT_AIN3,
        .vbatSensePin = NRF_SAADC_INPUT_AIN6,
        .vledSensePin = NRF_SAADC_INPUT_AIN2,

        // Magnet pin
        .magnetPin = 9,

        // LED config
        .ledCount = 20,
    };

    // 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15 16 17 18 19 20   <-- prev
    // 01 12 18 08 10 19 04 06 05 07 14 16 15 17 02 11 13 03 09 20   <-- next


    // The board we're currently using
    static const Board* currentBoard = nullptr;

    void init() {
        // Sample adc board pin
        nrf_gpio_cfg_output(BOARD_DETECT_DRIVE_PIN);
        nrf_gpio_pin_set(BOARD_DETECT_DRIVE_PIN);

        float vboard = DriversNRF::A2D::readVBoard();

        // Now that we're done reading, we can turn off the drive pin
        nrf_gpio_cfg_default(BOARD_DETECT_DRIVE_PIN);

        // Do some computation to figure out which variant we're working with!
        // D20v5 board uses 33k over 100k voltage divider, or 56k over 100k (because I ran out of 33k 0402 resistors)
        // D20v3 board uses 20k over 100k voltage divider
        // i.e. the voltage read should be 3.3V * 20k / 120k = 0.55V
        // The D6v2 board uses 47k over 100k, i.e. 1.05V
        // The D20v2 board should read 0 (unconnected)
        // So we can allow a decent
        const float vdd = 3.2f; // supply voltage 3.2V
        const float tolerance = 0.2f; // +- 0.2V
        float D20V5BoardVoltage1 = (vdd * D20BoardV5.boardResistorValues[0]) / (float)(100000 + D20BoardV5.boardResistorValues[0]);
        float D20V5BoardVoltage2 = (vdd * D20BoardV5.boardResistorValues[1]) / (float)(100000 + D20BoardV5.boardResistorValues[1]);
        float D20BoardVoltage = (vdd * D20Board.boardResistorValues[0]) / (float)(100000 + D20Board.boardResistorValues[0]);
        float D6BoardVoltage = (vdd * D6Board.boardResistorValues[0]) / (float)(100000 + D6Board.boardResistorValues[0]);
        if ((vboard >= D20V5BoardVoltage1 - tolerance && vboard <= D20V5BoardVoltage1 + tolerance) ||
            (vboard >= D20V5BoardVoltage2 - tolerance && vboard <= D20V5BoardVoltage2 + tolerance)) {
            currentBoard = &D20BoardV5;
            NRF_LOG_INFO("Board is D20v5, boardIdVoltage=" NRF_LOG_FLOAT_MARKER, NRF_LOG_FLOAT(vboard));
        } else if (vboard >= D20BoardVoltage - tolerance && vboard <= D20BoardVoltage + tolerance) {
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
