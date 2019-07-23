using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationUI : MonoBehaviour
{
    public Text notificationField;
    public Button okButton;
    public Button cancelButton;

    public static NotificationUI instance { get; private set; }

    CanvasGroup cg;
    Coroutine timeoutCoroutine;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Multiple notification UI instances in scene");
        }
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
        cg.alpha = 0.0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
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
