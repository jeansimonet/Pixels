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
}
