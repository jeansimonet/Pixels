PROJECT_NAME     := blinky_pca10040e_mbr
TARGETS          := nrf52810_xxaa
OUTPUT_DIRECTORY := _build

SDK_ROOT := C:/nRF5_SDK
PROJ_DIR := .

$(OUTPUT_DIRECTORY)/nrf52810_xxaa.out: \
	LINKER_SCRIPT  := blinky_gcc_nrf52.ld

# Source files common to all targets
SRC_FILES += \
	$(SDK_ROOT)/modules/nrfx/mdk/gcc_startup_nrf52810.S \
	$(PROJ_DIR)/main.c \
	$(SDK_ROOT)/components/libraries/log/src/nrf_log_frontend.c \
	$(SDK_ROOT)/components/libraries/log/src/nrf_log_str_formatter.c \
	$(SDK_ROOT)/components/boards/boards.c \
	$(SDK_ROOT)/components/libraries/util/app_error.c \
	$(SDK_ROOT)/components/libraries/util/app_error_handler_gcc.c \
	$(SDK_ROOT)/components/libraries/util/app_error_weak.c \
	$(SDK_ROOT)/components/libraries/util/app_util_platform.c \
	$(SDK_ROOT)/components/libraries/util/nrf_assert.c \
	$(SDK_ROOT)/components/libraries/atomic/nrf_atomic.c \
	$(SDK_ROOT)/components/libraries/balloc/nrf_balloc.c \
	$(SDK_ROOT)/external/fprintf/nrf_fprintf.c \
	$(SDK_ROOT)/external/fprintf/nrf_fprintf_format.c \
	$(SDK_ROOT)/components/libraries/memobj/nrf_memobj.c \
	$(SDK_ROOT)/components/libraries/ringbuf/nrf_ringbuf.c \
	$(SDK_ROOT)/components/libraries/strerror/nrf_strerror.c \
	$(SDK_ROOT)/modules/nrfx/mdk/system_nrf52810.c \
	$(SDK_ROOT)/external/segger_rtt/SEGGER_RTT.c \
	$(SDK_ROOT)/external/segger_rtt/SEGGER_RTT_printf.c \
	$(SDK_ROOT)/external/segger_rtt/SEGGER_RTT_Syscalls_GCC.c \
 
# Include folders common to all targets
INC_FOLDERS += \
	$(SDK_ROOT)/components \
	$(SDK_ROOT)/modules/nrfx/mdk \
	$(SDK_ROOT)/components/softdevice/mbr/nrf52810/headers \
	$(PROJ_DIR) \
	$(SDK_ROOT)/components/libraries/strerror \
	$(SDK_ROOT)/components/toolchain/cmsis/include \
	$(SDK_ROOT)/components/libraries/util \
	$(SDK_ROOT)/components/libraries/balloc \
	$(SDK_ROOT)/components/libraries/ringbuf \
	$(SDK_ROOT)/modules/nrfx/hal \
	$(SDK_ROOT)/components/libraries/bsp \
	$(SDK_ROOT)/components/libraries/log \
	$(SDK_ROOT)/modules/nrfx \
	$(SDK_ROOT)/components/libraries/experimental_section_vars \
	$(SDK_ROOT)/components/libraries/delay \
	$(SDK_ROOT)/integration/nrfx \
	$(SDK_ROOT)/components/libraries/atomic \
	$(SDK_ROOT)/components/boards \
	$(SDK_ROOT)/components/libraries/memobj \
	$(SDK_ROOT)/external/fprintf \
	$(SDK_ROOT)/components/libraries/log/src \
	$(SDK_ROOT)/external/segger_rtt \
	$(SDK_ROOT)/components/softdevice/s112/headers \
	$(SDK_ROOT)/components/softdevice/common \
 
# Libraries common to all targets
LIB_FILES += \

# Optimization flags
OPT = -O3 -g3
# Uncomment the line below to enable link time optimization
#OPT += -flto

# C flags common to all targets
CFLAGS += $(OPT)
CFLAGS += -DBOARD_CUSTOM
CFLAGS += -DBSP_DEFINES_ONLY
CFLAGS += -DCONFIG_GPIO_AS_PINRESET
#CFLAGS += -DDEVELOP_IN_NRF52832
CFLAGS += -DFLOAT_ABI_SOFT
CFLAGS += -DNRF52810_XXAA
#CFLAGS += -DNRF52_PAN_74
CFLAGS += -DNRF_SD_BLE_API_VERSION=6
CFLAGS += -DS112
CFLAGS += -DSOFTDEVICE_PRESENT
CFLAGS += -mcpu=cortex-m4
CFLAGS += -mthumb -mabi=aapcs
CFLAGS += -Wall -Werror
CFLAGS += -mfloat-abi=soft
# keep every function in a separate section, this allows linker to discard unused ones
CFLAGS += -ffunction-sections -fdata-sections -fno-strict-aliasing
CFLAGS += -fno-builtin -fshort-enums

# C++ flags common to all targets
CXXFLAGS += $(OPT)

# Assembler flags common to all targets
ASMFLAGS += -g3
ASMFLAGS += -mcpu=cortex-m4
ASMFLAGS += -mthumb -mabi=aapcs
ASMFLAGS += -mfloat-abi=soft
ASMFLAGS += -DBOARD_CUSTOM
ASMFLAGS += -DBSP_DEFINES_ONLY
ASMFLAGS += -DCONFIG_GPIO_AS_PINRESET
#ASMFLAGS += -DDEVELOP_IN_NRF52832
ASMFLAGS += -DFLOAT_ABI_SOFT
ASMFLAGS += -DNRF52810_XXAA
#ASMFLAGS += -DNRF52_PAN_74
ASMFLAGS += -DNRF_SD_BLE_API_VERSION=6
ASMFLAGS += -DS112
ASMFLAGS += -DSOFTDEVICE_PRESENT

# Linker flags
LDFLAGS += $(OPT)
LDFLAGS += -mthumb -mabi=aapcs -L$(SDK_ROOT)/modules/nrfx/mdk -T$(LINKER_SCRIPT)
LDFLAGS += -mcpu=cortex-m4
# let linker dump unused sections
LDFLAGS += -Wl,--gc-sections
# use newlib in nano version
LDFLAGS += --specs=nano.specs

nrf52810_xxaa: CFLAGS += -D__HEAP_SIZE=2048
nrf52810_xxaa: CFLAGS += -D__STACK_SIZE=2048
nrf52810_xxaa: ASMFLAGS += -D__HEAP_SIZE=2048
nrf52810_xxaa: ASMFLAGS += -D__STACK_SIZE=2048

# Add standard libraries at the very end of the linker input, after all objects
# that may need symbols provided by these libraries.
LIB_FILES += -lc -lnosys -lm


.PHONY: default help

# Default target - first one defined
default: nrf52810_xxaa
 
# Print all targets that can be built
help:
	@echo following targets are available:
	@echo		nrf52810_xxaa
	@echo		flash_mbr
	@echo		sdk_config - starting external tool for editing sdk_config.h
	@echo		flash      - flashing binary

TEMPLATE_PATH := $(SDK_ROOT)/components/toolchain/gcc


include $(TEMPLATE_PATH)/Makefile.common

$(foreach target, $(TARGETS), $(call define_target, $(target)))

.PHONY: flash erase zip

erase:
	nrfjprog -f nrf52 -s 801001366 --eraseall

zip: default
	nrfutil pkg generate --application $(OUTPUT_DIRECTORY)/nrf52810_xxaa.hex --application-version 0xff --hw-version 52 --key-file private.pem --sd-req 0xB0 $(OUTPUT_DIRECTORY)/blinky.zip

settings: default
	nrfutil settings generate --family NRF52810 --application $(OUTPUT_DIRECTORY)/nrf52810_xxaa.hex --application-version 0xff --bootloader-version 0xff --bl-settings-version 1 $(OUTPUT_DIRECTORY)/nrf52810_xxaa_settings.hex

# Flash the program
flash: settings zip
	@echo Flashing: $(OUTPUT_DIRECTORY)/nrf52810_xxaa.hex
	nrfjprog -f nrf52 -s 801001366 --program $(OUTPUT_DIRECTORY)/nrf52810_xxaa.hex --sectorerase
	nrfjprog -f nrf52 -s 801001366 --program $(OUTPUT_DIRECTORY)/nrf52810_xxaa_settings.hex --sectorerase
	nrfjprog -f nrf52 -s 801001366 --reset



