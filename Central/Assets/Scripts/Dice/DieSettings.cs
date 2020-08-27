using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Dice
{
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public class DieSettings
{
	enum D20Version : byte
	{
		Default = 0,
		Black, // Was cast in the wrong orientation so indices are all swapped
		White, // Was cast in the wrong orientation so indices are all swapped
	};

	const int MAX_LED_COUNT = 21;
	const uint SETTINGS_VALID_KEY = 0x05E77165; // 0SETTINGS in leet speak ;)

	// Indicates whether there is valid data (should be 
	uint headMarker = SETTINGS_VALID_KEY;

	// Face detector
	float jerkClamp = 10.0f;
	float sigmaDecay = 0.5f;
	float startMovingThreshold = 5.0f;
	float stopMovingThreshold = 0.5f;
	float faceThreshold = 0.98f;
	float fallingThreshold = 0.1f;
	float shockThreshold = 7.5f;
	float accDecay = 0.9f;
	float heatUpRate = 0.0004f;
	float coolDownRate = 0.995f;
	int minRollTime = 300; // ms

	// Battery
	float batteryLow = 3.0f;
	float batteryHigh = 4.0f;

	D20Version d20Version = D20Version.Default;
	byte filler1;
	byte filler2;
	byte filler3;

	// Calibration data
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_LED_COUNT)]
	Vector3[] faceNormals;

	// Indicates whether there is valid data
	uint tailMarker = SETTINGS_VALID_KEY;

	public static byte[] ToByteArray(DieSettings settings)
    {
        int size = Marshal.SizeOf<DieSettings>();
        System.IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(settings, ptr, false);
        byte[] ret = new byte[size];
        Marshal.Copy(ptr, ret, 0, size);
        Marshal.FreeHGlobal(ptr);
        return ret;
    }

    public static DieSettings FromByteArray(byte[] data)
    {
        System.IntPtr ptr = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, ptr, data.Length);
        var ret = Marshal.PtrToStructure<DieSettings>(ptr);
        Marshal.FreeHGlobal(ptr);
        return ret;
    }

}
}