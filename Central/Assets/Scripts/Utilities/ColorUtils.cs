using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class ColorUtils
{
    public static uint toColor(byte red, byte green, byte blue)
    {
        return (uint)red << 16 | (uint)green << 8 | (uint)blue;
    }
    public static byte getRed(uint color)
    {
        return (byte)((color >> 16) & 0xFF);
    }
    public static byte getGreen(uint color)
    {
        return (byte)((color >> 8) & 0xFF);
    }
    public static byte getBlue(uint color)
    {
        return (byte)((color) & 0xFF);
    }
    public static byte getGreyscale(uint color)
    {
        return (byte)Mathf.Max(getRed(color), Mathf.Max(getGreen(color), getBlue(color)));
    }
    public static float desaturate(Color color)
    {
        return  (Mathf.Min(color.r, Mathf.Min(color.g, color.b)) + Mathf.Max(color.r, Mathf.Max(color.g, color.b))) * 0.5f;
    }
	public static uint addColors(uint a, uint b) {
		byte red = (byte)Mathf.Max(getRed(a), getRed(b));
		byte green = (byte)Mathf.Max(getGreen(a), getGreen(b));
		byte blue = (byte)Mathf.Max(getBlue(a), getBlue(b));
		return toColor(red,green,blue);
	}

	public static uint interpolateColors(uint color1, int time1, uint color2, int time2, int time) {
		// To stick to integer math, we'll scale the values
		int scaler = 1024;
		int scaledPercent = (time - time1) * scaler / (time2 - time1);
		int scaledRed = getRed(color1)* (scaler - scaledPercent) + getRed(color2) * scaledPercent;
		int scaledGreen = getGreen(color1) * (scaler - scaledPercent) + getGreen(color2) * scaledPercent;
		int scaledBlue = getBlue(color1) * (scaler - scaledPercent) + getBlue(color2) * scaledPercent;
		return toColor((byte)(scaledRed / scaler), (byte)(scaledGreen / scaler), (byte)(scaledBlue / scaler));
	}

    public static byte interpolateIntensity(byte intensity1, int time1, byte intensity2, int time2, int time) {
		int scaler = 1024;
		int scaledPercent = (time - time1) * scaler / (time2 - time1);
		return (byte)((intensity1 * (scaler - scaledPercent) + intensity2 * scaledPercent) / scaler);
    }

    public static uint modulateColor(uint color, byte intensity) {
		int red = getRed(color) * intensity / 255;
		int green = getGreen(color) * intensity / 255;
		int blue = getBlue(color) * intensity / 255;
		return toColor((byte)red, (byte)green, (byte)blue);
    }

    // Input a value 0 to 255 to get a color value.
    // The colours are a transition r - g - b - back to r.
    public static uint rainbowWheel(byte WheelPos, byte intensity)
    {
        if (WheelPos < 85)
        {
            return toColor((byte)(WheelPos * 3 * intensity / 255), (byte)((255 - WheelPos * 3) * intensity / 255), 0);
        }
        else if (WheelPos < 170)
        {
            WheelPos -= 85;
            return toColor((byte)((255 - WheelPos * 3) * intensity / 255), 0, (byte)(WheelPos * 3 * intensity / 255));
        }
        else
        {
            WheelPos -= 170;
            return toColor(0, (byte)(WheelPos * 3 * intensity / 255), (byte)((255 - WheelPos * 3) * intensity / 255));
        }
    }

    public static float computeSqrColorDistance(Color color1, Color color2)
    {
        return
            (color1.r - color2.r) * (color1.r - color2.r) +
            (color1.g - color2.g) * (color1.g - color2.g) +
            (color1.b - color2.b) * (color1.b - color2.b);
    }


    public static List<Animations.EditRGBKeyframe> extractKeyframes(Color[] pixels)
    {
        var ret = new List<Animations.EditRGBKeyframe>();
        
        float computeInterpolationError(int firstIndex, int lastIndex) {
            Color startColor = pixels[firstIndex];
            Color endColor = pixels[lastIndex];
            float sumError = 0.0f;
            for (int i = firstIndex; i <= lastIndex; ++i) {
                float pct = (float)(i - firstIndex) / (lastIndex - firstIndex);
                sumError += computeSqrColorDistance(pixels[i], Color.Lerp(startColor, endColor, pct));
            }
            return sumError;
        }

        float computePixelTime(int pixelIndex) {
            return (float)pixelIndex * 0.02f; // 0.02 is the smallest time increment in the keyframe data
        }

        // Always add the first color
        ret.Add(new Animations.EditRGBKeyframe()
        {
            time = 0,
            color = pixels[0]
        });

        const float sqrEpsilon = 0.2f;

        int currentPrev = 0;
        int currentNext = 1;
        while (currentNext < pixels.Length) {
            while (currentNext < pixels.Length && computeInterpolationError(currentPrev, currentNext) < sqrEpsilon) {
                currentNext++;
            }

            // Too much error, add a keyframe
            ret.Add(new Animations.EditRGBKeyframe()
            {
                time = computePixelTime(currentNext-1),
                color = pixels[currentNext-1]
            });

            // Next segment
            currentPrev = currentNext-1;
        }

        return ret;
    }

    static byte[] GammaTable =
    {
          0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
          0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
          1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  2,
          2,  2,  2,  2,  2,  2,  2,  3,  3,  3,  3,  3,  3,  3,  4,  4,
          4,  4,  4,  5,  5,  5,  5,  6,  6,  6,  6,  6,  7,  7,  7,  8,
          8,  8,  8,  9,  9,  9, 10, 10, 10, 11, 11, 12, 12, 12, 13, 13,
         14, 14, 14, 15, 15, 16, 16, 17, 17, 18, 18, 19, 19, 20, 20, 21,
         22, 22, 23, 23, 24, 25, 25, 26, 27, 27, 28, 29, 29, 30, 31, 32,
         32, 33, 34, 35, 35, 36, 37, 38, 39, 40, 40, 41, 42, 43, 44, 45,
         46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 60, 61, 62,
         63, 64, 65, 67, 68, 69, 70, 72, 73, 74, 76, 77, 78, 80, 81, 82,
         84, 85, 87, 88, 90, 91, 93, 94, 96, 97, 99,101,102,104,105,107,
        109,111,112,114,116,118,119,121,123,125,127,129,131,132,134,136,
        138,140,142,144,147,149,151,153,155,157,159,162,164,166,168,171,
        173,175,178,180,182,185,187,190,192,195,197,200,202,205,207,210,
        213,215,218,221,223,226,229,232,235,237,240,243,246,249,252,255,
    };

    static byte[] ReverseGammaTable =
    {
        0, 70, 80, 87, 92, 97, 101, 105, 108, 112, 114, 117, 119, 122, 124, 126,
        128, 130, 132, 134, 135, 137, 138, 140, 141, 143, 144, 146, 147, 148, 149,
        151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165,
        166, 167, 168, 169, 170, 170, 171, 172, 173, 174, 174, 175, 176, 177, 177,
        178, 179, 180, 180, 181, 182, 182, 183, 184, 184, 185, 186, 186, 187, 188,
        188, 189, 189, 190, 191, 191, 192, 192, 193, 194, 194, 195, 195, 196, 196,
        197, 197, 198, 198, 199, 200, 200, 201, 201, 202, 202, 203, 203, 204, 204,
        204, 205, 205, 206, 206, 207, 207, 208, 208, 209, 209, 210, 210, 210, 211,
        211, 212, 212, 213, 213, 214, 214, 214, 215, 215, 216, 216, 216, 217, 217,
        218, 218, 218, 219, 219, 220, 220, 220, 221, 221, 222, 222, 222, 223, 223,
        223, 224, 224, 224, 225, 225, 226, 226, 226, 227, 227, 227, 228, 228, 228,
        229, 229, 229, 230, 230, 230, 231, 231, 231, 232, 232, 232, 233, 233, 233,
        234, 234, 234, 235, 235, 235, 236, 236, 236, 237, 237, 237, 237, 238, 238,
        238, 239, 239, 239, 240, 240, 240, 241, 241, 241, 241, 242, 242, 242, 243,
        243, 243, 243, 244, 244, 244, 245, 245, 245, 245, 246, 246, 246, 247, 247,
        247, 247, 248, 248, 248, 248, 249, 249, 249, 249, 250, 250, 250, 251, 251,
        251, 251, 252, 252, 252, 252, 253, 253, 253, 253, 254, 254, 254, 254, 255,
    };

    public static byte gamma8(byte x)
    {
        return GammaTable[x]; // 0-255 in, 0-255 out
    }

    public static Color32 gamma(Color32 color)
    {
        byte r = gamma8(color.r);
        byte g = gamma8(color.g);
        byte b = gamma8(color.b);
        return new Color32(r, g, b, 255);
    }

    public static uint gamma(uint color)
    {
        byte r = gamma8(getRed(color));
        byte g = gamma8(getGreen(color));
        byte b = gamma8(getBlue(color));
        return toColor(r, g, b);
    }

    public static byte reverseGamma8(byte x)
    {
        return ReverseGammaTable[x]; // 0-255 in, 0-255 out
    }

    public static Color32 reverseGamma(Color32 color)
    {
        byte r = reverseGamma8(color.r);
        byte g = reverseGamma8(color.g);
        byte b = reverseGamma8(color.b);
        return new Color32(r, g, b, 255);
    }


}
