using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(Central))]
public class DemoController : MonoBehaviour
{
	public Text consoleOutputUI;
	public Button scanButtonUI;
	public Text scanButtonTextUI;
	public GameObject diceRootUI;
	public UIDie diePrefab;
	public GameObject mainUI;
	public DiceControl diceControlUI;

	Central central;

    List<UIDie> dice;

	void Awake()
	{
		central = GetComponent<Central>();
        dice = new List<UIDie>();

    }

	// Use this for initialization
	void Start()
	{
        // Initialize Central
        central.BeginScanForDice((newDie) =>
        {
            var dieUI = GameObject.Instantiate<UIDie>(diePrefab);
            dieUI.transform.SetParent(diceRootUI.transform);
            dieUI.transform.position = Vector3.zero;
            dieUI.transform.rotation = Quaternion.identity;
            dieUI.transform.localScale = Vector3.one;
            dieUI.die = newDie;
            dieUI.faceNumber = -1;
            dieUI.OnMoreOptions += () => ShowDieCommands(newDie);
            dice.Add(dieUI);

            central.ConnectToDie(newDie, null, (lostDie) =>
            {
                GameObject.Destroy(dice.FirstOrDefault(uid => uid.die == lostDie).gameObject);
            });
        });
	}

	// Update is called once per frame
	void Update ()
	{
	}

	void DisplayMessage(string message)
	{
		consoleOutputUI.text = message;
		consoleOutputUI.color = Color.white;
	}

	void DisplayError(string message)
	{
		consoleOutputUI.text = message;
		consoleOutputUI.color = Color.red;
	}

	void ClearConsole()
	{
		consoleOutputUI.text = "";
	}

	void ShowDieCommands(Die die)
	{
		mainUI.SetActive(false);
		diceControlUI.Setup(die, HideDieCommands);
		diceControlUI.gameObject.SetActive(true);
		diceControlUI.AddCommand("1", () => die.PlayAnimation(0));
		diceControlUI.AddCommand("2", () => die.PlayAnimation(1));
		diceControlUI.AddCommand("3", () => die.PlayAnimation(2));
		diceControlUI.AddCommand("4", () => die.PlayAnimation(3));
		diceControlUI.AddCommand("5", () => die.PlayAnimation(4));
		diceControlUI.AddCommand("6", () => die.PlayAnimation(5));
		diceControlUI.AddCommand("Rand", () => die.PlayAnimation(6));
	}

	void HideDieCommands()
	{
		diceControlUI.gameObject.SetActive(false);
		mainUI.SetActive(true);
	}
}
