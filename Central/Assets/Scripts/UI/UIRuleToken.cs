using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Behaviors;

public class UIRuleToken : MonoBehaviour
{
    [Header("Controls")]
    public Button editButtton;
    public Button menuButton;
    public RectTransform tokenRoot;
    public Image menuButtonImage;
    public Image backgroundImage;
    public RectTransform expandedRoot;
    public Button moveUpButton;
    public Button removeButton;
    public Button duplicateButton;
    public Button editButton;
    public Button moveDownButton;

    [Header("Properties")]
    public Sprite expandImage;
    public Sprite contractImage;
    public Color backgroundColor;
    public Color expandedColor;
    public Sprite backgroundSprite;
    public Sprite expandedSprite;

    public EditRule editRule { get; private set; }

    public Button.ButtonClickedEvent onClick => editButtton.onClick;
    public Button.ButtonClickedEvent onMoveUp => moveUpButton.onClick;
    public Button.ButtonClickedEvent onMoveDown => moveDownButton.onClick;
    public Button.ButtonClickedEvent onRemove => removeButton.onClick;
    public Button.ButtonClickedEvent onDuplicate => duplicateButton.onClick;
    public Button.ButtonClickedEvent onEdit => editButton.onClick;
    public Button.ButtonClickedEvent onExpand => menuButton.onClick;


    public bool isExpanded => expandedRoot.gameObject.activeSelf;


    public void Setup(EditRule rule)
    {
        editRule = rule;
        // Create the lines describing the rule.
        // First the condition
        UIRuleTokenManager.Instance.CreateConditionToken(rule.condition, tokenRoot);
        for (int i = 0; i < rule.actions.Count; ++i)
        {
            var action = rule.actions[i];
            UIRuleTokenManager.Instance.CreateActionToken(action, i == 0, tokenRoot);
        }
        Expand(false);
    }

    public void Expand(bool expand)
    {
        if (expand)
        {
            menuButtonImage.sprite = contractImage;
            backgroundImage.sprite = expandedSprite;
            backgroundImage.color = expandedColor;
            expandedRoot.gameObject.SetActive(true);
        }
        else
        {
            menuButtonImage.sprite = expandImage;
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.color = backgroundColor;
            expandedRoot.gameObject.SetActive(false);
        }
    }
}
