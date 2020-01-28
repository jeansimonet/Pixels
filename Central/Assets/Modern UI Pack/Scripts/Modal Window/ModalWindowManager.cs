using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class ModalWindowManager : MonoBehaviour
    {
        [Header("RESOURCES")]
        public Image windowIcon;
        public TextMeshProUGUI windowTitle;
        public TextMeshProUGUI windowDescription;

        [Header("CONTENT")]
        public Sprite icon;
        public string titleText = "Title";
        [TextArea] public string descriptionText = "Description here";

        [Header("SETTINGS")]
        public bool sharpAnimations = false;
        public bool useCustomValues = false;

        Animator mwAnimator;
        bool isOn = false;

        void Start()
        {
            mwAnimator = gameObject.GetComponent<Animator>();

            if (useCustomValues == false)
            {
                UpdateUI();
            }
        }

        public void UpdateUI()
        {
            windowIcon.sprite = icon;
            windowTitle.text = titleText;
            windowDescription.text = descriptionText;
        }

        public void OpenWindow()
        {
            if (isOn == false)
            {
                if (sharpAnimations == false)
                    mwAnimator.CrossFade("Fade-in", 0.1f);
                else
                    mwAnimator.Play("Fade-in");

                isOn = true;
            }
        }

        public void CloseWindow()
        {
            if (isOn == true)
            {
                if (sharpAnimations == false)
                    mwAnimator.CrossFade("Fade-out", 0.1f);
                else
                    mwAnimator.Play("Fade-out");

                isOn = false;
            }
        }

        public void AnimateWindow()
        {
            if (isOn == false)
            {
                if (sharpAnimations == false)
                    mwAnimator.CrossFade("Fade-in", 0.1f);
                else
                    mwAnimator.Play("Fade-in");

                isOn = true;
            }

            else
            {
                if (sharpAnimations == false)
                    mwAnimator.CrossFade("Fade-out", 0.1f);
                else
                    mwAnimator.Play("Fade-out");

                isOn = false;
            }
        }
    }
}