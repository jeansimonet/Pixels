using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class CurrentDicePoolDice
    : MonoBehaviour
{
    public Text nameText;
    public Text statusText;
    public Image diceImage;
    public Button diceButton;
    public CanvasGroup commandGroup;
    public Button renameButton;
    public Button forgetButton;
    public Button flashButton;
    public Button setColorButton;
    public RenameDieDialog renameDieDialog;

    public Die die { get; private set; }
    public Central central;
    bool commandsShown
    {
        get
        {
            return commandGroup.interactable;
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Setup(Die die, Central central)
    {
        if (this.die != null)
        {
            die.OnSettingsChanged -= OnDieSettingsChanged;
        }

        this.die = die;
        this.central = central;
        nameText.text = die.name;
        statusText.text = die.connectionState.ToString();
        diceImage.color = Color.white;
        HideCommands();

        diceButton.onClick.RemoveAllListeners();
        diceButton.onClick.AddListener(ShowHideCommands);

        this.die.OnSettingsChanged += OnDieSettingsChanged;
        this.die.OnConnectionStateChanged += UpdateConnectionState;
    }

    private void OnDestroy()
    {
        this.die.OnSettingsChanged -= OnDieSettingsChanged;
        this.die.OnConnectionStateChanged -= UpdateConnectionState;
    }

    void ShowHideCommands()
    {
        if (commandsShown)
        {
            HideCommands();
        }
        else
        {
            ShowCommands();
        }
    }

    void ShowCommands()
    {
        commandGroup.interactable = true;
        commandGroup.blocksRaycasts = true;
        commandGroup.alpha = 1.0f;

        // Register commands
        forgetButton.onClick.RemoveAllListeners();
        forgetButton.onClick.AddListener(ForgetDie);

        renameButton.onClick.RemoveAllListeners();
        renameButton.onClick.AddListener(RenameDie);

        flashButton.onClick.RemoveAllListeners();
        flashButton.onClick.AddListener(FlashDie);

        setColorButton.onClick.RemoveAllListeners();
        setColorButton.onClick.AddListener(SetNewDieColor);

    }

    void HideCommands()
    {
        commandGroup.interactable = false;
        commandGroup.blocksRaycasts = false;
        commandGroup.alpha = 0.0f;
    }

    void ForgetDie()
    {
        // Tell central to forget about this die
        central.ForgetDie(die);
        HideCommands();
    }

    void OnTelemetryData(string characteristic, byte[] data)
    {
        Debug.Log("Telemetry Message Received");
    }

    void RenameDie()
    {
        StartCoroutine(telemetryCoroutine());
    }

    IEnumerator telemetryCoroutine()
    {
        BluetoothLEHardwareInterface.SubscribeCharacteristic(
            die.address,
            "6E401000-B5A3-F393-E0A9-E50E24DCCA9E",
            "6E401001-B5A3-F393-E0A9-E50E24DCCA9E",
            null,
            OnTelemetryData);

        yield return new WaitForSeconds(0.5f);

        BluetoothLEHardwareInterface.WriteCharacteristic(
            die.address,
            "6E401000-B5A3-F393-E0A9-E50E24DCCA9E",
            "6E401002-B5A3-F393-E0A9-E50E24DCCA9E",
            new byte[1] { 1 },
            1, false, null);
    }

    void FlashDie()
    {
        HideCommands();
        die.Flash(0);
    }

    void SetNewDieColor()
    {
        HideCommands();
        die.SetNewColor(null);
    }

    void UpdateDieName()
    {
        nameText.text = die.name;
    }

    void OnDieSettingsChanged(Die die)
    {
        UpdateDieName();
    }

    void UpdateConnectionState(Die die, Die.ConnectionState newState)
    {
        statusText.text = newState.ToString();
    }
}
