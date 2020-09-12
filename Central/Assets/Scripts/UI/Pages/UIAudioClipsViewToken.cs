using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAudioClipsViewToken : MonoBehaviour
{
    [Header("Controls")]
    public Button mainButton;
    public RawImage previewImage;
    public Text clipNameText;
    public Button menuButton;
    public Button playButton;
    public Image menuButtonImage;
    public Canvas overrideCanvas;
    public Image backgroundImage;
    public RectTransform expandedRoot;
    public Button removeButton;
    public Button renameButton;

    [Header("Properties")]
    public Sprite expandImage;
    public Sprite contractImage;
    public Color backgroundColor;
    public Color expandedColor;
    public Sprite backgroundSprite;
    public Sprite expandedSprite;

    public AudioClipManager.AudioClipInfo clip { get; private set; }

    public Button.ButtonClickedEvent onClick => mainButton.onClick;
    public Button.ButtonClickedEvent onPlay => playButton.onClick;
    public Button.ButtonClickedEvent onRemove => removeButton.onClick;
    public Button.ButtonClickedEvent onRename => renameButton.onClick;
    public Button.ButtonClickedEvent onExpand => menuButton.onClick;


    public bool isExpanded => expandedRoot.gameObject.activeSelf;


    public void Setup(AudioClipManager.AudioClipInfo clip)
    {
        this.clip = clip;
        previewImage.texture = clip.preview;
        clipNameText.text = clip.clip.name;
        Expand(false);
    }

    public void Expand(bool expand)
    {
        if (expand)
        {
            menuButtonImage.sprite = contractImage;
            overrideCanvas.overrideSorting = true;
            backgroundImage.sprite = expandedSprite;
            backgroundImage.color = expandedColor;
            expandedRoot.gameObject.SetActive(true);
        }
        else
        {
            menuButtonImage.sprite = expandImage;
            overrideCanvas.overrideSorting = false;
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.color = backgroundColor;
            expandedRoot.gameObject.SetActive(false);
        }
    }
}
