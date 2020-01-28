using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Michsky.UI.ModernUIPack
{
    public class AnimatedIconHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("SETTINGS")]
        public PlayType playType;

        Animator iconAnimator;
        Button eventButton;
        bool isClicked;

        public enum PlayType
        {
            CLICK,
            ON_POINTER_ENTER
        }

        void Start()
        {
            iconAnimator = gameObject.GetComponent<Animator>();

            if (playType == PlayType.CLICK)
            {
                eventButton = gameObject.GetComponent<Button>();
                eventButton.onClick.AddListener(ClickEvent);
            }
        }

        public void ClickEvent()
        {
            if (isClicked == true)
            {
                iconAnimator.Play("Out");
                isClicked = false;
            }

            else
            {
                iconAnimator.Play("In");
                isClicked = true;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (playType == PlayType.ON_POINTER_ENTER)
                iconAnimator.Play("In");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (playType == PlayType.ON_POINTER_ENTER)
                iconAnimator.Play("Out");
        }
    }
}