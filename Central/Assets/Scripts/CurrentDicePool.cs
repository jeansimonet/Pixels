using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentDicePool
    : MonoBehaviour
{
    [Header("Fields")]
    public Button doneButton;
    public Button addDiceButton;
    public Button menuButton;
    public GameObject diceListRoot;
    public GameObject noDiceIndicator;
    public CanvasGroup canvasGroup;
    public Canvas parentCanvas;

    [Header("References")]
    public AddDiceToPool addDiceDialog;

    [Header("Prefabs")]
    public CurrentDicePoolDice diceUIPrefab;

    public delegate void DiceEventHandler(Die die);
    public event DiceEventHandler DiceAdded;
    public event DiceEventHandler DiceRemoved;

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

        DicePool.Instance.onDieDisconnected += RemoveDie;
    }

    // Update is called once per frame
    void Update ()
    {
	}

    public void Show()
    {
        parentCanvas.gameObject.SetActive(true); // Unity WTF? Got to call this twice with 2019.1.10
        parentCanvas.gameObject.SetActive(true);
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;
    }

    public void Hide()
    {
        // canvasGroup.interactable = false;
        // canvasGroup.blocksRaycasts = false;
        // canvasGroup.alpha = 0.0f;
        parentCanvas.gameObject.SetActive(false);
    }

    public void AddDie(Die die)
    {
        noDiceIndicator.SetActive(false);
        diceListRoot.SetActive(true);

        // Add the newly created ui to our list
        var cmp = GameObject.Instantiate<CurrentDicePoolDice>(diceUIPrefab, diceListRoot.transform);
        cmp.Setup(die);
        dice.Add(cmp);

        if (die.connectionState == Die.ConnectionState.Advertising)
        {
            die.Connect();
        }

        DiceAdded?.Invoke(die);
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

            DiceRemoved?.Invoke(die);
        }
    }

}
