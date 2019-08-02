using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;



public class TelemetryDemo
    : MonoBehaviour
{
    public GameObject diceRoot;
    public GameObject noDiceIndicator;

    Dictionary<Die, TelemetryDemoDie> trackedDice;

	void Awake()
	{
        trackedDice = new Dictionary<Die, TelemetryDemoDie>();

        // Clean up
        if (diceRoot.transform.childCount > 0)
        {
            int count = diceRoot.transform.childCount;
            for (int i = 1; i < count; ++i)
            {
                GameObject.Destroy(diceRoot.transform.GetChild(i).gameObject);
            }
        }
        else
        {
            Debug.LogError("No templace available dice ui!");
        }

        noDiceIndicator.SetActive(true);
        diceRoot.SetActive(false);
    }

    IEnumerator Start()
	{
        // Until we can properly record data, disable
        yield return new WaitUntil(() => Central.Instance.state == Central.State.Idle);

        // Register to be notified of new dice getting connected
        Central.Instance.onDieReady += OnNewDie;
    }

    private void OnDisable()
    {
        Central.Instance.onDieConnected -= OnNewDie;
    }

    public void OnNewDie(Die die)
    {
        // Create the UI for the die
        // Make sure to turn on the list!
        noDiceIndicator.SetActive(false);
        diceRoot.SetActive(true);

        var template = diceRoot.transform.GetChild(0).gameObject;
        var go = template;
        if (trackedDice.Count > 0)
        {
            // Copy first item rather than use it
            go = GameObject.Instantiate(template, diceRoot.transform);
        }

        var cmp = go.GetComponent<TelemetryDemoDie>();
        cmp.Setup(die);

        trackedDice.Add(die, cmp);
        RegisterDieEvents(die);
    }

    void Update()
    {
    }

    void OnDieTelemetry(Die die, AccelFrame frame)
    {
        trackedDice[die].OnTelemetryReceived(frame);
    }

    void OnDieConnectionStateChanged(Die die, Die.ConnectionState newConnected)
    {
        if (newConnected == Die.ConnectionState.Unavailable)
        {
            if (trackedDice.Count > 1)
            {
                var go = trackedDice[die].gameObject;
                GameObject.Destroy(go);
            }
            else
            {
                noDiceIndicator.SetActive(true);
                diceRoot.SetActive(false);
            }
            trackedDice.Remove(die);
            UnregisterDieEvents(die);
        }
    }

    void RegisterDieEvents(Die die)
    {
        die.OnConnectionStateChanged += OnDieConnectionStateChanged;
        die.OnTelemetry += OnDieTelemetry;
    }

    void UnregisterDieEvents(Die die)
    {
        die.OnConnectionStateChanged -= OnDieConnectionStateChanged;
        die.OnTelemetry -= OnDieTelemetry;
    }
}
