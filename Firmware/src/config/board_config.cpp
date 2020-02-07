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
    static const uint8_t sixSidedRemap[] = {
        // FIXME!!!
        0, 1, 2, 3, 4, 5,
        1, 2, 3, 4, 5, 0,
        2, 3, 4, 5, 0, 1,
        3, 4, 5, 0, 1, 2,
        4, 5, 0, 1, 2, 3, 
        5, 0, 1, 2, 3, 4, 
    };

    static const uint8_t sixSidedMisalignedRemap[] {
        // FIXME!!!
        0, 1, 2, 3, 4, 5
    };

    static const uint8_t twentySidedRemap[] = {
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
        1, 11, 10, 5, 17, 12, 6, 16, 19, 4, 15, 0, 3, 13, 7, 2, 14, 9, 8, 18,
        2, 3, 7, 13, 4, 8, 18, 0, 10, 14, 5, 9, 19, 1, 11, 15, 6, 12, 16, 17,
        3, 9, 5, 8, 12, 19, 18, 6, 17, 4, 15, 2, 13, 1, 0, 7, 11, 14, 10, 16,
        4, 8, 0, 6, 2, 3, 5, 7, 9, 1, 18, 10, 12, 14, 16, 17, 13, 19, 11, 15,
        5, 4, 12, 19, 3, 18, 8, 6, 9, 17, 2, 10, 13, 11, 1, 16, 0, 7, 15, 14,
        6, 1, 3, 9, 12, 15, 11, 5, 19, 2, 17, 0, 14, 8, 4, 7, 10, 16, 18, 13,
        7, 13, 0, 1, 4, 10, 16, 2, 5, 11, 8, 14, 17, 3, 9, 15, 18, 19, 6, 12,
        8, 4, 19, 17, 13, 16, 10, 18, 14, 12, 7, 5, 1, 9, 3, 6, 2, 0, 15, 11,
        9, 2, 15, 19, 14, 13, 18, 11, 16, 12, 7, 3, 8, 1, 6, 5, 0, 4, 17, 10,
        10, 4, 17, 12, 1, 6, 5, 16, 11, 19, 0, 8, 3, 14, 13, 18, 7, 2, 15, 9,
        11, 1, 9, 2, 14, 7, 0, 15, 13, 3, 16, 6, 4, 19, 12, 5, 17, 10, 18, 8,
        12, 17, 3, 18, 5, 8, 19, 6, 4, 9, 10, 15, 13, 0, 11, 14, 1, 16, 2, 7,
        13, 14, 8, 10, 19, 17, 16, 18, 12, 4, 15, 7, 1, 3, 2, 0, 9, 11, 5, 6,
        14, 13, 11, 0, 9, 2, 7, 15, 3, 1, 18, 16, 4, 12, 17, 10, 19, 8, 6, 5,
        15, 12, 14, 13, 9, 18, 19, 11, 2, 16, 3, 17, 8, 0, 1, 10, 6, 5, 7, 4,
        16, 14, 10, 4, 1, 0, 7, 17, 6, 8, 11, 13, 2, 12, 19, 18, 15, 9, 5, 3,
        17, 12, 16, 14, 1, 11, 15, 10, 0, 13, 6, 19, 9, 4, 8, 18, 5, 3, 7, 2,
        18, 9, 8, 4, 13, 7, 2, 19, 16, 5, 14, 3, 0, 17, 12, 6, 15, 11, 10, 1,
        19, 17, 18, 9, 13, 14, 15, 8, 7, 3, 16, 12, 11, 4, 5, 6, 10, 1, 2, 0,
    };

    static const uint8_t twentySidedMisalignedRemap[] {
        15, 19, 11, 1, 14, 16, 17, 9, 7, 6, 13, 12, 10, 2, 3, 5, 18, 8, 0, 4
    };



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
        .CoilStatePin = 0,
        .vbatSensePin = NRF_SAADC_INPUT_AIN2,

        // Magnet pin
        .magnetPin = 10,

        // LED config
        .ledCount = 21,
        .faceToLedLookup = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 },
        .faceNormals = {
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
        } ,
        .faceRemapLookup = twentySidedRemap,
        .screwupRemapLookup = twentySidedMisalignedRemap
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
        .CoilStatePin = 9,
        .vbatSensePin = NRF_SAADC_INPUT_AIN3,

        // Magnet pin
        .magnetPin = 6,

        // LED config
        .ledCount = 6,
        .faceToLedLookup = { 1, 4, 0, 3, 2, 5, },
        .faceNormals = {
            { 0, -1,  0},
            { 0,  0,  1},
            { 1,  0,  0},
            {-1,  0,  0},
            { 0,  0, -1},
            { 0,  1,  0}
        },
        .faceRemapLookup = sixSidedRemap,
        .screwupRemapLookup = sixSidedMisalignedRemap
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
        .CoilStatePin = 9,
        .vbatSensePin = NRF_SAADC_INPUT_AIN3,

        // Magnet pin
        .magnetPin = 6,

        // LED config
        .ledCount = 20,
        .faceToLedLookup = { 15, 4, 7, 1, 0, 19, 8, 10, 6, 14, 9, 11, 5, 13, 3, 17, 16, 12, 18, 2 },
        // FYI This is the board order: 4, 3, 19, 14, 1, 12, 8, 2, 6, 10, 7, 11, 17, 13, 9, 0, 16, 15, 18, 5
        .faceNormals = {
            {-0.1273862f,  0.3333025f,  0.9341605f},
            { 0.6667246f, -0.7453931f, -0.0000000f},
            { 0.8726854f,  0.3333218f, -0.3568645f},
            {-0.3333083f, -0.7453408f, -0.5773069f},
            { 0.0000000f, -1.0000000f, -0.0000000f},
            {-0.7453963f,  0.3333219f,  0.5773357f},
            { 0.3333614f,  0.7453930f, -0.5774010f},
            {-0.7453431f,  0.3333741f, -0.5773722f},
            { 0.8726999f,  0.3333025f,  0.3567604f},
            { 0.1273475f, -0.3333741f,  0.9341723f},
            {-0.1273475f,  0.3333741f, -0.9341723f},
            {-0.8726999f, -0.3333025f, -0.3567604f},
            { 0.7453431f, -0.3333741f,  0.5773722f},
            {-0.3331230f, -0.7450288f,  0.5778139f},
            { 0.7453963f, -0.3333219f, -0.5773357f},
            { 0.0000000f,  1.0000000f, -0.0000000f},
            { 0.3333083f,  0.7453408f,  0.5773069f},
            {-0.8726854f, -0.3333218f,  0.3568645f},
            {-0.6667246f,  0.7453931f, -0.0000000f},
            { 0.1273862f, -0.3333025f, -0.9341605f},
        },
        .faceRemapLookup = twentySidedRemap,
        .screwupRemapLookup = twentySidedMisalignedRemap
    };

    // The board we're currently using
    static const Board* currentBoard = nullptr;

    uint8_t Board::remapLed(uint8_t animRemapIndex, uint8_t thisLedIndex) const {
        uint8_t remapped = faceRemapLookup[animRemapIndex * ledCount + thisLedIndex];
        // Fix casting screw up
        return screwupRemapLookup[remapped];
    }

    void init() {
        // Sample adc board pin
        nrf_gpio_cfg_output(BOARD_DETECT_DRIVE_PIN);
        nrf_gpio_pin_set(BOARD_DETECT_DRIVE_PIN);

        float vboard = DriversNRF::A2D::readVBoard();

        // Now that we're done reading, we can turn off the drive pin
        nrf_gpio_cfg_default(BOARD_DETECT_DRIVE_PIN);

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
