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


    public EditDie die { get; private set; }
    public SingleDiceRenderer dieRenderer { get; private set; }
    public bool selected { get; private set; }

    public void Setup(EditDie die)
    {
        this.die = die;
        this.dieRenderer = DiceRendererManager.Instance.CreateDiceRenderer(die.designAndColor);
        if (dieRenderer != null)
        {
            dieRenderImage.texture = dieRenderer.renderTexture;
        }
        UpdateState();
        SetSelected(false);

        if (die.die == null)
        {
            die.onDieFound += OnDieFound;
        }
        else
        {
            OnDieFound(die);
        }
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

    public void UpdateState()
    {
        dieNameText.text = die.name;
        if (die.deviceId != 0)
        {
            dieIDText.text = "ID: " + die.deviceId.ToString("X08");
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
            switch (die.die.lastError)
            {
                case Die.LastError.None:
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
                    }
                    break;
                case Die.LastError.ConnectionError:
                    dieRenderer.SetAuto(false);
                    dieRenderImage.color = AppConstants.Instance.DieUnavailableColor;
                    batteryView.gameObject.SetActive(false);
                    signalView.gameObject.SetActive(false);
                    statusText.text = "Connection Error";
                    disconnectedTextRoot.gameObject.SetActive(false);
                    errorTextRoot.gameObject.SetActive(true);
                    break;
                case Die.LastError.Disconnected:
                    dieRenderer.SetAuto(false);
                    dieRenderImage.color = AppConstants.Instance.DieUnavailableColor;
                    batteryView.gameObject.SetActive(false);
                    signalView.gameObject.SetActive(false);
                    statusText.text = "Disconnected";
                    disconnectedTextRoot.gameObject.SetActive(true);
                    errorTextRoot.gameObject.SetActive(false);
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
            die.die.OnError -= OnError;
            die.die.OnAppearanceChanged -= OnAppearanceChanged;
            die.die.OnBatteryLevelChanged -= OnBatteryLevelChanged;
            die.die.OnRssiChanged -= OnRssiChanged;
        }
    }

    void OnConnectionStateChanged(Die die, Die.ConnectionState oldState, Die.ConnectionState newState)
    {
        UpdateState();
    }

    void OnError(Die die, Die.LastError lastError)
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

    void OnNameChanged(Die die, string newName)
    {
        this.die.name = die.name;
        UpdateState();
    }

    void OnAppearanceChanged(Die die, int newFaceCount, DesignAndColor newDesign)
    {
        this.die.designAndColor = newDesign;
        if (this.dieRenderer != null)
        {
            DiceRendererManager.Instance.DestroyDiceRenderer(this.dieRenderer);
            this.dieRenderer = null;
        }
        this.dieRenderer = DiceRendererManager.Instance.CreateDiceRenderer(newDesign);
        if (dieRenderer != null)
        {
            dieRenderImage.texture = dieRenderer.renderTexture;
        }
    }

    void OnDieFound(EditDie editDie)
    {
        Debug.Assert(editDie == die);
        die.die.OnConnectionStateChanged += OnConnectionStateChanged;
        die.die.OnError += OnError;
        die.die.OnAppearanceChanged += OnAppearanceChanged;
        die.die.OnBatteryLevelChanged += OnBatteryLevelChanged;
        die.die.OnRssiChanged += OnRssiChanged;

        bool saveUpdatedData = false;
        if (die.designAndColor != die.die.designAndColor)
        {
            OnAppearanceChanged(die.die, die.die.faceCount, die.die.designAndColor);
            saveUpdatedData = true;
        }

        if (die.name != die.die.name)
        {
            OnNameChanged(die.die, die.die.name);
            saveUpdatedData = true;
        }

        if (saveUpdatedData)
        {
            AppDataSet.Instance.SaveData();
        }
    }

    void OnDieWillBeLost(EditDie editDie)
    {
        editDie.die.OnConnectionStateChanged -= OnConnectionStateChanged;
        editDie.die.OnAppearanceChanged -= OnAppearanceChanged;
        editDie.die.OnBatteryLevelChanged -= OnBatteryLevelChanged;
        editDie.die.OnRssiChanged -= OnRssiChanged;
    }
}
