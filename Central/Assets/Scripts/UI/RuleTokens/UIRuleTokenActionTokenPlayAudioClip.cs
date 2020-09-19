using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Behaviors;
using Dice;

public class UIRuleTokenActionTokenPlayAudioClip
    : UIRuleTokenActionToken
{
    [Header("Controls")]
    public Text labelText;
    public Text actionText;
    public RawImage previewImage;

    static readonly ActionType[] supportedActionTypes = new ActionType[]
    {
        ActionType.PlayAudioClip
    };

    public override IEnumerable<ActionType> actionTypes
    {
        get { return supportedActionTypes; }
    }

    public override void Setup(EditAction action, bool first)
    {
        var playAudioClipAction = action as EditActionPlayAudioClip;
        actionText.text = action.ToString();
        labelText.text = first ? "Then" : "And";

        var clipInfo = AudioClipManager.Instance.FindClip(playAudioClipAction.clip?.name);
        if (clipInfo != null)
        {
            previewImage.gameObject.SetActive(true);
            previewImage.texture = clipInfo.preview;
        }
        else
        {
            previewImage.gameObject.SetActive(false);
        }
    }
}
