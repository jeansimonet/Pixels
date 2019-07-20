//
// Turn logging ON/OFF per library
//

//==========================================================
// nrf_log - Logger
//==========================================================

// <e> NRF_LOG_ENABLED - nrf_log - Logger
#define NRF_LOG_ENABLED 1

// <h> Log message pool - Configuration of log message pool

// <o> NRF_LOG_MSGPOOL_ELEMENT_SIZE - Size of a single element in the pool of memory objects. 
// <i> If a small value is set, then performance of logs processing
// <i> is degraded because data is fragmented. Bigger value impacts
// <i> RAM memory utilization. The size is set to fit a message with
// <i> a timestamp and up to 2 arguments in a single memory object.
#define NRF_LOG_MSGPOOL_ELEMENT_SIZE 64

// <o> NRF_LOG_MSGPOOL_ELEMENT_COUNT - Number of elements in the pool of memory objects 
// <i> If a small value is set, then it may lead to a deadlock
// <i> in certain cases if backend has high latency and holds
// <i> multiple messages for long time. Bigger value impacts
// <i> RAM memory usage.
#define NRF_LOG_MSGPOOL_ELEMENT_COUNT 16

// <q> NRF_LOG_ALLOW_OVERFLOW  - Configures behavior when circular buffer is full.
// <i> If set then oldest logs are overwritten. Otherwise a 
// <i> marker is injected informing about overflow.
#define NRF_LOG_ALLOW_OVERFLOW 1

// <o> NRF_LOG_BUFSIZE  - Size of the buffer for storing logs (in bytes).
// <i> Must be power of 2 and multiple of 4.
// <i> If NRF_LOG_DEFERRED = 0 then buffer size can be reduced to minimum.
// <128=> 128 -> <16384=> 16384
#define NRF_LOG_BUFSIZE 128

// <q> NRF_LOG_CLI_CMDS  - Enable CLI commands for the module.
#define NRF_LOG_CLI_CMDS 0

// <o> NRF_LOG_DEFAULT_LEVEL  - Default Severity level
// <0=> Off 
// <1=> Error 
// <2=> Warning 
// <3=> Info 
// <4=> Debug 
#define NRF_LOG_DEFAULT_LEVEL 3

// <q> NRF_LOG_DEFERRED  - Enable deffered logger.
// <i> Log data is buffered and can be processed in idle.
#define NRF_LOG_DEFERRED 0

// <q> NRF_LOG_FILTERS_ENABLED  - Enable dynamic filtering of logs.
#define NRF_LOG_FILTERS_ENABLED 0

// <o> NRF_LOG_STR_PUSH_BUFFER_SIZE  - Size of the buffer dedicated for strings stored using @ref NRF_LOG_PUSH.
// <16=> 16 -> <1024=> 1024 
#define NRF_LOG_STR_PUSH_BUFFER_SIZE 64

// <e> NRF_LOG_USES_COLORS - If enabled then ANSI escape code for colors is prefixed to every string
#define NRF_LOG_USES_COLORS 0

// <e> NRF_LOG_USES_TIMESTAMP - Enable timestamping
#define NRF_LOG_USES_TIMESTAMP 0

// <o> NRF_LOG_TIMESTAMP_DEFAULT_FREQUENCY - Default frequency of the timestamp (in Hz) or 0 to use app_timer frequency. 
#define NRF_LOG_TIMESTAMP_DEFAULT_FREQUENCY 0


//==========================================================
// nrf_log_str_formatter - Log string formatter
//==========================================================

// <q> NRF_LOG_STR_FORMATTER_TIMESTAMP_FORMAT_ENABLED  - nrf_log_str_formatter - Log string formatter
#define NRF_LOG_STR_FORMATTER_TIMESTAMP_FORMAT_ENABLED 0


#define NRF_LOG_BACKEND_UART_ENABLED 1

#define NRF_LOG_BACKEND_UART_TX_PIN 20
 
#define NRF_LOG_BACKEND_UART_BAUDRATE 30801920
 
#define NRF_LOG_BACKEND_UART_TEMP_BUFFER_SIZE 256

// //==========================================================
// // <h> nRF_Segger_RTT 
// //==========================================================

// // <o> SEGGER_RTT_CONFIG_BUFFER_SIZE_UP - Size of upstream buffer. 
// // <i> Note that either @ref NRF_LOG_BACKEND_RTT_OUTPUT_BUFFER_SIZE
// // <i> or this value is actually used. It depends on which one is bigger.
// #define SEGGER_RTT_CONFIG_BUFFER_SIZE_UP 1024

// // <o> SEGGER_RTT_CONFIG_MAX_NUM_UP_BUFFERS - Size of upstream buffer. 
// #define SEGGER_RTT_CONFIG_MAX_NUM_UP_BUFFERS 2

// // <o> SEGGER_RTT_CONFIG_BUFFER_SIZE_DOWN - Size of upstream buffer. 
// #define SEGGER_RTT_CONFIG_BUFFER_SIZE_DOWN 16

// // <o> SEGGER_RTT_CONFIG_MAX_NUM_DOWN_BUFFERS - Size of upstream buffer. 
// #define SEGGER_RTT_CONFIG_MAX_NUM_DOWN_BUFFERS 2

// // <o> SEGGER_RTT_CONFIG_DEFAULT_MODE  - RTT behavior if the buffer is full.
// // <i> The following modes are supported:
// // <0=> - SKIP  - Do not block, output nothing.
// // <1=> - TRIM  - Do not block, output as much as fits.
// // <2=> - BLOCK - Wait until there is space in the buffer.
// #define SEGGER_RTT_CONFIG_DEFAULT_MODE 0


// //==========================================================
// // nrf_log_backend_rtt - Log RTT backend
// //==========================================================
// // <e> NRF_LOG_BACKEND_RTT_ENABLED - nrf_log_backend_rtt - Log RTT backend
// #define NRF_LOG_BACKEND_RTT_ENABLED 1

// // <o> NRF_LOG_BACKEND_RTT_TEMP_BUFFER_SIZE - Size of buffer for partially processed strings. 
// // <i> Size of the buffer is a trade-off between RAM usage and processing.
// // <i> if buffer is smaller then strings will often be fragmented.
// // <i> It is recommended to use size which will fit typical log and only the
// // <i> longer one will be fragmented.
// #define NRF_LOG_BACKEND_RTT_TEMP_BUFFER_SIZE 64

// // <o> NRF_LOG_BACKEND_RTT_TX_RETRY_DELAY_MS - Period before retrying writing to RTT 
// #define NRF_LOG_BACKEND_RTT_TX_RETRY_DELAY_MS 1

// // <o> NRF_LOG_BACKEND_RTT_TX_RETRY_CNT - Writing to RTT retries. 
// // <i> If RTT fails to accept any new data after retries
// // <i> module assumes that host is not active and on next
// // <i> request it will perform only one write attempt.
// // <i> On successful writing, module assumes that host is active
// // <i> and scheme with retry is applied again.
// #define NRF_LOG_BACKEND_RTT_TX_RETRY_CNT 3


#define NRFX_CLOCK_CONFIG_LOG_ENABLED 0
#define NRFX_GPIOTE_CONFIG_LOG_ENABLED 0
#define NRFX_PRS_CONFIG_LOG_ENABLED 0
#define NRFX_UARTE_CONFIG_LOG_ENABLED 0
#define NRFX_UART_CONFIG_LOG_ENABLED 0
#define NRF_MPU_CONFIG_LOG_ENABLED 0
#define NRF_STACK_GUARD_CONFIG_LOG_ENABLED 0
#define TASK_MANAGER_CONFIG_LOG_ENABLED 0
#define CLOCK_CONFIG_LOG_ENABLED 0
#define COMP_CONFIG_LOG_ENABLED 0
#define GPIOTE_CONFIG_LOG_ENABLED 0
#define LPCOMP_CONFIG_LOG_ENABLED 0
#define MAX3421E_HOST_CONFIG_LOG_ENABLED 0
#define PDM_CONFIG_LOG_ENABLED 0
#define PPI_CONFIG_LOG_ENABLED 0
#define PWM_CONFIG_LOG_ENABLED 0
#define QDEC_CONFIG_LOG_ENABLED 0
#define RNG_CONFIG_LOG_ENABLED 0
#define RNG_CONFIG_RANDOM_NUMBER_LOG_ENABLED 0
#define RTC_CONFIG_LOG_ENABLED 0
#define SPIS_CONFIG_LOG_ENABLED 0
#define SPI_CONFIG_LOG_ENABLED 0
#define TIMER_CONFIG_LOG_ENABLED 0
#define TWIS_CONFIG_LOG_ENABLED 0
#define TWI_CONFIG_LOG_ENABLED 0
#define UART_CONFIG_LOG_ENABLED 0
#define USBD_CONFIG_LOG_ENABLED 0
#define WDT_CONFIG_LOG_ENABLED 0
#define APP_TIMER_CONFIG_LOG_ENABLED 0
#define APP_USBD_CDC_ACM_CONFIG_LOG_ENABLED 0
#define APP_USBD_CONFIG_LOG_ENABLED 0
#define APP_USBD_DUMMY_CONFIG_LOG_ENABLED 0
#define APP_USBD_MSC_CONFIG_LOG_ENABLED 0
#define APP_USBD_NRF_DFU_TRIGGER_CONFIG_LOG_ENABLED 0
#define NRF_ATFIFO_CONFIG_LOG_ENABLED 0
#define NRF_BALLOC_CONFIG_LOG_ENABLED 0
#define NRF_BLOCK_DEV_EMPTY_CONFIG_LOG_ENABLED 0
#define NRF_BLOCK_DEV_QSPI_CONFIG_LOG_ENABLED 0
#define NRF_BLOCK_DEV_RAM_CONFIG_LOG_ENABLED 0
#define NRF_CLI_BLE_UART_CONFIG_LOG_ENABLED 0
#define NRF_CLI_LIBUARTE_CONFIG_LOG_ENABLED 0
#define NRF_CLI_UART_CONFIG_LOG_ENABLED 0
#define NRF_LIBUARTE_CONFIG_LOG_ENABLED 0
#define NRF_MEMOBJ_CONFIG_LOG_ENABLED 0
#define NRF_PWR_MGMT_CONFIG_LOG_ENABLED 0
#define NRF_QUEUE_CONFIG_LOG_ENABLED 0
#define NRF_SDH_ANT_LOG_ENABLED 0
#define NRF_SDH_BLE_LOG_ENABLED 0
#define NRF_SDH_LOG_ENABLED 0
#define NRF_SDH_SOC_LOG_ENABLED 0
#define NRF_SORTLIST_CONFIG_LOG_ENABLED 0
#define NRF_TWI_SENSOR_CONFIG_LOG_ENABLED 0
#define PM_LOG_ENABLED 0
#define SER_HAL_TRANSPORT_CONFIG_LOG_ENABLED 0
#define BLE_BAS_CONFIG_LOG_ENABLED 0
#define BLE_IAS_CONFIG_LOG_ENABLED 0
#define BLE_NUS_CONFIG_LOG_ENABLED 0
#define NRFX_WDT_CONFIG_LOG_ENABLED 0
#define SAADC_CONFIG_LOG_ENABLED 0
#define NRFX_SAADC_CONFIG_LOG_ENABLED 0


#define NRFX_GPIOTE_CONFIG_INFO_COLOR 0
#define NRFX_PRS_CONFIG_INFO_COLOR 0
#define NRFX_UARTE_CONFIG_INFO_COLOR 0
#define NRFX_UART_CONFIG_INFO_COLOR 0
#define NRF_MPU_CONFIG_INFO_COLOR 0
#define NRF_STACK_GUARD_CONFIG_INFO_COLOR 0
#define TASK_MANAGER_CONFIG_INFO_COLOR 0
#define CLOCK_CONFIG_INFO_COLOR 0
#define COMP_CONFIG_INFO_COLOR 0
#define GPIOTE_CONFIG_INFO_COLOR 0
#define LPCOMP_CONFIG_INFO_COLOR 0
#define MAX3421E_HOST_CONFIG_INFO_COLOR 0
#define PDM_CONFIG_INFO_COLOR 0
#define PPI_CONFIG_INFO_COLOR 0
#define PWM_CONFIG_INFO_COLOR 0
#define QDEC_CONFIG_INFO_COLOR 0
#define RNG_CONFIG_INFO_COLOR 0
#define RNG_CONFIG_RANDOM_NUMBER_INFO_COLOR 0
#define RTC_CONFIG_INFO_COLOR 0
#define SAADC_CONFIG_INFO_COLOR 0
#define SPIS_CONFIG_INFO_COLOR 0
#define SPI_CONFIG_INFO_COLOR 0
#define TIMER_CONFIG_INFO_COLOR 0
#define TWIS_CONFIG_INFO_COLOR 0
#define TWI_CONFIG_INFO_COLOR 0
#define UART_CONFIG_INFO_COLOR 0
#define USBD_CONFIG_INFO_COLOR 0
#define WDT_CONFIG_INFO_COLOR 0
#define APP_TIMER_CONFIG_INFO_COLOR 0
#define APP_USBD_CDC_ACM_CONFIG_INFO_COLOR 0
#define APP_USBD_CONFIG_INFO_COLOR 0
#define APP_USBD_DUMMY_CONFIG_INFO_COLOR 0
#define APP_USBD_MSC_CONFIG_INFO_COLOR 0
#define APP_USBD_NRF_DFU_TRIGGER_CONFIG_INFO_COLOR 0
#define NRF_ATFIFO_CONFIG_INFO_COLOR 0
#define NRF_BALLOC_CONFIG_INFO_COLOR 0
#define NRF_BLOCK_DEV_EMPTY_CONFIG_INFO_COLOR 0
#define NRF_BLOCK_DEV_QSPI_CONFIG_INFO_COLOR 0
#define NRF_BLOCK_DEV_RAM_CONFIG_INFO_COLOR 0
#define NRF_CLI_BLE_UART_CONFIG_INFO_COLOR 0
#define NRF_CLI_LIBUARTE_CONFIG_INFO_COLOR 0
#define NRF_CLI_UART_CONFIG_INFO_COLOR 0
#define NRF_LIBUARTE_CONFIG_INFO_COLOR 0
#define NRF_MEMOBJ_CONFIG_INFO_COLOR 0
#define NRF_PWR_MGMT_CONFIG_INFO_COLOR 0
#define NRF_QUEUE_CONFIG_INFO_COLOR 0
#define NRF_SDH_ANT_INFO_COLOR 0
#define NRF_SDH_BLE_INFO_COLOR 0
#define NRF_SDH_INFO_COLOR 0
#define NRF_SDH_SOC_INFO_COLOR 0
#define NRF_SORTLIST_CONFIG_INFO_COLOR 0
#define NRF_TWI_SENSOR_CONFIG_INFO_COLOR 0
#define PM_LOG_INFO_COLOR 0
#define SER_HAL_TRANSPORT_CONFIG_INFO_COLOR 0
#define BLE_BAS_CONFIG_INFO_COLOR 0
#define BLE_IAS_CONFIG_INFO_COLOR 0
#define BLE_NUS_CONFIG_INFO_COLOR 0
#define NRFX_WDT_CONFIG_INFO_COLOR 0
#define NRFX_SAADC_CONFIG_INFO_COLOR 0


#define NRFX_GPIOTE_CONFIG_DEBUG_COLOR 0
#define NRFX_PRS_CONFIG_DEBUG_COLOR 0
#define NRFX_UARTE_CONFIG_DEBUG_COLOR 0
#define NRFX_UART_CONFIG_DEBUG_COLOR 0
#define NRF_MPU_CONFIG_DEBUG_COLOR 0
#define NRF_STACK_GUARD_CONFIG_DEBUG_COLOR 0
#define TASK_MANAGER_CONFIG_DEBUG_COLOR 0
#define CLOCK_CONFIG_DEBUG_COLOR 0
#define COMP_CONFIG_DEBUG_COLOR 0
#define GPIOTE_CONFIG_DEBUG_COLOR 0
#define LPCOMP_CONFIG_DEBUG_COLOR 0
#define MAX3421E_HOST_CONFIG_DEBUG_COLOR 0
#define PDM_CONFIG_DEBUG_COLOR 0
#define PPI_CONFIG_DEBUG_COLOR 0
#define PWM_CONFIG_DEBUG_COLOR 0
#define QDEC_CONFIG_DEBUG_COLOR 0
#define RNG_CONFIG_DEBUG_COLOR 0
#define RNG_CONFIG_RANDOM_NUMBER_DEBUG_COLOR 0
#define RTC_CONFIG_DEBUG_COLOR 0
#define SAADC_CONFIG_DEBUG_COLOR 0
#define SPIS_CONFIG_DEBUG_COLOR 0
#define SPI_CONFIG_DEBUG_COLOR 0
#define TIMER_CONFIG_DEBUG_COLOR 0
#define TWIS_CONFIG_DEBUG_COLOR 0
#define TWI_CONFIG_DEBUG_COLOR 0
#define UART_CONFIG_DEBUG_COLOR 0
#define USBD_CONFIG_DEBUG_COLOR 0
#define WDT_CONFIG_DEBUG_COLOR 0
#define APP_TIMER_CONFIG_DEBUG_COLOR 0
#define APP_USBD_CDC_ACM_CONFIG_DEBUG_COLOR 0
#define APP_USBD_CONFIG_DEBUG_COLOR 0
#define APP_USBD_DUMMY_CONFIG_DEBUG_COLOR 0
#define APP_USBD_MSC_CONFIG_DEBUG_COLOR 0
#define APP_USBD_NRF_DFU_TRIGGER_CONFIG_DEBUG_COLOR 0
#define NRF_ATFIFO_CONFIG_DEBUG_COLOR 0
#define NRF_BALLOC_CONFIG_DEBUG_COLOR 0
#define NRF_BLOCK_DEV_EMPTY_CONFIG_DEBUG_COLOR 0
#define NRF_BLOCK_DEV_QSPI_CONFIG_DEBUG_COLOR 0
#define NRF_BLOCK_DEV_RAM_CONFIG_DEBUG_COLOR 0
#define NRF_CLI_BLE_UART_CONFIG_DEBUG_COLOR 0
#define NRF_CLI_LIBUARTE_CONFIG_DEBUG_COLOR 0
#define NRF_CLI_UART_CONFIG_DEBUG_COLOR 0
#define NRF_LIBUARTE_CONFIG_DEBUG_COLOR 0
#define NRF_MEMOBJ_CONFIG_DEBUG_COLOR 0
#define NRF_PWR_MGMT_CONFIG_DEBUG_COLOR 0
#define NRF_QUEUE_CONFIG_DEBUG_COLOR 0
#define NRF_SDH_ANT_DEBUG_COLOR 0
#define NRF_SDH_BLE_DEBUG_COLOR 0
#define NRF_SDH_DEBUG_COLOR 0
#define NRF_SDH_SOC_DEBUG_COLOR 0
#define NRF_SORTLIST_CONFIG_DEBUG_COLOR 0
#define NRF_TWI_SENSOR_CONFIG_DEBUG_COLOR 0
#define PM_LOG_DEBUG_COLOR 0
#define SER_HAL_TRANSPORT_CONFIG_DEBUG_COLOR 0
#define BLE_BAS_CONFIG_DEBUG_COLOR 0
#define BLE_IAS_CONFIG_DEBUG_COLOR 0
#define BLE_NUS_CONFIG_DEBUG_COLOR 0
#define NRFX_WDT_CONFIG_DEBUG_COLOR 0
#define NRFX_SAADC_CONFIG_DEBUG_COLOR 0


#define NRFX_GPIOTE_CONFIG_LOG_LEVEL 3
#define NRFX_PRS_CONFIG_LOG_LEVEL 3
#define NRFX_UARTE_CONFIG_LOG_LEVEL 3
#define NRFX_UART_CONFIG_LOG_LEVEL 3
#define NRF_MPU_CONFIG_LOG_LEVEL 3
#define NRF_STACK_GUARD_CONFIG_LOG_LEVEL 3
#define TASK_MANAGER_CONFIG_LOG_LEVEL 3
#define CLOCK_CONFIG_LOG_LEVEL 3
#define COMP_CONFIG_LOG_LEVEL 3
#define GPIOTE_CONFIG_LOG_LEVEL 3
#define LPCOMP_CONFIG_LOG_LEVEL 3
#define MAX3421E_HOST_CONFIG_LOG_LEVEL 3
#define PDM_CONFIG_LOG_LEVEL 3
#define PPI_CONFIG_LOG_LEVEL 3
#define PWM_CONFIG_LOG_LEVEL 3
#define QDEC_CONFIG_LOG_LEVEL 3
#define RNG_CONFIG_LOG_LEVEL 3
#define RNG_CONFIG_RANDOM_NUMBER_LOG_LEVEL 3
#define RTC_CONFIG_LOG_LEVEL 3
#define SAADC_CONFIG_LOG_LEVEL 3
#define SPIS_CONFIG_LOG_LEVEL 3
#define SPI_CONFIG_LOG_LEVEL 3
#define TIMER_CONFIG_LOG_LEVEL 3
#define TWIS_CONFIG_LOG_LEVEL 3
#define TWI_CONFIG_LOG_LEVEL 3
#define UART_CONFIG_LOG_LEVEL 3
#define USBD_CONFIG_LOG_LEVEL 3
#define WDT_CONFIG_LOG_LEVEL 3
#define APP_TIMER_CONFIG_LOG_LEVEL 3
#define APP_USBD_CDC_ACM_CONFIG_LOG_LEVEL 3
#define APP_USBD_CONFIG_LOG_LEVEL 3
#define APP_USBD_DUMMY_CONFIG_LOG_LEVEL 3
#define APP_USBD_MSC_CONFIG_LOG_LEVEL 3
#define APP_USBD_NRF_DFU_TRIGGER_CONFIG_LOG_LEVEL 3
#define NRF_ATFIFO_CONFIG_LOG_LEVEL 4
#define NRF_BALLOC_CONFIG_LOG_LEVEL 3
#define NRF_BLOCK_DEV_EMPTY_CONFIG_LOG_LEVEL 3
#define NRF_BLOCK_DEV_QSPI_CONFIG_LOG_LEVEL 3
#define NRF_BLOCK_DEV_RAM_CONFIG_LOG_LEVEL 3
#define NRF_CLI_BLE_UART_CONFIG_LOG_LEVEL 3
#define NRF_CLI_LIBUARTE_CONFIG_LOG_LEVEL 3
#define NRF_CLI_UART_CONFIG_LOG_LEVEL 3
#define NRF_LIBUARTE_CONFIG_LOG_LEVEL 3
#define NRF_MEMOBJ_CONFIG_LOG_LEVEL 3
#define NRF_PWR_MGMT_CONFIG_LOG_LEVEL 3
#define NRF_QUEUE_CONFIG_LOG_LEVEL 3
#define NRF_SDH_ANT_LOG_LEVEL 3
#define NRF_SDH_BLE_LOG_LEVEL 3
#define NRF_SDH_LOG_LEVEL 3
#define NRF_SDH_SOC_LOG_LEVEL 3
#define NRF_SORTLIST_CONFIG_LOG_LEVEL 3
#define NRF_TWI_SENSOR_CONFIG_LOG_LEVEL 3
#define PM_LOG_LEVEL 3
#define SER_HAL_TRANSPORT_CONFIG_LOG_LEVEL 3
#define BLE_BAS_CONFIG_LOG_LEVEL 3
#define BLE_IAS_CONFIG_LOG_LEVEL 3
#define BLE_NUS_CONFIG_LOG_LEVEL 3
#define NRFX_WDT_CONFIG_LOG_LEVEL 3
#define NRFX_SAADC_CONFIG_LOG_LEVEL 3
