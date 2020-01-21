using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class RadialSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private const string PREFS_UI_SAVE_NAME = "Radial";

        [Header("RESOURCES")]
        public Image sliderImage;
        public Transform indicatorPivot;
        public TextMeshProUGUI valueText;

        [Header("SETTINGS")]
        public string sliderTag;
        public float maxValue = 100;
        public float currentValue = 50.0f;
        [Range(0, 8)] public int decimals;
        public bool isPercent;
        public bool rememberValue;

        [System.Serializable]
        public class SliderEvent : UnityEvent<float> { }

        [Header("EVENTS")]
        [SerializeField]
        private SliderEvent onValueChanged = new SliderEvent();
        public UnityEvent onPointerEnter;
        public UnityEvent onPointerExit;

        private GraphicRaycaster graphicRaycaster;
        private RectTransform hitRectTransform;
        private bool isPointerDown;
        private float currentAngle;
        private float currentAngleOnPointerDown;
        private float valueDisplayPrecision;

        public float SliderAngle
        {
            get { return currentAngle; }
            set { currentAngle = Mathf.Clamp(value, 0.0f, 360.0f); }
        }

        // Slider value with applied display precision, i.e. the number of decimals to show.
        public float SliderValue
        {
            get { return (long)(SliderValueRaw * valueDisplayPrecision) / valueDisplayPrecision; }
            set { SliderValueRaw = value; }
        }

        // Raw slider value, i.e. without any display precision applied to its value.
        public float SliderValueRaw
        {
            get { return SliderAngle / 360.0f * maxValue; }
            set { SliderAngle = value * 360.0f / maxValue; }
        }

        private void Awake()
        {
            graphicRaycaster = GetComponentInParent<GraphicRaycaster>();

            if (graphicRaycaster == null)
            {
                Debug.LogWarning("Could not find GraphicRaycaster component in parent of this GameObject: " + name);
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            valueDisplayPrecision = Mathf.Pow(10, decimals);
            LoadState();
            SliderAngle = currentValue * 3.6f;
            UpdateUI();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            hitRectTransform = eventData.pointerCurrentRaycast.gameObject.GetComponent<RectTransform>();
            isPointerDown = true;
            currentAngleOnPointerDown = SliderAngle;
            HandleSliderMouseInput(eventData, true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (HasValueChanged())
                SaveState();

            hitRectTransform = null;
            isPointerDown = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            HandleSliderMouseInput(eventData, false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onPointerExit.Invoke();
        }

        public void LoadState()
        {
            if (!rememberValue)
                return;

            currentAngle = PlayerPrefs.GetFloat(sliderTag + PREFS_UI_SAVE_NAME);
        }

        public void SaveState()
        {
            if (!rememberValue)
                return;

            PlayerPrefs.SetFloat(sliderTag + PREFS_UI_SAVE_NAME, currentAngle);
        }

        public void UpdateUI()
        {
            float normalizedAngle = SliderAngle / 360.0f; 
            indicatorPivot.transform.localEulerAngles = new Vector3(180.0f, 0.0f, SliderAngle);
            sliderImage.fillAmount = normalizedAngle;

            valueText.text = string.Format("{0}{1}", SliderValue, isPercent ? "%" : "");
            currentValue = SliderValue;
        }

        private bool HasValueChanged()
        {
            return SliderAngle != currentAngleOnPointerDown;
        }

        private void HandleSliderMouseInput(PointerEventData eventData, bool allowValueWrap)
        {
            if (!isPointerDown)
                return;

            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(hitRectTransform, eventData.position, eventData.pressEventCamera, out localPos);
            float newAngle = Mathf.Atan2(-localPos.y, localPos.x) * Mathf.Rad2Deg + 180f;

            if (!allowValueWrap)
            {
                float currentAngle = SliderAngle;
                bool needsClamping = Mathf.Abs(newAngle - currentAngle) >= 180;

                if (needsClamping)
                    newAngle = currentAngle < newAngle ? 0.0f : 360.0f;
            }

            SliderAngle = newAngle;
            UpdateUI();

            if (HasValueChanged())
                onValueChanged.Invoke(SliderValueRaw);
        }
    }
}