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
    public Button behaviorsButton;
    public Button lightingButton;
    public Button ledPatternButton;
    public Button rgbPattherButton;

    // Start is called before the first frame update
    void Awake()
    {
        menuButton.onClick.AddListener(Hide);
        outsideButton.onClick.AddListener(Hide);
        homeButton.onClick.AddListener(() => GoToRoot(UIPage.PageId.Home));
        diceBagButton.onClick.AddListener(() => GoToRoot(UIPage.PageId.DicePool));
        pfofilesButton.onClick.AddListener(() => GoToRoot(UIPage.PageId.Presets));
        behaviorsButton.onClick.AddListener(() => GoToRoot(UIPage.PageId.Behaviors));
        lightingButton.onClick.AddListener(() => GoToRoot(UIPage.PageId.Patterns));
        ledPatternButton.onClick.AddListener(() => GoToRoot(UIPage.PageId.GradientPatterns));
        rgbPattherButton.onClick.AddListener(() => GoToRoot(UIPage.PageId.RGBPatterns));
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

    void GoToPage(UIPage.PageId page)
    {
        this.gameObject.SetActive(false);
        NavigationManager.Instance.GoToPage(page, null);
    }
}
