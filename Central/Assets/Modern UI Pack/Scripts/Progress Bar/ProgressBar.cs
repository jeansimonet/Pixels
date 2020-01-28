using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class ProgressBar : MonoBehaviour
    {
        [Header("OBJECTS")]
        public Transform loadingBar;
        public Transform textPercent;

        [Header("SETTINGS")]
        public bool isOn;
        public bool restart;
        public bool invert;
        [Range(0, 100)] public float currentPercent;
        [Range(0, 100)] public int speed;

        void Start()
        {
            if(isOn == false)
            {
                loadingBar.GetComponent<Image>().fillAmount = currentPercent / 100;
                textPercent.GetComponent<TextMeshProUGUI>().text = ((int)currentPercent).ToString("F0") + "%";
            }
        }

        void Update()
        {
            if (isOn == true)
            {
                if (currentPercent <= 100 && invert == false)
                    currentPercent += speed * Time.deltaTime;

                else if (currentPercent >= 0 && invert == true)
                    currentPercent -= speed * Time.deltaTime;

                if (currentPercent == 100 || currentPercent >= 100 && restart == true && invert == false)
                    currentPercent = 0;

                else if (currentPercent == 0 || currentPercent <= 0 && restart == true && invert == true)
                    currentPercent = 100;

                loadingBar.GetComponent<Image>().fillAmount = currentPercent / 100;
                textPercent.GetComponent<TextMeshProUGUI>().text = ((int)currentPercent).ToString("F0") + "%";
            }
        }
    }
}