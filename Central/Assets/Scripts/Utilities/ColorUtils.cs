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

    public static List<Animations.EditRGBKeyframe> extractKeyframes(Color[] pixels)
    {
        var ret = new List<Animations.EditRGBKeyframe>();
        
        float computeSqrDistance(Color color1, Color color2)
        {
            return 
                (color1.r - color2.r) * (color1.r - color2.r) + 
                (color1.g - color2.g) * (color1.g - color2.g) + 
                (color1.b - color2.b) * (color1.b - color2.b);
        }

        float computeInterpolationError(int firstIndex, int lastIndex) {
            Color startColor = pixels[firstIndex];
            Color endColor = pixels[lastIndex];
            float sumError = 0.0f;
            for (int i = firstIndex; i <= lastIndex; ++i) {
                float pct = (float)(i - firstIndex) / (lastIndex - firstIndex);
                sumError += computeSqrDistance(pixels[i], Color.Lerp(startColor, endColor, pct));
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

        const float sqrEpsilon = 0.1f;

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
}
