using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.ModernUIPack
{
    public class DemoManager : MonoBehaviour
    {
        [Header("PANEL LIST")]
        public List<GameObject> panels = new List<GameObject>();

        [Header("BUTTON LIST")]
        public List<GameObject> buttons = new List<GameObject>();

        // [Header("PANEL ANIMS")]
        private string panelFadeIn = "Demo Panel In";
        private string panelFadeOut = "Demo Panel Out";

        // [Header("BUTTON ANIMS")]
        private string buttonFadeIn = "Normal to Pressed";
        private string buttonFadeOut = "Pressed to Normal";

        private GameObject currentPanel;
        private GameObject nextPanel;

        private GameObject currentButton;
        private GameObject nextButton;

        [Header("RESOURCES")]
        public Scrollbar listScrollbar;

        private Animator currentPanelAnimator;
        private Animator nextPanelAnimator;

        private Animator currentButtonAnimator;
        private Animator nextButtonAnimator;

        [HideInInspector] public int currentPanelIndex = 0;
        [HideInInspector] public int currentButtonlIndex = 0;
        [HideInInspector] public float animValue = 1;
        [HideInInspector] public bool enabeScrolling = false;

        void Start()
        {
            currentButton = buttons[currentPanelIndex];
            currentButtonAnimator = currentButton.GetComponent<Animator>();
            currentButtonAnimator.Play(buttonFadeIn);

            currentPanel = panels[currentPanelIndex];
            currentPanelAnimator = currentPanel.GetComponent<Animator>();
            currentPanelAnimator.Play(panelFadeIn);
        }

        void Update()
        {
            if (listScrollbar.value.ToString("F2") != animValue.ToString("F2") && enabeScrolling == false)
                listScrollbar.value = Mathf.Lerp(listScrollbar.value, animValue, 0.1f);
        }

        public void EnableScrolling()
        {
            enabeScrolling = true;
        }

        public void DisableScrolling()
        {
            enabeScrolling = false;
        }

        public void PanelAnim(int newPanel)
        {
            if (newPanel != currentPanelIndex)
            {
                currentPanel = panels[currentPanelIndex];

                currentPanelIndex = newPanel;
                nextPanel = panels[currentPanelIndex];

                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                nextPanelAnimator = nextPanel.GetComponent<Animator>();

                currentPanelAnimator.Play(panelFadeOut);
                nextPanelAnimator.Play(panelFadeIn);

                currentButton = buttons[currentButtonlIndex];

                currentButtonlIndex = newPanel;
                nextButton = buttons[currentButtonlIndex];

                currentButtonAnimator = currentButton.GetComponent<Animator>();
                nextButtonAnimator = nextButton.GetComponent<Animator>();

                currentButtonAnimator.Play(buttonFadeOut);

                if (!nextButtonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hover to Pressed"))
                    nextButtonAnimator.Play(buttonFadeIn);
            }
        }
    }
}