#pragma once

#include "animations/keyframes.h"

using namespace Animations;

namespace DataSet
{
    struct AnimationBits
    {
        // The palette for all animations, stored in RGB RGB RGB etc...
        // Maximum 128 * 3 = 376 bytes
        const uint8_t* palette;
        uint32_t paletteSize; // In bytes (divide by 3 for colors)

        #define PALETTE_COLOR_FROM_FACE     127
        #define PALETTE_COLOR_FROM_RANDOM   126

        // The individual RGB keyframes we have, i.e. time and color, packed in
        const Animations::RGBKeyframe* rgbKeyframes; // pointer to the array of tracks
        uint32_t rgbKeyFrameCount;

        // The RGB tracks we have
        const Animations::RGBTrack* rgbTracks; // pointer to the array of tracks
        uint32_t rgbTrackCount;

        // The individual intensity keyframes we have, i.e. time and intensity, packed in
        const Animations::Keyframe* keyframes; // pointer to the array of tracks
        uint32_t keyFrameCount;

        // The RGB tracks we have
        const Animations::Track* tracks; // pointer to the array of tracks
        uint32_t trackCount;

        // Palette
        uint16_t getPaletteSize() const;
        uint32_t getPaletteColor(uint16_t colorIndex) const;

        // Animation keyframes (time and color)
        const Animations::RGBKeyframe& getRGBKeyframe(uint16_t keyFrameIndex) const;
        uint16_t getRGBKeyframeCount() const;

        // RGB tracks, list of keyframes
        const Animations::RGBTrack& getRGBTrack(uint16_t trackIndex) const;
        Animations::RGBTrack const * const getRGBTracks(uint16_t tracksStartIndex) const;
        uint16_t getRGBTrackCount() const;

        // Animation keyframes (time and intensity)
        const Animations::Keyframe& getKeyframe(uint16_t keyFrameIndex) const;
        uint16_t getKeyframeCount() const;

        // RGB tracks, list of keyframes
        const Animations::Track& getTrack(uint16_t trackIndex) const;
        Animations::Track const * const getTracks(uint16_t tracksStartIndex) const;
        uint16_t getTrackCount() const;

        void Clear();
    };
}