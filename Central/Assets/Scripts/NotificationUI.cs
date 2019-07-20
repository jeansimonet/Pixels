using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationUI : MonoBehaviour
{
    public Text notificationField;
    public Button notificationButton;

    public static NotificationUI instance { get; private set; }

    CanvasGroup cg;

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

    public void Show(string message, System.Action callback)
    {
        notificationField.text = message;
        cg.alpha = 1.0f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        notificationButton.onClick.RemoveAllListeners();
        notificationButton.onClick.AddListener(() =>
        {
            Hide();
            callback?.Invoke();
        });
    }

    public void Hide()
    {
        cg.alpha = 0.0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}
