using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    [Header("Controls")]
    public Button menuButton;
    public Button outsideButton;

    public Button homeButton;
    public Button diceBagButton;
    public Button pfofilesButton;
    public Button tutorialButton;
    public Button lightingButton;
    public Button ledPatternButton;
    public Button audioClipsButton;
    public Button disableOnDeviceButton;
    public Button importPatternButton;

    // Start is called before the first frame update
    void Awake()
    {
        menuButton.onClick.AddListener(Hide);
        outsideButton.onClick.AddListener(Hide);
        homeButton.onClick.AddListener(() => { Hide(); GoToRoot(UIPage.PageId.Home);});
        diceBagButton.onClick.AddListener(() => { Hide(); GoToRoot(UIPage.PageId.DicePool);});
        pfofilesButton.onClick.AddListener(() => { Hide(); GoToRoot(UIPage.PageId.Presets);});
        tutorialButton.onClick.AddListener(() => { Hide(); PixelsApp.Instance.RestartTutorial();});
        lightingButton.onClick.AddListener(() => { Hide(); GoToRoot(UIPage.PageId.Patterns);});
        ledPatternButton.onClick.AddListener(() => { Hide(); GoToRoot(UIPage.PageId.GradientPatterns);});
        audioClipsButton.onClick.AddListener(() => { Hide(); GoToRoot(UIPage.PageId.AudioClips);});
        disableOnDeviceButton.onClick.AddListener(() =>
        {
            Hide();
            if (AppDataSet.Instance.defaultBehavior == null)
            {
                AppDataSet.Instance.defaultBehavior = new Behaviors.EditBehavior();
            }
            GoToPage(UIPage.PageId.Behavior, AppDataSet.Instance.defaultBehavior);
        });
        importPatternButton.onClick.AddListener(() => { Hide(); PixelsApp.Instance.ImportPattern(); });
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    void GoToRoot(UIPage.PageId page)
    {
        this.gameObject.SetActive(false);
        NavigationManager.Instance.GoToRoot(page);
    }

    void GoToPage(UIPage.PageId page, object context)
    {
        this.gameObject.SetActive(false);
        NavigationManager.Instance.GoToPage(page, context);
    }
}
