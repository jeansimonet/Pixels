#include "led_color_tester.h"
#include "bluetooth/bluetooth_messages.h"
#include "bluetooth/bluetooth_message_service.h"
#include "drivers_hw/apa102.h"
#include "utils/utils.h"
#include "nrf_log.h"
#include "modules/anim_controller.h"
#include "animations/animation.h"
#include "animations/animation_set.h"
#include "config/board_config.h"
#include "config/settings.h"
#include "config/dice_variants.h"

using namespace Modules;
using namespace DriversHW;
using namespace Bluetooth;
using namespace Utils;
using namespace Animations;
using namespace Config;

namespace Modules
{
namespace LEDColorTester
{
    void SetLEDToColorHandler(void* context, const Message* msg);
    void SetAllLEDsToColorHandler(void* context, const Message* msg);
    void PlayLEDAnim(void* context, const Message* msg);

    void LightUpFaceHandler(void* context, const Message* msg);

    void init() {
        MessageService::RegisterMessageHandler(Message::MessageType_SetLEDToColor, nullptr, SetLEDToColorHandler);
        MessageService::RegisterMessageHandler(Message::MessageType_SetAllLEDsToColor, nullptr, SetAllLEDsToColorHandler);
        MessageService::RegisterMessageHandler(Message::MessageType_LightUpFace, nullptr, LightUpFaceHandler);
        NRF_LOG_INFO("LED Color tester initialized");
    }

    void SetLEDToColorHandler(void* context, const Message* msg) {
        auto colorMsg = (const MessageSetLEDToColor*)msg;
        uint8_t led = colorMsg->ledIndex;
        uint8_t r = Utils::getRed(colorMsg->color);
        uint8_t g = Utils::getGreen(colorMsg->color);
        uint8_t b = Utils::getBlue(colorMsg->color);
        uint32_t color = Utils::toColor(r, g, b);
        NRF_LOG_INFO("Setting LED %d to %06x -> %06x", led, colorMsg->color, color);
        APA102::setPixelColor(led, color);
        APA102::show();
    }

    void SetAllLEDsToColorHandler(void* context, const Message* msg) {
        auto colorMsg = (const MessageSetAllLEDsToColor*)msg;
        uint8_t r = Utils::getRed(colorMsg->color);
        uint8_t g = Utils::getGreen(colorMsg->color);
        uint8_t b = Utils::getBlue(colorMsg->color);
        uint32_t color = Utils::toColor(r, g, b);
        NRF_LOG_INFO("Setting All LEDs to %06x -> %06x", colorMsg->color, color);
        APA102::setAll(color);
        APA102::show();
    }

    void LightUpFaceHandler(void* context, const Message* msg) {
		// The transformation is:
		// animFaceIndex
		//	-> rotatedOutsideAnimFaceIndex (based on remapFace and remapping table, i.e. what actual face should light up to "retarget" the animation around the current up face)
		//		-> rotatedInsideFaceIndex (based on internal pcb rotation, i.e. what face the electronics should light up to account for the fact that the pcb is probably rotated inside the dice)
		//			-> ledIndex (based on pcb face to led mapping, i.e. to account for the fact that the LEDs are not accessed in the same order as the number of the faces)

		auto s = SettingsManager::getSettings();
		auto b = BoardManager::getBoard();
		int c = b->ledCount;

        auto lufmsg = static_cast<const MessageLightUpFace*>(msg);

        BLE_LOG_INFO("li: %d, %d", s->faceLayoutLookupIndex, Accelerometer::currentFace());

        NRF_LOG_INFO("Light Up Face: face: %d, remapFace: %d, layoutIndex: %d, color: %08x", lufmsg->face, lufmsg->opt_remapFace, lufmsg->opt_layoutIndex, lufmsg->color);

        int layoutIndex = lufmsg->opt_layoutIndex;
        if (lufmsg->opt_layoutIndex == 0xFF) {
            layoutIndex = s->faceLayoutLookupIndex;
        }

		auto l = DiceVariants::getLayout(c, layoutIndex);

        int remapFace = lufmsg->opt_remapFace;
        if (remapFace == 0xFF) {
            remapFace = Accelerometer::currentFace();
        }
        int rotatedOutsideAnimFaceIndex = l->faceRemap[remapFace * c + lufmsg->face];
        BLE_LOG_INFO("roafi: %d", rotatedOutsideAnimFaceIndex);

		uint16_t ledIndex = s->faceToLEDLookup[rotatedOutsideAnimFaceIndex];

        NRF_LOG_INFO(" -> LED Index: %d, color: %08x", ledIndex, lufmsg->color);
        BLE_LOG_INFO("ledIndex: %d", ledIndex);

        APA102::setPixelColor(ledIndex, lufmsg->color);
        APA102::show();
    }

}
}