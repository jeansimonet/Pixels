#pragma once

#include "core/float3.h"
#include "stdint.h"

#define MAX_VARIANT_COUNT 2

namespace Config
{
    namespace DiceVariants
    {
        // This enum describes what the dice looks like, so the App can use the appropriate 3D model/color
        enum DesignAndColor : uint8_t
        {
            DesignAndColor_Unknown = 0,
            DesignAndColor_Generic,
            DesignAndColor_V3_Orange,
            DesignAndColor_V4_BlackClear,
            DesignAndColor_V4_WhiteClear,
            DesignAndColor_V5_Grey,
            DesignAndColor_V5_White,
            DesignAndColor_V5_Black,
            DesignAndColor_V5_Gold,
        };

        struct Layout
        {
            const Core::float3* faceNormals;
            const uint8_t* faceRemap;
            const uint8_t* rotationRemap;
            int rotationRemapCount;
            const uint8_t* faceToLedLookup;
        };

        struct Layouts
        {
            const Layout* layouts[MAX_VARIANT_COUNT];
            int count;
        };

        const Layouts* getLayouts(int faceCount);
        const Layout* getLayout(int faceCount, int variantIndex);

        const Core::float3* getDefaultNormals(int faceCount);
        const uint8_t* getDefaultLookup(int faceCount);
    }
}