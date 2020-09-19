using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using System.Linq;

public static class AudioUtils
{
    // Copied from https://answers.unity.com/questions/699595/how-to-generate-waveform-from-audioclip.html
    public static Texture2D PaintWaveformSpectrum(AudioClip audio, float saturation, int width, int height, Color col)
    {
      Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
      float[] samples = new float[audio.samples];
      float[] waveform = new float[width];
      audio.GetData(samples, 0);
      int packSize = ( audio.samples / width ) + 1;
      int s = 0;
      for (int i = 0; i < audio.samples; i += packSize) {
          waveform[s] = Mathf.Abs(samples[i]);
          s++;
      }
 
      for (int x = 0; x < width; x++) {
          for (int y = 0; y < height; y++) {
              tex.SetPixel(x, y, Color.clear);
          }
      }
 
      for (int x = 0; x < waveform.Length; x++) {
          for (int y = 0; y <= waveform[x] * ((float)height * saturation); y++) {
              tex.SetPixel(x, ( height / 2 ) + y, col);
              tex.SetPixel(x, ( height / 2 ) - y, col);
          }
      }
      tex.Apply();
 
      return tex;
  }

    public static uint GenerateWaveformHash(AudioClip audio)
    {
        float[] samples = new float[audio.samples];
        audio.GetData(samples, 0);
        var byteArray = new byte[samples.Length * 4];
        System.Buffer.BlockCopy(samples, 0, byteArray, 0, byteArray.Length);
        return Utils.computeHash(byteArray);
    }
}
