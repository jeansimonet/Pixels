using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Dice;

public class UIDiscoveredDieView : MonoBehaviour
{
    // Controls
    [Header("Controls")]
    public Image backgroundImage;
    public RawImage dieRenderImage;
    public Text dieNameText;
    public Text dieIDText;
    public UIDieLargeBatteryView batteryView;
    public UIDieLargeSignalView signalView;
    public Button selectButton;
    public Image toggleImage;

    [Header("Images")]
    public Sprite backgroundSelectedSprite;
    public Sprite backgroundUnselectedSprite;

    public Die die { get; private set; }
    public SingleDiceRenderer dieRenderer { get; private set; }
    public bool selected { get; private set; }

    public delegate void SelectedEvent(UIDiscoveredDieView uidie, bool selected);
    public SelectedEvent onSelected;

    public void Setup(Die die)
    {
        this.die = die;
        this.dieRenderer = DiceRendererManager.Instance.CreateDiceRenderer(die.designAndColor);
        if (dieRenderer != null)
        {
            dieRenderer.SetAuto(true);
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
        batteryView.SetLevel(die.batteryLevel);
        signalView.SetRssi(die.rssi);
        die.OnBatteryLevelChanged += OnBatteryLevelChanged;
        die.OnRssiChanged += OnRssiChanged;
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        this.selected = selected;
        backgroundImage.sprite = selected ? backgroundSelectedSprite : backgroundUnselectedSprite;
        toggleImage.gameObject.SetActive(selected);
        onSelected?.Invoke(this, selected);
    }

    void Awake()
    {
        // Hook up to events
        selectButton.onClick.AddListener(OnToggle);
    }

    void OnToggle()
    {
        SetSelected(!selected);
    }

    void OnDestroy()
    {
        die.OnBatteryLevelChanged += OnBatteryLevelChanged;
        die.OnRssiChanged += OnRssiChanged;
        if (this.dieRenderer != null)
        {
            DiceRendererManager.Instance.DestroyDiceRenderer(this.dieRenderer);
            this.dieRenderer = null;
        }
    }
    
    void OnBatteryLevelChanged(Die die, float? level)
    {
        batteryView.SetLevel(die.batteryLevel);
    }

    void OnRssiChanged(Die die, int? rssi)
    {
        signalView.SetRssi(die.rssi);
    }

}
