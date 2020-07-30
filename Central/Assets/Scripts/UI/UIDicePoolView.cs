using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDicePoolView : MonoBehaviour
{
    [Header("Controls")]
    public GameObject contentRoot;
    public Button addNewDiceButton;

    [Header("Prefabs")]
    public UIPairedDieView pairedDieViewPrefab;

    List<UIPairedDieView> pairedDice = new List<UIPairedDieView>();

    void Awake()
    {
        addNewDiceButton.onClick.AddListener(AddNewDice);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    void OnEnable()
    {
        // Grab the list of dice from the pool and create new entries for each
        foreach (var die in DicePool.Instance.allPoolDice)
        {
            var pairedDie = CreatePairedDie(die);
            pairedDice.Add(pairedDie);
        }
    }

    void OnDisable()
    {
        foreach (var die in pairedDice)
        {
            GameObject.Destroy(die.gameObject);
        }
        pairedDice.Clear();
    }

    UIPairedDieView CreatePairedDie(Die die)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIPairedDieView>(pairedDieViewPrefab, Vector3.zero, Quaternion.identity, contentRoot.transform);
        // Initialize it
        ret.Setup(die);
        return ret;
    }

    void AddNewDice()
    {
        //        
    }
}
