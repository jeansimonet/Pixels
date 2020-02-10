using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleColorPicker.Scripts;


public class CalibrateColor : MonoBehaviour
{
    public Palette palette;
    public ColorPicker picker;
    public Image Swatch;
    public Text PaletteR;
    public Text PaletteG;
    public Text PaletteB;
    public Text MapR;
    public Text MapG;
    public Text MapB;
    Die die;

    // Start is called before the first frame update
    void Start()
    {
        DicePool.Instance.onDieConnected += AddDie;
        DicePool.Instance.onDieDisconnected += RemoveDie;
        palette.ColorSelected += SetPaletteColor;
        picker.ColorChanged += SetDieColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void AddDie(Die die)
    {
        this.die = die;
    }
    
    void RemoveDie(Die die)
    {
        if (this.die == die)
        {
            this.die = null;
        }
    }

    public void SetPaletteColor(Color color)
    {
        Color32 c = color;
        Swatch.color = color;
        PaletteR.text = c.r.ToString();
        PaletteG.text = c.g.ToString();
        PaletteB.text = c.b.ToString();

        picker.SetColor(color);
    }

    public void SetDieColor(Color color)
    {
        Color32 c = color;
        MapR.text = c.r.ToString();
        MapG.text = c.g.ToString();
        MapB.text = c.b.ToString();
        if (die != null)
        {
            die.SetLEDsToColor(color);
        }
    }
}
