using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    [ExecuteInEditMode]
    public class UIManagerModalWindow : MonoBehaviour
    {
        [Header("SETTINGS")]
        public UIManager UIManagerAsset;

        [Header("RESOURCES")]
        public Image background;
        public Image contentBackground;
        public Image icon;
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;

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
                background.color = UIManagerAsset.modalWindowBackgroundColor;
                contentBackground.color = UIManagerAsset.modalWindowContentPanelColor;
                icon.color = UIManagerAsset.modalWindowIconColor;
                title.color = UIManagerAsset.modalWindowTitleColor;
                description.color = UIManagerAsset.modalWindowDescriptionColor;
                title.font = UIManagerAsset.modalWindowTitleFont;
                description.font = UIManagerAsset.modalWindowContentFont;
            }

            catch { }
        }
    }
}