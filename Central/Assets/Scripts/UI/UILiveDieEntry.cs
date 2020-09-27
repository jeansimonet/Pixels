using Dice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILiveDieEntry : MonoBehaviour
{
    [Header("Controls")]
    public RawImage dieImage;
    public Text rollText;
    public Text timestamp;

    public void Setup(EditDie die, int faceIndex, float time)
    {
        //var dieTexture = DiceRendererManager.Instance.GetTextureForDie(die.die);
        //dieImage.texture = dieTexture;

        rollText.text = die.name + " rolled a " + (faceIndex + 1).ToString();
        timestamp.text = "";
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
