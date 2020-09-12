using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AudioClips;

public class UIAudioClipPickerToken : MonoBehaviour
{
    [Header("Controls")]
    public Button mainButton;
    public RawImage previewImage;
    public Text clipNameText;
    public Image frame;
    public Button playButton;

    [Header("Parameters")]
    public Color defaultTextColor;
    public Color defaultFrameColor;
    public Color selectedColor;

    public AudioClipManager.AudioClipInfo clip { get; private set; }

    public Button.ButtonClickedEvent onClick => mainButton.onClick;
    public Button.ButtonClickedEvent onPlay => playButton.onClick;
    public bool selected { get; private set; }

    public void Setup(AudioClipManager.AudioClipInfo clip)
    {
        this.clip = clip;
        previewImage.texture = clip.preview;
        clipNameText.text = clip.clip.name;
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        this.selected = selected;
        if (selected)
        {
            clipNameText.color = selectedColor;
            frame.color = selectedColor;
        }
        else
        {
            clipNameText.color = defaultTextColor;
            frame.color = defaultFrameColor;
        }
    }

}
