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
    public Text voltageText;
    public Image diceImage;
    public Button diceButton;
    public CanvasGroup commandGroup;
    public Button forgetButton;
    public Button testButton;
    public Button calibrateButton;
    public Button calibrateFaceButton;
    public Button AttractModeButton;
    public Button ResetParamsButton;
    public Transform faceSelectionRoot;

    public Die die { get; private set; }
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

    public void Setup(Die die)
    {
        this.die = die;
        nameText.text = die.name;
        statusText.text = die.connectionState.ToString();
        diceImage.color = Color.white;
        HideCommands();

        diceButton.onClick.RemoveAllListeners();
        diceButton.onClick.AddListener(ShowHideCommands);

        this.die.OnConnectionStateChanged += UpdateConnectionState;

        voltageText.text = "Batt: Unknown";
        if (die.connectionState >= Die.ConnectionState.Ready)
        {
            MonitorBatteryLevel(true);
        }
    }

    private void OnDestroy()
    {
        if (this.die != null)
        {
            this.die.OnConnectionStateChanged -= UpdateConnectionState;
        }
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

        testButton.onClick.RemoveAllListeners();
        testButton.onClick.AddListener(TestDie);

        calibrateButton.onClick.RemoveAllListeners();
        calibrateButton.onClick.AddListener(CalibrateDie);

        calibrateFaceButton.onClick.RemoveAllListeners();
        calibrateFaceButton.onClick.AddListener(() => ShowFaceSelection());

        //AttractModeButton.onClick.RemoveAllListeners();
        //AttractModeButton.onClick.AddListener(() => StartAttrackMode());

        AttractModeButton.onClick.RemoveAllListeners();
        AttractModeButton.onClick.AddListener(() => PrintNormals());

        ResetParamsButton.onClick.RemoveAllListeners();
        ResetParamsButton.onClick.AddListener(() => ResetParams());
    }

    void HideCommands()
    {
        HideFaceSelection();
        commandGroup.interactable = false;
        commandGroup.blocksRaycasts = false;
        commandGroup.alpha = 0.0f;
    }

    void ForgetDie()
    {
        // Tell central to forget about this die
        DicePool.Instance.DisconnectDie(die);
        HideCommands();
    }

    void TestDie()
    {
        die.StartHardwareTest();
        HideCommands();
    }

    void CalibrateDie()
    {
        die.StartCalibration();
        HideCommands();
    }

    void OnTelemetryData(string characteristic, byte[] data)
    {
        Debug.Log("Telemetry Message Received");
    }

    Coroutine updateBatteryLevelCoroutine = null;
    void MonitorBatteryLevel(bool monitor)
    {
        if (monitor)
        {
            if (updateBatteryLevelCoroutine == null)
            {
                updateBatteryLevelCoroutine = StartCoroutine(UpdateBatteryLevelCr());
            }
        }
        else
        {
            if (updateBatteryLevelCoroutine != null)
            {
                StopCoroutine(updateBatteryLevelCoroutine);
            }
        }
    }

    IEnumerator UpdateBatteryLevelCr()
    {
        while (true)
        {
            UpdateBatteryLevel();
            yield return new WaitForSeconds(5.0f);
        }
    }

    void UpdateBatteryLevel()
    {
        this.die.GetBatteryLevel((lvl) =>
        {
            if (lvl.HasValue)
            {
                voltageText.text = "Batt: " + lvl.Value.ToString("0.00") + "V";
            }
            else
            {
                voltageText.text = "Batt: Unknown";
            }
        });
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

    void UpdateConnectionState(Die die, Die.ConnectionState newState)
    {
        statusText.text = newState.ToString();
        MonitorBatteryLevel(die.connectionState >= Die.ConnectionState.Ready);
    }

    void ShowFaceSelection()
    {
        faceSelectionRoot.gameObject.SetActive(true);
        for (int i = 0; i < faceSelectionRoot.childCount; ++i)
        {
            var childButton = faceSelectionRoot.GetChild(i).GetComponent<Button>();
            childButton.onClick.RemoveAllListeners();
            int face = i;
            childButton.onClick.AddListener(() => CalibrateFace(face));
        }
    }

    void StartAttrackMode()
    {
        die.StartAttractMode();
    }

    void HideFaceSelection()
    {
        faceSelectionRoot.gameObject.SetActive(false);
    }

    void CalibrateFace(int face)
    {
        HideFaceSelection();
        HideCommands();
        die.CalibrateFace(face);
    }

    void PrintNormals()
    {
        HideCommands();
        die.PrintNormals();
    }

    void ResetParams()
    {
        HideCommands();
        die.ResetParams();
    }
}
