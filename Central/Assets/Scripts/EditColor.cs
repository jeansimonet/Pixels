using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public struct EditColor
{
    public enum ColorType
    {
        RGB = 0,
        Face,
        Random
    }

    public ColorType type;
    public Color32 rgbColor; // Used when type is ColorType.RGB

    [JsonIgnore]
    public Color32 asColor32
    {
        get
        {
            switch (type)
            {
                case ColorType.RGB:
                    return rgbColor;
                case ColorType.Face:
                case ColorType.Random:
                default:
                    throw new System.NotImplementedException();
            }
        }
    }

    public static EditColor MakeRGB(Color rgb)
    {
        return new EditColor() { type = ColorType.RGB, rgbColor = rgb };
    }

    public uint toColorIndex(ref List<Color> palette)
    {
        switch (type)
        {
            case ColorType.RGB:
                {
                    float colorThreshold = 0.02f;
                    var rgb = rgbColor;
                    int colorIndex = palette.FindIndex(c => ColorUtils.computeSqrColorDistance(c, rgb) < colorThreshold);
                    if (colorIndex == -1)
                    {
                        colorIndex = palette.Count;
                        palette.Add(rgb);
                    }
                    return (uint)colorIndex;
                }
            case ColorType.Face:
                return DataSet.AnimationBits.PALETTE_COLOR_FROM_FACE;
            case ColorType.Random:
                return DataSet.AnimationBits.PALETTE_COLOR_FROM_RANDOM;
            default:
                throw new System.NotImplementedException();
        }
    }

}
