using UnityEngine;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    [CreateAssetMenu(fileName = "New UI Manager", menuName = "Modern UI Pack/New UI Manager")]
    public class UIManager : ScriptableObject
    {
        [HideInInspector] public bool enableDynamicUpdate = true;
        [HideInInspector] public bool enableExtendedColorPicker = true;
        [HideInInspector] public bool editorHints = true;

        // [Header("ANIMATED ICON")]
        public Color animatedIconColor = new Color(255, 255, 255, 255);

        // [Header("BUTTON")]
        public ButtonThemeType buttonThemeType;
        public TMP_FontAsset buttonFont;
        public float buttonFontSize = 22.5f;
        public Color buttonBorderColor = new Color(255, 255, 255, 255);
        public Color buttonFilledColor = new Color(255, 255, 255, 255);
        public Color buttonTextBasicColor = new Color(255, 255, 255, 255);
        public Color buttonTextColor = new Color(255, 255, 255, 255);
        public Color buttonTextHighlightedColor = new Color(255, 255, 255, 255);
        public Color buttonIconBasicColor = new Color(255, 255, 255, 255);
        public Color buttonIconColor = new Color(255, 255, 255, 255);
        public Color buttonIconHighlightedColor = new Color(255, 255, 255, 255);

        // [Header("DROPDOWN")]
        public TMP_FontAsset dropdownItemFont;
        public float dropdownItemFontSize = 22.5f;
        public DropdownThemeType dropdownThemeType;
        public DropdownAnimationType dropdownAnimationType;
        public TMP_FontAsset dropdownFont;
        public float dropdownFontSize = 22.5f;
        public Color dropdownColor = new Color(255, 255, 255, 255);
        public Color dropdownTextColor = new Color(255, 255, 255, 255);
        public Color dropdownIconColor = new Color(255, 255, 255, 255);
        public Color dropdownItemColor = new Color(255, 255, 255, 255);
        public Color dropdownItemTextColor = new Color(255, 255, 255, 255);
        public Color dropdownItemIconColor = new Color(255, 255, 255, 255);

        // [Header("HORIZONTAL SELECTOR")]
        public TMP_FontAsset selectorFont;
        public float hSelectorFontSize = 28;
        public Color selectorColor = new Color(255, 255, 255, 255);
        public Color selectorHighlightedColor = new Color(255, 255, 255, 255);
        public bool hSelectorInvertAnimation = false;
        public bool hSelectorLoopSelection = false;

        // [Header("INPUT FIELD")]
        public TMP_FontAsset inputFieldFont;
        public float inputFieldFontSize = 28;
        public Color inputFieldColor = new Color(255, 255, 255, 255);

        // [Header("MODAL WINDOW")]
        public TMP_FontAsset modalWindowTitleFont;
        public TMP_FontAsset modalWindowContentFont;
        public DropdownThemeType modalThemeType;
        public Color modalWindowTitleColor = new Color(255, 255, 255, 255);
        public Color modalWindowDescriptionColor = new Color(255, 255, 255, 255);
        public Color modalWindowIconColor = new Color(255, 255, 255, 255);
        public Color modalWindowBackgroundColor = new Color(255, 255, 255, 255);
        public Color modalWindowContentPanelColor = new Color(255, 255, 255, 255);

        // [Header("NOTIFICATION")]
        public TMP_FontAsset notificationTitleFont;
        public float notificationTitleFontSize = 22.5f;
        public TMP_FontAsset notificationDescriptionFont;
        public float notificationDescriptionFontSize = 18;
        public NotificationThemeType notificationThemeType;
        public Color notificationBackgroundColor = new Color(255, 255, 255, 255);
        public Color notificationTitleColor = new Color(255, 255, 255, 255);
        public Color notificationDescriptionColor = new Color(255, 255, 255, 255);
        public Color notificationIconColor = new Color(255, 255, 255, 255);

        // [Header("PROGRESS BAR")]
        public TMP_FontAsset progressBarLabelFont;
        public float progressBarLabelFontSize = 25;
        public Color progressBarColor = new Color(255, 255, 255, 255);
        public Color progressBarBackgroundColor = new Color(255, 255, 255, 255);
        public Color progressBarLoopBackgroundColor = new Color(255, 255, 255, 255);
        public Color progressBarLabelColor = new Color(255, 255, 255, 255);

        // [Header("SCROLLBAR")]
        public Color scrollbarColor = new Color(255, 255, 255, 255);
        public Color scrollbarBackgroundColor = new Color(255, 255, 255, 255);

        // [Header("SLIDER")]
        public TMP_FontAsset sliderLabelFont;
        public float sliderLabelFontSize = 24;
        public SliderThemeType sliderThemeType;
        public Color sliderColor = new Color(255, 255, 255, 255);
        public Color sliderBackgroundColor = new Color(255, 255, 255, 255);
        public Color sliderLabelColor = new Color(255, 255, 255, 255);
        public Color sliderPopupLabelColor = new Color(255, 255, 255, 255);
        public Color sliderHandleColor = new Color(255, 255, 255, 255);

        // [Header("SWITCH")]
        public Color switchBorderColor = new Color(255, 255, 255, 255);
        public Color switchBackgroundColor = new Color(255, 255, 255, 255);
        public Color switchHandleOnColor = new Color(255, 255, 255, 255);
        public Color switchHandleOffColor = new Color(255, 255, 255, 255);

        // [Header("TOGGLE")]
        public TMP_FontAsset toggleFont;
        public float toggleFontSize = 35;
        public ToggleThemeType toggleThemeType;
        public Color toggleTextColor = new Color(255, 255, 255, 255);
        public Color toggleBorderColor = new Color(255, 255, 255, 255);
        public Color toggleBackgroundColor = new Color(255, 255, 255, 255);
        public Color toggleCheckColor = new Color(255, 255, 255, 255);

        // [Header("TOOLTIP")]
        public TMP_FontAsset tooltipFont;
        public float tooltipFontSize = 22;
        public Color tooltipTextColor = new Color(255, 255, 255, 255);
        public Color tooltipBackgroundColor = new Color(255, 255, 255, 255);

        public enum ButtonThemeType
        {
            BASIC,
            CUSTOM
        }

        public enum DropdownThemeType
        {
            BASIC,
            CUSTOM
        }

        public enum DropdownAnimationType
        {
            FADING,
            SLIDING,
            STYLISH
        }

        public enum ModalWindowThemeType
        {
            BASIC,
            CUSTOM
        }

        public enum NotificationThemeType
        {
            BASIC,
            CUSTOM
        }

        public enum SliderThemeType
        {
            BASIC,
            CUSTOM
        }

        public enum ToggleThemeType
        {
            BASIC,
            CUSTOM
        }
    }
}