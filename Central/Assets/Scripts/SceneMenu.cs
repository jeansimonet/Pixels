using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneMenu : MonoBehaviour
{
    public Button ledAnimatorButton;
    public Button battleGameButton;
    public Button telemetryButton;

    private void Awake()
    {
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Show()
    {
        var canvasGroup = GetComponent<CanvasGroup>();

        ledAnimatorButton.onClick.RemoveAllListeners();
        ledAnimatorButton.onClick.AddListener(() =>
        {
            Hide();
            Central.Instance.Deinitialize();
            SceneManager.LoadScene(0);
        });

        battleGameButton.onClick.RemoveAllListeners();
        battleGameButton.onClick.AddListener(() => {
            Hide();
            Central.Instance.Deinitialize();
            SceneManager.LoadScene(1);
        });

        telemetryButton.onClick.RemoveAllListeners();
        telemetryButton.onClick.AddListener(() => {
            Hide();
            Central.Instance.Deinitialize();
            SceneManager.LoadScene(2);
        });

        gameObject.SetActive(true);
        canvasGroup.alpha = 1.0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.0f;
    }
}
