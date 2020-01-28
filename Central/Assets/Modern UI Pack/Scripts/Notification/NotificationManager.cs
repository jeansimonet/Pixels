using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class NotificationManager : MonoBehaviour
    {
        [Header("NOTIFICATION CONTENT")]
        public Sprite icon;
        public string title = "Notification Title";
        [TextArea] public string description = "Notification description";

        [Header("TIMER")]
        public bool enableTimer = true;
        public float timer = 3f;

        [Header("SETTINGS")]
        public bool useCustomContent = false;
        public NotificationStyle notificationStyle;

        Animator notificationAnimator;
        Image iconObj;
        TextMeshProUGUI titleObj;
        TextMeshProUGUI descriptionObj;

        public enum NotificationStyle
        {
            FADING,
            POPUP,
            SLIDING
        }

        void Start()
        {
            try
            {
                if (notificationAnimator == null)
                    notificationAnimator = gameObject.GetComponent<Animator>();

                if (useCustomContent == false)
                {
                    if (notificationStyle == NotificationStyle.SLIDING)
                    {
                        iconObj = gameObject.transform.Find("Content/Icon").GetComponent<Image>();
                        titleObj = gameObject.transform.Find("Content/Title").GetComponent<TextMeshProUGUI>();
                        descriptionObj = gameObject.transform.Find("Content/Description").GetComponent<TextMeshProUGUI>();
                    }

                    else
                    {
                        iconObj = gameObject.transform.Find("Icon").GetComponent<Image>();
                        titleObj = gameObject.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                        descriptionObj = gameObject.transform.Find("Description").GetComponent<TextMeshProUGUI>();
                    }

                    iconObj.sprite = icon;
                    titleObj.text = title;
                    descriptionObj.text = description;
                }
            }

            catch
            {
                Debug.LogError("Notification - Cannot initalize the object due to missing components.");
            }         
        }

        IEnumerator StartTimer()
        {
            yield return new WaitForSeconds(timer);
            notificationAnimator.Play("Out");
            StopCoroutine("StartTimer");
        }

        public void OpenNotification()
        {
            notificationAnimator.Play("In");

            if (enableTimer == true)
                StartCoroutine("StartTimer");
        }

        public void CloseNotification()
        {
            notificationAnimator.Play("Out");
        }

        public void UpdateUI()
        {
            try
            {
                iconObj.sprite = icon;
                titleObj.text = title;
                descriptionObj.text = description;
            }

            catch
            {
                Debug.LogError("Notification - Cannot update the object due to missing components.");
            }
        }
    }
}