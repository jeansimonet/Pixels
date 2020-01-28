using UnityEngine;

namespace Michsky.UI.ModernUIPack
{
    public class TooltipManager : MonoBehaviour
    {
        [Header("RESOURCES")]
        public GameObject tooltipObject;
        public GameObject tooltipContent;

        [Header("SETTINGS")]
        [Range(0.05f, 0.5f)] public float tooltipSmoothness = 0.1f;

        [Header("TOOLTIP BOUNDS")]
        public int vBorderTop = -115;
        public int vBorderBottom = 100;
        public int hBorderLeft = 230;
        public int hBorderRight = -210;

        [HideInInspector] public bool allowUpdating = false;

        Vector2 uiPos;
        Vector3 cursorPos;
        RectTransform tooltipRect;
        RectTransform tooltipZHelper;
        private Vector3 contentPos = new Vector3(0, 0, 0);
        Vector3 tooltipVelocity = Vector3.zero;

        void Start()
        {
            tooltipZHelper = gameObject.GetComponentInParent<RectTransform>();
            tooltipRect = tooltipObject.GetComponent<RectTransform>();
            contentPos = new Vector3(vBorderTop, hBorderLeft, 0);
        }

        void Update()
        {
            if (allowUpdating == true)
            {
                cursorPos = Input.mousePosition;
                cursorPos.z = tooltipZHelper.position.z;
                tooltipRect.position = Camera.main.ScreenToWorldPoint(cursorPos);
                uiPos = tooltipRect.anchoredPosition;
                CheckForBounds();
                tooltipContent.transform.localPosition = Vector3.SmoothDamp(tooltipContent.transform.localPosition, contentPos, ref tooltipVelocity, tooltipSmoothness);
            }
        }

        public void CheckForBounds()
        {
            if (uiPos.x <= -400)
            {
                contentPos = new Vector3(hBorderLeft, contentPos.y, 0);
                tooltipContent.GetComponent<RectTransform>().pivot = new Vector2(0f, tooltipContent.GetComponent<RectTransform>().pivot.y);
            }

            if (uiPos.x >= 400)
            {
                contentPos = new Vector3(hBorderRight, contentPos.y, 0);
                tooltipContent.GetComponent<RectTransform>().pivot = new Vector2(1f, tooltipContent.GetComponent<RectTransform>().pivot.y);
            }

            if (uiPos.y <= -325)
            {
                contentPos = new Vector3(contentPos.x, vBorderBottom, 0);
                tooltipContent.GetComponent<RectTransform>().pivot = new Vector2(tooltipContent.GetComponent<RectTransform>().pivot.x, 0f);
            }

            if (uiPos.y >= 325)
            {
                contentPos = new Vector3(contentPos.x, vBorderTop, 0);
                tooltipContent.GetComponent<RectTransform>().pivot = new Vector2(tooltipContent.GetComponent<RectTransform>().pivot.x, 1f);
            }
        }
    }
}