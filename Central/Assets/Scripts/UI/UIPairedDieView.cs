using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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


    public Die die { get; private set; }
    public SingleDiceRenderer dieRenderer { get; private set; }
    public bool selected { get; private set; }

    public void Setup(Die die)
    {
        this.die = die;
        this.dieRenderer = DiceRendererManager.Instance.CreateDiceRenderer(die.designAndColor);
        if (dieRenderer != null)
        {
            dieRenderImage.texture = dieRenderer.renderTexture;
        }
        dieNameText.text = die.name;
        if (die.deviceId != 0)
        {
            dieIDText.text = "ID: " + die.deviceId.ToString("X016");
        }
        else
        {
            dieIDText.text = "ID: Unavailable";
        }
        batteryView.SetLevel(null);
        signalView.SetRssi(null);
        SetState(die.connectionState);
        SetSelected(false);

        die.OnConnectionStateChanged += OnConnectionStateChanged;
        die.OnBatteryLevelChanged += OnBatteryLevelChanged;
        die.OnRssiChanged += OnRssiChanged;

    }

    public void BeginRefreshPool()
    {
        // Pause die events
        //die.OnConnectionStateChanged -= OnConnectionStateChanged;
    }

    public void FinishRefreshPool()
    {
       // SetState(die.connectionState);
       // die.OnConnectionStateChanged += OnConnectionStateChanged;
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

    void SetState(Die.ConnectionState newState)
    {
        switch (newState)
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
            case Die.ConnectionState.Unknown:
                dieRenderer.SetAuto(false);
                dieRenderImage.color = Color.white;
                batteryView.gameObject.SetActive(true);
                signalView.gameObject.SetActive(true);
                statusText.text = "Disconnected";
                disconnectedTextRoot.gameObject.SetActive(false);
                errorTextRoot.gameObject.SetActive(false);
                break;
            case Die.ConnectionState.New:
                dieRenderer.SetAuto(true);
                dieRenderImage.color = Color.white;
                batteryView.gameObject.SetActive(true);
                signalView.gameObject.SetActive(true);
                statusText.text = "New die";
                disconnectedTextRoot.gameObject.SetActive(false);
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
            case Die.ConnectionState.Missing:
                dieRenderer.SetAuto(false);
                dieRenderImage.color = Color.white;
                batteryView.gameObject.SetActive(false);
                signalView.gameObject.SetActive(false);
                statusText.text = "Unreachable";
                disconnectedTextRoot.gameObject.SetActive(true);
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

    void OnForget()
    {
        PixelsApp.Instance.ShowDialogBox(
            "Forget " + die.name + "?",
            "Are you sure you want to remove it from your dice bag?",
            "Forget",
            "Cancel",
            (forget) =>
            {
                if (forget)
                {
                    DicePool.Instance.ForgetDie(die);
                }
            });
    }

    void OnDestroy()
    {
        if (this.dieRenderer != null)
        {
            DiceRendererManager.Instance.DestroyDiceRenderer(this.dieRenderer);
            this.dieRenderer = null;
        }
        die.OnConnectionStateChanged -= OnConnectionStateChanged;
        die.OnBatteryLevelChanged -= OnBatteryLevelChanged;
        die.OnRssiChanged -= OnRssiChanged;
    }

    void OnConnectionStateChanged(Die die, Die.ConnectionState oldState, Die.ConnectionState newState)
    {
        Debug.Assert(die == this.die);
        SetState(newState);
        if (newState == Die.ConnectionState.Ready)
        {
            dieIDText.text = die.deviceId.ToString("X016");
        }
    }

    void OnBatteryLevelChanged(Die die, float? level)
    {
        batteryView.SetLevel(level);
    }

    void OnRssiChanged(Die die, int? rssi)
    {
        signalView.SetRssi(rssi);
    }
}
