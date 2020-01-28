using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class ButtonManagerWithIcon : MonoBehaviour
    {
        [Header("CONTENT")]
        public Sprite buttonIcon;
        public string buttonText = "Button";

        [Header("SETTINGS")]
        public bool useCustomContent = false;

        Image normalIcon;
        Image highlightedIcon;
        TextMeshProUGUI normalText;
        TextMeshProUGUI highlightedText;

        void Start()
        {
            if (useCustomContent == false)
            {
                normalIcon = gameObject.transform.Find("Normal/Icon").GetComponent<Image>();
                highlightedIcon = gameObject.transform.Find("Highlighted/Icon").GetComponent<Image>();
                normalText = gameObject.transform.Find("Normal/Text").GetComponent<TextMeshProUGUI>();
                highlightedText = gameObject.transform.Find("Highlighted/Text").GetComponent<TextMeshProUGUI>();

                normalIcon.sprite = buttonIcon;
                highlightedIcon.sprite = buttonIcon;
                normalText.text = buttonText;
                highlightedText.text = buttonText;
            }
        }
    }
}