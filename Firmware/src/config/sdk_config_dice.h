
//==========================================================
// HARDWARE TESTING
//==========================================================
#ifndef DICE_SELFTEST
#define DICE_SELFTEST 0
#endif

#define WATCHDOG_SELFTEST 0
#define TIMERS_SELFTEST 0
#define LOG_SELFTEST 0
#define DFU_SELFTEST 0
#define A2D_SELFTEST 0
#define A2D_SELFTEST_BATT 0
#define POWER_MANAGER_SELFTEST 0
#define FLASH_SELFTEST 0
#define BOARD_MANAGER_SELFTEST 0
#define I2C_SELFTEST 0
#define APA102_SELFTEST 0
#define LIS2DE12_SELFTEST 0
#define LIS2DE12_SELFTEST_INT 0
#define BATTERY_SELFTEST 0
#define MAGNET_SELFTEST 0
#define SETTINGS_MANAGER_SELFTEST 0
#define BULK_DATA_TRANSFER_SELFTEST 0

// Force logging on!
#if DICE_SELFTEST
#undef NRF_LOG_ENABLED
#define NRF_LOG_ENABLED 1
#endif