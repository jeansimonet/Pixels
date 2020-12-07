#include "utils.h"
#include <nrf_delay.h>
#include <app_timer.h>
#include "core/float3.h"
#include "core/matrix3x3.h"
#include "config/settings.h"
#include "config/board_config.h"
#include "config/dice_variants.h"
#include "nrf_log.h"
#include "bluetooth/bluetooth_message_service.h"


using namespace Core;
using namespace Config;

namespace Utils
{
	uint32_t roundUpTo4(uint32_t address) {
		return 4 * ((address + 3) / 4);
	}

	uint32_t addColors(uint32_t a, uint32_t b) {
		uint8_t red = MAX(getRed(a), getRed(b));
		uint8_t green = MAX(getGreen(a), getGreen(b));
		uint8_t blue = MAX(getBlue(a), getBlue(b));
		return toColor(red,green,blue);
	}

	uint32_t interpolateColors(uint32_t color1, uint32_t time1, uint32_t color2, uint32_t time2, uint32_t time) {
		// To stick to integer math, we'll scale the values
		int scaler = 1024;
		int scaledPercent = (time - time1) * scaler / (time2 - time1);
		int scaledRed = getRed(color1)* (scaler - scaledPercent) + getRed(color2) * scaledPercent;
		int scaledGreen = getGreen(color1) * (scaler - scaledPercent) + getGreen(color2) * scaledPercent;
		int scaledBlue = getBlue(color1) * (scaler - scaledPercent) + getBlue(color2) * scaledPercent;
		return toColor(scaledRed / scaler, scaledGreen / scaler, scaledBlue / scaler);
	}

	/// <summary>
	/// Parses the first word out of a string (typically a command or parameter)
	/// </summary>
	/// <param name="text">The string to parse the first word from</param>
	/// <param name="len">The length of the string</param>
	/// <param name="outWord">The return string buffer</param>
	/// <param name="outWordLen">The max length of the return string buffer</param>
	/// <returns>The length of the found word, otherwise 0</returns>
	int parseWord(char*& text, int& len, char* outWord, int outWordLen)
	{
		while (len > 0&& (*text == ' ' || *text == '\t'))
		{
			text++;
			len--;
		}

		int wordLen = 0;
		if (len > 0)
		{
			while (len > 0 && wordLen < outWordLen && *text != ' ' && *text != '\t' && *text != '\n' && *text != '\r' && *text != 0)
			{
				*outWord = *text;
				outWord++;
				text++;
				len--;
				wordLen++;
			}

			*outWord = 0;
			wordLen++;
		}

		return wordLen;
	}

	/* A PROGMEM (flash mem) table containing 8-bit unsigned sine wave (0-255).
	Copy & paste this snippet into a Python REPL to regenerate:
	import math
	for x in range(256):
		print("{:3},".format(int((math.sin(x/128.0*math.pi)+1.0)*127.5+0.5))),
		if x&15 == 15: print
	*/
	static const uint8_t _sineTable[256] = {
	128,131,134,137,140,143,146,149,152,155,158,162,165,167,170,173,
	176,179,182,185,188,190,193,196,198,201,203,206,208,211,213,215,
	218,220,222,224,226,228,230,232,234,235,237,238,240,241,243,244,
	245,246,248,249,250,250,251,252,253,253,254,254,254,255,255,255,
	255,255,255,255,254,254,254,253,253,252,251,250,250,249,248,246,
	245,244,243,241,240,238,237,235,234,232,230,228,226,224,222,220,
	218,215,213,211,208,206,203,201,198,196,193,190,188,185,182,179,
	176,173,170,167,165,162,158,155,152,149,146,143,140,137,134,131,
	128,124,121,118,115,112,109,106,103,100, 97, 93, 90, 88, 85, 82,
	79, 76, 73, 70, 67, 65, 62, 59, 57, 54, 52, 49, 47, 44, 42, 40,
	37, 35, 33, 31, 29, 27, 25, 23, 21, 20, 18, 17, 15, 14, 12, 11,
	10,  9,  7,  6,  5,  5,  4,  3,  2,  2,  1,  1,  1,  0,  0,  0,
		0,  0,  0,  0,  1,  1,  1,  2,  2,  3,  4,  5,  5,  6,  7,  9,
	10, 11, 12, 14, 15, 17, 18, 20, 21, 23, 25, 27, 29, 31, 33, 35,
	37, 40, 42, 44, 47, 49, 52, 54, 57, 59, 62, 65, 67, 70, 73, 76,
	79, 82, 85, 88, 90, 93, 97,100,103,106,109,112,115,118,121,124 };

	/* Similar to above, but for an 8-bit gamma-correction table.
	Copy & paste this snippet into a Python REPL to regenerate:
import math
gamma=5
for x in range(256):
	print("{:3},".format(int(math.pow((x)/255.0,gamma)*255.0+0.5))),
	if x&15 == 15: print
	*/
	static const uint8_t _gammaTable[256] = {
		0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
		0,  0,  0,  0,  0,  0,  0,  0,  1,  1,  1,  1,  1,  1,  1,  1,
		1,  1,  1,  1,  2,  2,  2,  2,  2,  2,  2,  2,  3,  3,  3,  3,
		3,  3,  4,  4,  4,  4,  5,  5,  5,  5,  5,  6,  6,  6,  6,  7,
		7,  7,  8,  8,  8,  9,  9,  9, 10, 10, 10, 11, 11, 11, 12, 12,
	13, 13, 13, 14, 14, 15, 15, 16, 16, 17, 17, 18, 18, 19, 19, 20,
	20, 21, 21, 22, 22, 23, 24, 24, 25, 25, 26, 27, 27, 28, 29, 29,
	30, 31, 31, 32, 33, 34, 34, 35, 36, 37, 38, 38, 39, 40, 41, 42,
	42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57,
	58, 59, 60, 61, 62, 63, 64, 65, 66, 68, 69, 70, 71, 72, 73, 75,
	76, 77, 78, 80, 81, 82, 84, 85, 86, 88, 89, 90, 92, 93, 94, 96,
	97, 99,100,102,103,105,106,108,109,111,112,114,115,117,119,120,
	122,124,125,127,129,130,132,134,136,137,139,141,143,145,146,148,
	150,152,154,156,158,160,162,164,166,168,170,172,174,176,178,180,
	182,184,186,188,191,193,195,197,199,202,204,206,209,211,213,215,
	218,220,223,225,227,230,232,235,237,240,242,245,247,250,252,255 };

	uint8_t sine8(uint8_t x) {
		return _sineTable[x]; // 0-255 in, 0-255 out
	}

	uint8_t gamma8(uint8_t x) {
		return _gammaTable[x]; // 0-255 in, 0-255 out
	}

	uint32_t gamma(uint32_t color) {
        uint8_t r = gamma8(getRed(color));
        uint8_t g = gamma8(getGreen(color));
        uint8_t b = gamma8(getBlue(color));
        return toColor(r, g, b);
	}

	int findClosestNormal(const float3* normals, int count, const float3& n) {
		float bestDot = -1000.0f;
		int bestFace = -1;
		for (int i = 0; i < count; ++i) {
			float dot = float3::dot(n, normals[i]);
			if (dot > bestDot) {
				bestDot = dot;
				bestFace = i;
			}
		}
		return bestFace;
	}

	int CalibrateNormals(
		int face1Index, const float3& face1Normal,
		int face2Index, const float3& face2Normal,
		int face3Index, const float3& face3Normal,
		const DiceVariants::Layouts* layouts,
		float3* outNormals, int faceCount) {

		// Figure out the rotation that transforms canonical normals into accelerometer reference frame

		float bestDot = -1000.0f;
		int bestDotIndex = -1;
		matrix3x3 bestDotRot;

		for (int i = 0; i < layouts->count; ++i) {
			auto& canonNormals = layouts->layouts[i]->faceNormals;

			// int closestCanonNormal1 = findClosestNormal(canonNormals, count, face1Normal);
			// int closestCanonNormal2 = findClosestNormal(canonNormals, count, face2Normal);

			// We need to build a rotation matrix that turns canonical face normals into the reference frame
			// of the accelerator, as defined by the measured coordinates of the 2 passed in face normals.
			float3 canonFace1Normal = canonNormals[face1Index];
			float3 canonFace2Normal = canonNormals[face2Index];

			// Create our intermediate reference frame in both spaces
			// Canonical space
			float3 intX_Canon = canonFace1Normal; intX_Canon.normalize();
			float3 intZ_Canon = float3::cross(intX_Canon, canonFace2Normal); intZ_Canon.normalize();
			float3 intY_Canon = float3::cross(intZ_Canon, intX_Canon);
			matrix3x3 int_Canon(intX_Canon, intY_Canon, intZ_Canon);

			// BLE_LOG_INFO("intX_Canon: %d, %d, %d", (int)(intX_Canon.x * 100), (int)(intX_Canon.y * 100), (int)(intX_Canon.z * 100));
			// BLE_LOG_INFO("intY_Canon: %d, %d, %d", (int)(intY_Canon.x * 100), (int)(intY_Canon.y * 100), (int)(intY_Canon.z * 100));
			// BLE_LOG_INFO("intZ_Canon: %d, %d, %d", (int)(intY_Canon.x * 100), (int)(intY_Canon.y * 100), (int)(intY_Canon.z * 100));

			// Accelerometer space
			float3 intX_Acc = face1Normal; intX_Acc.normalize();
			float3 intZ_Acc = float3::cross(intX_Acc, face2Normal); intZ_Acc.normalize();
			float3 intY_Acc = float3::cross(intZ_Acc, intX_Acc);
			matrix3x3 int_Acc(intX_Acc, intY_Acc, intZ_Acc);

			// This is the matrix that rotates canonical normals into accelerometer reference frame
			matrix3x3 rot = matrix3x3::mul(int_Acc, matrix3x3::transpose(int_Canon));

			// Compare the rotation of the third face with the measured one
			float3 canonFace3Normal = canonNormals[face3Index];
			//NRF_LOG_INFO("canon: %d, %d, %d", (int)(canonNormal.x * 100.0f), (int)(canonNormal.y * 100.0f), (int)(canonNormal.z * 100.0f));
			float3 rotatedFace3Normal = matrix3x3::mul(rot, canonFace3Normal);
			float dot = float3::dot(rotatedFace3Normal, face3Normal);
			if (dot > bestDot) {
				bestDot = dot;
				bestDotIndex = i;
				bestDotRot = rot;
			}
		}

		// Now transform all the normals
		auto& canonNormals = layouts->layouts[bestDotIndex]->faceNormals;
		for (int i = 0; i < faceCount; ++i) {
			float3 canonNormal = canonNormals[i];
			//NRF_LOG_INFO("canon: %d, %d, %d", (int)(canonNormal.x * 100.0f), (int)(canonNormal.y * 100.0f), (int)(canonNormal.z * 100.0f));
			float3 newNormal = matrix3x3::mul(bestDotRot, canonNormal);
			//NRF_LOG_INFO("new: %d, %d, %d", (int)(newNormal.x * 100.0f), (int)(newNormal.y * 100.0f), (int)(newNormal.z * 100.0f));
			outNormals[i] = newNormal;
		}

		// Return the index as well so we can use the proper remap face table
		return bestDotIndex;
	}

	bool CalibrateInternalRotation(
		int led0Index, const Core::float3& led0Normal,
		int led1Index, const Core::float3& led1Normal,
		const Core::float3* newNormals,
		const Config::DiceVariants::Layout* layout,
		uint8_t* faceToLEDOut,
		int faceCount) {

		// Iterate over all possible rotations to find the one that maps to our measured normals
		// We have the measured normal for led index 0, we can find which remapping it corresponds to
		float bestDot = -1000.0f;
		int led0FaceIndex = -1;
		for (int i = 0; i < faceCount; ++i) {
			float dot = float3::dot(led0Normal, newNormals[i]);
			if (dot > bestDot) {
				bestDot = dot;
				led0FaceIndex = i;
			}
		}

		// Iterate over all possible rotations to find the one that maps to our measured normals
		// We have the measured normal for led index 0, we can find which remapping it corresponds to
		bestDot = -1000.0f;
		int led1FaceIndex = -1;
		for (int i = 0; i < faceCount; ++i) {
			float dot = float3::dot(led1Normal, newNormals[i]);
			if (dot > bestDot) {
				bestDot = dot;
				led1FaceIndex = i;
			}
		}

		// Find which combination of rotation and remapping matches both
		for (int i = 0; i < faceCount; ++i) {
			for (int j = 0; j < layout->rotationRemapCount; ++j) {
				int rotf1 = layout->rotationRemap[j * faceCount + led0FaceIndex];
				int li1 = layout->faceRemap[i * faceCount + rotf1];
				if (layout->faceToLedLookup[li1] == led0Index) {
					// We found a combination of rotations that maps led0 to the measured face,
					// now see if we can match the other face
					int rotf2 = layout->rotationRemap[j * faceCount + led1FaceIndex];
					int li2 = layout->faceRemap[i * faceCount + rotf2];
					if (layout->faceToLedLookup[li2] == led1Index) {

						// Found it, write out the remapping
						for (int k = 0; k < faceCount; ++k) {
							int rotfk = layout->rotationRemap[j * faceCount + k];
							int lk = layout->faceRemap[i * faceCount + rotfk];
							faceToLEDOut[k] = layout->faceToLedLookup[lk];
						}

						// Bail now!
						return true;
					}
				}
			}
		}

		// Did not find a mapping
		return false;
	}

	/* D. J. Bernstein hash function */
	uint32_t computeHash(const uint8_t* data, int size) {
		uint32_t hash = 5381;
		for (int i = 0; i < size; ++i) {
			hash = 33 * hash ^ data[i];
		}
		return hash;
	}

	// Originals: https://github.com/andyherbert/lz1
	
	uint32_t lz77_compress (uint8_t *uncompressed_text, uint32_t uncompressed_size, uint8_t *compressed_text)
	{
		uint8_t pointer_length, temp_pointer_length;
		uint16_t pointer_pos, temp_pointer_pos, output_pointer;
		uint32_t compressed_pointer, output_size, coding_pos, output_lookahead_ref, look_behind, look_ahead;
		
		*((uint32_t *) compressed_text) = uncompressed_size;
		compressed_pointer = output_size = 4;
		
		for(coding_pos = 0; coding_pos < uncompressed_size; ++coding_pos)
		{
			pointer_pos = 0;
			pointer_length = 0;
			for(temp_pointer_pos = 1; (temp_pointer_pos < 4096) && (temp_pointer_pos <= coding_pos); ++temp_pointer_pos)
			{
				look_behind = coding_pos - temp_pointer_pos;
				look_ahead = coding_pos;
				for(temp_pointer_length = 0; uncompressed_text[look_ahead++] == uncompressed_text[look_behind++]; ++temp_pointer_length)
					if(temp_pointer_length == 15)
						break;
				if(temp_pointer_length > pointer_length)
				{
					pointer_pos = temp_pointer_pos;
					pointer_length = temp_pointer_length;
					if(pointer_length == 15)
						break;
				}
			}
			coding_pos += pointer_length;
			if(pointer_length && (coding_pos == uncompressed_size))
			{
				output_pointer = (pointer_pos << 4) | (pointer_length - 1);
				output_lookahead_ref = coding_pos - 1;
			}
			else
			{
				output_pointer = (pointer_pos << 4) | pointer_length;
				output_lookahead_ref = coding_pos;
			}
			*((uint32_t *) (compressed_text + compressed_pointer)) = output_pointer;
			compressed_pointer += 2;
			*(compressed_text + compressed_pointer++) = *(uncompressed_text + output_lookahead_ref);
			output_size += 3;
		}
		
		return output_size;
	}

	uint32_t lz77_decompress (uint8_t *compressed_text, uint8_t *uncompressed_text)
	{
		uint8_t pointer_length;
		uint16_t input_pointer, pointer_pos;
		uint32_t compressed_pointer, coding_pos, pointer_offset, uncompressed_size;
		
		uncompressed_size = *((uint32_t *) compressed_text);
		compressed_pointer = 4;
		
		for(coding_pos = 0; coding_pos < uncompressed_size; ++coding_pos)
		{
			input_pointer = *((uint32_t *) (compressed_text + compressed_pointer));
			compressed_pointer += 2;
			pointer_pos = input_pointer >> 4;
			pointer_length = input_pointer & 15;
			if(pointer_pos)
				for(pointer_offset = coding_pos - pointer_pos; pointer_length > 0; --pointer_length)
					uncompressed_text[coding_pos++] = uncompressed_text[pointer_offset++];
			*(uncompressed_text + coding_pos) = *(compressed_text + compressed_pointer++);
		}
		
		return coding_pos;
	}	

	uint8_t interpolateIntensity(uint8_t intensity1, int time1, uint8_t intensity2, int time2, int time) {
		int scaler = 1024;
		int scaledPercent = (time - time1) * scaler / (time2 - time1);
		return (uint8_t)((intensity1 * (scaler - scaledPercent) + intensity2 * scaledPercent) / scaler);
    }

    uint32_t modulateColor(uint32_t color, uint8_t intensity) {
		int red = getRed(color) * intensity / 255;
		int green = getGreen(color) * intensity / 255;
		int blue = getBlue(color) * intensity / 255;
		return toColor((uint8_t)red, (uint8_t)green, (uint8_t)blue);
    }

	uint16_t nextRand(uint16_t prevRand) {
		const uint32_t m = 1 << 16; // 16 bits
		const uint32_t a = 1103515245;
		const uint32_t c = 12345;
		return (uint16_t)((a * (uint32_t)prevRand + c) % m);
	}

}
