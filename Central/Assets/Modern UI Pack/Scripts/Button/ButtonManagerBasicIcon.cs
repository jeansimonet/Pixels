using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.ModernUIPack
{
    public class ButtonManagerBasicIcon : MonoBehaviour
    {
        [Header("CONTENT")]
        public Sprite buttonIcon;

        [Header("SETTINGS")]
        public bool useCustomContent = false;

        Image normalIcon;

        void Start()
        {
            if (useCustomContent == false)
            {
                normalIcon = gameObject.transform.Find("Icon").GetComponent<Image>();
                normalIcon.sprite = buttonIcon;
            }
        }
    }
}