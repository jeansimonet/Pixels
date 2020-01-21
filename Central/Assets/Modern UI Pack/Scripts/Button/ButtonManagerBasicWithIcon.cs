using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class ButtonManagerBasicWithIcon : MonoBehaviour
    {
        [Header("CONTENT")]
        public Sprite buttonIcon;
        public string buttonText = "Button";

        [Header("SETTINGS")]
        public bool useCustomContent = false;

        Image normalImage;
        TextMeshProUGUI normalText;

        void Start()
        {
            if (useCustomContent == false)
            {
                normalImage = gameObject.transform.Find("Icon").GetComponent<Image>();
                normalText = gameObject.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                normalImage.sprite = buttonIcon;
                normalText.text = buttonText;
            }
        }
    }
}