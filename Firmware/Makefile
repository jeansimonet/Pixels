PROJECT_NAME     := Firmware
TARGETS          := firmware
OUTPUT_DIRECTORY := _build

SDK_ROOT := C:/nRF5_SDK
PROJ_DIR := .

$(OUTPUT_DIRECTORY)/firmware.out: \
	LINKER_SCRIPT  := Firmware.ld

# Source files common to all targets
SRC_FILES += \
	$(SDK_ROOT)/modules/nrfx/mdk/gcc_startup_nrf52810.S \
	$(SDK_ROOT)/modules/nrfx/mdk/system_nrf52810.c \
	$(SDK_ROOT)/modules/nrfx/drivers/src/nrfx_gpiote.c \
	$(SDK_ROOT)/components/ble/ble_advertising/ble_advertising.c \
	$(SDK_ROOT)/components/ble/ble_services/ble_lbs/ble_lbs.c \
	$(SDK_ROOT)/components/ble/ble_services/ble_dfu/ble_dfu.c \
	$(SDK_ROOT)/components/ble/ble_services/ble_dfu/ble_dfu_unbonded.c \
	$(SDK_ROOT)/components/ble/common/ble_advdata.c \
	$(SDK_ROOT)/components/ble/common/ble_conn_params.c \
	$(SDK_ROOT)/components/ble/common/ble_conn_state.c \
	$(SDK_ROOT)/components/ble/common/ble_srv_common.c \
	$(SDK_ROOT)/components/ble/nrf_ble_gatt/nrf_ble_gatt.c \
	$(SDK_ROOT)/components/ble/nrf_ble_qwr/nrf_ble_qwr.c \
	$(SDK_ROOT)/components/ble/peer_manager/peer_manager.c \
	$(SDK_ROOT)/components/ble/peer_manager/peer_manager_handler.c \
	$(SDK_ROOT)/components/ble/peer_manager/peer_id.c \
	$(SDK_ROOT)/components/ble/peer_manager/gatt_cache_manager.c \
	$(SDK_ROOT)/components/ble/peer_manager/gatts_cache_manager.c \
	$(SDK_ROOT)/components/ble/peer_manager/id_manager.c \
	$(SDK_ROOT)/components/ble/peer_manager/peer_data_storage.c \
	$(SDK_ROOT)/components/ble/peer_manager/peer_database.c \
	$(SDK_ROOT)/components/ble/peer_manager/peer_id.c \
	$(SDK_ROOT)/components/ble/peer_manager/peer_manager.c \
	$(SDK_ROOT)/components/ble/peer_manager/peer_manager_handler.c \
	$(SDK_ROOT)/components/ble/peer_manager/pm_buffer.c \
	$(SDK_ROOT)/components/ble/peer_manager/security_dispatcher.c \
	$(SDK_ROOT)/components/ble/peer_manager/security_manager.c \
	$(SDK_ROOT)/components/boards/boards.c \
	$(SDK_ROOT)/components/libraries/atomic_fifo/nrf_atfifo.c \
	$(SDK_ROOT)/components/libraries/atomic_flags/nrf_atflags.c \
	$(SDK_ROOT)/components/libraries/bootloader/dfu/nrf_dfu_svci.c \
	$(SDK_ROOT)/components/libraries/fds/fds.c \
	$(SDK_ROOT)/components/libraries/fstorage/nrf_fstorage.c \
	$(SDK_ROOT)/components/libraries/fstorage/nrf_fstorage_sd.c \
	$(SDK_ROOT)/components/libraries/log/src/nrf_log_backend_rtt.c \
	$(SDK_ROOT)/components/libraries/log/src/nrf_log_backend_serial.c \
	$(SDK_ROOT)/components/libraries/log/src/nrf_log_default_backends.c \
	$(SDK_ROOT)/components/libraries/log/src/nrf_log_frontend.c \
	$(SDK_ROOT)/components/libraries/log/src/nrf_log_str_formatter.c \
	$(SDK_ROOT)/components/libraries/bsp/bsp.c \
	$(SDK_ROOT)/components/libraries/button/app_button.c \
	$(SDK_ROOT)/components/libraries/util/app_util_platform.c \
	$(SDK_ROOT)/components/libraries/pwr_mgmt/nrf_pwr_mgmt.c \
	$(SDK_ROOT)/components/libraries/memobj/nrf_memobj.c \
	$(SDK_ROOT)/components/libraries/ringbuf/nrf_ringbuf.c \
	$(SDK_ROOT)/components/libraries/balloc/nrf_balloc.c \
	$(SDK_ROOT)/components/libraries/strerror/nrf_strerror.c \
	$(SDK_ROOT)/components/libraries/scheduler/app_scheduler.c \
	$(SDK_ROOT)/components/libraries/atomic/nrf_atomic.c \
	$(SDK_ROOT)/components/libraries/timer/app_timer.c \
	$(SDK_ROOT)/components/libraries/util/app_error.c \
	$(SDK_ROOT)/components/libraries/util/app_error_weak.c \
	$(SDK_ROOT)/components/libraries/util/app_error_handler_gcc.c \
	$(SDK_ROOT)/components/libraries/experimental_section_vars/nrf_section_iter.c \
	$(SDK_ROOT)/components/softdevice/common/nrf_sdh.c \
	$(SDK_ROOT)/components/softdevice/common/nrf_sdh_ble.c \
	$(SDK_ROOT)/components/softdevice/common/nrf_sdh_soc.c \
	$(SDK_ROOT)/external/segger_rtt/SEGGER_RTT.c \
	$(SDK_ROOT)/external/segger_rtt/SEGGER_RTT_printf.c \
	$(SDK_ROOT)/external/segger_rtt/SEGGER_RTT_Syscalls_GCC.c \
	$(SDK_ROOT)/external/fprintf/nrf_fprintf.c \
	$(SDK_ROOT)/external/fprintf/nrf_fprintf_format.c \
	$(SDK_ROOT)/integration/nrfx/legacy/nrf_drv_twi.c \
	$(SDK_ROOT)/integration/nrfx/legacy/nrf_drv_twi.c \
	$(SDK_ROOT)/integration/nrfx/legacy/nrf_drv_clock.c \
	$(SDK_ROOT)/modules/nrfx/drivers/src/nrfx_twim.c \
	$(SDK_ROOT)/modules/nrfx/drivers/src/nrfx_gpiote.c \
	$(SDK_ROOT)/modules/nrfx/drivers/src/nrfx_clock.c \
	$(SDK_ROOT)/modules/nrfx/drivers/src/nrfx_power_clock.c \
	$(SDK_ROOT)/modules/nrfx/drivers/src/prs/nrfx_prs.c \
	$(PROJ_DIR)/src/main.cpp \
	$(PROJ_DIR)/src/AccelController.cpp \
	$(PROJ_DIR)/src/Accelerometer.cpp \
	$(PROJ_DIR)/src/Adafruit_DotStar.cpp \
	$(PROJ_DIR)/src/Animation.cpp \
	$(PROJ_DIR)/src/AnimController.cpp \
	$(PROJ_DIR)/src/APA102LEDs.cpp \
	$(PROJ_DIR)/src/BluetoothMessage.cpp \
	$(PROJ_DIR)/src/I2C.cpp \
	$(PROJ_DIR)/src/LEDs.cpp \
	$(PROJ_DIR)/src/Rainbow.cpp \
	$(PROJ_DIR)/src/Utils.cpp \
	# $(SDK_ROOT)/components/ble/peer_manager/peer_data_storage.c \
	# $(SDK_ROOT)/components/ble/peer_manager/peer_database.c \
	# $(SDK_ROOT)/components/ble/peer_manager/peer_id.c \
	# $(SDK_ROOT)/components/ble/peer_manager/peer_manager.c \
	# $(SDK_ROOT)/components/ble/peer_manager/peer_manager_handler.c \
	# $(SDK_ROOT)/components/ble/peer_manager/pm_buffer.c \
	# $(SDK_ROOT)/components/ble/peer_manager/security_dispatcher.c \
	# $(SDK_ROOT)/components/ble/peer_manager/security_manager.c \
	# $(SDK_ROOT)/components/ble/peer_manager/gatt_cache_manager.c \
	# $(SDK_ROOT)/components/ble/peer_manager/gatts_cache_manager.c \
	# $(SDK_ROOT)/components/ble/peer_manager/id_manager.c \


# Include folders common to all targets
INC_FOLDERS += \
	$(PROJ_DIR) \
	$(PROJ_DIR)/src \
	$(PROJ_DIR)/src_toconvert \
	$(SDK_ROOT) \
	$(SDK_ROOT)/modules/nrfx \
	$(SDK_ROOT)/modules/nrfx/hal \
	$(SDK_ROOT)/modules/nrfx/mdk \
	$(SDK_ROOT)/modules/nrfx/soc \
	$(SDK_ROOT)/modules/nrfx/drivers/include \
	$(SDK_ROOT)/components/ble/common \
	$(SDK_ROOT)/components/ble/ble_advertising \
	$(SDK_ROOT)/components/ble/ble_services \
	$(SDK_ROOT)/components/ble/ble_services/ble_lbs \
	$(SDK_ROOT)/components/ble/ble_services/ble_dfu \
	$(SDK_ROOT)/components/ble/nrf_ble_gatt \
	$(SDK_ROOT)/components/ble/nrf_ble_qwr \
	$(SDK_ROOT)/components/ble/peer_manager \
	$(SDK_ROOT)/components/boards \
	$(SDK_ROOT)/components/softdevice/common \
	$(SDK_ROOT)/components/softdevice/mbr/nrf52810/headers \
	$(SDK_ROOT)/components/softdevice/s112/headers \
	$(SDK_ROOT)/components/libraries \
	$(SDK_ROOT)/components/libraries/bootloader \
	$(SDK_ROOT)/components/libraries/bootloader/dfu \
	$(SDK_ROOT)/components/libraries/bootloader/ble_dfu \
	$(SDK_ROOT)/components/libraries/atomic_flags \
	$(SDK_ROOT)/components/libraries/util \
	$(SDK_ROOT)/components/libraries/bsp \
	$(SDK_ROOT)/components/libraries/button \
	$(SDK_ROOT)/components/libraries/fds \
	$(SDK_ROOT)/components/libraries/fstorage \
	$(SDK_ROOT)/components/libraries/memobj \
	$(SDK_ROOT)/components/libraries/ringbuf \
	$(SDK_ROOT)/components/libraries/balloc \
	$(SDK_ROOT)/components/libraries/strerror \
	$(SDK_ROOT)/components/libraries/log \
	$(SDK_ROOT)/components/libraries/log/src \
	$(SDK_ROOT)/components/libraries/atomic \
	$(SDK_ROOT)/components/libraries/atomic_fifo \
	$(SDK_ROOT)/components/libraries/mutex \
	$(SDK_ROOT)/components/libraries/timer \
	$(SDK_ROOT)/components/libraries/delay \
	$(SDK_ROOT)/components/libraries/pwr_mgmt \
	$(SDK_ROOT)/components/libraries/experimental_section_vars \
	$(SDK_ROOT)/components/libraries/scheduler \
	$(SDK_ROOT)/components/libraries/sensorsim \
	$(SDK_ROOT)/components/libraries/svc \
	$(SDK_ROOT)/components/toolchain/cmsis/include \
	$(SDK_ROOT)/integration/nrfx \
	$(SDK_ROOT)/integration/nrfx/legacy \
	$(SDK_ROOT)/external/fprintf \
	$(SDK_ROOT)/external/segger_rtt \
	# $(RTOS_DIR)/Source/include \
	# $(RTOS_DIR)/config \
	# $(RTOS_DIR)/portable/GCC/nrf52 \
	# $(RTOS_DIR)/portable/CMSIS/nrf52 \
	# $(ARDUINO_CORE) \
	# $(ARDUINO_CORE)/cmsis/include \
	# $(ARDUINO_CORE)/sysview/SEGGER \
	# $(ARDUINO_CORE)/sysview/Config \
	# $(ARDUINO_CORE)/usb \
	# $(ARDUINO_CORE)/usb/tinyusb/src \
	# $(ARDUINO_ROOT)/libraries/Wire \
	# $(ARDUINO_ROOT)/libraries/SPI \
	# $(ARDUINO_ROOT)/


# Libraries common to all targets
LIB_FILES += \

# Optimization flags
OPT = -Os -g3
#OPT = -O0 -g3
# Uncomment the line below to enable link time optimization
OPT += -flto

# C flags common to all targets
CFLAGS += $(OPT)
CFLAGS += -DBL_SETTINGS_ACCESS_ONLY
CFLAGS += -DNRF52_SERIES
CFLAGS += -DBOARD_CUSTOM
# CFLAGS += -DDEBUG
# CFLAGS += -DDEBUG_NRF
CFLAGS += -DCONFIG_GPIO_AS_PINRESET
CFLAGS += -DFLOAT_ABI_SOFT
CFLAGS += -DNRF52810_XXAA
CFLAGS += -DNRF_SD_BLE_API_VERSION=6
CFLAGS += -DS112
CFLAGS += -DSOFTDEVICE_PRESENT
CFLAGS += -DSWI_DISABLE0
CFLAGS += -DRGB_LED
CFLAGS += -DNRF_DFU_SVCI_ENABLED
CFLAGS += -DNRF_DFU_TRANSPORT_BLE=1
CFLAGS += -mcpu=cortex-m4
CFLAGS += -mthumb -mabi=aapcs
CFLAGS += -Wall
CFLAGS += -mfloat-abi=soft
# keep every function in a separate section, this allows linker to discard unused ones
CFLAGS += -ffunction-sections -fdata-sections -fno-strict-aliasing
CFLAGS += -fno-builtin -fshort-enums
CFLAGS += -DDEVELOP_IN_NRF52832
CFLAGS += -DNRF52_PAN_74

# Debug
#CFLAGS += -DDEBUG

# C++ flags common to all targets
CXXFLAGS += $(OPT)
CXXFLAGS += -fno-rtti
CXXFLAGS += -fno-exceptions

# Assembler flags common to all targets
ASMFLAGS += -g3
ASMFLAGS += -DBL_SETTINGS_ACCESS_ONLY
ASMFLAGS += -mcpu=cortex-m4
ASMFLAGS += -mthumb -mabi=aapcs
ASMFLAGS += -mfloat-abi=soft
# ASMFLAGS += -DDEBUG
# ASMFLAGS += -DDEBUG_NRF
ASMFLAGS += -DNRF52_SERIES
ASMFLAGS += -DBOARD_CUSTOM
ASMFLAGS += -DCONFIG_GPIO_AS_PINRESET
ASMFLAGS += -DFLOAT_ABI_SOFT
ASMFLAGS += -DNRF52810_XXAA
ASMFLAGS += -DNRF_DFU_SVCI_ENABLED
ASMFLAGS += -DNRF_DFU_TRANSPORT_BLE=1
ASMFLAGS += -DNRF_SD_BLE_API_VERSION=6
ASMFLAGS += -DS112
ASMFLAGS += -DSOFTDEVICE_PRESENT
ASMFLAGS += -DSWI_DISABLE0
ASMFLAGS += -DRGB_LED
ASMFLAGS += -D_CONSOLE
ASMFLAGS += -DDEVELOP_IN_NRF52832
ASMFLAGS += -DNRF52_PAN_74

# Linker flags
LDFLAGS += $(OPT)
LDFLAGS += -mthumb -mabi=aapcs -L$(SDK_ROOT)/modules/nrfx/mdk -T$(LINKER_SCRIPT)
LDFLAGS += -mcpu=cortex-m4
# let linker dump unused sections
LDFLAGS += -Wl,--gc-sections
# use newlib in nano version
LDFLAGS += --specs=nano.specs
#LDFLAGS += -u _printf_float

firmware: CFLAGS += -D__HEAP_SIZE=2048
firmware: CFLAGS += -D__STACK_SIZE=2048
firmware: ASMFLAGS += -D__HEAP_SIZE=2048
firmware: ASMFLAGS += -D__STACK_SIZE=2048

# Add standard libraries at the very end of the linker input, after all objects
# that may need symbols provided by these libraries.
LIB_FILES += -lc -lnosys -lm


.PHONY: default

# Default target - first one defined
default: firmware
 
TEMPLATE_PATH := $(SDK_ROOT)/components/toolchain/gcc

include $(TEMPLATE_PATH)/Makefile.common

$(foreach target, $(TARGETS), $(call define_target, $(target)))

.PHONY: flash erase zip

reset:
	nrfjprog -f nrf52 -s 801001366 --reset

erase:
	nrfjprog -f nrf52 -s 801001366 --eraseall

zip: default
	nrfutil pkg generate --application $(OUTPUT_DIRECTORY)/firmware.hex --application-version 0xff --hw-version 52 --key-file private.pem --sd-req 0xB0 $(OUTPUT_DIRECTORY)/firmware.zip

settings: default
	nrfutil settings generate --family NRF52810 --application $(OUTPUT_DIRECTORY)/firmware.hex --application-version 0xff --bootloader-version 0xff --bl-settings-version 1 $(OUTPUT_DIRECTORY)/firmware_settings.hex

# Flash the program
flash: settings zip
	@echo Flashing: $(OUTPUT_DIRECTORY)/firmware.hex
	nrfjprog -f nrf52 -s 801001366 --program $(OUTPUT_DIRECTORY)/firmware.hex --sectorerase
	nrfjprog -f nrf52 -s 801001366 --program $(OUTPUT_DIRECTORY)/firmware_settings.hex --sectorerase
	nrfjprog -f nrf52 -s 801001366 --reset
