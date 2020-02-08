using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetRGB : MonoBehaviour
{
    public InputField R;
    public InputField G;
    public InputField B;
    public Image img;

    // Start is called before the first frame update
    void Start()
    {
        R.text = "255";
        G.text = "255";
        B.text = "255";
        img.color = Color.white;

        R.onValueChanged.AddListener((s) => UpdateInColor());
        G.onValueChanged.AddListener((s) => UpdateInColor());
        B.onValueChanged.AddListener((s) => UpdateInColor());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateInColor()
    {
        byte r = System.Convert.ToByte(R.text);
        byte g = System.Convert.ToByte(G.text);
        byte b = System.Convert.ToByte(B.text);
        Color32 c = new Color32(r, g, b, (byte)255);
        img.color = c;
    }
}
