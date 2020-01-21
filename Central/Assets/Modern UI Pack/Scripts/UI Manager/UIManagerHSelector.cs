using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    [ExecuteInEditMode]
    public class UIManagerHSelector : MonoBehaviour
    {
        [Header("SETTINGS")]
        public UIManager UIManagerAsset;

        [Header("RESOURCES")]
        public List<GameObject> images = new List<GameObject>();
        public List<GameObject> imagesHighlighted = new List<GameObject>();
        public List<GameObject> texts = new List<GameObject>();

        bool dynamicUpdateEnabled;
        HorizontalSelector hSelector;

        void OnEnable()
        {
            try
            {
                hSelector = gameObject.GetComponent<HorizontalSelector>();
            }

            catch { }

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
                UpdateSelector();
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
                    UpdateSelector();
            }
        }

        void UpdateSelector()
        {
            for (int i = 0; i < images.Count; ++i)
            {
                Image currentImage = images[i].GetComponent<Image>();
                currentImage.color = new Color(UIManagerAsset.selectorColor.r, UIManagerAsset.selectorColor.g, UIManagerAsset.selectorColor.b, currentImage.color.a);
            }

            for (int i = 0; i < imagesHighlighted.Count; ++i)
            {
                Image currentAlphaImage = imagesHighlighted[i].GetComponent<Image>();
                currentAlphaImage.color = new Color(UIManagerAsset.selectorHighlightedColor.r, UIManagerAsset.selectorHighlightedColor.g, UIManagerAsset.selectorHighlightedColor.b, currentAlphaImage.color.a);
            }

            for (int i = 0; i < texts.Count; ++i)
            {
                TextMeshProUGUI currentText = texts[i].GetComponent<TextMeshProUGUI>();
                currentText.color = new Color(UIManagerAsset.selectorColor.r, UIManagerAsset.selectorColor.g, UIManagerAsset.selectorColor.b, currentText.color.a);
                currentText.font = UIManagerAsset.selectorFont;
                currentText.fontSize = UIManagerAsset.hSelectorFontSize;
            }

            if (hSelector != null)
            {
                hSelector.invertAnimation = UIManagerAsset.hSelectorInvertAnimation;
                hSelector.loopSelection = UIManagerAsset.hSelectorLoopSelection;
            }
        }
    }
}