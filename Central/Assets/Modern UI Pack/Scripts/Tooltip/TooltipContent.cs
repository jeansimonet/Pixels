using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class TooltipContent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("CONTENT")]
        [TextArea] public string description;

        [Header("RESOURCES")]
        public GameObject tooltipObject;
        public TextMeshProUGUI descriptionText;

        TooltipManager tpManager;
        [HideInInspector] public Animator tooltipAnimator;

        void Start()
        {
            if (tooltipObject == null)
            {
                try
                {
                    tooltipObject = GameObject.Find("Tooltip Rect");
                    descriptionText = tooltipObject.transform.GetComponentInChildren<TextMeshProUGUI>();
                }

                catch
                {
                    Debug.LogError("No Tooltip object assigned.");
                }
            }

            if (tooltipObject != null)
            {
                tpManager = tooltipObject.GetComponentInParent<TooltipManager>();
                tooltipAnimator = tooltipObject.GetComponentInParent<Animator>();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tooltipObject.activeSelf && tooltipObject != null)
            {
                descriptionText.text = description;
                tpManager.allowUpdating = true;
                tooltipAnimator.Play("In");
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltipObject.activeSelf && tooltipObject != null)
            {
                tooltipAnimator.Play("Out");
                tpManager.allowUpdating = true;
            }
        }
    }
}