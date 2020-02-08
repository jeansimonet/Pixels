using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class GeneratePalette
{
    [MenuItem("Tools/Create Palette")]
    static void CreatePalette()
    {
        int HCount = 36;
        int SCount = 3;
        int VCount = 2;
        Texture2D texture = new Texture2D(HCount, SCount * VCount);
        for (int i = 0; i < HCount; ++i)
        {
            for (int k = 0; k < VCount; ++k)
            {
                for (int j = 0; j < SCount; ++j)
                {
                    float h = (float)i / (HCount - 1);
                    float s = 1.0f - 0.66f * (float)j / SCount;
                    float v = 1.0f - 0.5f * (float)k / (VCount - 1);
                    Color c = ColorSelector.HSVToRGB(h, s * s, v);
                    texture.SetPixel(i, k * SCount + j, c);
                }
            }
        }
        texture.Apply();

        byte[] bytes = texture.EncodeToPNG();
        FileStream stream = new FileStream("Assets/Images/Palette.png", FileMode.OpenOrCreate, FileAccess.Write);
        BinaryWriter writer = new BinaryWriter(stream);
        for (int i = 0; i < bytes.Length; i++)
        {
            writer.Write(bytes[i]);
        }
        writer.Close();
        stream.Close();
        Object.DestroyImmediate(texture);
        AssetDatabase.Refresh();
    }
}
