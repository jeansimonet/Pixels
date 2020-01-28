using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.ModernUIPack
{
    public class ButtonManagerIcon : MonoBehaviour
    {
        [Header("CONTENT")]
        public Sprite buttonIcon;

        [Header("SETTINGS")]
        public bool useCustomContent = false;

        Image normalIcon;
        Image highlightedIcon;

        void Start()
        {
            if (useCustomContent == false)
            {
                normalIcon = gameObject.transform.Find("Normal/Icon").GetComponent<Image>();
                highlightedIcon = gameObject.transform.Find("Highlighted/Icon").GetComponent<Image>();

                normalIcon.sprite = buttonIcon;
                highlightedIcon.sprite = buttonIcon;
            }
        }
    }
}