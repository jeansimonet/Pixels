using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    public Central central;
    public RollStatBar rollBarPrefab;
    Die die;

    RollStatBar[] bars;
    int[] counts;

    // Start is called before the first frame update
    void Start()
    {
        central.onDieReady += AddDie;
        central.onDieDisconnected += RemoveDie;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void AddDie(Die die)
    {
        this.die = die;

        // Create the bars
        int count = die.dieType == Die.DieType.TwentySided ? 20 : 6;
        bars = new RollStatBar[count];
        counts = new int[count];
        for (int i = 0; i < count; ++i)
        {
            bars[i] = GameObject.Instantiate<RollStatBar>(rollBarPrefab, transform, false);
            bars[i].name = "Bar" + i;
            bars[i].Init(i);
            counts[i] = 0;
        }

        die.OnStateChanged += OnDieStateChanged;
    }

    void RemoveDie(Die die)
    {
        if (this.die == die)
        {
            this.die = null;
            foreach (var bar in bars)
            {
                GameObject.Destroy(bar);
            }
            bars = null;
            counts = null;
        }
    }

    void OnDieStateChanged(Die die, Die.State newState)
    {
        if (newState == Die.State.Idle)
        {
            // Record roll
            int newCount = counts[die.face] + 1;
            bars[die.face].UpdateCount(newCount);
            counts[die.face] = newCount;
        }
    }
}
