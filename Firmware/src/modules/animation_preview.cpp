#include "animation_preview.h"
#include "animations/animation.h"
#include "animations/animation_simple.h"
#include "data_set/data_animation_bits.h"
#include "bluetooth/bluetooth_messages.h"
#include "bluetooth/bluetooth_message_service.h"
#include "bluetooth/bulk_data_transfer.h"
#include "anim_controller.h"
#include "accelerometer.h"
#include "utils/utils.h"
#include "malloc.h"
#include "nrf_log.h"

using namespace Bluetooth;
using namespace DataSet;

namespace Modules
{
namespace AnimationPreview
{
    void ReceiveTestAnimSet(void* context, const Message* msg);
    void FlashDie(void* context, const Message* msg);

    AnimationBits animationBits;
    const Animation* animation;
    void* animationData;
    uint32_t animationDataHash;

    void init()
    {
        MessageService::RegisterMessageHandler(Message::MessageType_TransferTestAnimSet, nullptr, ReceiveTestAnimSet);
        MessageService::RegisterMessageHandler(Message::MessageType_Flash, nullptr, FlashDie);
        animationData = nullptr;
        animation = nullptr;
        animationDataHash = 0;

		NRF_LOG_INFO("Animation Preview Initialized");
    }


    void ReceiveTestAnimSet(void* context, const Message* msg)
    {
		NRF_LOG_INFO("Received Request to play test animation");
		const MessageTransferTestAnimSet* message = (const MessageTransferTestAnimSet*)msg;

        if (animationData == nullptr || animationDataHash != message->hash) {
            // We should download the data
            if (animationData != nullptr) {
                free(animationData);
                animationData = nullptr;
                animationDataHash = 0;
            }

            NRF_LOG_DEBUG("Animation Data to be received:");
            NRF_LOG_DEBUG("Palette: %d * %d", message->paletteSize, sizeof(uint8_t));
            NRF_LOG_DEBUG("RGB Keyframes: %d * %d", message->rgbKeyFrameCount, sizeof(RGBKeyframe));
            NRF_LOG_DEBUG("RGB Tracks: %d * %d", message->rgbTrackCount, sizeof(RGBTrack));
            NRF_LOG_DEBUG("Keyframes: %d * %d", message->keyFrameCount, sizeof(Keyframe));
            NRF_LOG_DEBUG("Tracks: %d * %d", message->trackCount, sizeof(Track));
            NRF_LOG_DEBUG("Animation: %d", message->animationSize);

            int bufferSize =
                message->paletteSize * sizeof(uint8_t) +
                message->rgbKeyFrameCount * sizeof(RGBKeyframe) +
                message->rgbTrackCount * sizeof(RGBTrack) +
                message->keyFrameCount * sizeof(Keyframe) +
                message->trackCount * sizeof(Track) +
                message->animationSize;

            // Allocate anim data
            animationData = malloc(bufferSize);
            if (animationData != nullptr) {
                // Setup pointers
                NRF_LOG_DEBUG("bufferSize: 0x%04x", bufferSize);
                uint32_t address = (uint32_t)animationData;
                animationBits.palette = (const uint8_t*)address;
                animationBits.paletteSize = message->paletteSize;
                address += message->paletteSize * sizeof(uint8_t);

                animationBits.rgbKeyframes = (const RGBKeyframe*)address;
                animationBits.rgbKeyFrameCount = message->rgbKeyFrameCount;
                address += message->rgbKeyFrameCount * sizeof(RGBKeyframe);

                animationBits.rgbTracks = (const RGBTrack*)address;
                animationBits.rgbTrackCount = message->rgbTrackCount;
                address += message->rgbTrackCount * sizeof(RGBTrack);

                animationBits.keyframes = (const Keyframe*)address;
                animationBits.keyFrameCount = message->keyFrameCount;
                address += message->keyFrameCount * sizeof(Keyframe);

                animationBits.tracks = (const Track*)address;
                animationBits.trackCount = message->trackCount;
                address += message->trackCount * sizeof(Track);

                animation = (const Animation*)address;

                // Send Ack and receive data
                MessageTransferTestAnimSetAck ackMsg;
                ackMsg.ackType = TransferTestAnimSetAck_Download;
                MessageService::SendMessage(&ackMsg);

                // Receive all the buffers directly to flash
                ReceiveBulkData::receive(nullptr,
                    [](void* context, uint16_t size) -> uint8_t* {
                        // Regardless of the size passed in, we return the pre-allocated animation data buffer
                        return (uint8_t*)animationData;
                    },
                    [](void* context, bool result, uint8_t* data, uint16_t size) {
                    if (result) {
		                animationDataHash = Utils::computeHash((uint8_t*)animationData, size);
		                NRF_LOG_INFO("Temp animation dataset hash=0x%08x", animationDataHash);

                		MessageService::SendMessage(Message::MessageType_TransferTestAnimSetFinished);

                        // Play the ANIMATION NOW!!!
                        AnimController::play(animation, &animationBits, 0, false);
                    } else {
                        NRF_LOG_ERROR("Failed to download temp animation");
                        free(animationData);
                        animationData = nullptr;
                        animationDataHash = 0;
                        animation = nullptr;
                    }
                });
            } else {
                // No memory
                animationData = nullptr;
                animationDataHash = 0;
                animation = nullptr;
                MessageTransferTestAnimSetAck ackMsg;
                ackMsg.ackType = TransferTestAnimSetAck_NoMemory;
                MessageService::SendMessage(&ackMsg);
            }
       } else {
           // The animation data is valid and matches the app data
            MessageTransferTestAnimSetAck ackMsg;
            ackMsg.ackType = TransferTestAnimSetAck_UpToDate;
            MessageService::SendMessage(&ackMsg);

            // Play the ANIMATION NOW!!!
            AnimController::play(animation, &animationBits, (uint8_t)Accelerometer::currentFace(), false);
       }
   }

    AnimationSimple flashAnim;
    void FlashDie(void* context, const Message* msg)
    {
		NRF_LOG_INFO("Received Request to flash the die");
		const MessageFlash* message = (const MessageFlash*)msg;

        // Create a small anim on the spot
        animationBits.Clear();
        flashAnim.type = Animation_Simple;
        flashAnim.duration = 1000;
		flashAnim.faceMask = 0xFFFFF;
        flashAnim.count = message->flashCount;
        flashAnim.fade = 255;
        flashAnim.color = message->color;
        AnimController::play(&flashAnim, 0, false);
        MessageService::SendMessage(Message::MessageType_FlashFinished);
    }
}
}