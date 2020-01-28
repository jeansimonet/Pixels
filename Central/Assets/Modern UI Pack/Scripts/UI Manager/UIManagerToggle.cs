using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    [ExecuteInEditMode]
    public class UIManagerToggle : MonoBehaviour
    {
        [Header("SETTINGS")]
        public UIManager UIManagerAsset;

        [Header("RESOURCES")]
        public Image border;
        public Image background;
        public Image check;
        public TextMeshProUGUI onLabel;
        public TextMeshProUGUI offLabel;

        bool dynamicUpdateEnabled;

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
                UpdateDropdown();
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
                    UpdateDropdown();
            }
        }

        void UpdateDropdown()
        {
            try
            {
                border.color = UIManagerAsset.toggleBorderColor;
                background.color = UIManagerAsset.toggleBackgroundColor;
                check.color = UIManagerAsset.toggleCheckColor;
                onLabel.color = new Color(UIManagerAsset.toggleTextColor.r, UIManagerAsset.toggleTextColor.g, UIManagerAsset.toggleTextColor.b, onLabel.color.a);
                onLabel.font = UIManagerAsset.toggleFont;
                onLabel.fontSize = UIManagerAsset.toggleFontSize;
                offLabel.color = new Color(UIManagerAsset.toggleTextColor.r, UIManagerAsset.toggleTextColor.g, UIManagerAsset.toggleTextColor.b, offLabel.color.a);
                offLabel.font = UIManagerAsset.toggleFont;
                offLabel.fontSize = UIManagerAsset.toggleFontSize;
            }

            catch { }
        }
    }
}