using UnityEngine;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class ButtonManagerBasic : MonoBehaviour
    {
        [Header("CONTENT")]
        public string buttonText = "Button";

        [Header("SETTINGS")]
        public bool useCustomContent = false;

        TextMeshProUGUI normalText;

        void Start()
        {
            if (useCustomContent == false)
            {
                normalText = gameObject.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                normalText.text = buttonText;
            }
        }
    }
}