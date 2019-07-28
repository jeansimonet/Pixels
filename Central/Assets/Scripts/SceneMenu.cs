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

    CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
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
        ledAnimatorButton.onClick.RemoveAllListeners();
        ledAnimatorButton.onClick.AddListener(() =>
        {
            Hide();
            SceneManager.LoadScene(0);
        });

        battleGameButton.onClick.RemoveAllListeners();
        battleGameButton.onClick.AddListener(() => {
            Hide();
            SceneManager.LoadScene(1);
        });

        telemetryButton.onClick.RemoveAllListeners();
        telemetryButton.onClick.AddListener(() => {
            Hide();
            SceneManager.LoadScene(2);
        });

        canvasGroup.alpha = 1.0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.0f;
    }
}
