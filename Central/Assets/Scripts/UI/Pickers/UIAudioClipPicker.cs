using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AudioClips;

public class UIAudioClipPicker : MonoBehaviour
{
    [Header("Controls")]
    public Button backButton;
    public Text titleText;
    public RectTransform contentRoot;
    public Button addAudioClipButton;

    [Header("Prefabs")]
    public UIAudioClipPickerToken clipTokenPrefab;

    AudioClipManager.AudioClipInfo currentClip;
    System.Action<bool, AudioClipManager.AudioClipInfo> closeAction;

    // The list of controls we have created to display patterns
    List<UIAudioClipPickerToken> patterns = new List<UIAudioClipPickerToken>();

    public bool isShown => gameObject.activeSelf;

    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        backButton.onClick.AddListener(Back);
        addAudioClipButton.onClick.AddListener(AddNewClip);
    }

    /// <summary>
    /// Invoke the color picker
    /// </sumary>
    public void Show(string title, AudioClipManager.AudioClipInfo previousClip, System.Action<bool, AudioClipManager.AudioClipInfo> closeAction)
    {
        if (isShown)
        {
            Debug.LogWarning("Previous AudioClip picker still active");
            ForceHide();
        }

        foreach (var clip in AudioClipManager.Instance.audioClips)
        {
            // New clip
            var newClipUI = CreateClipToken(clip);
            newClipUI.SetSelected(clip == previousClip);
            patterns.Add(newClipUI);
        }

        gameObject.SetActive(true);
        currentClip = previousClip;
        titleText.text = title;


        this.closeAction = closeAction;
    }

    UIAudioClipPickerToken CreateClipToken(AudioClipManager.AudioClipInfo clip)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIAudioClipPickerToken>(clipTokenPrefab, Vector3.zero, Quaternion.identity, contentRoot.transform);

        // When we click on the pattern main button, go to the edit page
        ret.onClick.AddListener(() => Hide(true, ret.clip));
        ret.onPlay.AddListener(() => PlayClip(clip));

        addAudioClipButton.transform.SetAsLastSibling();
        // Initialize it
        ret.Setup(clip);
        return ret;
    }

    /// <summary>
    /// If for some reason the app needs to close the dialog box, this will do it!
    /// Normally it closes itself when you tap ok or cancel
    /// </sumary>
    public void ForceHide()
    {
        Hide(false, currentClip);
    }

    void Hide(bool result, AudioClipManager.AudioClipInfo clip)
    {
        foreach (var uipattern in patterns)
        {
            DestroyClipToken(uipattern);
        }
        patterns.Clear();

        gameObject.SetActive(false);
        closeAction?.Invoke(result, clip);
        closeAction = null;
    }

    void Back()
    {
        Hide(false, currentClip);
    }

    void DestroyClipToken(UIAudioClipPickerToken token)
    {
        GameObject.Destroy(token.gameObject);
    }

    void AddNewClip()
    {
        // TODO
    }

    void PlayClip(AudioClipManager.AudioClipInfo clip)
    {
        audioSource.PlayOneShot(clip.clip);
    }

}
