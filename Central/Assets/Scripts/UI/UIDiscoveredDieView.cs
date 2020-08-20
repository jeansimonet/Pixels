using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
            dieRenderer.rotating = true;
            dieRenderImage.texture = dieRenderer.renderTexture;
        }
        dieNameText.text = die.name;
        dieIDText.text = "ID: " + die.deviceId.ToString("X016");
        batteryView.SetLevel(die.batteryLevel);
        signalView.SetRssi(die.rssi);
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
        if (this.dieRenderer != null)
        {
            DiceRendererManager.Instance.DestroyDiceRenderer(this.dieRenderer);
            this.dieRenderer = null;
        }
    }
}
