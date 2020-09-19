#pragma once

#include <stdint.h>

#pragma pack(push, 1)

namespace Behaviors
{
    /// <summary>
    /// A behavior is made of rules, and this is what a rule is:
    /// a pairing of a condition and an actions. We are using indices and not pointers
    /// because this stuff is stored in flash and so we don't really know what the pointers are.
    /// </summary>
    struct Rule
    {
        uint16_t condition;
        uint16_t actionOffset;
        uint16_t actionCount;
        uint16_t actionCountPadding;
    };

    /// <summary>
    /// A behavior is a set of condition->animation pairs, that's it!
    /// </summary>
    struct Behavior
    {
        uint16_t rulesOffset;
        uint16_t rulesCount;
    };
}

#pragma pack(pop)