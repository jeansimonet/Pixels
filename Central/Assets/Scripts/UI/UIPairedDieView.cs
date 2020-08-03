using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPairedDieView : MonoBehaviour
{
    // Controls
    [Header("Controls")]
    public Image backgroundImage;
    public RawImage dieRenderImage;
    public Text dieNameText;
    public Text dieIDText;
    public UIDieLargeBatteryView batteryView;
    public UIDieLargeSignalView signalView;
    public Button expandButton;
    public Image expandButtonImage;
    public GameObject expandGroup;
    public Text statusText;
    public RectTransform disconnectedTextRoot;
    public RectTransform errorTextRoot;

    [Header("ExpandedControls")]
    public Button statsButton;
    public Button renameButton;
    public Button forgetButton;
    public Button resetButton;

    [Header("Images")]
    public Sprite backgroundCollapsedSprite;
    public Sprite backgroundExpandedSprite;
    public Sprite buttonCollapsedSprite;
    public Sprite buttonExpandedSprite;

    public bool expanded => expandGroup.activeSelf;
    public Die die { get; private set; }
    public DiceRenderer dieRenderer { get; private set; }

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
        SetState(die.connectionState);
        die.OnConnectionStateChanged += OnConnectionStateChanged;
    }

    public void BeginRefreshPool()
    {
        // Pause die events
        die.OnConnectionStateChanged -= OnConnectionStateChanged;
    }

    public void FinishRefreshPool()
    {
        SetState(die.connectionState);
        die.OnConnectionStateChanged += OnConnectionStateChanged;
    }

    void SetState(Die.ConnectionState newState)
    {
        switch (newState)
        {
            case Die.ConnectionState.Invalid:
                gameObject.SetActive(false);
                dieRenderer.rotating = false;
                dieRenderImage.color = AppConstants.Instance.DieUnavailableColor;
                batteryView.gameObject.SetActive(false);
                signalView.gameObject.SetActive(false);
                statusText.text = "Invalid";
                disconnectedTextRoot.gameObject.SetActive(true);
                errorTextRoot.gameObject.SetActive(false);
                break;
            case Die.ConnectionState.Unknown:
                gameObject.SetActive(true);
                dieRenderer.rotating = false;
                dieRenderImage.color = Color.white;
                batteryView.gameObject.SetActive(false);
                signalView.gameObject.SetActive(false);
                statusText.text = "Disconnected";
                disconnectedTextRoot.gameObject.SetActive(false);
                errorTextRoot.gameObject.SetActive(false);
                break;
            case Die.ConnectionState.New:
                gameObject.SetActive(false);
                dieRenderer.rotating = true;
                dieRenderImage.color = Color.white;
                batteryView.gameObject.SetActive(true);
                batteryView.SetAvailable(false);
                signalView.gameObject.SetActive(true);
                batteryView.SetAvailable(false);
                statusText.text = "New die";
                disconnectedTextRoot.gameObject.SetActive(false);
                errorTextRoot.gameObject.SetActive(false);
                break;
            case Die.ConnectionState.Available:
                gameObject.SetActive(true);
                dieRenderer.rotating = true;
                dieRenderImage.color = Color.white;
                batteryView.gameObject.SetActive(true);
                batteryView.SetAvailable(false);
                signalView.gameObject.SetActive(true);
                batteryView.SetAvailable(false);
                statusText.text = "Available";
                disconnectedTextRoot.gameObject.SetActive(false);
                errorTextRoot.gameObject.SetActive(false);
                break;
            case Die.ConnectionState.Connecting:
                gameObject.SetActive(true);
                dieRenderer.rotating = false;
                dieRenderImage.color = Color.white;
                batteryView.gameObject.SetActive(false);
                signalView.gameObject.SetActive(false);
                statusText.text = "Identifying";
                disconnectedTextRoot.gameObject.SetActive(false);
                errorTextRoot.gameObject.SetActive(false);
                break;
            case Die.ConnectionState.Identifying:
                gameObject.SetActive(true);
                dieRenderer.rotating = true;
                dieRenderImage.color = Color.white;
                batteryView.gameObject.SetActive(true);
                batteryView.SetAvailable(true);
                signalView.gameObject.SetActive(true);
                batteryView.SetAvailable(true);
                statusText.text = "Identifying";
                disconnectedTextRoot.gameObject.SetActive(false);
                errorTextRoot.gameObject.SetActive(false);
                break;
            case Die.ConnectionState.Ready:
                gameObject.SetActive(true);
                dieRenderer.rotating = true;
                dieRenderImage.color = Color.white;
                batteryView.gameObject.SetActive(true);
                batteryView.SetAvailable(false);
                signalView.gameObject.SetActive(true);
                batteryView.SetAvailable(false);
                statusText.text = "Ready";
                disconnectedTextRoot.gameObject.SetActive(false);
                errorTextRoot.gameObject.SetActive(false);
                break;
            case Die.ConnectionState.Missing:
                gameObject.SetActive(true);
                dieRenderer.rotating = false;
                dieRenderImage.color = Color.white;
                batteryView.gameObject.SetActive(false);
                signalView.gameObject.SetActive(false);
                statusText.text = "Unreachable";
                disconnectedTextRoot.gameObject.SetActive(true);
                errorTextRoot.gameObject.SetActive(false);
                break;
            case Die.ConnectionState.CommError:
                gameObject.SetActive(true);
                dieRenderer.rotating = false;
                dieRenderImage.color = Color.white;
                batteryView.gameObject.SetActive(false);
                signalView.gameObject.SetActive(false);
                statusText.text = "Communication Error";
                disconnectedTextRoot.gameObject.SetActive(false);
                errorTextRoot.gameObject.SetActive(true);
                break;
        }
    }

    void Awake()
    {
        // Hook up to events
        expandButton.onClick.AddListener(OnToggle);
        forgetButton.onClick.AddListener(OnForget);
    }

    void OnToggle()
    {
        bool newActive = !expanded;
        expandGroup.SetActive(newActive);
        backgroundImage.sprite = newActive ? backgroundExpandedSprite : backgroundCollapsedSprite;
        expandButtonImage.sprite = newActive ? buttonExpandedSprite : buttonCollapsedSprite;
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
    }

    void OnConnectionStateChanged(Die die, Die.ConnectionState oldState, Die.ConnectionState newState)
    {
        Debug.Assert(die == this.die);
        SetState(newState);
    }
}
