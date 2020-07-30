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
        dieIDText.text = "ID: " + die.deviceId.ToString("X016");
    }

    void Awake()
    {
        // Hook up to events
        expandButton.onClick.AddListener(OnToggle);
    }

    void OnToggle()
    {
        bool newActive = !expanded;
        expandGroup.SetActive(newActive);
        backgroundImage.sprite = newActive ? backgroundExpandedSprite : backgroundCollapsedSprite;
        expandButtonImage.sprite = newActive ? buttonExpandedSprite : buttonCollapsedSprite;
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
