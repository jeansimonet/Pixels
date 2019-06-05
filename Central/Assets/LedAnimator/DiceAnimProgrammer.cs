using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceAnimProgrammer
    : MonoBehaviour
    , IClient
{
    public Central central;
    public TimelineView timeline;

    HashSet<Die> dice;

    void Awake()
    {
        dice = new HashSet<Die>();
    }

    // Use this for initialization
    IEnumerator Start()
    {
        // Until we can properly record data, disable
        yield return new WaitUntil(() => central.state == CentralState.Idle);

        // Register to be notified of new dice getting connected
        central.RegisterClient(this);
    }

    public void OnNewDie(Die die)
    {
        //// Create the UI for the die
        //// Make sure to turn on the list!
        //noDiceIndicator.SetActive(false);
        //diceRoot.SetActive(true);

        //var template = diceRoot.transform.GetChild(0).gameObject;
        //var go = template;
        //if (trackedDice.Count > 0)
        //{
        //    // Copy first item rather than use it
        //    go = GameObject.Instantiate(template, diceRoot.transform);
        //}

        //var cmp = go.GetComponent<TelemetryDemoDie>();
        //cmp.Setup(die);
        Debug.Log("Led animator connected to " + die.name);
        dice.Add(die);
    }


    // Update is called once per frame
    void Update () {
		
	}

    public void UploadCurrentAnim(Animations.AnimationSet animSet)
    {
        // Try to send the anim down!
        foreach (var die in dice)
        {
            Debug.Log("Uploading anim on die " + die.name);
            StartCoroutine(die.UploadAnimationSet(animSet));
        }

    }

    public void PlayAnim()
    {
        foreach (var die in dice)
        {
            Debug.Log("Playing anim on die " + die.name);
            die.PlayAnimation(0);
        }
    }
}
