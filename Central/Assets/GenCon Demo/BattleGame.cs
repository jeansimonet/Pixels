using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGame : MonoBehaviour
{
    [SerializeField]
    BattleGameUI _UI;

    [SerializeField]
    Central _Central;

    // The list of dice that are part of the game
    List<Die> dice = new List<Die>();

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PeriodicScanAndConnectCr());
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator PeriodicScanAndConnectCr()
    {
        yield return new WaitUntil(() => _Central.state == Central.State.Idle);
        _Central.onDieDiscovered += onDieDiscovered;

        while (true)
        {
            _Central.BeginScanForDice();
            yield return new WaitForSeconds(3.0f);
            _Central.StopScanForDice();
            yield return new WaitForSeconds(3.0f);
        }
    }

    public void onDieDiscovered(Die die)
    {
        // Connect to the die!

        // Here we should check against a list of dice that we know are the
        // wrong type so we don't connect for no reason. For now we connect all the time

        die.OnConnectionStateChanged += onDieConnectionStateChanged;
        die.Connect();
    }


    void onDieConnectionStateChanged(Die die, Die.ConnectionState newState)
    {
        switch (newState)
        {
            case Die.ConnectionState.Ready:
                // Now we can check the type of the die
                if (die.dieType == Die.DieType.SixSided)
                {
                    // Yay, good die!

                    // We shouldn't need to filter double discovering the same die, but there is a bug
                    // in the virtual bluetooth interface...
                    if (!dice.Contains(die))
                    {
                        AddDieToGame(die);
                    }
                }
                // Else we probably should disconnect and then keep a list
                // of invalid dice so we don't reconnected automatically
                break;
            case Die.ConnectionState.Unavailable:
                RemoveDieFromGame(die);
                break;
            default:
                break;
        }
    }

    void AddDieToGame(Die die)
    {
        dice.Add(die);
        _UI.AddDie(die);
    }

    void RemoveDieFromGame(Die die)
    {
        _UI.RemoveDie(die);
        dice.Remove(die);
    }
}
