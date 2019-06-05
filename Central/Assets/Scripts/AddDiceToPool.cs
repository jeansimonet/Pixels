using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddDiceToPool : MonoBehaviour {

    public Central central;

    [Header("Fields")]
    public Button addSelectedButton;
    public Button cancelButton;
    public GameObject availableDiceListRoot;
    public GameObject noDiceIndicator;
    public CanvasGroup canvasGroup;

    HashSet<Die> selectedDice;
    List<AddDiceToPoolDice> availableDice;

    private void Awake()
    {
        Hide();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Show()
    {
        canvasGroup.gameObject.SetActive(true);
        Populate();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;
    }

    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.0f;
    }

    public void Populate()
    {
        selectedDice = new HashSet<Die>();
        availableDice = new List<AddDiceToPoolDice>();

        noDiceIndicator.SetActive(true);
        availableDiceListRoot.SetActive(false);

        if (availableDiceListRoot.transform.childCount > 0)
        {
            int count = availableDiceListRoot.transform.childCount;
            for (int i = 1; i < count; ++i)
            {
                GameObject.Destroy(availableDiceListRoot.transform.GetChild(i).gameObject);
            }
        }
        else
        {
            Debug.LogError("No templace available dice!");
        }

        // Setup buttons
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() =>
        {
            central.StopScanForDice();
            Hide();
        });

        addSelectedButton.onClick.RemoveAllListeners();
        addSelectedButton.onClick.AddListener(() =>
        {
            central.StopScanForDice();
            if (selectedDice.Count > 0)
            {
                // Have central connect to selected dice
                central.ConnectToDiceList(selectedDice);
            }
            Hide();
        });

        // Kickoff a scan right away!
        central.BeginScanForDice(AddAvailableDice);
    }

    private void AddAvailableDice(Die die)
    {
        Debug.Log("notified of discovered die name:" + die.name + ", addr:" + die.address);
        // Make sure to turn on the list!
        noDiceIndicator.SetActive(false);
        availableDiceListRoot.SetActive(true);

        AddDiceToPoolDice cmp = null;
        var template = availableDiceListRoot.transform.GetChild(0).gameObject;
        if (availableDice.Count > 0)
        {
            var go = GameObject.Instantiate(template, availableDiceListRoot.transform);
            cmp = go.GetComponent<AddDiceToPoolDice>();
        }
        else
        {
            cmp = template.GetComponent<AddDiceToPoolDice>();
        }
        cmp.Setup(die);
        cmp.onSelected += (sdie, selected) =>
        {
            if (selected)
                selectedDice.Add(sdie);
            else
                selectedDice.Remove(sdie);
        };
        availableDice.Add(cmp);
    }
}
