using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Presets;
using Dice;
using System.Linq;

public class ActiveBehaviorMonitor : MonoBehaviour
{
    List<EditDie> connectedDice = new List<EditDie>();

    // Start is called before the first frame update
    void Awake()
    {
        PixelsApp.Instance.onPresetDownloadEvent += OnPresetDownloadedEvent;        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnPresetDownloadedEvent(Presets.EditPreset newPreset)
    {
        // Check whether we should stay connected to some of the dice
        List<EditDie> toDisconnect = new List<EditDie>(connectedDice);
        if (newPreset != null)
        {
            foreach (var assignment in newPreset.dieAssignments)
            {
                if (assignment.behavior.CollectAudioClips().Any())
                {
                    // This die assignment uses a behavior that has audio clips, so stay connected to the die
                    if (connectedDice.Contains(assignment.die))
                    {
                        toDisconnect.Remove(assignment.die);
                    }
                    else
                    {
                        // Connect to the new die
                        DiceManager.Instance.ConnectDie(assignment.die, (d, res, _) =>
                        {
                            if (res)
                            {
                                connectedDice.Add(d);
                            }
                        });
                    }
                }
            }
        }

        foreach (var die in toDisconnect)
        {
            DiceManager.Instance.DisconnectDie(die);
            connectedDice.Remove(die);
        }
    }
}
