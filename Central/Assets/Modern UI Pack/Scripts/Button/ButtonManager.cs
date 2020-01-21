using UnityEngine;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class ButtonManager : MonoBehaviour
    {
        [Header("CONTENT")]
        public string buttonText = "Button";

        [Header("SETTINGS")]
        public bool useCustomContent = false;

        TextMeshProUGUI normalText;
        TextMeshProUGUI highlightedText;

        void Start()
        {
            if (useCustomContent == false)
            {
                normalText = gameObject.transform.Find("Normal/Text").GetComponent<TextMeshProUGUI>();
                highlightedText = gameObject.transform.Find("Highlighted/Text").GetComponent<TextMeshProUGUI>();
                UpdateUI();
            }
        }

        public void UpdateUI()
        {
            normalText.text = buttonText;
            highlightedText.text = buttonText;
        }
    }
}