/*------------------------------------------------------------------------
  This file is part of the Adafruit Dot Star library.

  Adafruit Dot Star is free software: you can redistribute it and/or
  modify it under the terms of the GNU Lesser General Public License
  as published by the Free Software Foundation, either version 3 of
  the License, or (at your option) any later version.

  Adafruit Dot Star is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU Lesser General Public License for more details.

  You should have received a copy of the GNU Lesser General Public
  License along with DotStar.  If not, see <http://www.gnu.org/licenses/>.
  ------------------------------------------------------------------------*/

#pragma once

#include <stdint.h>

namespace DriversHW
{
  namespace APA102
  {
    void init();
    void clear();
    void show(void);
    void setPixelColor(uint16_t n, uint8_t r, uint8_t g, uint8_t b);
    void setPixelColor(uint16_t n, uint32_t c);
    void setAll(uint32_t c);
    void setPixelColors(int* indices, uint32_t* colors, int count);
    void setPixelColors(uint32_t* colors);
    uint32_t color(uint8_t r, uint8_t g, uint8_t b);
    uint32_t getPixelColor(uint16_t n);
    uint16_t numPixels();
    uint8_t *getPixels();

    void selfTest();

		typedef void(*APA102ClientMethod)(void* param, bool powerOn);
		void hookPowerState(APA102ClientMethod method, void* param);
		void unHookPowerState(APA102ClientMethod client);
		void unHookPowerStateWithParam(void* param);
  }
}
