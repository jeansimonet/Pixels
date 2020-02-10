using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetDieColor : MonoBehaviour
{
    public ColorSelector colorSelector;

    Die die;
    private void Awake()
    {
    }
    
    // Start is called before the first frame update
    void Start()
    {
        DicePool.Instance.onDieConnected += AddDie;
        DicePool.Instance.onDieDisconnected += RemoveDie;

        colorSelector.onMouseButtonUp.AddListener(SetColor);
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

    public void SetColor()
    {
        if (die != null)
        {
            die.SetLEDsToColor(ColorSelector.GetColor());
        }
    }

}
