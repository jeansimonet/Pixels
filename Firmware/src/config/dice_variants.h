#pragma once

#include "core/float3.h"
#include "stdint.h"

#define MAX_VARIANT_COUNT 2

namespace Config
{
    namespace DiceVariants
    {
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