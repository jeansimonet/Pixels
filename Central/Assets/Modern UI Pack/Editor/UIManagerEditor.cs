using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Presets;
#endif

#if UNITY_EDITOR
namespace Michsky.UI.ModernUIPack
{
    [CustomEditor(typeof(UIManager))]
    [System.Serializable]
    public class UIManagerEditor : Editor
    {
        Texture2D muipLogo;

        protected static bool showAnimatedIcon = false;
        protected static bool showButton = false;
        protected static bool showDropdown = false;
        protected static bool showHorSelector = false;
        protected static bool showInputField = false;
        protected static bool showModalWindow = false;
        protected static bool showNotification = false;
        protected static bool showProgressBar = false;
        protected static bool showScrollbar = false;
        protected static bool showSlider = false;
        protected static bool showSwitch = false;
        protected static bool showToggle = false;
        protected static bool showTooltip = false;

        void OnEnable()
        {
            if (EditorGUIUtility.isProSkin == true)
                muipLogo = Resources.Load<Texture2D>("Editor\\MUIP Editor Dark");
            else
                muipLogo = Resources.Load<Texture2D>("Editor\\MUIP Editor Light");
        }

        public override void OnInspectorGUI()
        {
            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
            foldoutStyle.fontStyle = FontStyle.Bold;
            foldoutStyle.fontSize = 12;

            // Logo
            GUILayout.Label(muipLogo, GUILayout.Width(250), GUILayout.Height(40));

            // Animated Icon
            var animatedIconColor = serializedObject.FindProperty("animatedIconColor");
            showAnimatedIcon = EditorGUILayout.Foldout(showAnimatedIcon, "Animated Icon", foldoutStyle);

            if (showAnimatedIcon)
            {
                EditorGUILayout.PropertyField(animatedIconColor, new GUIContent("Color"));
            }

            GUILayout.Space(6);

            // Button
            var buttonTheme = serializedObject.FindProperty("buttonThemeType");
            var buttonFont = serializedObject.FindProperty("buttonFont");
            var buttonFontSize = serializedObject.FindProperty("buttonFontSize");
            var buttonBorderColor = serializedObject.FindProperty("buttonBorderColor");
            var buttonFilledColor = serializedObject.FindProperty("buttonFilledColor");
            var buttonTextBasicColor = serializedObject.FindProperty("buttonTextBasicColor");
            var buttonTextColor = serializedObject.FindProperty("buttonTextColor");
            var buttonTextHighlightedColor = serializedObject.FindProperty("buttonTextHighlightedColor");
            var buttonIconBasicColor = serializedObject.FindProperty("buttonIconBasicColor");
            var buttonIconColor = serializedObject.FindProperty("buttonIconColor");
            var buttonIconHighlightedColor = serializedObject.FindProperty("buttonIconHighlightedColor");
            showButton = EditorGUILayout.Foldout(showButton, "Button", foldoutStyle);

            if (showButton && buttonTheme.enumValueIndex == 0)
            {
                EditorGUILayout.PropertyField(buttonTheme, new GUIContent("Theme Type"));
                EditorGUILayout.PropertyField(buttonFont, new GUIContent("Font"));
                EditorGUILayout.PropertyField(buttonFontSize, new GUIContent("Font Size"));
                EditorGUILayout.PropertyField(buttonBorderColor, new GUIContent("Primary Color"));
                EditorGUILayout.PropertyField(buttonFilledColor, new GUIContent("Secondary Color"));
            }

            if (showButton && buttonTheme.enumValueIndex == 1)
            {
                EditorGUILayout.PropertyField(buttonTheme, new GUIContent("Theme Type"));
                EditorGUILayout.PropertyField(buttonFont, new GUIContent("Font"));
                EditorGUILayout.PropertyField(buttonFontSize, new GUIContent("Font Size"));
                EditorGUILayout.PropertyField(buttonBorderColor, new GUIContent("Border Color"));
                EditorGUILayout.PropertyField(buttonFilledColor, new GUIContent("Filled Color"));
                EditorGUILayout.PropertyField(buttonTextBasicColor, new GUIContent("Text Basic Color"));
                EditorGUILayout.PropertyField(buttonTextColor, new GUIContent("Text Color"));
                EditorGUILayout.PropertyField(buttonTextHighlightedColor, new GUIContent("Text Highlighted Color"));
                EditorGUILayout.PropertyField(buttonIconBasicColor, new GUIContent("Icon Basic Color"));
                EditorGUILayout.PropertyField(buttonIconColor, new GUIContent("Icon Color"));
                EditorGUILayout.PropertyField(buttonIconHighlightedColor, new GUIContent("Icon Highlighted Color"));
            }

            GUILayout.Space(6);

            // Dropdown
            var dropdownTheme = serializedObject.FindProperty("dropdownThemeType");
            var dropdownAnimationType = serializedObject.FindProperty("dropdownAnimationType");
            var dropdownFont = serializedObject.FindProperty("dropdownFont");
            var dropdownFontSize = serializedObject.FindProperty("dropdownFontSize");
            var dropdownItemFont = serializedObject.FindProperty("dropdownItemFont");
            var dropdownItemFontSize = serializedObject.FindProperty("dropdownItemFontSize");
            var dropdownColor = serializedObject.FindProperty("dropdownColor");
            var dropdownTextColor = serializedObject.FindProperty("dropdownTextColor");
            var dropdownIconColor = serializedObject.FindProperty("dropdownIconColor");
            var dropdownItemColor = serializedObject.FindProperty("dropdownItemColor");
            var dropdownItemTextColor = serializedObject.FindProperty("dropdownItemTextColor");
            var dropdownItemIconColor = serializedObject.FindProperty("dropdownItemIconColor");
            showDropdown = EditorGUILayout.Foldout(showDropdown, "Dropdown", foldoutStyle);

            if (showDropdown && dropdownTheme.enumValueIndex == 0)
            {
                EditorGUILayout.PropertyField(dropdownTheme, new GUIContent("Theme Type"));
                EditorGUILayout.PropertyField(dropdownAnimationType, new GUIContent("Animation Type"));
                EditorGUILayout.PropertyField(dropdownFont, new GUIContent("Font"));
                EditorGUILayout.PropertyField(dropdownFontSize, new GUIContent("Font Size"));
                EditorGUILayout.PropertyField(dropdownColor, new GUIContent("Primary Color"));
                EditorGUILayout.PropertyField(dropdownTextColor, new GUIContent("Secondary Color"));
                EditorGUILayout.PropertyField(dropdownItemColor, new GUIContent("Item Background"));
                EditorGUILayout.HelpBox("Item values will be applied at start.", MessageType.Info);
            }

            if (showDropdown && dropdownTheme.enumValueIndex == 1)
            {
                EditorGUILayout.PropertyField(dropdownTheme, new GUIContent("Theme Type"));
                EditorGUILayout.PropertyField(dropdownAnimationType, new GUIContent("Animation Type"));
                EditorGUILayout.PropertyField(dropdownFont, new GUIContent("Font"));
                EditorGUILayout.PropertyField(dropdownFontSize, new GUIContent("Font Size"));
                EditorGUILayout.PropertyField(dropdownItemFont, new GUIContent("Item Font"));
                EditorGUILayout.PropertyField(dropdownItemFontSize, new GUIContent("Item Font Size"));
                EditorGUILayout.PropertyField(dropdownColor, new GUIContent("Color"));
                EditorGUILayout.PropertyField(dropdownTextColor, new GUIContent("Text Color"));
                EditorGUILayout.PropertyField(dropdownIconColor, new GUIContent("Icon Color"));
                EditorGUILayout.PropertyField(dropdownItemColor, new GUIContent("Item Color"));
                EditorGUILayout.PropertyField(dropdownItemTextColor, new GUIContent("Item Text Color"));
                EditorGUILayout.PropertyField(dropdownItemIconColor, new GUIContent("Item Icon Color"));
                EditorGUILayout.HelpBox("Item values will be applied at start.", MessageType.Info);
            }

            GUILayout.Space(6);

            // Horizontal Selector
            var selectorFont = serializedObject.FindProperty("selectorFont");
            var hSelectorFontSize = serializedObject.FindProperty("hSelectorFontSize");
            var selectorColor = serializedObject.FindProperty("selectorColor");
            var selectorHighlightedColor = serializedObject.FindProperty("selectorHighlightedColor");
            var hSelectorInvertAnimation = serializedObject.FindProperty("hSelectorInvertAnimation");
            var hSelectorLoopSelection = serializedObject.FindProperty("hSelectorLoopSelection");
            showHorSelector = EditorGUILayout.Foldout(showHorSelector, "Horizontal Selector", foldoutStyle);

            if (showHorSelector)
            {
                EditorGUILayout.PropertyField(selectorFont, new GUIContent("Font"));
                EditorGUILayout.PropertyField(hSelectorFontSize, new GUIContent("Font Size"));
                EditorGUILayout.PropertyField(selectorColor, new GUIContent("Color"));
                EditorGUILayout.PropertyField(selectorHighlightedColor, new GUIContent("Highlighted Color"));
                EditorGUILayout.PropertyField(hSelectorInvertAnimation, new GUIContent("Invert Animation"));
                EditorGUILayout.PropertyField(hSelectorLoopSelection, new GUIContent("Loop Selection"));
            }

            GUILayout.Space(6);

            // Input Field
            var inputFieldFont = serializedObject.FindProperty("inputFieldFont");
            var inputFieldFontSize = serializedObject.FindProperty("inputFieldFontSize");
            var inputFieldColor = serializedObject.FindProperty("inputFieldColor");
            showInputField = EditorGUILayout.Foldout(showInputField, "Input Field", foldoutStyle);

            if (showInputField)
            {
                EditorGUILayout.PropertyField(inputFieldFont, new GUIContent("Font"));
                EditorGUILayout.PropertyField(inputFieldFontSize, new GUIContent("Font Size"));
                EditorGUILayout.PropertyField(inputFieldColor, new GUIContent("Color"));
            }

            GUILayout.Space(6);

            // Modal Window
            var modalWindowTitleFont = serializedObject.FindProperty("modalWindowTitleFont");
            var modalWindowContentFont = serializedObject.FindProperty("modalWindowContentFont");
            var modalWindowTitleColor = serializedObject.FindProperty("modalWindowTitleColor");
            var modalWindowDescriptionColor = serializedObject.FindProperty("modalWindowDescriptionColor");
            var modalWindowIconColor = serializedObject.FindProperty("modalWindowIconColor");
            var modalWindowBackgroundColor = serializedObject.FindProperty("modalWindowBackgroundColor");
            var modalWindowContentPanelColor = serializedObject.FindProperty("modalWindowContentPanelColor");
            showModalWindow = EditorGUILayout.Foldout(showModalWindow, "Modal Window", foldoutStyle);

            if (showModalWindow)
            {
                EditorGUILayout.PropertyField(modalWindowTitleFont, new GUIContent("Title Font"));
                EditorGUILayout.PropertyField(modalWindowContentFont, new GUIContent("Content Font"));
                EditorGUILayout.PropertyField(modalWindowTitleColor, new GUIContent("Title Color"));
                EditorGUILayout.PropertyField(modalWindowDescriptionColor, new GUIContent("Description Color"));
                EditorGUILayout.PropertyField(modalWindowIconColor, new GUIContent("Icon Color"));
                EditorGUILayout.PropertyField(modalWindowBackgroundColor, new GUIContent("Background Color"));
                EditorGUILayout.PropertyField(modalWindowContentPanelColor, new GUIContent("Content Panel Color"));
                EditorGUILayout.HelpBox("These values will only affect 'Style 1 - Standard' window.", MessageType.Info);
            }

            GUILayout.Space(6);

            // Notification
            var notificationTitleFont = serializedObject.FindProperty("notificationTitleFont");
            var notificationTitleFontSize = serializedObject.FindProperty("notificationTitleFontSize");
            var notificationDescriptionFont = serializedObject.FindProperty("notificationDescriptionFont");
            var notificationDescriptionFontSize = serializedObject.FindProperty("notificationDescriptionFontSize");
            var notificationBackgroundColor = serializedObject.FindProperty("notificationBackgroundColor");
            var notificationTitleColor = serializedObject.FindProperty("notificationTitleColor");
            var notificationDescriptionColor = serializedObject.FindProperty("notificationDescriptionColor");
            var notificationIconColor = serializedObject.FindProperty("notificationIconColor");
            showNotification = EditorGUILayout.Foldout(showNotification, "Notification", foldoutStyle);

            if (showNotification)
            {
                EditorGUILayout.PropertyField(notificationTitleFont, new GUIContent("Title Font"));
                EditorGUILayout.PropertyField(notificationTitleFontSize, new GUIContent("Title Font Size"));
                EditorGUILayout.PropertyField(notificationDescriptionFont, new GUIContent("Description Font"));
                EditorGUILayout.PropertyField(notificationDescriptionFontSize, new GUIContent("Description Font Size"));
                EditorGUILayout.PropertyField(notificationBackgroundColor, new GUIContent("Background Color"));
                EditorGUILayout.PropertyField(notificationTitleColor, new GUIContent("Title Color"));
                EditorGUILayout.PropertyField(notificationDescriptionColor, new GUIContent("Description Color"));
                EditorGUILayout.PropertyField(notificationIconColor, new GUIContent("Icon Color"));
            }

            GUILayout.Space(6);

            // Progress Bar
            var progressBarLabelFont = serializedObject.FindProperty("progressBarLabelFont");
            var progressBarLabelFontSize = serializedObject.FindProperty("progressBarLabelFontSize");
            var progressBarColor = serializedObject.FindProperty("progressBarColor");
            var progressBarBackgroundColor = serializedObject.FindProperty("progressBarBackgroundColor");
            var progressBarLoopBackgroundColor = serializedObject.FindProperty("progressBarLoopBackgroundColor");
            var progressBarLabelColor = serializedObject.FindProperty("progressBarLabelColor");
            showProgressBar = EditorGUILayout.Foldout(showProgressBar, "Progress Bar", foldoutStyle);

            if (showProgressBar)
            {
                EditorGUILayout.PropertyField(progressBarLabelFont, new GUIContent("Label Font"));
                EditorGUILayout.PropertyField(progressBarLabelFontSize, new GUIContent("Label Font Size"));
                EditorGUILayout.PropertyField(progressBarColor, new GUIContent("Color"));
                EditorGUILayout.PropertyField(progressBarLabelColor, new GUIContent("Label Color"));
                EditorGUILayout.PropertyField(progressBarBackgroundColor, new GUIContent("Background Color"));
                EditorGUILayout.PropertyField(progressBarLoopBackgroundColor, new GUIContent("Loop Background Color"));
            }

            GUILayout.Space(6);

            // Scrollbar
            var scrollbarColor = serializedObject.FindProperty("scrollbarColor");
            var scrollbarBackgroundColor = serializedObject.FindProperty("scrollbarBackgroundColor");
            showScrollbar = EditorGUILayout.Foldout(showScrollbar, "Scrollbar", foldoutStyle);

            if (showScrollbar)
            {
                EditorGUILayout.PropertyField(scrollbarColor, new GUIContent("Bar Color"));
                EditorGUILayout.PropertyField(scrollbarBackgroundColor, new GUIContent("Background Color"));
            }

            GUILayout.Space(6);

            // Slider
            var sliderThemeType = serializedObject.FindProperty("sliderThemeType");
            var sliderLabelFont = serializedObject.FindProperty("sliderLabelFont");
            var sliderLabelFontSize = serializedObject.FindProperty("sliderLabelFontSize");
            var sliderColor = serializedObject.FindProperty("sliderColor");
            var sliderLabelColor = serializedObject.FindProperty("sliderLabelColor");
            var sliderPopupLabelColor = serializedObject.FindProperty("sliderPopupLabelColor");
            var sliderHandleColor = serializedObject.FindProperty("sliderHandleColor");
            var sliderBackgroundColor = serializedObject.FindProperty("sliderBackgroundColor");
            showSlider = EditorGUILayout.Foldout(showSlider, "Slider", foldoutStyle);

            if (showSlider && sliderThemeType.enumValueIndex == 0)
            {
                EditorGUILayout.PropertyField(sliderThemeType, new GUIContent("Theme Type"));
                EditorGUILayout.PropertyField(sliderLabelFont, new GUIContent("Label Font"));
                EditorGUILayout.PropertyField(sliderLabelFontSize, new GUIContent("Label Font Size"));
                EditorGUILayout.PropertyField(sliderColor, new GUIContent("Primary Color"));
                EditorGUILayout.PropertyField(sliderBackgroundColor, new GUIContent("Secondary Color"));
                EditorGUILayout.PropertyField(sliderLabelColor, new GUIContent("Label Popup Color"));
            }

            if (showSlider && sliderThemeType.enumValueIndex == 1)
            {
                EditorGUILayout.PropertyField(sliderThemeType, new GUIContent("Theme Type"));
                EditorGUILayout.PropertyField(sliderLabelFont, new GUIContent("Label Font"));
                EditorGUILayout.PropertyField(sliderLabelFontSize, new GUIContent("Label Font Size"));
                EditorGUILayout.PropertyField(sliderColor, new GUIContent("Color"));
                EditorGUILayout.PropertyField(sliderLabelColor, new GUIContent("Label Color"));
                EditorGUILayout.PropertyField(sliderPopupLabelColor, new GUIContent("Label Popup Color"));
                EditorGUILayout.PropertyField(sliderHandleColor, new GUIContent("Handle Color"));
                EditorGUILayout.PropertyField(sliderBackgroundColor, new GUIContent("Background Color"));
            }

            GUILayout.Space(6);

            // Switch
            var switchBorderColor = serializedObject.FindProperty("switchBorderColor");
            var switchBackgroundColor = serializedObject.FindProperty("switchBackgroundColor");
            var switchHandleOnColor = serializedObject.FindProperty("switchHandleOnColor");
            var switchHandleOffColor = serializedObject.FindProperty("switchHandleOffColor");
            showSwitch = EditorGUILayout.Foldout(showSwitch, "Switch", foldoutStyle);

            if (showSwitch)
            {
                EditorGUILayout.PropertyField(switchBorderColor, new GUIContent("Border Color"));
                EditorGUILayout.PropertyField(switchBackgroundColor, new GUIContent("Background Color"));
                EditorGUILayout.PropertyField(switchHandleOnColor, new GUIContent("Handle On Color"));
                EditorGUILayout.PropertyField(switchHandleOffColor, new GUIContent("Handle Off Color"));
            }

            GUILayout.Space(6);

            // Toggle
            var toggleFont = serializedObject.FindProperty("toggleFont");
            var toggleFontSize = serializedObject.FindProperty("toggleFontSize");
            var toggleTextColor = serializedObject.FindProperty("toggleTextColor");
            var toggleBorderColor = serializedObject.FindProperty("toggleBorderColor");
            var toggleBackgroundColor = serializedObject.FindProperty("toggleBackgroundColor");
            var toggleCheckColor = serializedObject.FindProperty("toggleCheckColor");
            showToggle = EditorGUILayout.Foldout(showToggle, "Toggle", foldoutStyle);

            if (showToggle)
            {
                EditorGUILayout.PropertyField(toggleFont, new GUIContent("Font"));
                EditorGUILayout.PropertyField(toggleFontSize, new GUIContent("Font Size"));
                EditorGUILayout.PropertyField(toggleTextColor, new GUIContent("Text Color"));
                EditorGUILayout.PropertyField(toggleBorderColor, new GUIContent("Border Color"));
                EditorGUILayout.PropertyField(toggleBackgroundColor, new GUIContent("Background Color"));
                EditorGUILayout.PropertyField(toggleCheckColor, new GUIContent("Check Color"));
            }

            GUILayout.Space(6);

            // Tooltip
            var tooltipFont = serializedObject.FindProperty("tooltipFont");
            var tooltipFontSize = serializedObject.FindProperty("tooltipFontSize");
            var tooltipTextColor = serializedObject.FindProperty("tooltipTextColor");
            var tooltipBackgroundColor = serializedObject.FindProperty("tooltipBackgroundColor");
            showTooltip = EditorGUILayout.Foldout(showTooltip, "Tooltip", foldoutStyle);

            if (showTooltip)
            {
                EditorGUILayout.PropertyField(tooltipFont, new GUIContent("Font"));
                EditorGUILayout.PropertyField(tooltipFontSize, new GUIContent("Font Size"));
                EditorGUILayout.PropertyField(tooltipTextColor, new GUIContent("Text Color"));
                EditorGUILayout.PropertyField(tooltipBackgroundColor, new GUIContent("Background Color"));
            }

            GUILayout.Space(7);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(6);

            var enableDynamicUpdate = serializedObject.FindProperty("enableDynamicUpdate");
            EditorGUILayout.PropertyField(enableDynamicUpdate, new GUIContent("Update Values"));

            GUILayout.Space(7);

            var enableExtendedColorPicker = serializedObject.FindProperty("enableExtendedColorPicker");
            EditorGUILayout.PropertyField(enableExtendedColorPicker, new GUIContent("Extended Color Picker"));

            if (enableExtendedColorPicker.boolValue == true)
                EditorPrefs.SetInt("UIManager.EnableExtendedColorPicker", 1);

            else
                EditorPrefs.SetInt("UIManager.EnableExtendedColorPicker", 0);

            GUILayout.Space(7);

            var editorHints = serializedObject.FindProperty("editorHints");
            EditorGUILayout.PropertyField(editorHints, new GUIContent("UI Manager Hints"));

            if (editorHints.boolValue == true)
            {
                EditorGUILayout.HelpBox("These values are universal and will affect any object that contains 'UI Manager' component.", MessageType.Info);
                EditorGUILayout.HelpBox("Remove 'UI Manager' component from the object if you want unique values.", MessageType.Info);
				EditorGUILayout.HelpBox("Press 'CTRL + SHIFT + M' to open UI Manager quickly.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(7);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("Reset to defaults")))
                ResetToDefaults();

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // Apply & Update button
            GUILayout.Space(18);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            // GUILayout.FlexibleSpace();
            GUILayout.Label("Need help? Contact me via:");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            // GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent("Discord")))
                Discord();

            if (GUILayout.Button(new GUIContent("E-mail")))
                Email();

            if (GUILayout.Button(new GUIContent("YouTube")))
                YouTube();

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("Website")))
                Website();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(18);
            GUILayout.Label("Loved the package and wanna support us?");
            if (GUILayout.Button(new GUIContent("Write a review")))
                Review();
        }

        void Discord()
        {
            Application.OpenURL("https://discord.gg/VXpHyUt");
        }

        void Email()
        {
            Application.OpenURL("mailto:isa.steam@outlook.com?subject=Contact");
        }

        void YouTube()
        {
            Application.OpenURL("https://www.youtube.com/c/michsky");
        }

        void Website()
        {
            Application.OpenURL("https://www.michsky.com/");
        }

        void Review()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/gui/modern-ui-pack-150824/reviews/?page=1&sort_by=helpful");
        }

        void ResetToDefaults()
        {
            if (EditorUtility.DisplayDialog("Reset to defaults", "Are you sure you want to reset UI Manager values to default?", "Yes", "Cancel"))
            {
                try
                {
                    Preset defaultPreset = Resources.Load<Preset>("UI Manager Presets/Default");
                    defaultPreset.ApplyTo(Resources.Load("MUIP Manager"));
                    Selection.activeObject = null;
                    Debug.Log("UI Manager - Resetting is successful.");
                }

                catch
                {
                    Debug.LogWarning("UI Manager - Resetting is failed.");
                }
            }    
        }
    }
}
#endif