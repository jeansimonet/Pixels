using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorMapping
{
    static Color32[] SourceColors =
    {
        new Color32(0, 0, 0, 255),
        new Color32(255, 255, 255, 255),
        new Color32(128, 0, 0, 255),
        new Color32(128, 0, 44, 255),
        new Color32(128, 0, 66, 255),
        new Color32(128, 0, 87, 255),
        new Color32(128, 0, 109, 255),
        new Color32(124, 0, 128, 255),
        new Color32(102, 0, 128, 255),
        new Color32(80, 0, 128, 255),
        new Color32(58, 0, 128, 255),
        new Color32(36, 0, 128, 255),
        new Color32(15, 0, 128, 255),
        new Color32(0, 7, 128, 255),
        new Color32(0, 29, 128, 255),
        new Color32(0, 51, 128, 255),
        new Color32(0, 73, 128, 255),
        new Color32(0, 95, 128, 255),
        new Color32(0, 117, 128, 255),
        new Color32(0, 128, 117, 255),
        new Color32(0, 128, 73, 255),
        new Color32(0, 128, 29, 255),
        new Color32(15, 128, 0, 255),
        new Color32(58, 128, 0, 255),
        new Color32(80, 128, 0, 255),
        new Color32(102, 128, 0, 255),
        new Color32(124, 128, 0, 255),
        new Color32(128, 109, 0, 255),
        new Color32(128, 87, 0, 255),
        new Color32(128, 66, 0, 255),
        new Color32(128, 44, 0, 255),
        new Color32(128, 22, 0, 255),
        new Color32(128, 88, 88, 255),
        new Color32(128, 88, 108, 255),
        new Color32(128, 88, 122, 255),
        new Color32(120, 88, 128, 255),
        new Color32(106, 88, 128, 255),
        new Color32(92, 88, 128, 255),
        new Color32(88, 97, 128, 255),
        new Color32(88, 110, 128, 255),
        new Color32(88, 124, 128, 255),
        new Color32(88, 128, 110, 255),
        new Color32(92, 128, 88, 255),
        new Color32(113, 128, 88, 255),
        new Color32(126, 128, 88, 255),
        new Color32(128, 115, 88, 255),
        new Color32(128, 101, 88, 255),
        new Color32(128, 128, 128, 255),
        new Color32(255, 0, 0, 255),
        new Color32(255, 0, 87, 255),
        new Color32(255, 0, 131, 255),
        new Color32(255, 0, 175, 255),
        new Color32(255, 0, 219, 255),
        new Color32(248, 0, 255, 255),
        new Color32(204, 0, 255, 255),
        new Color32(160, 0, 255, 255),
        new Color32(117, 0, 255, 255),
        new Color32(73, 0, 255, 255),
        new Color32(29, 0, 255, 255),
        new Color32(0, 15, 255, 255),
        new Color32(0, 58, 255, 255),
        new Color32(0, 102, 255, 255),
        new Color32(0, 146, 255, 255),
        new Color32(0, 189, 255, 255),
        new Color32(0, 233, 255, 255),
        new Color32(0, 255, 233, 255),
        new Color32(0, 255, 146, 255),
        new Color32(0, 255, 58, 255),
        new Color32(29, 255, 0, 255),
        new Color32(117, 255, 0, 255),
        new Color32(160, 255, 0, 255),
        new Color32(204, 255, 0, 255),
        new Color32(248, 255, 0, 255),
        new Color32(255, 219, 0, 255),
        new Color32(255, 175, 0, 255),
        new Color32(255, 131, 0, 255),
        new Color32(255, 87, 0, 255),
        new Color32(255, 44, 0, 255),
        new Color32(255, 175, 175, 255),
        new Color32(255, 175, 216, 255),
        new Color32(255, 175, 244, 255),
        new Color32(239, 175, 255, 255),
        new Color32(212, 175, 255, 255),
        new Color32(184, 175, 255, 255),
        new Color32(175, 193, 255, 255),
        new Color32(175, 221, 255, 255),
        new Color32(175, 248, 255, 255),
        new Color32(175, 255, 221, 255),
        new Color32(184, 255, 175, 255),
        new Color32(225, 255, 175, 255),
        new Color32(253, 255, 175, 255),
        new Color32(255, 230, 175, 255),
        new Color32(255, 202, 175, 255),
    };

    static Color32[] DestColors =
    {
        new Color32(0, 0, 0, 255),
        new Color32(129, 228, 255, 255),
        new Color32(16, 0, 0, 255),
        new Color32(18, 0, 1, 255),
        new Color32(20, 0, 2, 255),
        new Color32(18, 0, 6, 255),
        new Color32(16, 0, 10, 255),
        new Color32(13, 0, 13, 255),
        new Color32(8, 0, 18, 255),
        new Color32(4, 0, 18, 255),
        new Color32(2, 0, 20, 255),
        new Color32(1, 0, 20, 255),
        new Color32(0, 0, 18, 255),
        new Color32(0, 0, 18, 255),
        new Color32(0, 1, 20, 255),
        new Color32(0, 2, 20, 255),
        new Color32(0, 3, 16, 255),
        new Color32(0, 5, 12, 255),
        new Color32(0, 12, 13, 255),
        new Color32(0, 13, 8, 255),
        new Color32(1, 16, 3, 255),
        new Color32(2, 18, 0, 255),
        new Color32(4, 18, 0, 255),
        new Color32(6, 16, 0, 255),
        new Color32(8, 14, 0, 255),
        new Color32(11, 16, 0, 255),
        new Color32(13, 13, 0, 255),
        new Color32(16, 11, 0, 255),
        new Color32(18, 6, 0, 255),
        new Color32(20, 4, 0, 255),
        new Color32(20, 2, 0, 255),
        new Color32(20, 1, 0, 255),
        new Color32(18, 6, 6, 255),
        new Color32(18, 9, 11, 255),
        new Color32(14, 5, 14, 255),
        new Color32(13, 5, 15, 255),
        new Color32(10, 8, 16, 255),
        new Color32(7, 7, 18, 255),
        new Color32(7, 10, 20, 255),
        new Color32(5, 11, 14, 255),
        new Color32(3, 13, 11, 255),
        new Color32(4, 16, 5, 255),
        new Color32(5, 18, 2, 255),
        new Color32(14, 15, 2, 255),
        new Color32(14, 13, 1, 255),
        new Color32(16, 8, 1, 255),
        new Color32(18, 6, 1, 255),
        new Color32(12, 12, 12, 255),
        new Color32(255, 0, 0, 255),
        new Color32(255, 0, 11, 255),
        new Color32(255, 0, 100, 255),
        new Color32(240, 0, 255, 255),
        new Color32(183, 0, 255, 255),
        new Color32(141, 0, 255, 255),
        new Color32(100, 0, 255, 255),
        new Color32(57, 0, 255, 255),
        new Color32(32, 0, 255, 255),
        new Color32(11, 0, 255, 255),
        new Color32(2, 0, 255, 255),
        new Color32(0, 2, 255, 255),
        new Color32(0, 6, 255, 255),
        new Color32(0, 17, 155, 255),
        new Color32(0, 54, 255, 255),
        new Color32(0, 81, 255, 255),
        new Color32(0, 179, 255, 255),
        new Color32(0, 255, 222, 255),
        new Color32(18, 255, 43, 255),
        new Color32(29, 255, 0, 255),
        new Color32(45, 255, 0, 255),
        new Color32(80, 255, 0, 255),
        new Color32(96, 255, 0, 255),
        new Color32(116, 255, 0, 255),
        new Color32(144, 255, 0, 255),
        new Color32(213, 219, 0, 255),
        new Color32(255, 140, 0, 255),
        new Color32(255, 76, 0, 255),
        new Color32(255, 24, 0, 255),
        new Color32(255, 8, 0, 255),
        new Color32(255, 87, 130, 255),
        new Color32(198, 80, 216, 255),
        new Color32(134, 88, 255, 255),
        new Color32(110, 49, 255, 255),
        new Color32(68, 26, 255, 255),
        new Color32(52, 31, 255, 255),
        new Color32(45, 61, 255, 255),
        new Color32(27, 116, 255, 255),
        new Color32(17, 200, 255, 255),
        new Color32(52, 255, 138, 255),
        new Color32(88, 255, 91, 255),
        new Color32(106, 255, 48, 255),
        new Color32(171, 255, 125, 255),
        new Color32(224, 255, 175, 255),
        new Color32(255, 179, 153, 255),
    };

    static byte[] GammaTable =
    {
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
        218,220,223,225,227,230,232,235,237,240,242,245,247,250,252,255
    };


    public static Color32 RemapColor(Color color)
    {
        Vector3 inputAsVec = new Vector3(color.r, color.g, color.b);
        // This is not interpolating, just find the closest color and return the matching mapped color
        float minDist = float.MaxValue;
        Color32 ret = Color.white;
        for (int i = 0; i < SourceColors.Length; ++i)
        {
            Color src = SourceColors[i];
            Vector3 colAsVec = new Vector3(src.r, src.g, src.b);
            float dist = Vector3.Distance(inputAsVec, colAsVec);
            if (dist < minDist)
            {
                minDist = dist;
                ret = DestColors[i];
            }
        }

        // Inverse gamma correct the color, so that it looks correct on the die
        for (int i = 0; i < 3; ++i)
        {
            byte comp = ret[i];
            if (comp == 255)
            {
                ret[i] = 255;
            }
            else if (comp == 0)
            {
                ret[i] = 0;
            }
            else
            {
                byte index = 255;
                while (GammaTable[index] > comp)
                {
                    index--;
                }
                ret[i] = index;
            }
        }

        return ret;
    }

    public static Color32 InverseRemap(Color color)
    {
        Vector3 inputAsVec = new Vector3(color.r, color.g, color.b);
        // This is not interpolating, just find the closest color and return the matching mapped color
        float minDist = float.MaxValue;
        Color32 ret = Color.white;
        for (int i = 0; i < DestColors.Length; ++i)
        {
            Color src = DestColors[i];
            Vector3 colAsVec = new Vector3(src.r, src.g, src.b);
            float dist = Vector3.Distance(inputAsVec, colAsVec);
            if (dist < minDist)
            {
                minDist = dist;
                ret = SourceColors[i];
            }
        }
        return ret;
    }
}
