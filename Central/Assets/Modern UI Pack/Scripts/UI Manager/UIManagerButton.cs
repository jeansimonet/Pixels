using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    [ExecuteInEditMode]
    public class UIManagerButton : MonoBehaviour
    {
        [Header("SETTINGS")]
        public UIManager UIManagerAsset;
        public ButtonType buttonType;

        bool dynamicUpdateEnabled;

        // Basic Resources
        [HideInInspector] public Image basicFilled;
        [HideInInspector] public TextMeshProUGUI basicText;

        // Basic Only Icon Resources
        [HideInInspector] public Image basicOnlyIconFilled;
        [HideInInspector] public Image basicOnlyIconIcon;

        // Basic With Icon Resources
        [HideInInspector] public Image basicWithIconFilled;
        [HideInInspector] public Image basicWithIconIcon;
        [HideInInspector] public TextMeshProUGUI basicWithIconText;

        // Basic Outline Resources
        [HideInInspector] public Image basicOutlineBorder;
        [HideInInspector] public Image basicOutlineFilled;
        [HideInInspector] public TextMeshProUGUI basicOutlineText;
        [HideInInspector] public TextMeshProUGUI basicOutlineTextHighligted;

        // Basic Outline Only Icon Resources
        [HideInInspector] public Image basicOutlineOOBorder;
        [HideInInspector] public Image basicOutlineOOFilled;
        [HideInInspector] public Image basicOutlineOOIcon;
        [HideInInspector] public Image basicOutlineOOIconHighlighted;

        // Basic Outline With Icon Resources
        [HideInInspector] public Image basicOutlineWOBorder;
        [HideInInspector] public Image basicOutlineWOFilled;
        [HideInInspector] public Image basicOutlineWOIcon;
        [HideInInspector] public Image basicOutlineWOIconHighlighted;
        [HideInInspector] public TextMeshProUGUI basicOutlineWOText;
        [HideInInspector] public TextMeshProUGUI basicOutlineWOTextHighligted;

        // Radial Only Icon Resources
        [HideInInspector] public Image radialOOBackground;
        [HideInInspector] public Image radialOOIcon;

        // Radial Outline Only Icon Resources
        [HideInInspector] public Image radialOutlineOOBorder;
        [HideInInspector] public Image radialOutlineOOFilled;
        [HideInInspector] public Image radialOutlineOOIcon;
        [HideInInspector] public Image radialOutlineOOIconHighlighted;

        // Rounded Resources
        [HideInInspector] public Image roundedBackground;
        [HideInInspector] public TextMeshProUGUI roundedText;

        // Rounded Outline Resources
        [HideInInspector] public Image roundedOutlineBorder;
        [HideInInspector] public Image roundedOutlineFilled;
        [HideInInspector] public TextMeshProUGUI roundedOutlineText;
        [HideInInspector] public TextMeshProUGUI roundedOutlineTextHighligted;

        public enum ButtonType
        {
            BASIC,
            BASIC_ONLY_ICON,
            BASIC_WITH_ICON,
            BASIC_OUTLINE,
            BASIC_OUTLINE_ONLY_ICON,
            BASIC_OUTLINE_WITH_ICON,
            RADIAL_ONLY_ICON,
            RADIAL_OUTLINE_ONLY_ICON,
            ROUNDED,
            ROUNDED_OUTLINE,
        }

        void OnEnable()
        {
            if (UIManagerAsset == null)
            {
                try
                {
                    UIManagerAsset = Resources.Load<UIManager>("MUIP Manager");
                }

                catch
                {
                    Debug.Log("No UI Manager found. Assign it manually, otherwise you'll get errors about it.");
                }
            }
        }

        void Awake()
        {
            if (dynamicUpdateEnabled == false)
            {
                this.enabled = true;
                UpdateButton();
            }
        }

        void LateUpdate()
        {
            if (UIManagerAsset != null)
            {
                if (UIManagerAsset.enableDynamicUpdate == true)
                    dynamicUpdateEnabled = true;
                else
                    dynamicUpdateEnabled = false;

                if (dynamicUpdateEnabled == true)
                    UpdateButton();
            }
        }

        void UpdateButton()
        {
            try
            {
                if (UIManagerAsset.buttonThemeType == UIManager.ButtonThemeType.BASIC)
                {
                    if (buttonType == ButtonType.BASIC)
                    {
                        basicFilled.color = UIManagerAsset.buttonBorderColor;
                        basicText.color = UIManagerAsset.buttonFilledColor;
                        basicText.font = UIManagerAsset.buttonFont;
                        basicText.fontSize = UIManagerAsset.buttonFontSize;
                    }

                    else if (buttonType == ButtonType.BASIC_ONLY_ICON)
                    {
                        basicOnlyIconFilled.color = UIManagerAsset.buttonBorderColor;
                        basicOnlyIconIcon.color = UIManagerAsset.buttonFilledColor;
                    }

                    else if (buttonType == ButtonType.BASIC_WITH_ICON)
                    {
                        basicWithIconFilled.color = UIManagerAsset.buttonBorderColor;
                        basicWithIconIcon.color = UIManagerAsset.buttonFilledColor;
                        basicWithIconText.color = UIManagerAsset.buttonFilledColor;
                        basicWithIconText.font = UIManagerAsset.buttonFont;
                        basicWithIconText.fontSize = UIManagerAsset.buttonFontSize;
                    }

                    else if (buttonType == ButtonType.BASIC_OUTLINE)
                    {
                        basicOutlineBorder.color = UIManagerAsset.buttonBorderColor;
                        basicOutlineFilled.color = UIManagerAsset.buttonBorderColor;
                        basicOutlineText.color = UIManagerAsset.buttonBorderColor;
                        basicOutlineTextHighligted.color = UIManagerAsset.buttonFilledColor;
                        basicOutlineText.font = UIManagerAsset.buttonFont;
                        basicOutlineTextHighligted.font = UIManagerAsset.buttonFont;
                        basicOutlineText.fontSize = UIManagerAsset.buttonFontSize;
                        basicOutlineTextHighligted.fontSize = UIManagerAsset.buttonFontSize;
                    }

                    else if (buttonType == ButtonType.BASIC_OUTLINE_ONLY_ICON)
                    {
                        basicOutlineOOBorder.color = UIManagerAsset.buttonBorderColor;
                        basicOutlineOOFilled.color = UIManagerAsset.buttonBorderColor;
                        basicOutlineOOIcon.color = UIManagerAsset.buttonBorderColor;
                        basicOutlineOOIconHighlighted.color = UIManagerAsset.buttonFilledColor;
                    }

                    else if (buttonType == ButtonType.BASIC_OUTLINE_WITH_ICON)
                    {
                        basicOutlineWOBorder.color = UIManagerAsset.buttonBorderColor;
                        basicOutlineWOFilled.color = UIManagerAsset.buttonBorderColor;
                        basicOutlineWOIcon.color = UIManagerAsset.buttonBorderColor;
                        basicOutlineWOIconHighlighted.color = UIManagerAsset.buttonFilledColor;
                        basicOutlineWOText.color = UIManagerAsset.buttonBorderColor;
                        basicOutlineWOTextHighligted.color = UIManagerAsset.buttonFilledColor;
                        basicOutlineWOText.font = UIManagerAsset.buttonFont;
                        basicOutlineWOTextHighligted.font = UIManagerAsset.buttonFont;
                        basicOutlineWOText.fontSize = UIManagerAsset.buttonFontSize;
                        basicOutlineWOTextHighligted.fontSize = UIManagerAsset.buttonFontSize;

                    }

                    else if (buttonType == ButtonType.RADIAL_ONLY_ICON)
                    {
                        radialOOBackground.color = UIManagerAsset.buttonBorderColor;
                        radialOOIcon.color = UIManagerAsset.buttonFilledColor;
                    }

                    else if (buttonType == ButtonType.RADIAL_OUTLINE_ONLY_ICON)
                    {
                        radialOutlineOOBorder.color = UIManagerAsset.buttonBorderColor;
                        radialOutlineOOFilled.color = UIManagerAsset.buttonBorderColor;
                        radialOutlineOOIcon.color = UIManagerAsset.buttonIconColor;
                        radialOutlineOOIconHighlighted.color = UIManagerAsset.buttonFilledColor;
                    }

                    else if (buttonType == ButtonType.ROUNDED)
                    {
                        roundedBackground.color = UIManagerAsset.buttonBorderColor;
                        roundedText.color = UIManagerAsset.buttonFilledColor;
                        roundedText.font = UIManagerAsset.buttonFont;
                        roundedText.fontSize = UIManagerAsset.buttonFontSize;
                    }

                    else if (buttonType == ButtonType.ROUNDED_OUTLINE)
                    {
                        roundedOutlineBorder.color = UIManagerAsset.buttonBorderColor;
                        roundedOutlineFilled.color = UIManagerAsset.buttonBorderColor;
                        roundedOutlineText.color = UIManagerAsset.buttonBorderColor;
                        roundedOutlineTextHighligted.color = UIManagerAsset.buttonFilledColor;
                        roundedOutlineText.font = UIManagerAsset.buttonFont;
                        roundedOutlineTextHighligted.font = UIManagerAsset.buttonFont;
                        roundedOutlineText.fontSize = UIManagerAsset.buttonFontSize;
                        roundedOutlineTextHighligted.fontSize = UIManagerAsset.buttonFontSize;
                    }
                }

                else if (UIManagerAsset.buttonThemeType == UIManager.ButtonThemeType.CUSTOM)
                {
                    if (buttonType == ButtonType.BASIC)
                    {
                        basicFilled.color = UIManagerAsset.buttonFilledColor;
                        basicText.color = UIManagerAsset.buttonTextBasicColor;
                        basicText.font = UIManagerAsset.buttonFont;
                        basicText.fontSize = UIManagerAsset.buttonFontSize;
                    }

                    else if (buttonType == ButtonType.BASIC_ONLY_ICON)
                    {
                        basicOnlyIconFilled.color = UIManagerAsset.buttonFilledColor;
                        basicOnlyIconIcon.color = UIManagerAsset.buttonIconBasicColor;
                    }

                    else if (buttonType == ButtonType.BASIC_WITH_ICON)
                    {
                        basicWithIconFilled.color = UIManagerAsset.buttonFilledColor;
                        basicWithIconIcon.color = UIManagerAsset.buttonIconBasicColor;
                        basicWithIconText.color = UIManagerAsset.buttonTextBasicColor;
                        basicWithIconText.font = UIManagerAsset.buttonFont;
                        basicWithIconText.fontSize = UIManagerAsset.buttonFontSize;
                    }

                    else if (buttonType == ButtonType.BASIC_OUTLINE)
                    {
                        basicOutlineBorder.color = UIManagerAsset.buttonBorderColor;
                        basicOutlineFilled.color = UIManagerAsset.buttonFilledColor;
                        basicOutlineText.color = UIManagerAsset.buttonTextColor;
                        basicOutlineTextHighligted.color = UIManagerAsset.buttonTextHighlightedColor;
                        basicOutlineText.font = UIManagerAsset.buttonFont;
                        basicOutlineTextHighligted.font = UIManagerAsset.buttonFont;
                        basicOutlineText.fontSize = UIManagerAsset.buttonFontSize;
                        basicOutlineTextHighligted.fontSize = UIManagerAsset.buttonFontSize;
                    }

                    else if (buttonType == ButtonType.BASIC_OUTLINE_ONLY_ICON)
                    {
                        basicOutlineOOBorder.color = UIManagerAsset.buttonBorderColor;
                        basicOutlineOOFilled.color = UIManagerAsset.buttonFilledColor;
                        basicOutlineOOIcon.color = UIManagerAsset.buttonBorderColor;
                        basicOutlineOOIconHighlighted.color = UIManagerAsset.buttonFilledColor;
                    }

                    else if (buttonType == ButtonType.BASIC_OUTLINE_WITH_ICON)
                    {
                        basicOutlineWOBorder.color = UIManagerAsset.buttonBorderColor;
                        basicOutlineWOFilled.color = UIManagerAsset.buttonFilledColor;
                        basicOutlineWOIcon.color = UIManagerAsset.buttonIconColor;
                        basicOutlineWOIconHighlighted.color = UIManagerAsset.buttonIconHighlightedColor;
                        basicOutlineWOText.color = UIManagerAsset.buttonTextColor;
                        basicOutlineWOTextHighligted.color = UIManagerAsset.buttonTextHighlightedColor;
                        basicOutlineWOText.font = UIManagerAsset.buttonFont;
                        basicOutlineWOTextHighligted.font = UIManagerAsset.buttonFont;
                        basicOutlineWOText.fontSize = UIManagerAsset.buttonFontSize;
                        basicOutlineWOTextHighligted.fontSize = UIManagerAsset.buttonFontSize;
                    }

                    else if (buttonType == ButtonType.RADIAL_ONLY_ICON)
                    {
                        radialOOBackground.color = UIManagerAsset.buttonFilledColor;
                        radialOOIcon.color = UIManagerAsset.buttonIconBasicColor;
                    }

                    else if (buttonType == ButtonType.RADIAL_OUTLINE_ONLY_ICON)
                    {
                        radialOutlineOOBorder.color = UIManagerAsset.buttonBorderColor;
                        radialOutlineOOFilled.color = UIManagerAsset.buttonFilledColor;
                        radialOutlineOOIcon.color = UIManagerAsset.buttonIconColor;
                        radialOutlineOOIconHighlighted.color = UIManagerAsset.buttonIconHighlightedColor;
                    }

                    else if (buttonType == ButtonType.ROUNDED)
                    {
                        roundedBackground.color = UIManagerAsset.buttonFilledColor;
                        roundedText.color = UIManagerAsset.buttonTextBasicColor;
                        roundedText.font = UIManagerAsset.buttonFont;
                        roundedText.fontSize = UIManagerAsset.buttonFontSize;
                    }

                    else if (buttonType == ButtonType.ROUNDED_OUTLINE)
                    {
                        roundedOutlineBorder.color = UIManagerAsset.buttonBorderColor;
                        roundedOutlineFilled.color = UIManagerAsset.buttonFilledColor;
                        roundedOutlineText.color = UIManagerAsset.buttonTextColor;
                        roundedOutlineTextHighligted.color = UIManagerAsset.buttonTextHighlightedColor;
                        roundedOutlineText.font = UIManagerAsset.buttonFont;
                        roundedOutlineTextHighligted.font = UIManagerAsset.buttonFont;
                        roundedOutlineText.fontSize = UIManagerAsset.buttonFontSize;
                        roundedOutlineTextHighligted.fontSize = UIManagerAsset.buttonFontSize;
                    }
                }
            }

            catch { }
        }
    }
}