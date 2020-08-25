PROJECT_NAME     := Firmware
TARGETS          := firmware
OUTPUT_DIRECTORY := _build
PUBLISH_DIRECTORY := binaries

VERSION			 := 08_13

SDK_ROOT := C:/nRF5_SDK
PROJ_DIR := .

$(OUTPUT_DIRECTORY)/firmware.out: \
	LINKER_SCRIPT  := Firmware.ld
 
# Source files common to all targets
SRC_FILES += \
	$(SDK_ROOT)/components/ble/ble_advertising/ble_advertising.c \
	$(SDK_ROOT)/components/ble/ble_services/ble_dfu/ble_dfu.c \
	$(SDK_ROOT)/components/ble/ble_services/ble_dfu/ble_dfu_unbonded.c \
	$(SDK_ROOT)/components/ble/common/ble_advdata.c \
	$(SDK_ROOT)/components/ble/common/ble_conn_params.c \
	$(SDK_ROOT)/components/ble/common/ble_conn_state.c \
	$(SDK_ROOT)/components/ble/common/ble_srv_common.c \
	$(SDK_ROOT)/components/ble/nrf_ble_gatt/nrf_ble_gatt.c \
	$(SDK_ROOT)/components/ble/nrf_ble_qwr/nrf_ble_qwr.c \
	$(SDK_ROOT)/components/ble/peer_manager/gatt_cache_manager.c \
	$(SDK_ROOT)/components/ble/peer_manager/gatts_cache_manager.c \
	$(SDK_ROOT)/components/ble/peer_manager/id_manager.c \
	$(SDK_ROOT)/components/boards/boards.c \
	$(SDK_ROOT)/components/libraries/atomic/nrf_atomic.c \
	$(SDK_ROOT)/components/libraries/atomic_fifo/nrf_atfifo.c \
	$(SDK_ROOT)/components/libraries/atomic_flags/nrf_atflags.c \
	$(SDK_ROOT)/components/libraries/balloc/nrf_balloc.c \
	$(SDK_ROOT)/components/libraries/bootloader/dfu/nrf_dfu_svci.c \
	$(SDK_ROOT)/components/libraries/bsp/bsp.c \
	$(SDK_ROOT)/components/libraries/experimental_section_vars/nrf_section_iter.c \
	$(SDK_ROOT)/components/libraries/fds/fds.c \
	$(SDK_ROOT)/components/libraries/fstorage/nrf_fstorage.c \
	$(SDK_ROOT)/components/libraries/fstorage/nrf_fstorage_sd.c \
	$(SDK_ROOT)/components/libraries/log/src/nrf_log_backend_uart.c \
	$(SDK_ROOT)/components/libraries/log/src/nrf_log_backend_serial.c \
	$(SDK_ROOT)/components/libraries/log/src/nrf_log_default_backends.c \
	$(SDK_ROOT)/components/libraries/log/src/nrf_log_frontend.c \
	$(SDK_ROOT)/components/libraries/log/src/nrf_log_str_formatter.c \
	$(SDK_ROOT)/components/libraries/memobj/nrf_memobj.c \
	$(SDK_ROOT)/components/libraries/pwr_mgmt/nrf_pwr_mgmt.c \
	$(SDK_ROOT)/components/libraries/queue/nrf_queue.c \
	$(SDK_ROOT)/components/libraries/ringbuf/nrf_ringbuf.c \
	$(SDK_ROOT)/components/libraries/scheduler/app_scheduler.c \
	$(SDK_ROOT)/components/libraries/strerror/nrf_strerror.c \
	$(SDK_ROOT)/components/libraries/timer/app_timer.c \
	$(SDK_ROOT)/components/libraries/util/app_error.c \
	$(SDK_ROOT)/components/libraries/util/app_error_weak.c \
	$(SDK_ROOT)/components/libraries/util/app_error_handler_gcc.c \
	$(SDK_ROOT)/components/libraries/util/app_util_platform.c \
	$(SDK_ROOT)/components/softdevice/common/nrf_sdh.c \
	$(SDK_ROOT)/components/softdevice/common/nrf_sdh_ble.c \
	$(SDK_ROOT)/components/softdevice/common/nrf_sdh_soc.c \
	$(SDK_ROOT)/external/fprintf/nrf_fprintf.c \
	$(SDK_ROOT)/external/fprintf/nrf_fprintf_format.c \
	$(SDK_ROOT)/integration/nrfx/legacy/nrf_drv_clock.c \
	$(SDK_ROOT)/integration/nrfx/legacy/nrf_drv_twi.c \
	$(SDK_ROOT)/integration/nrfx/legacy/nrf_drv_uart.c \
	$(SDK_ROOT)/modules/nrfx/drivers/src/nrfx_clock.c \
	$(SDK_ROOT)/modules/nrfx/drivers/src/nrfx_power_clock.c \
	$(SDK_ROOT)/modules/nrfx/drivers/src/nrfx_gpiote.c \
	$(SDK_ROOT)/modules/nrfx/drivers/src/nrfx_saadc.c \
	$(SDK_ROOT)/modules/nrfx/drivers/src/nrfx_twim.c \
	$(SDK_ROOT)/modules/nrfx/drivers/src/nrfx_uarte.c \
	$(SDK_ROOT)/modules/nrfx/drivers/src/nrfx_wdt.c \
	$(SDK_ROOT)/modules/nrfx/drivers/src/prs/nrfx_prs.c \
	$(SDK_ROOT)/modules/nrfx/mdk/gcc_startup_nrf52810.S \
	$(SDK_ROOT)/modules/nrfx/mdk/system_nrf52810.c \
	$(PROJ_DIR)/src/die_init.cpp \
	$(PROJ_DIR)/src/die_main.cpp \
	$(PROJ_DIR)/src/animations/animation.cpp \
	$(PROJ_DIR)/src/animations/animation_simple.cpp \
	$(PROJ_DIR)/src/animations/animation_keyframed.cpp \
	$(PROJ_DIR)/src/animations/animation_rainbow.cpp \
	$(PROJ_DIR)/src/animations/animation_gradientpattern.cpp \
	$(PROJ_DIR)/src/animations/keyframes.cpp \
	$(PROJ_DIR)/src/behaviors/action.cpp \
	$(PROJ_DIR)/src/behaviors/condition.cpp \
	$(PROJ_DIR)/src/bluetooth/bluetooth_stack.cpp \
	$(PROJ_DIR)/src/bluetooth/bluetooth_messages.cpp \
	$(PROJ_DIR)/src/bluetooth/bluetooth_message_service.cpp \
	$(PROJ_DIR)/src/bluetooth/bulk_data_transfer.cpp \
	$(PROJ_DIR)/src/bluetooth/telemetry.cpp \
	$(PROJ_DIR)/src/config/board_config.cpp \
	$(PROJ_DIR)/src/config/settings.cpp \
	$(PROJ_DIR)/src/config/dice_variants.cpp \
	$(PROJ_DIR)/src/data_set/data_animation_bits.cpp \
	$(PROJ_DIR)/src/data_set/data_set.cpp \
	$(PROJ_DIR)/src/data_set/data_set_defaults.cpp \
	$(PROJ_DIR)/src/drivers_hw/apa102.cpp \
	$(PROJ_DIR)/src/drivers_hw/battery.cpp \
	$(PROJ_DIR)/src/drivers_hw/lis2de12.cpp \
	$(PROJ_DIR)/src/drivers_hw/magnet.cpp \
	$(PROJ_DIR)/src/drivers_nrf/a2d.cpp \
	$(PROJ_DIR)/src/drivers_nrf/dfu.cpp \
	$(PROJ_DIR)/src/drivers_nrf/flash.cpp \
	$(PROJ_DIR)/src/drivers_nrf/gpiote.cpp \
	$(PROJ_DIR)/src/drivers_nrf/i2c.cpp \
	$(PROJ_DIR)/src/drivers_nrf/log.cpp \
	$(PROJ_DIR)/src/drivers_nrf/power_manager.cpp \
	$(PROJ_DIR)/src/drivers_nrf/scheduler.cpp \
	$(PROJ_DIR)/src/drivers_nrf/timers.cpp \
	$(PROJ_DIR)/src/drivers_nrf/watchdog.cpp \
	$(PROJ_DIR)/src/modules/accelerometer.cpp \
	$(PROJ_DIR)/src/modules/anim_controller.cpp \
	$(PROJ_DIR)/src/modules/animation_preview.cpp \
	$(PROJ_DIR)/src/modules/behavior_controller.cpp \
	$(PROJ_DIR)/src/modules/led_color_tester.cpp \
	$(PROJ_DIR)/src/modules/battery_controller.cpp \
	$(PROJ_DIR)/src/modules/hardware_test.cpp \
	$(PROJ_DIR)/src/modules/rssi_controller.cpp \
	$(PROJ_DIR)/src/utils/abi.cpp \
	$(PROJ_DIR)/src/utils/rainbow.cpp \
	$(PROJ_DIR)/src/utils/utils.cpp \
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
	# $(SDK_ROOT)/external/segger_rtt/SEGGER_RTT.c \
	# $(SDK_ROOT)/external/segger_rtt/SEGGER_RTT_printf.c \
	# $(SDK_ROOT)/external/segger_rtt/SEGGER_RTT_Syscalls_GCC.c \
	# $(SDK_ROOT)/components/libraries/log/src/nrf_log_backend_rtt.c \
	# $(SDK_ROOT)/components/libraries/button/app_button.c \


# Include folders common to all targets
INC_FOLDERS += \
	$(PROJ_DIR) \
	$(PROJ_DIR)/src \
	$(PROJ_DIR)/src/config \
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
#OPT += -flto

COMMON_FLAGS = -DBL_SETTINGS_ACCESS_ONLY
COMMON_FLAGS += -DNRF52_SERIES
COMMON_FLAGS += -DBOARD_CUSTOM
COMMON_FLAGS += -DCONFIG_GPIO_AS_PINRESET
COMMON_FLAGS += -DFLOAT_ABI_SOFT
COMMON_FLAGS += -DNRF52810_XXAA
COMMON_FLAGS += -DS112
COMMON_FLAGS += -DSOFTDEVICE_PRESENT
COMMON_FLAGS += -DSWI_DISABLE0
COMMON_FLAGS += -mcpu=cortex-m4
COMMON_FLAGS += -mthumb -mabi=aapcs
COMMON_FLAGS += -mfloat-abi=soft
COMMON_FLAGS += -DNRF52_PAN_74
COMMON_FLAGS += -DNRF_DFU_SVCI_ENABLED
COMMON_FLAGS += -DNRF_DFU_TRANSPORT_BLE=1
COMMON_FLAGS += -DFIRMWARE_VERSION=\"$(VERSION)\"

# COMMON_FLAGS += -DDEVELOP_IN_NRF52832
DEBUG_FLAGS = -DNRF_LOG_ENABLED=0
DEBUG_FLAGS += -DDICE_SELFTEST=0

firmware_debug: DEBUG_FLAGS = -DDEBUG
firmware_debug: DEBUG_FLAGS += -DDEBUG_NRF
firmware_debug: DEBUG_FLAGS += -DNRF_LOG_ENABLED=1

COMMON_FLAGS += $(DEBUG_FLAGS)

FSTORAGE_ADDR_DEFINES = -DFSTORAGE_START=0x2B000

firmware_release: FSTORAGE_ADDR_DEFINES = -DFSTORAGE_START=0x26000

COMMON_FLAGS += $(FSTORAGE_ADDR_DEFINES)

# C flags common to all targets
CFLAGS += $(OPT)
CFLAGS += $(COMMON_FLAGS)
CFLAGS += -Wall
# keep every function in a separate section, this allows linker to discard unused ones
CFLAGS += -ffunction-sections -fdata-sections -fno-strict-aliasing
CFLAGS += -fno-builtin -fshort-enums

# Debug
#CFLAGS += -DDEBUG

# C++ flags common to all targets
CXXFLAGS += $(OPT)
CXXFLAGS += $(COMMON_FLAGS)
CXXFLAGS += -fno-rtti
CXXFLAGS += -fno-exceptions

# Assembler flags common to all targets
ASMFLAGS += -g3
ASMFLAGS += $(COMMON_FLAGS)
ASMFLAGS += -D_CONSOLE

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

firmware_debug: firmware
firmware_release: firmware

.PHONY: default

# Default target - first one defined
default: firmware_debug
 
TEMPLATE_PATH := $(SDK_ROOT)/components/toolchain/gcc

include $(TEMPLATE_PATH)/Makefile.common

$(foreach target, $(TARGETS), $(call define_target, $(target)))

.PHONY: flash erase zip

reset:
	nrfjprog -f nrf52 -s 801001366 --reset

hardreset:
	nrfjprog -f nrf52 -s 801001366 --pinreset

erase:
	nrfjprog -f nrf52 -s 801001366 --eraseall

zip: firmware_release
	nrfutil pkg generate --application $(OUTPUT_DIRECTORY)/firmware.hex --application-version 0xff --hw-version 52 --key-file private.pem --sd-req 0xB0 $(OUTPUT_DIRECTORY)/firmware_$(VERSION).zip

publish: zip
	copy $(OUTPUT_DIRECTORY)/firmware_$(VERSION).zip $(PUBLISH_DIRECTORY)

settings:
	nrfutil settings generate --family NRF52810 --application $(OUTPUT_DIRECTORY)/firmware.hex --application-version 0xff --bootloader-version 0xff --bl-settings-version 1 $(OUTPUT_DIRECTORY)/firmware_settings.hex

# Flash the program
flash: firmware_debug settings
	@echo Flashing: $(OUTPUT_DIRECTORY)/firmware.hex
	nrfjprog -f nrf52 -s 801001366 --program $(OUTPUT_DIRECTORY)/firmware.hex --sectorerase
	nrfjprog -f nrf52 -s 801001366 --program $(OUTPUT_DIRECTORY)/firmware_settings.hex --sectorerase
	nrfjprog -f nrf52 -s 801001366 --reset

# Flash the program
flash_release: firmware_release settings
	@echo Flashing: $(OUTPUT_DIRECTORY)/firmware.hex
	nrfjprog -f nrf52 -s 801001366 --program $(OUTPUT_DIRECTORY)/firmware.hex --sectorerase
	nrfjprog -f nrf52 -s 801001366 --program $(OUTPUT_DIRECTORY)/firmware_settings.hex --sectorerase
	nrfjprog -f nrf52 -s 801001366 --reset

# Flash over BLE, you must use DICE=D_XXXXXXX argument to make flash_ble
# e.g. make flash_ble DICE=D_71902510
flash_ble: zip
	@echo Flashing: $(OUTPUT_DIRECTORY)/firmware_$(VERSION).zip over BLE DFU
#	nrfutil dfu ble -cd 0 -ic NRF52 -p COM4 -snr 682511527 -f -n $(DICE) -pkg $(OUTPUT_DIRECTORY)/firmware_$(VERSION).zip
	nrfutil dfu ble -cd 0 -ic NRF51 -p COM5 -snr 680120179 -f -n $(DICE) -pkg $(OUTPUT_DIRECTORY)/firmware_$(VERSION).zip

# Flash softdevice
flash_softdevice:
	@echo Flashing: s112_nrf52_6.1.1_softdevice.hex
	nrfjprog -f nrf52 -s 801001366 --program $(SDK_ROOT)/components/softdevice/s112/hex/s112_nrf52_6.1.0_softdevice.hex --sectorerase
	nrfjprog -f nrf52 -s 801001366 --reset

flash_bootloader:
	@echo Flashing: $(PROJ_DIR)/../Bootloader/_build/nrf52810_xxaa_s112.hex
	nrfjprog -f nrf52 -s 801001366 --program $(PROJ_DIR)/../Bootloader/_build/nrf52810_xxaa_s112.hex --sectorerase
	nrfjprog -f nrf52 -s 801001366 --reset

flash_board: erase flash_softdevice flash_bootloader flash


reflash:  erase  flash  flash_softdevice

reflash_release: erase flash_release flash_softdevice
