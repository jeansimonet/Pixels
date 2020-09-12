using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AudioClips;

public class UIParameterAudioClip
    : UIParameter
{
    [Header("Controls")]
    public Text nameText;
    public Text clipNameText;
    public RawImage previewImage;
    public Button selectClipButton;

    System.Func<object> getterFunc;
    System.Action<object> setterAction;

    public override bool CanEdit(System.Type parameterType, IEnumerable<object> attributes = null)
    {
        return parameterType == typeof(EditAudioClip);
    }

    protected override void SetupControls(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null)
    {
        this.getterFunc = getterFunc;
        this.setterAction = setterAction;
        EditAudioClip initialClip = (EditAudioClip)getterFunc();
        
        // Set name
        nameText.text = name;

        // Fetch the clip info
        if (initialClip != null)
        {
            var clipInfo = AudioClipManager.Instance.FindClip(initialClip.name);
            if (clipInfo != null)
            {
                previewImage.texture = clipInfo.preview;
                clipNameText.text = initialClip.name;
            }
        }

        selectClipButton.onClick.AddListener(SelectClip);
    }

    void SetAudioClip(EditAudioClip clip)
    {
        var clipInfo = AudioClipManager.Instance.FindClip(clip?.name);
        if (clipInfo != null)
        {
            previewImage.gameObject.SetActive(true);
            previewImage.texture = clipInfo.preview;
            clipNameText.text = clipInfo.clip.name;
        }
        else
        {
            previewImage.gameObject.SetActive(false);
            previewImage.texture = null;
            clipNameText.text = "- Please select an Audio Clip -";
        }
    }

    void SelectClip()
    {
        EditAudioClip currentEditClip = (EditAudioClip)getterFunc();
        var currentInfo = AudioClipManager.Instance.FindClip(currentEditClip?.name);
        PixelsApp.Instance.ShowAudioClipPicker("Select Audio Clip", currentInfo, (res, newInfo) => 
        {
            if (res)
            {
                // Is this an edit clip we already know about?
                var newEditClip = AppDataSet.Instance.FindAudioClip(newInfo.clip.name);
                if (newEditClip == null)
                {
                    newEditClip = AppDataSet.Instance.AddAudioClip(newInfo.clip.name);
                }
                SetAudioClip(newEditClip);
                setterAction?.Invoke(newEditClip);
            }
        });
    }
}
