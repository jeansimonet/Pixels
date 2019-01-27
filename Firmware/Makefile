PROJECT_NAME     := Firmware
TARGETS          := firmware
OUTPUT_DIRECTORY := _build

SDK_ROOT := C:/nRF5_SDK
ARDUINO_ROOT := C:/Projects/Dice4/Adafruit_nRF52_Arduino
PROJ_DIR := .
ARDUINO_CORE := $(ARDUINO_ROOT)/cores/nRF5
RTOS_DIR := $(ARDUINO_CORE)/freertos

$(OUTPUT_DIRECTORY)/firmware.out: \
	LINKER_SCRIPT  := Firmware.ld

# Source files common to all targets
SRC_FILES += \
	$(SDK_ROOT)/modules/nrfx/mdk/gcc_startup_nrf52810.S \
	$(SDK_ROOT)/components/boards/boards.c \
	$(SDK_ROOT)/modules/nrfx/mdk/system_nrf52810.c \
	$(SDK_ROOT)/external/segger_rtt/SEGGER_RTT.c \
	$(SDK_ROOT)/external/segger_rtt/SEGGER_RTT_printf.c \
	$(SDK_ROOT)/external/segger_rtt/SEGGER_RTT_Syscalls_GCC.c \
	$(SDK_ROOT)/components/libraries/util/app_util_platform.c \
	$(ARDUINO_ROOT)/libraries/Wire/Wire_nRF52.cpp \
	$(ARDUINO_CORE)/delay.c \
	$(ARDUINO_CORE)/RingBuffer.cpp \
	$(ARDUINO_CORE)/Print.cpp \
	$(RTOS_DIR)/Source/list.c \
	$(RTOS_DIR)/Source/queue.c \
	$(RTOS_DIR)/Source/stream_buffer.c \
	$(RTOS_DIR)/Source/tasks.c \
	$(RTOS_DIR)/Source/timers.c \
	$(RTOS_DIR)/portable/CMSIS/nrf52/port_cmsis_systick.c \
	$(RTOS_DIR)/portable/CMSIS/nrf52/port_cmsis_nofpu.c \
	$(PROJ_DIR)/src/main.cpp \
	$(PROJ_DIR)/src/variant.cpp \
	$(PROJ_DIR)/src/HWTesting.cpp \
	$(PROJ_DIR)/src/Console.cpp \
	$(PROJ_DIR)/src/Utils.cpp \
	# $(RTOS_DIR)/Source/croutine.c \
	# $(RTOS_DIR)/Source/event_groups.c \
	# $(RTOS_DIR)/Source/portable/MemMang/heap_3.c \
	# $(PROJ_DIR)/AccelController.cpp \
	# $(PROJ_DIR)/Accelerometer.cpp \
	# $(PROJ_DIR)/Adafruit_DotStar.cpp \
	# $(PROJ_DIR)/Animation.cpp \
	# $(PROJ_DIR)/AnimationSet.cpp \
	# $(PROJ_DIR)/AnimController.cpp \
	# $(PROJ_DIR)/APA102LEDs.cpp \
	# $(PROJ_DIR)/BLEConsole.cpp \
	# $(PROJ_DIR)/BluetoothMessage.cpp \
	# $(PROJ_DIR)/BulkDataTransfer.cpp \
	# $(PROJ_DIR)/Die.cpp \
	# $(PROJ_DIR)/EstimatorOnFace.cpp \
	# $(PROJ_DIR)/Firmware.ld \
	# $(PROJ_DIR)/I2C.cpp \
	# $(PROJ_DIR)/JerkMonitor.cpp \
	# $(PROJ_DIR)/Lazarus.cpp \
	# $(PROJ_DIR)/LEDs.cpp \
	# $(PROJ_DIR)/Rainbow.cpp \
	# $(PROJ_DIR)/Settings.cpp \
	# $(PROJ_DIR)/SimpleThrowDetector.cpp \
	# $(PROJ_DIR)/Telemetry.cpp \
	# $(PROJ_DIR)/Timer.cpp \
	# $(PROJ_DIR)/variant.cpp \
	# $(PROJ_DIR)/Watchdog.cpp \

# Include folders common to all targets
INC_FOLDERS += \
	$(PROJ_DIR) \
	$(PROJ_DIR)/src \
	$(SDK_ROOT) \
	$(SDK_ROOT)/modules/nrfx \
	$(SDK_ROOT)/modules/nrfx/hal \
	$(SDK_ROOT)/modules/nrfx/mdk \
	$(SDK_ROOT)/modules/nrfx/soc \
	$(SDK_ROOT)/modules/nrfx/drivers/include \
	$(SDK_ROOT)/components/softdevice/s112/headers \
	$(SDK_ROOT)/components/libraries/util \
	$(SDK_ROOT)/components/libraries/delay \
	$(SDK_ROOT)/integration/nrfx \
	$(RTOS_DIR)/Source/include \
	$(RTOS_DIR)/config \
	$(RTOS_DIR)/portable/GCC/nrf52 \
	$(RTOS_DIR)/portable/CMSIS/nrf52 \
	$(ARDUINO_CORE) \
	$(ARDUINO_CORE)/cmsis/include \
	$(ARDUINO_CORE)/sysview/SEGGER \
	$(ARDUINO_CORE)/sysview/Config \
	$(ARDUINO_CORE)/usb \
	$(ARDUINO_CORE)/usb/tinyusb/src \
	$(ARDUINO_ROOT)/libraries/Wire \
	$(ARDUINO_ROOT)/libraries/SPI \
	$(ARDUINO_ROOT)/
 
# Libraries common to all targets
LIB_FILES += \

# Optimization flags
OPT = -O3 -g3
# Uncomment the line below to enable link time optimization
#OPT += -flto

# C flags common to all targets
CFLAGS += $(OPT)
CFLAGS += -DARDUINO=188
CFLAGS += -DARDUINO_NRF52_ADAFRUIT
CFLAGS += -DNRF52_SERIES
CFLAGS += -DBOARD_CUSTOM
CFLAGS += -DBSP_DEFINES_ONLY
CFLAGS += -DCONFIG_GPIO_AS_PINRESET
CFLAGS += -DFLOAT_ABI_SOFT
CFLAGS += -DNRF52810_XXAA
CFLAGS += -DNRF_SD_BLE_API_VERSION=6
CFLAGS += -DS112
CFLAGS += -DSOFTDEVICE_PRESENT
CFLAGS += -D_CONSOLE
CFLAGS += -DRGB_LED
CFLAGS += -mcpu=cortex-m4
CFLAGS += -mthumb -mabi=aapcs
CFLAGS += -Wall
CFLAGS += -mfloat-abi=soft
# keep every function in a separate section, this allows linker to discard unused ones
CFLAGS += -ffunction-sections -fdata-sections -fno-strict-aliasing
CFLAGS += -fno-builtin -fshort-enums
#CFLAGS += -DDEVELOP_IN_NRF52832
#CFLAGS += -DNRF52_PAN_74

# C++ flags common to all targets
CXXFLAGS += $(OPT)
CXXFLAGS += -fno-rtti

# Assembler flags common to all targets
ASMFLAGS += -g3
ASMFLAGS += -mcpu=cortex-m4
ASMFLAGS += -mthumb -mabi=aapcs
ASMFLAGS += -mfloat-abi=soft
ASMFLAGS += -DARDUINO=188
ASMFLAGS += -DARDUINO_NRF52_ADAFRUIT
ASMFLAGS += -DNRF52_SERIES
ASMFLAGS += -DBOARD_CUSTOM
ASMFLAGS += -DBSP_DEFINES_ONLY
ASMFLAGS += -DCONFIG_GPIO_AS_PINRESET
ASMFLAGS += -DFLOAT_ABI_SOFT
ASMFLAGS += -DNRF52810_XXAA
ASMFLAGS += -DNRF_SD_BLE_API_VERSION=6
ASMFLAGS += -DS112
ASMFLAGS += -DSOFTDEVICE_PRESENT
ASMFLAGS += -DRGB_LED
ASMFLAGS += -D_CONSOLE
#ASMFLAGS += -DDEVELOP_IN_NRF52832
#ASMFLAGS += -DNRF52_PAN_74

# Linker flags
LDFLAGS += $(OPT)
LDFLAGS += -mthumb -mabi=aapcs -L$(SDK_ROOT)/modules/nrfx/mdk -T$(LINKER_SCRIPT)
LDFLAGS += -mcpu=cortex-m4
# let linker dump unused sections
LDFLAGS += -Wl,--gc-sections
# use newlib in nano version
LDFLAGS += --specs=nano.specs

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
