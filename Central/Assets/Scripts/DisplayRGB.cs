using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayRGB : MonoBehaviour
{
    public Text R;
    public Text G;
    public Text B;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Color32 c = (Color32)ColorSelector.GetColor();
        R.text = c.r.ToString();
        G.text = c.g.ToString();
        B.text = c.b.ToString();
    }
}
