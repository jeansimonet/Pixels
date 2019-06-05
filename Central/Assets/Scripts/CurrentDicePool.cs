using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentDicePool
    : MonoBehaviour
{
    public Central central;

    [Header("Fields")]
    public Button doneButton;
    public Button addDiceButton;
    public GameObject diceListRoot;
    public GameObject noDiceIndicator;
    public CanvasGroup canvasGroup;

    [Header("References")]
    public AddDiceToPool addDiceDialog;

    List<CurrentDicePoolDice> dice;

	// Use this for initialization
	void Awake()
    {
        Hide();
	}
	
	// Update is called once per frame
	void Update ()
    {
	}

    public void Show()
    {
        canvasGroup.gameObject.SetActive(true);
        Populate();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;
        RegisterEvents();
    }

    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.0f;
        UnregisterEvents();
    }

    public void Populate()
    {
        // Setup buttons
        doneButton.onClick.RemoveAllListeners();
        doneButton.onClick.AddListener(Hide);

        dice = new List<CurrentDicePoolDice>();

        int childCount = diceListRoot.transform.childCount;
        for (int i = 1; i < childCount; ++i)
        {
            GameObject.Destroy(diceListRoot.transform.GetChild(i).gameObject);
        }

        diceListRoot.SetActive(false);
        noDiceIndicator.SetActive(true);

        // Add all currently selected dice
        foreach(Die die in central.diceList)
        {
            Debug.Log("Considering die " + die.name + " (state:" + die.state + ")");
            if (die.connected)
            {
                AddDie(die);
            }
        }

        addDiceButton.onClick.RemoveAllListeners();
        addDiceButton.onClick.AddListener(() => addDiceDialog.Show());
    }

    void AddDie(Die die)
    {
        if (diceListRoot.transform.childCount > 0)
        {
            var template = diceListRoot.transform.GetChild(0).gameObject;

            noDiceIndicator.SetActive(false);
            diceListRoot.SetActive(true);

            CurrentDicePoolDice cmp = null;
            if (dice.Count == 0)
            {
                cmp = template.GetComponent<CurrentDicePoolDice>();
            }
            else
            {
                var go = GameObject.Instantiate(template, diceListRoot.transform);
                cmp = go.GetComponent<CurrentDicePoolDice>();
            }

            // Add the newly created ui to our list
            dice.Add(cmp);

            // Setup
            cmp.Setup(die, central);

        }
        else
        {
            Debug.LogError("No templace dice!");
        }
    }

    void RemoveDie(Die die)
    {
        var cmp = dice.Find(dui => dui.die == die);
        if (cmp != null)
        {
            if (dice.Count > 1)
            {
                GameObject.Destroy(cmp.gameObject);
            }
            dice.Remove(cmp);
            if (dice.Count == 0)
            {
                // Deactivate displays!
                diceListRoot.SetActive(false);
                noDiceIndicator.SetActive(true);
            }
        }
    }

    void RegisterEvents()
    {
        // Setup event to add newly connected dice
        central.onDieConnected += AddDie;

        // Setup events to remove disconnected dice
        central.onDieDisconnected += RemoveDie;
    }

    void UnregisterEvents()
    {
        central.onDieConnected -= AddDie;
        central.onDieDisconnected -= RemoveDie;
    }

}
