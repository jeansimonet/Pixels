#include "battery.h"
#include "../drivers_nrf/a2d.h"
#include "nrf_log.h"

using namespace DriversNRF;

namespace DriversHW
{
namespace Battery
{
    const float vBatMult = 1.4f; // Voltage divider 10M over 4M

    void init() {
        // Fetch config, init battery level, etc...

        // Read battery level and convert
        float vbattery = A2D::readVBat() * vBatMult;
        NRF_LOG_INFO("Battery initialized, Battery Voltage=" NRF_LOG_FLOAT_MARKER, NRF_LOG_FLOAT(vbattery));
    }
}
}