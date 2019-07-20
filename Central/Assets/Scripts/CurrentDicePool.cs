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

    [Header("Prefabs")]
    public CurrentDicePoolDice diceUIPrefab;

    List<CurrentDicePoolDice> dice;

	// Use this for initialization
	void Awake()
    {
        Hide();
    }

    void Start()
    {
        // Setup buttons
        doneButton.onClick.RemoveAllListeners();
        doneButton.onClick.AddListener(Hide);

        int childCount = diceListRoot.transform.childCount;
        for (int i = 1; i < childCount; ++i)
        {
            GameObject.Destroy(diceListRoot.transform.GetChild(i).gameObject);
        }

        diceListRoot.SetActive(false);
        noDiceIndicator.SetActive(true);

        addDiceButton.onClick.RemoveAllListeners();
        addDiceButton.onClick.AddListener(() => addDiceDialog.Show());

        dice = new List<CurrentDicePoolDice>();
    }

    // Update is called once per frame
    void Update ()
    {
	}

    public void Show()
    {
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;
        central.onDieForgotten += RemoveDie;
    }

    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.0f;
        central.onDieForgotten -= RemoveDie;
    }

    public void AddDie(Die die)
    {
        noDiceIndicator.SetActive(false);
        diceListRoot.SetActive(true);

        // Add the newly created ui to our list
        var cmp = GameObject.Instantiate<CurrentDicePoolDice>(diceUIPrefab, diceListRoot.transform);
        cmp.Setup(die, central);
        dice.Add(cmp);

        if (die.connectionState == Die.ConnectionState.Advertising)
        {
            die.Connect();
        }
    }

    public void RemoveDie(Die die)
    {
        var cmp = dice.Find(dui => dui.die == die);
        if (cmp != null)
        {
            dice.Remove(cmp);
            GameObject.Destroy(cmp.gameObject);
            if (dice.Count == 0)
            {
                // Deactivate displays!
                diceListRoot.SetActive(false);
                noDiceIndicator.SetActive(true);
            }
        }
    }

}
