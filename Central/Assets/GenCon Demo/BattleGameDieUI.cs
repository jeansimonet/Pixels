using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleGameDieUI : MonoBehaviour
{
    // Start is called before the first frame update
    public Die die { get; set; }

    [SerializeField]
    Text _NameField;

    public void Setup(Die die)
    {
        this.die = die;
        _NameField.text = die.name;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
