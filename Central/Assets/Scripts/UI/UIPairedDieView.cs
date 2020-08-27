using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Dice;
using Presets;

public class UIPairedDieView : MonoBehaviour
{
    // Controls
    [Header("Controls")]
    public RawImage dieRenderImage;
    public Text dieNameText;
    public Text dieIDText;
    public UIDieLargeBatteryView batteryView;
    public UIDieLargeSignalView signalView;
    public Text statusText;
    public RectTransform disconnectedTextRoot;
    public RectTransform errorTextRoot;

    [Header("Parameters")]
    public Color defaultTextColor;
    public Color selectedColor;


    public DiceManager.ManagedDie die { get; private set; }
    public SingleDiceRenderer dieRenderer { get; private set; }
    public bool selected { get; private set; }

    public void Setup(DiceManager.ManagedDie die)
    {
        this.die = die;
        this.dieRenderer = DiceRendererManager.Instance.CreateDiceRenderer(die.editDie.designAndColor);
        if (dieRenderer != null)
        {
            dieRenderImage.texture = dieRenderer.renderTexture;
        }
        UpdateState();
        SetSelected(false);

        if (die.die != null)
        {
            die.die.OnConnectionStateChanged += OnConnectionStateChanged;
            die.die.OnBatteryLevelChanged += OnBatteryLevelChanged;
            die.die.OnRssiChanged += OnRssiChanged;
        }
        die.onDieFound += OnDieFound;
        die.onDieWillBeLost += OnDieWillBeLost;
    }

    public void SetSelected(bool selected)
    {
        this.selected = selected;
        if (selected)
        {
            dieNameText.color = selectedColor;
        }
        else
        {
            dieNameText.color = defaultTextColor;
        }
    }

    void UpdateState()
    {
        dieNameText.text = die.editDie.name;
        if (die.editDie.deviceId != 0)
        {
            dieIDText.text = "ID: " + die.editDie.deviceId.ToString("X016");
        }
        else
        {
            dieIDText.text = "ID: Unavailable";
        }

        if (die.die == null)
        {
            batteryView.SetLevel(null);
            signalView.SetRssi(null);
            dieRenderer.SetAuto(false);
            dieRenderImage.color = Color.white;
            batteryView.gameObject.SetActive(true);
            signalView.gameObject.SetActive(true);
            statusText.text = "Disconnected";
            disconnectedTextRoot.gameObject.SetActive(false);
            errorTextRoot.gameObject.SetActive(false);
        }
        else
        {
            batteryView.SetLevel(die.die.batteryLevel);
            signalView.SetRssi(die.die.rssi);
            switch (die.die.connectionState)
            {
            case Die.ConnectionState.Invalid:
                dieRenderer.SetAuto(false);
                dieRenderImage.color = AppConstants.Instance.DieUnavailableColor;
                batteryView.gameObject.SetActive(false);
                signalView.gameObject.SetActive(false);
                statusText.text = "Invalid";
                disconnectedTextRoot.gameObject.SetActive(true);
                errorTextRoot.gameObject.SetActive(false);
                break;
            case Die.ConnectionState.Available:
                dieRenderer.SetAuto(true);
                dieRenderImage.color = Color.white;
                batteryView.gameObject.SetActive(true);
                signalView.gameObject.SetActive(true);
                statusText.text = "Available";
                disconnectedTextRoot.gameObject.SetActive(false);
                errorTextRoot.gameObject.SetActive(false);
                break;
            case Die.ConnectionState.Connecting:
                dieRenderer.SetAuto(false);
                dieRenderImage.color = Color.white;
                batteryView.gameObject.SetActive(true);
                signalView.gameObject.SetActive(true);
                statusText.text = "Identifying";
                disconnectedTextRoot.gameObject.SetActive(false);
                errorTextRoot.gameObject.SetActive(false);
                break;
            case Die.ConnectionState.Identifying:
                dieRenderer.SetAuto(true);
                dieRenderImage.color = Color.white;
                batteryView.gameObject.SetActive(true);
                signalView.gameObject.SetActive(true);
                statusText.text = "Identifying";
                disconnectedTextRoot.gameObject.SetActive(false);
                errorTextRoot.gameObject.SetActive(false);
                break;
            case Die.ConnectionState.Ready:
                dieRenderer.SetAuto(true);
                dieRenderImage.color = Color.white;
                batteryView.gameObject.SetActive(true);
                signalView.gameObject.SetActive(true);
                statusText.text = "Ready";
                disconnectedTextRoot.gameObject.SetActive(false);
                errorTextRoot.gameObject.SetActive(false);
                break;
            case Die.ConnectionState.CommError:
                dieRenderer.SetAuto(false);
                dieRenderImage.color = Color.white;
                batteryView.gameObject.SetActive(false);
                signalView.gameObject.SetActive(false);
                statusText.text = "Communication Error";
                disconnectedTextRoot.gameObject.SetActive(false);
                errorTextRoot.gameObject.SetActive(true);
                break;
            }
        }
    }

    void OnDestroy()
    {
        if (this.dieRenderer != null)
        {
            DiceRendererManager.Instance.DestroyDiceRenderer(this.dieRenderer);
            this.dieRenderer = null;
        }
        die.onDieFound -= OnDieFound;
        die.onDieWillBeLost -= OnDieWillBeLost;
        if (die.die != null)
        {
            die.die.OnConnectionStateChanged -= OnConnectionStateChanged;
            die.die.OnBatteryLevelChanged -= OnBatteryLevelChanged;
            die.die.OnRssiChanged -= OnRssiChanged;
        }
    }

    void OnConnectionStateChanged(Die die, Die.ConnectionState oldState, Die.ConnectionState newState)
    {
        UpdateState();
    }

    void OnBatteryLevelChanged(Die die, float? level)
    {
        UpdateState();
    }

    void OnRssiChanged(Die die, int? rssi)
    {
        UpdateState();
    }

    void OnDieFound(EditDie editDie, Die die)
    {
        die.OnConnectionStateChanged += OnConnectionStateChanged;
        die.OnBatteryLevelChanged += OnBatteryLevelChanged;
        die.OnRssiChanged += OnRssiChanged;
    }

    void OnDieWillBeLost(EditDie editDie1, Die die)
    {
        die.OnConnectionStateChanged -= OnConnectionStateChanged;
        die.OnBatteryLevelChanged -= OnBatteryLevelChanged;
        die.OnRssiChanged -= OnRssiChanged;
    }
}
