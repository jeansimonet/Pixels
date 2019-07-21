using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGameUI : MonoBehaviour
{
    [SerializeField]
    BattleGameDieUI _DiePrefab;

    Dictionary<Die, BattleGameDieUI> dice;

    // Start is called before the first frame update
    void Awake()
    {
        dice = new Dictionary<Die, BattleGameDieUI>();

        // Remove children
        for (int i = 0; i < transform.childCount; ++i)
        {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public BattleGameDieUI AddDie(Die die)
    {
        BattleGameDieUI dieUI = GameObject.Instantiate<BattleGameDieUI>(_DiePrefab);
        dieUI.transform.SetParent(transform);
        dieUI.Setup(die);
        dice.Add(die, dieUI);
        return dieUI;
    }

    public void RemoveDie(Die die)
    {
        BattleGameDieUI dieUI = null;
        if (dice.TryGetValue(die, out dieUI))
        {
            dice.Remove(die);
            GameObject.Destroy(dieUI.gameObject);
        }
    }
}
