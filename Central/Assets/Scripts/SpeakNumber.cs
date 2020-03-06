using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeakNumber : MonoBehaviour
{
    public Text numberText;
    public AudioClip[] numbers;
    AudioSource source;

    Die die;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }
    // Start is called before the first frame update
    void Start()
    {
        DicePool.Instance.onDieConnected += AddDie;
        DicePool.Instance.onDieDisconnected += RemoveDie;
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
        numberText.text = (die.face + 1).ToString();
        if (newState == Die.State.Idle)
        {
            Debug.Log("New Face: " + die.face);
            source.PlayOneShot(numbers[die.face]);
        }
    }
}
