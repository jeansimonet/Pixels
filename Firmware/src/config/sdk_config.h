#ifndef SDK_CONFIG_H
#define SDK_CONFIG_H

//==========================================================
// BLE DFU Service
//==========================================================
// <q> BLE_DFU_ENABLED  - Enable DFU Service.
#define BLE_DFU_ENABLED 1

// <q> NRF_DFU_BLE_BUTTONLESS_SUPPORTS_BONDS  - Buttonless DFU supports bonds.
#define NRF_DFU_BLE_BUTTONLESS_SUPPORTS_BONDS 0


//==========================================================
// GPIOTE Library
//==========================================================

// <e> GPIOTE_ENABLED - nrf_drv_gpiote - GPIOTE peripheral driver - legacy layer
#define GPIOTE_ENABLED 1

// <o> GPIOTE_CONFIG_NUM_OF_LOW_POWER_EVENTS - Number of lower power input pins 
#define GPIOTE_CONFIG_NUM_OF_LOW_POWER_EVENTS 4

// <o> GPIOTE_CONFIG_IRQ_PRIORITY  - Interrupt priority
// <i> Priorities 0,2 (nRF51) and 0,1,4,5 (nRF52) are reserved for SoftDevice
// <0=> 0 (highest) -> 7 (lowest)
#define GPIOTE_CONFIG_IRQ_PRIORITY 6


//==========================================================
// Clock Library
//==========================================================

// <e> NRFX_CLOCK_ENABLED - nrfx_clock - CLOCK peripheral driver
#define NRFX_CLOCK_ENABLED 1

// <o> NRFX_CLOCK_CONFIG_LF_SRC  - LF Clock Source
// <0=> RC 
// <1=> XTAL 
// <2=> Synth 
// <131073=> External Low Swing 
// <196609=> External Full Swing 
#define NRFX_CLOCK_CONFIG_LF_SRC 0

// <o> NRFX_CLOCK_CONFIG_IRQ_PRIORITY  - Interrupt priority
// <0=> 0 (highest) -> 7 (lowest)
#define NRFX_CLOCK_CONFIG_IRQ_PRIORITY 6


//==========================================================
// Analog to Digital converter
//==========================================================

// <e> NRFX_SAADC_ENABLED - nrfx_saadc - SAADC peripheral driver
#define NRFX_SAADC_ENABLED 1

// <o> NRFX_SAADC_CONFIG_RESOLUTION  - Resolution
// <0=> 8 bit 
// <1=> 10 bit 
// <2=> 12 bit 
// <3=> 14 bit 
#define NRFX_SAADC_CONFIG_RESOLUTION 1

// <o> NRFX_SAADC_CONFIG_OVERSAMPLE  - Sample period
// <0=> Disabled 
// <1=> 2x 
// <2=> 4x 
// <3=> 8x 
// <4=> 16x 
// <5=> 32x 
// <6=> 64x 
// <7=> 128x 
// <8=> 256x 
#define NRFX_SAADC_CONFIG_OVERSAMPLE 0

// <q> NRFX_SAADC_CONFIG_LP_MODE  - Enabling low power mode
#define NRFX_SAADC_CONFIG_LP_MODE 0

// <o> NRFX_SAADC_CONFIG_IRQ_PRIORITY  - Interrupt priority
// <0=> 0 (highest) -> 7 (lowest)
#define NRFX_SAADC_CONFIG_IRQ_PRIORITY 6


//==========================================================
// NRFX_GPIOTE Lib
//==========================================================

// <e> NRFX_GPIOTE_ENABLED - nrfx_gpiote - GPIOTE peripheral driver
#define NRFX_GPIOTE_ENABLED 1

// <o> NRFX_GPIOTE_CONFIG_NUM_OF_LOW_POWER_EVENTS - Number of lower power input pins 
#define NRFX_GPIOTE_CONFIG_NUM_OF_LOW_POWER_EVENTS 1

// <o> NRFX_GPIOTE_CONFIG_IRQ_PRIORITY  - Interrupt priority
// <0=> 0 (highest) -> 7 (lowest)
#define NRFX_GPIOTE_CONFIG_IRQ_PRIORITY 6


//==========================================================
// Peripheral Resource Sharing module
//==========================================================

// <e> NRFX_PRS_ENABLED - nrfx_prs - Peripheral Resource Sharing module
#define NRFX_PRS_ENABLED 1

// <q> NRFX_PRS_BOX_0_ENABLED  - Enables box 0 in the module.
#define NRFX_PRS_BOX_0_ENABLED 0

// <q> NRFX_PRS_BOX_1_ENABLED  - Enables box 1 in the module.
#define NRFX_PRS_BOX_1_ENABLED 0

// <q> NRFX_PRS_BOX_2_ENABLED  - Enables box 2 in the module.
#define NRFX_PRS_BOX_2_ENABLED 0

// <q> NRFX_PRS_BOX_3_ENABLED  - Enables box 3 in the module.
#define NRFX_PRS_BOX_3_ENABLED 0

// <q> NRFX_PRS_BOX_4_ENABLED  - Enables box 4 in the module.
#define NRFX_PRS_BOX_4_ENABLED 1


//==========================================================
// UARTE peripheral driver
//==========================================================

// <e> NRFX_UARTE_ENABLED - nrfx_uarte - UARTE peripheral driver
#define NRFX_UARTE_ENABLED 1

#define NRFX_UARTE0_ENABLED 1

#define UART_EASY_DMA_SUPPORT 1

#define UART_DEFAULT_CONFIG_HWFC 0

#define UART_DEFAULT_CONFIG_PARITY 0

// <323584=> 1200 baud 
// <643072=> 2400 baud 
// <1290240=> 4800 baud 
// <2576384=> 9600 baud 
// <3862528=> 14400 baud 
// <5152768=> 19200 baud 
// <7716864=> 28800 baud 
// <8388608=> 31250 baud 
// <10289152=> 38400 baud 
// <15007744=> 56000 baud 
// <15400960=> 57600 baud 
// <20615168=> 76800 baud 
// <30801920=> 115200 baud 
// <61865984=> 230400 baud 
// <67108864=> 250000 baud 
// <121634816=> 460800 baud 
// <251658240=> 921600 baud 
// <268435456=> 1000000 baud 
#define UART_DEFAULT_CONFIG_BAUDRATE 61865984 

#define UART_DEFAULT_CONFIG_IRQ_PRIORITY 6

//==========================================================
// UART peripheral driver
//==========================================================

// <e> NRFX_UART_ENABLED - nrfx_uart - UART peripheral driver
#define NRFX_UART_ENABLED 0


//==========================================================
// UART/UARTE peripheral driver - legacy layer
//==========================================================

// <e> UART_ENABLED - nrf_drv_uart - UART/UARTE peripheral driver - legacy layer
#define UART_ENABLED 1

#define UART0_ENABLED 1


//==========================================================
// CLOCK peripheral driver - legacy layer
//==========================================================

// <e> NRF_CLOCK_ENABLED - nrf_drv_clock - CLOCK peripheral driver - legacy layer
#define NRF_CLOCK_ENABLED 1

// <o> CLOCK_CONFIG_LF_SRC  - LF Clock Source
// <0=> RC 
// <1=> XTAL 
// <2=> Synth 
// <131073=> External Low Swing 
// <196609=> External Full Swing 
#define CLOCK_CONFIG_LF_SRC 0

// <o> CLOCK_CONFIG_IRQ_PRIORITY  - Interrupt priority
// <i> Priorities 0,2 (nRF51) and 0,1,4,5 (nRF52) are reserved for SoftDevice
// <0=> 0 (highest) -> 7 (lowest)
#define CLOCK_CONFIG_IRQ_PRIORITY 6


//==========================================================
// app_scheduler - Events scheduler
//==========================================================

// <e> APP_SCHEDULER_ENABLED - app_scheduler - Events scheduler
#define APP_SCHEDULER_ENABLED 1

// <q> APP_SCHEDULER_WITH_PAUSE  - Enabling pause feature
#define APP_SCHEDULER_WITH_PAUSE 1

// <q> APP_SCHEDULER_WITH_PROFILER  - Enabling scheduler profiling
#define APP_SCHEDULER_WITH_PROFILER 0


//==========================================================
// app_timer - Application timer functionality
//==========================================================

// <e> APP_TIMER_ENABLED - app_timer - Application timer functionality
#define APP_TIMER_ENABLED 1

// <o> APP_TIMER_CONFIG_RTC_FREQUENCY  - Configure RTC prescaler.
// <0=> 32768 Hz 
// <1=> 16384 Hz 
// <3=> 8192 Hz 
// <7=> 4096 Hz 
// <15=> 2048 Hz 
// <31=> 1024 Hz 
#define APP_TIMER_CONFIG_RTC_FREQUENCY 0

// <o> APP_TIMER_CONFIG_IRQ_PRIORITY  - Interrupt priority
// <i> Priorities 0,2 (nRF51) and 0,1,4,5 (nRF52) are reserved for SoftDevice
// <0=> 0 (highest) -> 7 (lowest)
#define APP_TIMER_CONFIG_IRQ_PRIORITY 6

// <o> APP_TIMER_CONFIG_OP_QUEUE_SIZE - Capacity of timer requests queue. 
// <i> Size of the queue depends on how many timers are used
// <i> in the system, how often timers are started and overall
// <i> system latency. If queue size is too small app_timer calls
// <i> will fail.
#define APP_TIMER_CONFIG_OP_QUEUE_SIZE 20

// <q> APP_TIMER_CONFIG_USE_SCHEDULER  - Enable scheduling app_timer events to app_scheduler
#define APP_TIMER_CONFIG_USE_SCHEDULER 1

// <q> APP_TIMER_KEEPS_RTC_ACTIVE  - Enable RTC always on
// <i> If option is enabled RTC is kept running even if there is no active timers.
// <i> This option can be used when app_timer is used for timestamping.
#define APP_TIMER_KEEPS_RTC_ACTIVE 0

// <o> APP_TIMER_SAFE_WINDOW_MS - Maximum possible latency (in milliseconds) of handling app_timer event. 
// <i> Maximum possible timeout that can be set is reduced by safe window.
// <i> Example: RTC frequency 16384 Hz, maximum possible timeout 1024 seconds - APP_TIMER_SAFE_WINDOW_MS.
// <i> Since RTC is not stopped when processor is halted in debugging session, this value
// <i> must cover it if debugging is needed. It is possible to halt processor for APP_TIMER_SAFE_WINDOW_MS
// <i> without corrupting app_timer behavior.
#define APP_TIMER_SAFE_WINDOW_MS 300000

// <h> App Timer Legacy configuration - Legacy configuration.

// <q> APP_TIMER_WITH_PROFILER  - Enable app_timer profiling
#define APP_TIMER_WITH_PROFILER 0

// <q> APP_TIMER_CONFIG_SWI_NUMBER  - Configure SWI instance used.
#define APP_TIMER_CONFIG_SWI_NUMBER 0


//==========================================================
// nrf_balloc - Block allocator module
//==========================================================

// <e> NRF_BALLOC_ENABLED - nrf_balloc - Block allocator module
#define NRF_BALLOC_ENABLED 1

// <e> NRF_BALLOC_CONFIG_DEBUG_ENABLED - Enables debug mode in the module.
#define NRF_BALLOC_CONFIG_DEBUG_ENABLED 0

// <o> NRF_BALLOC_CONFIG_HEAD_GUARD_WORDS - Number of words used as head guard.  <0-255> 
#define NRF_BALLOC_CONFIG_HEAD_GUARD_WORDS 1

// <o> NRF_BALLOC_CONFIG_TAIL_GUARD_WORDS - Number of words used as tail guard.  <0-255> 
#define NRF_BALLOC_CONFIG_TAIL_GUARD_WORDS 1

// <q> NRF_BALLOC_CONFIG_BASIC_CHECKS_ENABLED  - Enables basic checks in this module.
#define NRF_BALLOC_CONFIG_BASIC_CHECKS_ENABLED 0

// <q> NRF_BALLOC_CONFIG_DOUBLE_FREE_CHECK_ENABLED  - Enables double memory free check in this module.
#define NRF_BALLOC_CONFIG_DOUBLE_FREE_CHECK_ENABLED 0

// <q> NRF_BALLOC_CONFIG_DATA_TRASHING_CHECK_ENABLED  - Enables free memory corruption check in this module.
#define NRF_BALLOC_CONFIG_DATA_TRASHING_CHECK_ENABLED 0

// <q> NRF_BALLOC_CLI_CMDS  - Enable CLI commands specific to the module
#define NRF_BALLOC_CLI_CMDS 0


//==========================================================
// nrf_fprintf - fprintf function.
//==========================================================

// <q> NRF_FPRINTF_ENABLED  - nrf_fprintf - fprintf function.
#define NRF_FPRINTF_ENABLED 1

//==========================================================
// nrf_memobj - Linked memory allocator module
//==========================================================

// <q> NRF_MEMOBJ_ENABLED  - nrf_memobj - Linked memory allocator module
#define NRF_MEMOBJ_ENABLED 1


//==========================================================
// nrf_pwr_mgmt - Power management module
//==========================================================

// <e> NRF_PWR_MGMT_ENABLED - nrf_pwr_mgmt - Power management module
#define NRF_PWR_MGMT_ENABLED 1

// <e> NRF_PWR_MGMT_CONFIG_DEBUG_PIN_ENABLED - Enables pin debug in the module.
// <i> Selected pin will be set when CPU is in sleep mode.
#define NRF_PWR_MGMT_CONFIG_DEBUG_PIN_ENABLED 0

// <o> NRF_PWR_MGMT_SLEEP_DEBUG_PIN  - Pin number
// <0=> 0 (P0.0) -> 31 (P0.31)
// <4294967295=> Not connected 
#define NRF_PWR_MGMT_SLEEP_DEBUG_PIN 31

// <q> NRF_PWR_MGMT_CONFIG_CPU_USAGE_MONITOR_ENABLED  - Enables CPU usage monitor.
// <i> Module will trace percentage of CPU usage in one second intervals.
#define NRF_PWR_MGMT_CONFIG_CPU_USAGE_MONITOR_ENABLED 0

// <e> NRF_PWR_MGMT_CONFIG_STANDBY_TIMEOUT_ENABLED - Enable standby timeout.
#define NRF_PWR_MGMT_CONFIG_STANDBY_TIMEOUT_ENABLED 1

// <o> NRF_PWR_MGMT_CONFIG_STANDBY_TIMEOUT_S - Standby timeout (in seconds). 
// <i> Shutdown procedure will begin no earlier than after this number of seconds.
#define NRF_PWR_MGMT_CONFIG_STANDBY_TIMEOUT_S 30

// <q> NRF_PWR_MGMT_CONFIG_FPU_SUPPORT_ENABLED  - Enables FPU event cleaning.
#define NRF_PWR_MGMT_CONFIG_FPU_SUPPORT_ENABLED 0

// <q> NRF_PWR_MGMT_CONFIG_AUTO_SHUTDOWN_RETRY  - Blocked shutdown procedure will be retried every second.
#define NRF_PWR_MGMT_CONFIG_AUTO_SHUTDOWN_RETRY 0

// <q> NRF_PWR_MGMT_CONFIG_USE_SCHEDULER  - Module will use @ref app_scheduler.
#define NRF_PWR_MGMT_CONFIG_USE_SCHEDULER 0

// <o> NRF_PWR_MGMT_CONFIG_HANDLER_PRIORITY_COUNT - The number of priorities for module handlers. 
// <i> The number of stages of the shutdown process.
#define NRF_PWR_MGMT_CONFIG_HANDLER_PRIORITY_COUNT 3


//==========================================================
// nrf_section_iter - Section iterator
//==========================================================

// <q> NRF_SECTION_ITER_ENABLED  - nrf_section_iter - Section iterator
#define NRF_SECTION_ITER_ENABLED 1


//==========================================================
// nrf_strerror - Library for converting error code to string.
//==========================================================

// <q> NRF_STRERROR_ENABLED  - nrf_strerror - Library for converting error code to string.
#define NRF_STRERROR_ENABLED 1


//==========================================================
// app_button - buttons handling module
//==========================================================

// <q> BUTTON_ENABLED  - Enables Button module
#define BUTTON_ENABLED 0

// <q> BUTTON_HIGH_ACCURACY_ENABLED  - Enables GPIOTE high accuracy for buttons
#define BUTTON_HIGH_ACCURACY_ENABLED 0


//==========================================================
// nrfx_twim - TWIM peripheral driver
//==========================================================

// <e> NRFX_TWIM_ENABLED - nrfx_twim - TWIM peripheral driver
#define NRFX_TWIM_ENABLED 1

// <q> NRFX_TWIM0_ENABLED  - Enable TWIM0 instance
#define NRFX_TWIM0_ENABLED 1

// <q> NRFX_TWIM1_ENABLED  - Enable TWIM1 instance
#define NRFX_TWIM1_ENABLED 0

// <o> NRFX_TWIM_DEFAULT_CONFIG_FREQUENCY  - Frequency
// <26738688=> 100k 
// <67108864=> 250k 
// <104857600=> 400k 
#define NRFX_TWIM_DEFAULT_CONFIG_FREQUENCY 26738688

// <q> NRFX_TWIM_DEFAULT_CONFIG_HOLD_BUS_UNINIT  - Enables bus holding after uninit
#define NRFX_TWIM_DEFAULT_CONFIG_HOLD_BUS_UNINIT 0

// <o> NRFX_TWIM_DEFAULT_CONFIG_IRQ_PRIORITY  - Interrupt priority
// <0=> 0 (highest) -> 7 (lowest)
#define NRFX_TWIM_DEFAULT_CONFIG_IRQ_PRIORITY 6


//==========================================================
// <h> nRF_SoftDevice 
//==========================================================

// <e> NRF_SDH_BLE_ENABLED - nrf_sdh_ble - SoftDevice BLE event handler
#define NRF_SDH_BLE_ENABLED 1

// <h> BLE Stack configuration - Stack configuration parameters
// <i> The SoftDevice handler will configure the stack with these parameters when calling @ref nrf_sdh_ble_default_cfg_set.
// <i> Other libraries might depend on these values; keep them up-to-date even if you are not explicitely calling @ref nrf_sdh_ble_default_cfg_set.

// <o> NRF_SDH_BLE_GAP_DATA_LENGTH   <27-251> 
// <i> Requested BLE GAP data length to be negotiated.
#define NRF_SDH_BLE_GAP_DATA_LENGTH 132

// <o> NRF_SDH_BLE_PERIPHERAL_LINK_COUNT - Maximum number of peripheral links. 
#define NRF_SDH_BLE_PERIPHERAL_LINK_COUNT 1

// <o> NRF_SDH_BLE_CENTRAL_LINK_COUNT - Maximum number of central links. 
#define NRF_SDH_BLE_CENTRAL_LINK_COUNT 0

// <o> NRF_SDH_BLE_TOTAL_LINK_COUNT - Total link count. 
// <i> Maximum number of total concurrent connections using the default configuration.
#define NRF_SDH_BLE_TOTAL_LINK_COUNT 1

// <o> NRF_SDH_BLE_GAP_EVENT_LENGTH - GAP event length. 
// <i> The time set aside for this connection on every connection interval in 1.25 ms units.
#define NRF_SDH_BLE_GAP_EVENT_LENGTH 6

// <o> NRF_SDH_BLE_GATT_MAX_MTU_SIZE - Static maximum MTU size. 
#define NRF_SDH_BLE_GATT_MAX_MTU_SIZE 128

// <o> NRF_SDH_BLE_GATTS_ATTR_TAB_SIZE - Attribute Table size in bytes. The size must be a multiple of 4. 
#define NRF_SDH_BLE_GATTS_ATTR_TAB_SIZE 1408

// <o> NRF_SDH_BLE_VS_UUID_COUNT - The number of vendor-specific UUIDs. 
#define NRF_SDH_BLE_VS_UUID_COUNT 2

// <q> NRF_SDH_BLE_SERVICE_CHANGED  - Include the Service Changed characteristic in the Attribute Table.
#define NRF_SDH_BLE_SERVICE_CHANGED 1

//==========================================================
// nrf_sdh - SoftDevice handler
//==========================================================

// <e> NRF_SDH_ENABLED - nrf_sdh - SoftDevice handler
#define NRF_SDH_ENABLED 1

// <h> Dispatch model 
// <i> This setting configures how Stack events are dispatched to the application.
// <o> NRF_SDH_DISPATCH_MODEL
// <0=> NRF_SDH_DISPATCH_MODEL_INTERRUPT: SoftDevice events are passed to the application from the interrupt context.
// <1=> NRF_SDH_DISPATCH_MODEL_APPSH: SoftDevice events are scheduled using @ref app_scheduler.
// <2=> NRF_SDH_DISPATCH_MODEL_POLLING: SoftDevice events are to be fetched manually.
#define NRF_SDH_DISPATCH_MODEL 1

// <h> Clock - SoftDevice clock configuration

// <o> NRF_SDH_CLOCK_LF_SRC  - SoftDevice clock source.
// <0=> NRF_CLOCK_LF_SRC_RC 
// <1=> NRF_CLOCK_LF_SRC_XTAL 
// <2=> NRF_CLOCK_LF_SRC_SYNTH 
#define NRF_SDH_CLOCK_LF_SRC 0

// <o> NRF_SDH_CLOCK_LF_RC_CTIV - SoftDevice calibration timer interval. 
#define NRF_SDH_CLOCK_LF_RC_CTIV 1

// <o> NRF_SDH_CLOCK_LF_RC_TEMP_CTIV - SoftDevice calibration timer interval under constant temperature. 
// <i> How often (in number of calibration intervals) the RC oscillator shall be calibrated
// <i>  if the temperature has not changed.
#define NRF_SDH_CLOCK_LF_RC_TEMP_CTIV 0

// <o> NRF_SDH_CLOCK_LF_ACCURACY  - External clock accuracy used in the LL to compute timing.
// <0=> NRF_CLOCK_LF_ACCURACY_250_PPM 
// <1=> NRF_CLOCK_LF_ACCURACY_500_PPM 
// <2=> NRF_CLOCK_LF_ACCURACY_150_PPM 
// <3=> NRF_CLOCK_LF_ACCURACY_100_PPM 
// <4=> NRF_CLOCK_LF_ACCURACY_75_PPM 
// <5=> NRF_CLOCK_LF_ACCURACY_50_PPM 
// <6=> NRF_CLOCK_LF_ACCURACY_30_PPM 
// <7=> NRF_CLOCK_LF_ACCURACY_20_PPM 
// <8=> NRF_CLOCK_LF_ACCURACY_10_PPM 
// <9=> NRF_CLOCK_LF_ACCURACY_5_PPM 
// <10=> NRF_CLOCK_LF_ACCURACY_2_PPM 
// <11=> NRF_CLOCK_LF_ACCURACY_1_PPM 
#define NRF_SDH_CLOCK_LF_ACCURACY 1


//==========================================================
// nrf_sdh_soc - SoftDevice SoC event handler
//==========================================================

// <e> NRF_SDH_SOC_ENABLED - nrf_sdh_soc - SoftDevice SoC event handler
#define NRF_SDH_SOC_ENABLED 1

// <h> SoC Observers - Observers and priority levels

// <o> NRF_SDH_SOC_OBSERVER_PRIO_LEVELS - Total number of priority levels for SoC observers. 
// <i> This setting configures the number of priority levels available for the SoC event handlers.
// <i> The priority level of a handler determines the order in which it receives events, with respect to other handlers.
#define NRF_SDH_SOC_OBSERVER_PRIO_LEVELS 2


//==========================================================
// SDH Observers - Observers and priority levels
//==========================================================

// <o> NRF_SDH_REQ_OBSERVER_PRIO_LEVELS - Total number of priority levels for request observers. 
// <i> This setting configures the number of priority levels available for the SoftDevice request event handlers.
// <i> The priority level of a handler determines the order in which it receives events, with respect to other handlers.
#define NRF_SDH_REQ_OBSERVER_PRIO_LEVELS 2

// <o> NRF_SDH_STATE_OBSERVER_PRIO_LEVELS - Total number of priority levels for state observers. 
// <i> This setting configures the number of priority levels available for the SoftDevice state event handlers.
// <i> The priority level of a handler determines the order in which it receives events, with respect to other handlers.
#define NRF_SDH_STATE_OBSERVER_PRIO_LEVELS 2

// <o> NRF_SDH_STACK_OBSERVER_PRIO_LEVELS - Total number of priority levels for stack event observers. 
// <i> This setting configures the number of priority levels available for the SoftDevice stack event handlers (ANT, BLE, SoC).
// <i> The priority level of a handler determines the order in which it receives events, with respect to other handlers.
#define NRF_SDH_STACK_OBSERVER_PRIO_LEVELS 2

// <o> NRF_SDH_BLE_OBSERVER_PRIO_LEVELS - Total number of priority levels for BLE observers. 
// <i> This setting configures the number of priority levels available for BLE event handlers.
// <i> The priority level of a handler determines the order in which it receives events, with respect to other handlers.
#define NRF_SDH_BLE_OBSERVER_PRIO_LEVELS 4


//==========================================================
// <h> SoC Observers priorities - Invididual priorities
//==========================================================

// <o> CLOCK_CONFIG_STATE_OBSERVER_PRIO  
// <i> Priority with which state events are dispatched to the Clock driver.
#define CLOCK_CONFIG_STATE_OBSERVER_PRIO 0

// <o> POWER_CONFIG_STATE_OBSERVER_PRIO  
// <i> Priority with which state events are dispatched to the Power driver.
#define POWER_CONFIG_STATE_OBSERVER_PRIO 0

// <o> RNG_CONFIG_STATE_OBSERVER_PRIO  
// <i> Priority with which state events are dispatched to this module.
#define RNG_CONFIG_STATE_OBSERVER_PRIO 0

// <o> NRF_SDH_BLE_STACK_OBSERVER_PRIO  
// <i> This setting configures the priority with which BLE events are processed with respect to other events coming from the stack.
// <i> Modify this setting if you need to have BLE events dispatched before or after other stack events, such as ANT or SoC.
// <i> Zero is the highest priority.
#define NRF_SDH_BLE_STACK_OBSERVER_PRIO 0

// <o> NRF_SDH_SOC_STACK_OBSERVER_PRIO  
// <i> This setting configures the priority with which SoC events are processed with respect to other events coming from the stack.
// <i> Modify this setting if you need to have SoC events dispatched before or after other stack events, such as ANT or BLE.
// <i> Zero is the highest priority.
#define NRF_SDH_SOC_STACK_OBSERVER_PRIO 0

// <o> BLE_ADV_SOC_OBSERVER_PRIO  
// <i> Priority with which SoC events are dispatched to the Advertising module.
#define BLE_ADV_SOC_OBSERVER_PRIO 1

// <o> BLE_ADV_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Advertising module.
#define BLE_ADV_BLE_OBSERVER_PRIO 1

// <o> BLE_ANCS_C_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Apple Notification Service Client.
#define BLE_ANCS_C_BLE_OBSERVER_PRIO 2

// <o> BLE_ANS_C_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Alert Notification Service Client.
#define BLE_ANS_C_BLE_OBSERVER_PRIO 2

// <o> BLE_BAS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Battery Service.
#define BLE_BAS_BLE_OBSERVER_PRIO 2

// <o> BLE_BAS_C_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Battery Service Client.
#define BLE_BAS_C_BLE_OBSERVER_PRIO 2

// <o> BLE_BPS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Blood Pressure Service.
#define BLE_BPS_BLE_OBSERVER_PRIO 2

// <o> BLE_CONN_PARAMS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Connection parameters module.
#define BLE_CONN_PARAMS_BLE_OBSERVER_PRIO 1

// <o> BLE_CONN_STATE_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Connection State module.
#define BLE_CONN_STATE_BLE_OBSERVER_PRIO 0

// <o> BLE_CSCS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Cycling Speed and Cadence Service.
#define BLE_CSCS_BLE_OBSERVER_PRIO 2

// <o> BLE_CTS_C_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Current Time Service Client.
#define BLE_CTS_C_BLE_OBSERVER_PRIO 2

// <o> BLE_DB_DISC_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Database Discovery module.
#define BLE_DB_DISC_BLE_OBSERVER_PRIO 1

// <o> BLE_DFU_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the DFU Service.
#define BLE_DFU_BLE_OBSERVER_PRIO 2

// <o> BLE_DIS_C_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Device Information Client.
#define BLE_DIS_C_BLE_OBSERVER_PRIO 2

// <o> BLE_GLS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Glucose Service.
#define BLE_GLS_BLE_OBSERVER_PRIO 2

// <o> BLE_HIDS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Human Interface Device Service.
#define BLE_HIDS_BLE_OBSERVER_PRIO 2

// <o> BLE_HRS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Heart Rate Service.
#define BLE_HRS_BLE_OBSERVER_PRIO 2

// <o> BLE_HRS_C_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Heart Rate Service Client.
#define BLE_HRS_C_BLE_OBSERVER_PRIO 2

// <o> BLE_HTS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Health Thermometer Service.
#define BLE_HTS_BLE_OBSERVER_PRIO 2

// <o> BLE_IAS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Immediate Alert Service.
#define BLE_IAS_BLE_OBSERVER_PRIO 2

// <o> BLE_IAS_C_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Immediate Alert Service Client.
#define BLE_IAS_C_BLE_OBSERVER_PRIO 2

// <o> BLE_LBS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the LED Button Service.
#define BLE_LBS_BLE_OBSERVER_PRIO 2

// <o> BLE_LBS_C_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the LED Button Service Client.
#define BLE_LBS_C_BLE_OBSERVER_PRIO 2

// <o> BLE_LLS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Link Loss Service.
#define BLE_LLS_BLE_OBSERVER_PRIO 2

// <o> BLE_LNS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Location Navigation Service.
#define BLE_LNS_BLE_OBSERVER_PRIO 2

// <o> BLE_NUS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the UART Service.
#define BLE_NUS_BLE_OBSERVER_PRIO 2

// <o> BLE_NUS_C_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the UART Central Service.
#define BLE_NUS_C_BLE_OBSERVER_PRIO 2

// <o> BLE_OTS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Object transfer service.
#define BLE_OTS_BLE_OBSERVER_PRIO 2

// <o> BLE_OTS_C_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Object transfer service client.
#define BLE_OTS_C_BLE_OBSERVER_PRIO 2

// <o> BLE_RSCS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Running Speed and Cadence Service.
#define BLE_RSCS_BLE_OBSERVER_PRIO 2

// <o> BLE_RSCS_C_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Running Speed and Cadence Client.
#define BLE_RSCS_C_BLE_OBSERVER_PRIO 2

// <o> BLE_TPS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the TX Power Service.
#define BLE_TPS_BLE_OBSERVER_PRIO 2

// <o> BLE_DFU_SOC_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the DFU Service.
#define BLE_DFU_SOC_OBSERVER_PRIO 1

// <o> CLOCK_CONFIG_SOC_OBSERVER_PRIO  
// <i> Priority with which SoC events are dispatched to the Clock driver.
#define CLOCK_CONFIG_SOC_OBSERVER_PRIO 0

// <o> POWER_CONFIG_SOC_OBSERVER_PRIO  
// <i> Priority with which SoC events are dispatched to the Power driver.
#define POWER_CONFIG_SOC_OBSERVER_PRIO 0

// <o> NRF_BLE_BMS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Bond Management Service.
#define NRF_BLE_BMS_BLE_OBSERVER_PRIO 2

// <o> NRF_BLE_CGMS_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Contiuon Glucose Monitoring Service.
#define NRF_BLE_CGMS_BLE_OBSERVER_PRIO 2

// <o> NRF_BLE_ES_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Eddystone module.
#define NRF_BLE_ES_BLE_OBSERVER_PRIO 2

// <o> NRF_BLE_GATTS_C_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the GATT Service Client.
#define NRF_BLE_GATTS_C_BLE_OBSERVER_PRIO 2

// <o> NRF_BLE_GATT_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the GATT module.
#define NRF_BLE_GATT_BLE_OBSERVER_PRIO 1

// <o> NRF_BLE_QWR_BLE_OBSERVER_PRIO  
// <i> Priority with which BLE events are dispatched to the Queued writes module.
#define NRF_BLE_QWR_BLE_OBSERVER_PRIO 2

// <o> NRF_BLE_SCAN_OBSERVER_PRIO  
// <i> Priority for dispatching the BLE events to the Scanning Module.
#define NRF_BLE_SCAN_OBSERVER_PRIO 1

// <o> PM_BLE_OBSERVER_PRIO - Priority with which BLE events are dispatched to the Peer Manager module. 
#define PM_BLE_OBSERVER_PRIO 1


//==========================================================
// <h> nRF_BLE 
//==========================================================

// <q> BLE_ADVERTISING_ENABLED  - ble_advertising - Advertising module
#define BLE_ADVERTISING_ENABLED 1

// <q> BLE_DTM_ENABLED  - ble_dtm - Module for testing RF/PHY using DTM commands
#define BLE_DTM_ENABLED 0

// <q> BLE_RACP_ENABLED  - ble_racp - Record Access Control Point library
#define BLE_RACP_ENABLED 0

// <e> NRF_BLE_CONN_PARAMS_ENABLED - ble_conn_params - Initiating and executing a connection parameters negotiation procedure
#define NRF_BLE_CONN_PARAMS_ENABLED 1

// <o> NRF_BLE_CONN_PARAMS_MAX_SLAVE_LATENCY_DEVIATION - The largest acceptable deviation in slave latency. 
// <i> The largest deviation (+ or -) from the requested slave latency that will not be renegotiated.
#define NRF_BLE_CONN_PARAMS_MAX_SLAVE_LATENCY_DEVIATION 499

// <o> NRF_BLE_CONN_PARAMS_MAX_SUPERVISION_TIMEOUT_DEVIATION - The largest acceptable deviation (in 10 ms units) in supervision timeout. 
// <i> The largest deviation (+ or -, in 10 ms units) from the requested supervision timeout that will not be renegotiated.
#define NRF_BLE_CONN_PARAMS_MAX_SUPERVISION_TIMEOUT_DEVIATION 65535

// <q> NRF_BLE_GATT_ENABLED  - nrf_ble_gatt - GATT module
#define NRF_BLE_GATT_ENABLED 1

// <e> NRF_BLE_QWR_ENABLED - nrf_ble_qwr - Queued writes support module (prepare/execute write)
#define NRF_BLE_QWR_ENABLED 1

// <o> NRF_BLE_QWR_MAX_ATTR - Maximum number of attribute handles that can be registered. This number must be adjusted according to the number of attributes for which Queued Writes will be enabled. If it is zero, the module will reject all Queued Write requests. 
#define NRF_BLE_QWR_MAX_ATTR 1


//==========================================================
// <e> PEER_MANAGER_ENABLED - peer_manager - Peer Manager
//==========================================================

// <e> PEER_MANAGER_ENABLED - peer_manager - Peer Manager
#define PEER_MANAGER_ENABLED 0

// <o> PM_MAX_REGISTRANTS - Number of event handlers that can be registered. 
#define PM_MAX_REGISTRANTS 3

// <o> PM_FLASH_BUFFERS - Number of internal buffers for flash operations. 
// <i> Decrease this value to lower RAM usage.
#define PM_FLASH_BUFFERS 4

// <q> PM_CENTRAL_ENABLED  - Enable/disable central-specific Peer Manager functionality.
// <i> Enable/disable central-specific Peer Manager functionality.
#define PM_CENTRAL_ENABLED 0

// <q> PM_SERVICE_CHANGED_ENABLED  - Enable/disable the service changed management for GATT server in Peer Manager.
// <i> If not using a GATT server, or using a server wihout a service changed characteristic,
// <i> disable this to save code space.
#define PM_SERVICE_CHANGED_ENABLED 1

// <q> PM_PEER_RANKS_ENABLED  - Enable/disable the peer rank management in Peer Manager.
// <i> Set this to false to save code space if not using the peer rank API.
#define PM_PEER_RANKS_ENABLED 0

// <q> PM_LESC_ENABLED  - Enable/disable LESC support in Peer Manager.
// <i> If set to true, you need to call nrf_ble_lesc_request_handler() in the main loop to respond to LESC-related BLE events. If LESC support is not required, set this to false to save code space.
#define PM_LESC_ENABLED 0

// <e> PM_RA_PROTECTION_ENABLED - Enable/disable protection against repeated pairing attempts in Peer Manager.
#define PM_RA_PROTECTION_ENABLED 0

// <o> PM_RA_PROTECTION_TRACKED_PEERS_NUM - Maximum number of peers whose authorization status can be tracked. 
#define PM_RA_PROTECTION_TRACKED_PEERS_NUM 8

// <o> PM_RA_PROTECTION_MIN_WAIT_INTERVAL - Minimum waiting interval (in ms) before a new pairing attempt can be initiated. 
#define PM_RA_PROTECTION_MIN_WAIT_INTERVAL 4000

// <o> PM_RA_PROTECTION_MAX_WAIT_INTERVAL - Maximum waiting interval (in ms) before a new pairing attempt can be initiated. 
#define PM_RA_PROTECTION_MAX_WAIT_INTERVAL 64000

// <o> PM_RA_PROTECTION_REWARD_PERIOD - Reward period (in ms). 
// <i> The waiting interval is gradually decreased when no new failed pairing attempts are made during reward period.
#define PM_RA_PROTECTION_REWARD_PERIOD 10000

// <o> PM_HANDLER_SEC_DELAY_MS - Delay before starting security. 
// <i>  This might be necessary for interoperability reasons, especially as peripheral.
#define PM_HANDLER_SEC_DELAY_MS 0

//==========================================================
// <h> nRF_BLE_Services 
//==========================================================

#define BLE_ANCS_C_ENABLED 0    // <q> BLE_ANCS_C_ENABLED  - ble_ancs_c - Apple Notification Service Client
#define BLE_ANS_C_ENABLED 0     // <q> BLE_ANS_C_ENABLED  - ble_ans_c - Alert Notification Service Client
#define BLE_BAS_C_ENABLED 0     // <q> BLE_BAS_C_ENABLED  - ble_bas_c - Battery Service Client
#define BLE_BAS_ENABLED 0       // <e> BLE_BAS_ENABLED - ble_bas - Battery Service
#define BLE_CSCS_ENABLED 0      // <q> BLE_CSCS_ENABLED  - ble_cscs - Cycling Speed and Cadence Service
#define BLE_CTS_C_ENABLED 0     // <q> BLE_CTS_C_ENABLED  - ble_cts_c - Current Time Service Client
#define BLE_DIS_ENABLED 1       // <q> BLE_DIS_ENABLED  - ble_dis - Device Information Service
#define BLE_GLS_ENABLED 0       // <q> BLE_GLS_ENABLED  - ble_gls - Glucose Service
#define BLE_HIDS_ENABLED 0      // <q> BLE_HIDS_ENABLED  - ble_hids - Human Interface Device Service
#define BLE_HRS_C_ENABLED 0     // <q> BLE_HRS_C_ENABLED  - ble_hrs_c - Heart Rate Service Client
#define BLE_HRS_ENABLED 0       // <q> BLE_HRS_ENABLED  - ble_hrs - Heart Rate Service
#define BLE_HTS_ENABLED 0       // <q> BLE_HTS_ENABLED  - ble_hts - Health Thermometer Service
#define BLE_IAS_C_ENABLED 0     // <q> BLE_IAS_C_ENABLED  - ble_ias_c - Immediate Alert Service Client
#define BLE_IAS_ENABLED 0       // <e> BLE_IAS_ENABLED - ble_ias - Immediate Alert Service
#define BLE_LBS_C_ENABLED 0     // <q> BLE_LBS_C_ENABLED  - ble_lbs_c - Nordic LED Button Service Client
#define BLE_LBS_ENABLED 0       // <q> BLE_LBS_ENABLED  - ble_lbs - LED Button Service
#define BLE_LLS_ENABLED 0       // <q> BLE_LLS_ENABLED  - ble_lls - Link Loss Service
#define BLE_NUS_C_ENABLED 0     // <q> BLE_NUS_C_ENABLED  - ble_nus_c - Nordic UART Central Service
#define BLE_NUS_ENABLED 0       // <e> BLE_NUS_ENABLED - ble_nus - Nordic UART Service
#define BLE_RSCS_ENABLED 0      // <q> BLE_RSCS_ENABLED  - ble_rscs - Running Speed and Cadence Service
#define BLE_TPS_ENABLED 0       // <q> BLE_TPS_ENABLED  - ble_tps - TX Power Service


//==========================================================
// <h> nRF_Core 
//==========================================================

// <e> NRF_MPU_ENABLED - nrf_mpu - Module for MPU
#define NRF_MPU_ENABLED 0

// <q> NRF_MPU_CLI_CMDS  - Enable CLI commands specific to the module.
#define NRF_MPU_CLI_CMDS 0

// <e> NRF_STACK_GUARD_ENABLED - nrf_stack_guard - Stack guard
#define NRF_STACK_GUARD_ENABLED 0

// <o> NRF_STACK_GUARD_CONFIG_SIZE  - Size of the stack guard.
// <5=> 32 bytes 
// <6=> 64 bytes 
// <7=> 128 bytes 
// <8=> 256 bytes 
// <9=> 512 bytes 
// <10=> 1024 bytes 
// <11=> 2048 bytes 
// <12=> 4096 bytes 
#define NRF_STACK_GUARD_CONFIG_SIZE 7


//==========================================================
// <e> FDS_ENABLED - fds - Flash data storage module
//==========================================================

// <e> FDS_ENABLED - fds - Flash data storage module
#define FDS_ENABLED 0

// <o> FDS_VIRTUAL_PAGES - Number of virtual flash pages to use. 
// <i> One of the virtual pages is reserved by the system for garbage collection.
// <i> Therefore, the minimum is two virtual pages: one page to store data and one page to be used by the system for garbage collection.
// <i> The total amount of flash memory that is used by FDS amounts to @ref FDS_VIRTUAL_PAGES * @ref FDS_VIRTUAL_PAGE_SIZE * 4 bytes.
#define FDS_VIRTUAL_PAGES 2

// <o> FDS_VIRTUAL_PAGE_SIZE  - The size of a virtual flash page.
// <i> Expressed in number of 4-byte words.
// <i> By default, a virtual page is the same size as a physical page.
// <i> The size of a virtual page must be a multiple of the size of a physical page.
// <1024=> 1024 
// <2048=> 2048 
#define FDS_VIRTUAL_PAGE_SIZE 1024

//==========================================================
// <h> Backend - Backend configuration
// <i> Configure which nrf_fstorage backend is used by FDS to write to flash.
//==========================================================

// <o> FDS_BACKEND  - FDS flash backend.
// <i> NRF_FSTORAGE_SD uses the nrf_fstorage_sd backend implementation using the SoftDevice API. Use this if you have a SoftDevice present.
// <i> NRF_FSTORAGE_NVMC uses the nrf_fstorage_nvmc implementation. Use this setting if you don't use the SoftDevice.
// <1=> NRF_FSTORAGE_NVMC 
// <2=> NRF_FSTORAGE_SD 
#define FDS_BACKEND 2

// <o> FDS_OP_QUEUE_SIZE - Size of the internal queue. 
// <i> Increase this value if you frequently get synchronous FDS_ERR_NO_SPACE_IN_QUEUES errors.
#define FDS_OP_QUEUE_SIZE 4

// <e> FDS_CRC_CHECK_ON_READ - Enable CRC checks.
// <i> Save a record's CRC when it is written to flash and check it when the record is opened.
// <i> Records with an incorrect CRC can still be 'seen' by the user using FDS functions, but they cannot be opened.
// <i> Additionally, they will not be garbage collected until they are deleted.
//==========================================================
#define FDS_CRC_CHECK_ON_READ 0

// <o> FDS_CRC_CHECK_ON_WRITE  - Perform a CRC check on newly written records.
// <i> Perform a CRC check on newly written records.
// <i> This setting can be used to make sure that the record data was not altered while being written to flash.
// <1=> Enabled 
// <0=> Disabled 
#define FDS_CRC_CHECK_ON_WRITE 0

// <o> FDS_MAX_USERS - Maximum number of callbacks that can be registered. 
#define FDS_MAX_USERS 4


//==========================================================
// <e> NRF_FSTORAGE_ENABLED - nrf_fstorage - Flash abstraction library
//==========================================================
#define NRF_FSTORAGE_ENABLED 1

// <q> NRF_FSTORAGE_PARAM_CHECK_DISABLED  - Disable user input validation
// <i> If selected, use ASSERT to validate user input.
// <i> This effectively removes user input validation in production code.
// <i> Recommended setting: OFF, only enable this setting if size is a major concern.
#define NRF_FSTORAGE_PARAM_CHECK_DISABLED 0

// <h> nrf_fstorage_sd - Implementation using the SoftDevice
// <i> Configuration options for the fstorage implementation using the SoftDevice
// <o> NRF_FSTORAGE_SD_QUEUE_SIZE - Size of the internal queue of operations 
// <i> Increase this value if API calls frequently return the error @ref NRF_ERROR_NO_MEM.
#define NRF_FSTORAGE_SD_QUEUE_SIZE 4

// <o> NRF_FSTORAGE_SD_MAX_RETRIES - Maximum number of attempts at executing an operation when the SoftDevice is busy 
// <i> Increase this value if events frequently return the @ref NRF_ERROR_TIMEOUT error.
// <i> The SoftDevice might fail to schedule flash access due to high BLE activity.
#define NRF_FSTORAGE_SD_MAX_RETRIES 8

// <o> NRF_FSTORAGE_SD_MAX_WRITE_SIZE - Maximum number of bytes to be written to flash in a single operation 
// <i> This value must be a multiple of four.
// <i> Lowering this value can increase the chances of the SoftDevice being able to execute flash operations in between radio activity.
// <i> This value is bound by the maximum number of bytes that can be written to flash in a single call to @ref sd_flash_write.
// <i> That is 1024 bytes for nRF51 ICs and 4096 bytes for nRF52 ICs.
#define NRF_FSTORAGE_SD_MAX_WRITE_SIZE 4096

// <e> NRFX_WDT_ENABLED - nrfx_wdt - WDT peripheral driver
//==========================================================
#define NRFX_WDT_ENABLED 1

// <o> NRFX_WDT_CONFIG_BEHAVIOUR  - WDT behavior in CPU SLEEP or HALT mode
// <1=> Run in SLEEP, Pause in HALT 
// <8=> Pause in SLEEP, Run in HALT 
// <9=> Run in SLEEP and HALT 
// <0=> Pause in SLEEP and HALT 
#define NRFX_WDT_CONFIG_BEHAVIOUR 8

// <o> NRFX_WDT_CONFIG_RELOAD_VALUE - Reload value  <15-4294967295> 
#define NRFX_WDT_CONFIG_RELOAD_VALUE 2000

// <o> NRFX_WDT_CONFIG_IRQ_PRIORITY  - Interrupt priority
// 0 to 7, 0 is highest
#define NRFX_WDT_CONFIG_IRQ_PRIORITY 7


#include "sdk_config_logging.h"
#include "sdk_config_dice.h"

#endif //SDK_CONFIG_H

