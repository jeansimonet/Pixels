using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakNumber : MonoBehaviour
{
    public AudioClip[] numbers;

    Central central;
    AudioSource source;

    Die die;

    private void Awake()
    {
        central = GetComponent<Central>();
        source = GetComponent<AudioSource>();
    }
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

        die.OnStateChanged += OnDieStateChanged;
    }

    void RemoveDie(Die die)
    {
        if (this.die == die)
        {
            this.die = null;
        }
    }

    void OnDieStateChanged(Die die, Die.State newState)
    {
        if (newState == Die.State.Idle)
        {
            Debug.Log("New Face: " + die.face);
            source.PlayOneShot(numbers[die.face]);
        }
    }
}
