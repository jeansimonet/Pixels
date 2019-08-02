using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationUI : SingletonMonoBehaviour<NotificationUI>
{
    public Text notificationField;
    public Button okButton;
    public Button cancelButton;

    CanvasGroup cg;
    Coroutine timeoutCoroutine;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Show(string message, bool ok, bool cancel, float timeout, System.Action<bool> callback)
    {
        gameObject.SetActive(true); // Unity WTF? Got to call this twice with 2019.1.10
        gameObject.SetActive(true);
        notificationField.text = message;
        cg.alpha = 1.0f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        if (ok)
        {
            okButton.gameObject.SetActive(true);
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(() =>
            {
                StopCoroutine(timeoutCoroutine);
                Hide();
                callback?.Invoke(true);
            });
        }
        else
        {
            okButton.gameObject.SetActive(false);
        }

        if (cancel)
        {
            cancelButton.gameObject.SetActive(true);
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() =>
            {
                StopCoroutine(timeoutCoroutine);
                Hide();
                callback?.Invoke(false);
            });
        }
        else
        {
            cancelButton.gameObject.SetActive(false);
        }

        // Start timeout
        timeoutCoroutine = StartCoroutine(TimeoutCr(timeout, callback));
    }

    public void Hide()
    {
        // cg.alpha = 0.0f;
        // cg.interactable = false;
        // cg.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    IEnumerator TimeoutCr(float timeout, System.Action<bool> callback)
    {
        float startTime = Time.time;
        while (Time.time < startTime + timeout)
        {
            yield return null;
        }
        Hide();
        callback(false);
    }
}
