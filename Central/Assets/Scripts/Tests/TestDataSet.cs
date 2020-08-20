using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TestDataSet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TestData()
    {
        var gradientkf = new List<Animations.EditRGBKeyframe>();
        gradientkf.Add(new Animations.EditRGBKeyframe() { time = 0.2f, color = Color.red});
        gradientkf.Add(new Animations.EditRGBKeyframe() { time = 0.4f, color = Color.blue});
        gradientkf.Add(new Animations.EditRGBKeyframe() { time = 0.7f, color = Color.yellow});
        PixelsApp.Instance.ShowGradientEditor("Edit Gradient", new Animations.EditRGBGradient(){keyframes = gradientkf}, null);


        // var appset = AppDataSet.CreateTestDataSet();
        // var jsonText = appset.ToJson();
        // var filePath = System.IO.Path.Combine(Application.persistentDataPath, $"test_dataset.json");
        // File.WriteAllText(filePath, jsonText);
        // Debug.Log($"File written to {filePath}");
    }

    public void TestImportGradient()
    {
        var filePath = System.IO.Path.Combine(Application.persistentDataPath, $"gradientLine.png");
        byte[] fileData = File.ReadAllBytes(filePath);
        var tex = new Texture2D(2, 2);
        tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        var keyframes = ColorUtils.extractKeyframes(tex.GetPixels());
        var gradient = new Animations.EditRGBGradient() { keyframes = keyframes };
        PixelsApp.Instance.ShowGradientEditor("test", gradient, null);
    }
}
