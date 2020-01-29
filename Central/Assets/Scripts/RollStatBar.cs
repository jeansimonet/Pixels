using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RollStatBar : MonoBehaviour
{
    public Text FaceNumber;
    public Text FaceCount;
    public Image Bar;

    public const float BarScale = 10.0f; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init(int faceIndex)
    {
        FaceNumber.text = (faceIndex + 1).ToString();
        FaceCount.text = "0";
        Bar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0.0f);
    }

    public void UpdateCount(int newCount)
    {
        FaceCount.text = newCount.ToString();
        Bar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newCount * BarScale);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
