#include "dice_variants.h"

using namespace Core;

namespace Config
{
namespace DiceVariants
{
    static const uint8_t sixSidedRemap[] = {
        // FIXME!!!
        0, 1, 2, 3, 4, 5,
        1, 2, 3, 4, 5, 0,
        2, 3, 4, 5, 0, 1,
        3, 4, 5, 0, 1, 2,
        4, 5, 0, 1, 2, 3, 
        5, 0, 1, 2, 3, 4, 
    };

    static const Core::float3 sixSidedNormals[] = {
        { 0, -1,  0},
        { 0,  0,  1},
        { 1,  0,  0},
        {-1,  0,  0},
        { 0,  0, -1},
        { 0,  1,  0}
    };

	static const uint8_t sixSidedRotationRemap[] = {
		0, 1, 2, 3, 4, 5,
	};

    static const uint8_t sixSidedFaceToLedLookup[] = {
        1, 4, 0, 3, 2, 5,
    };

    static const Layout D6Variant0 = {

		.faceNormals = sixSidedNormals,
		.faceRemap = sixSidedRemap,
		.rotationRemap = sixSidedRotationRemap,
		.rotationRemapCount = 1,
        .faceToLedLookup = sixSidedFaceToLedLookup,
	};

	static const Layouts D6Variants = {
		.layouts = { &D6Variant0 },
		.count = 1
	};


    static const uint8_t twentySidedRemap[] = {
        19,	17,	18,	9,	13,	14,	15,	8,	7,	3,	16,	12,	11,	4,	5,	6,	10,	1,	2,	0,
        18,	9,	8,	4,	13,	7,	2,	19,	16,	5,	14,	3,	0,	17,	12,	6,	15,	11,	10,	1,
        17,	12,	16,	14,	1,	11,	15,	10,	0,	13,	6,	19,	9,	4,	8,	18,	5,	3,	7,	2,
        16,	14,	10,	4,	1,	0,	7,	17,	6,	8,	11,	13,	2,	12,	19,	18,	15,	9,	5,	3,
        15,	12,	14,	13,	9,	18,	19,	11,	2,	16,	3,	17,	8,	0,	1,	10,	6,	5,	7,	4,
        14,	13,	11,	0,	9,	2,	7,	15,	3,	1,	18,	16,	4,	12,	17,	10,	19,	8,	6,	5,
        13,	14,	8,	10,	19,	17,	16,	18,	12,	4,	15,	7,	1,	3,	2,	0,	9,	11,	5,	6,
        12,	17,	3,	18,	5,	8,	19,	6,	4,	9,	10,	15,	13,	0,	11,	14,	1,	16,	2,	7,
        11,	1,	9,	2,	14,	7,	0,	15,	13,	3,	16,	6,	4,	19,	12,	5,	17,	10,	18,	8,
        10,	4,	17,	12,	1,	6,	5,	16,	11,	19,	0,	8,	3,	14,	13,	18,	7,	2,	15,	9,
        9,	2,	15,	19,	14,	13,	18,	11,	16,	12,	7,	3,	8,	1,	6,	5,	0,	4,	17,	10,
        8,	4,	19,	17,	13,	16,	10,	18,	14,	12,	7,	5,	1,	9,	3,	6,	2,	0,	15,	11,
        7,	13,	0,	1,	4,	10,	16,	2,	5,	11,	8,	14,	17,	3,	9,	15,	18,	19,	6,	12,
        6,	1,	3,	9,	12,	15,	11,	5,	19,	2,	17,	0,	14,	8,	4,	7,	10,	16,	18,	13,
        5,	4,	12,	19,	3,	18,	8,	6,	9,	17,	2,	10,	13,	11,	1,	16,	0,	7,	15,	14,
        4,	8,	0,	6,	2,	3,	5,	7,	9,	1,	18,	10,	12,	14,	16,	17,	13,	19,	11,	15,
        3,	9,	5,	8,	12,	19,	18,	6,	17,	4,	15,	2,	13,	1,	0,	7,	11,	14,	10,	16,
        2,	3,	7,	13,	4,	8,	18,	0,	10,	14,	5,	9,	19,	1,	11,	15,	6,	12,	16,	17,
        1,	11,	10,	5,	17,	12,	6,	16,	19,	4,	15,	0,	3,	13,	7,	2,	14,	9,	8,	18,
        0,	1,	2,	3,	4,	5,	6,	7,	8,	9,	10,	11,	12,	13,	14,	15,	16,	17,	18,	19,
    };

	// Assuming face 1 and 20 are correct, there are only 3 ways that the electronics
	// can fit inside the dice, and this table stores the face remmaping that match
	// so that the dice can light up the correct LED
	static const uint8_t calibrationRemap[] = {
		 0,	 1,	 2,	 3,	 4,	 5,	 6,	 7,	 8,	 9,	10,	11,	12,	13,	14,	15,	16,	17,	18,	19,
		 0,	 6,	 7,	14,	 2,	 9,	11,	 4,	18,	16,	 3,	 1,	15,	 8,	10,	17,	 5,	12,	13,	19,
		 0,	11,	 4,	10,	 7,	16,	 1,	 2,	13,	 5,	14,	 6,	17,	18,	 3,	12,	 9,	15,	 8,	19,
	};

    static const uint8_t twentySidedFaceToLedLookup[] = {
        15,	4,	7,	1,	0,	19,	8,	10,	6,	14,	9,	11,	5,	13,	3,	17,	16,	12,	18,	2
    };

    static const Core::float3 twentySidedNormals[] = {
        {-0.1273862f,  0.3333025f,  0.9341605f},
        { 0.6667246f, -0.7453931f, -0.0000000f},
        { 0.8726854f,  0.3333218f, -0.3568645f},
        {-0.3333083f, -0.7453408f, -0.5773069f},
        { 0.0000000f, -1.0000000f, -0.0000000f},
        {-0.7453963f,  0.3333219f,  0.5773357f},
        { 0.3333614f,  0.7453930f, -0.5774010f},
        {-0.7453431f,  0.3333741f, -0.5773722f},
        { 0.8726999f,  0.3333025f,  0.3567604f},
        { 0.1273475f, -0.3333741f,  0.9341723f},
        {-0.1273475f,  0.3333741f, -0.9341723f},
        {-0.8726999f, -0.3333025f, -0.3567604f},
        { 0.7453431f, -0.3333741f,  0.5773722f},
        {-0.3331230f, -0.7450288f,  0.5778139f},
        { 0.7453963f, -0.3333219f, -0.5773357f},
        { 0.0000000f,  1.0000000f, -0.0000000f},
        { 0.3333083f,  0.7453408f,  0.5773069f},
        {-0.8726854f, -0.3333218f,  0.3568645f},
        {-0.6667246f,  0.7453931f, -0.0000000f},
        { 0.1273862f, -0.3333025f, -0.9341605f},
    };

    static const Layout D20Variant0 = {

		.faceNormals = twentySidedNormals,
		.faceRemap = twentySidedRemap,
		.rotationRemap = calibrationRemap,
		.rotationRemapCount = 3,
        .faceToLedLookup = twentySidedFaceToLedLookup,
	};

    static const uint8_t twentySidedRemapV5[] = {
        19,	18,	11,	10,	5,	4,	7,	6,	17,	16,	3,	2,	13,	12,	15,	14,	9,	8,	1,	0,
        18,	11,	5,	4,	6,	3,	2,	19,	10,	7,	12,	9,	0,	17,	16,	13,	15,	14,	8,	1,
        17,	15,	14,	8,	13,	0,	1,	16,	12,	9,	10,	7,	3,	18,	19,	6,	11,	5,	4,	2,
        16,	17,	15,	12,	14,	8,	9,	13,	18,	19,	0,	1,	6,	10,	11,	5,	7,	4,	2,	3,
        15,	14,	13,	0,	16,	10,	7,	17,	8,	1,	18,	11,	2,	12,	9,	3,	19,	6,	5,	4,
        14,	15,	12,	19,	9,	3,	6,	8,	17,	18,	1,	2,	11,	13,	16,	10,	0,	7,	4,	5,
        13,	16,	17,	18,	15,	12,	19,	14,	10,	11,	8,	9,	5,	0,	7,	4,	1,	2,	3,	6,
        12,	19,	6,	5,	3,	2,	4,	9,	18,	11,	8,	1,	10,	15,	17,	16,	14,	13,	0,	7,
        11,	5,	6,	3,	19,	12,	9,	18,	4,	2,	17,	15,	1,	10,	7,	0,	16,	13,	14,	8,
        10,	11,	18,	19,	17,	15,	12,	16,	5,	6,	13,	14,	3,	7,	4,	2,	0,	1,	8,	9,
        9,	12,	19,	18,	6,	5,	11,	3,	15,	17,	2,	4,	16,	8,	14,	13,	1,	0,	7,	10,
        8,	14,	15,	17,	12,	19,	18,	9,	13,	16,	3,	6,	10,	1,	0,	7,	2,	4,	5,	11,
        7,	0,	1,	8,	2,	3,	9,	4,	13,	14,	5,	6,	15,	10,	16,	17,	11,	18,	19,	12,
        6,	3,	9,	8,	12,	15,	14,	19,	2,	1,	18,	17,	0,	5,	4,	7,	11,	10,	16,	13,
        5,	6,	19,	12,	18,	17,	15,	11,	3,	9,	10,	16,	8,	4,	2,	1,	7,	0,	13,	14,
        4,	7,	0,	13,	1,	8,	14,	2,	10,	16,	3,	9,	17,	5,	11,	18,	6,	19,	12,	15,
        3,	9,	12,	15,	19,	18,	17,	6,	8,	14,	5,	11,	13,	2,	1,	0,	4,	7,	10,	16,
        2,	4,	7,	10,	0,	13,	16,	1,	5,	11,	8,	14,	18,	3,	6,	19,	9,	12,	15,	17,
        1,	2,	4,	5,	7,	10,	11,	0,	3,	6,	13,	16,	19,	8,	9,	12,	14,	15,	17,	18,
        0,	1,	2,	3,	4,	5,	6,	7,	8,	9,	10,	11,	12,	13,	14,	15,	16,	17,	18,	19,
    };



	// Assuming face 1 and 20 are correct, there are only 3 ways that the electronics
	// can fit inside the dice, and this table stores the face remmaping that match
	// so that the dice can light up the correct LED
	static const uint8_t calibrationRemapV5[] = {
		 0,	 1,	 2,	 3,	 4,	 5,	 6,	 7,	 8,	 9,	10,	11,	12,	13,	14,	15,	16,	17,	18,	19,
		 0,	13,	14,	15,	 8,	 9,	12,	 1,	16,	17,	 2,	 3,	18,	 7,	10,	11,	 4,	 5,	 6,	19,
		 0,	 7,	10,	11,	16,	17,	18,	13,	 4,	 5,	14,	15,	 6,	 1,	 2,	 3,	 8,	 9,	12,	19,
	};

    static const uint8_t twentySidedFaceToLedLookupV5[] = {
        15,	1,	17,	4,	13,	7,	19,	9,	6,	10,	5,	11,	14,	3,	12,	8,	18,	0,	16,	2
    };

    static const Core::float3 twentySidedNormalsV5[] = {
        {-0.1273862f,  0.3333025f,  0.9341605f},
        { 0.7453963f, -0.3333219f, -0.5773357f},
        {-0.8726854f, -0.3333218f,  0.3568645f},
        { 0.3333614f,  0.7453930f, -0.5774010f},
        { 0.8726999f,  0.3333025f,  0.3567604f},
        {-0.7453431f,  0.3333741f, -0.5773722f},
        { 0.1273475f, -0.3333741f,  0.9341723f},
        {-0.3333083f, -0.7453408f, -0.5773069f},
        {-0.6667246f,  0.7453931f, -0.0000000f},
        { 0.0000000f, -1.0000000f, -0.0000000f},
        { 0.0000000f,  1.0000000f, -0.0000000f},
        { 0.6667246f, -0.7453931f, -0.0000000f},
        { 0.3333083f,  0.7453408f,  0.5773069f},
        {-0.1273475f,  0.3333741f, -0.9341723f},
        { 0.7453431f, -0.3333741f,  0.5773722f},
        {-0.8726999f, -0.3333025f, -0.3567604f},
        {-0.3331230f, -0.7450288f,  0.5778139f},
        { 0.8726854f,  0.3333218f, -0.3568645f},
        {-0.7453963f,  0.3333219f,  0.5773357f},
        { 0.1273862f, -0.3333025f, -0.9341605f},
    };

    static const Layout D20Variant1 = {

		.faceNormals = twentySidedNormalsV5,
		.faceRemap = twentySidedRemapV5,
		.rotationRemap = calibrationRemapV5,
		.rotationRemapCount = 3,
        .faceToLedLookup = twentySidedFaceToLedLookupV5,
	};

	static const Layouts D20Variants = {
		.layouts = { &D20Variant0, &D20Variant1 },
		.count = 2
	};


	const Layouts* getLayouts(int faceCount) {
		switch (faceCount)
		{
			case 6:
			return &D6Variants;
			case 20:
			default:
			return &D20Variants;
		}
	}

	const Layout* getLayout(int faceCount, int variantIndex) {
		auto v = getLayouts(faceCount);
		return v->layouts[variantIndex];
	}

    const float3* getDefaultNormals(int faceCount) {
        return getLayout(faceCount, 0)->faceNormals;
    }

    const uint8_t* getDefaultLookup(int faceCount) {
        return getLayout(faceCount, 0)->faceToLedLookup;
    }

}
}