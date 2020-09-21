using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Tutorial : SingletonMonoBehaviour<Tutorial>
{
    [Header("Main Tutorial")]
    public RectTransform tutorialIntroRoot;
    public Button tutorialIntroNext;
    public Button tutorialIntroCancel;

    public RectTransform tutorialIntro2Root;
    public Button tutorialIntro2Next;

    public RectTransform scanningTutorialRoot;
    public Button scanningTutorialNext;

    public RectTransform poolTutorialRoot;
    public Button poolTutorialNext;

    public RectTransform homeTutorialRoot;
    public Button homeTutorialNext;

    public RectTransform home2TutorialRoot;
    public Button home2TutorialNext;

    public UIPage poolPage;

    [Header("Presets Tutorial")]
    public RectTransform presetsTutorialRoot;
    public Button presetsTutorialNext;

    public RectTransform presets2TutorialRoot;
    public Button presets2TutorialNext;

    [Header("Preset Tutorial")]
    public RectTransform presetTutorialRoot;
    public Button presetTutorialNext;

    public RectTransform preset2TutorialRoot;
    public Button preset2TutorialNext;

    [Header("Behavior Tutorial")]
    public RectTransform behaviorTutorialRoot;
    public Button behaviorTutorialNext;

    [Header("Rule Tutorial")]
    public RectTransform ruleTutorialRoot;
    public Button ruleTutorialNext;

    public RectTransform rule2TutorialRoot;
    public Button rule2TutorialNext;

    [Header("Animations Tutorial")]
    public RectTransform animationsTutorialRoot;
    public Button animationsTutorialNext;

    [Header("Animation Tutorial")]
    public RectTransform animationTutorialRoot;
    public Button animationTutorialNext;

    public RectTransform animation2TutorialRoot;
    public Button animation2TutorialNext;

    public void StartMainTutorial()
    {
        tutorialIntroRoot.gameObject.SetActive(true);
        tutorialIntroCancel.onClick.RemoveAllListeners();
        tutorialIntroCancel.onClick.AddListener(() =>
        {
            // Disable all tutorials
            AppSettings.Instance.DisableAllTutorials();
            tutorialIntroRoot.gameObject.SetActive(false);
        });
        tutorialIntroNext.onClick.RemoveAllListeners();
        tutorialIntroNext.onClick.AddListener(() =>
        {
            // Next step
            AppSettings.Instance.SetMainTutorialEnabled(false);
            tutorialIntroRoot.gameObject.SetActive(false);

            tutorialIntro2Root.gameObject.SetActive(true);
            tutorialIntro2Next.onClick.RemoveAllListeners();
            tutorialIntro2Next.onClick.AddListener(() =>
            {
                tutorialIntro2Root.gameObject.SetActive(false);
                NavigationManager.Instance.GoToRoot(UIPage.PageId.DicePool);
                NavigationManager.Instance.GoToPage(UIPage.PageId.DicePoolScanning, null);

                IEnumerator waitAndDisplayScanningTutorial()
                {
                    yield return new WaitForSeconds(0.25f);
                    scanningTutorialRoot.gameObject.SetActive(true);
                    scanningTutorialNext.onClick.RemoveAllListeners();
                    scanningTutorialNext.onClick.AddListener(() =>
                    {
                        scanningTutorialRoot.gameObject.SetActive(false);

                        // Now we wait until the user connects their dice
                        void checkCanGoToPage(UIPage page, object context, System.Action goToPage)
                        {
                            if (page != poolPage || (DiceManager.Instance.allDice.Count() == 0 && DiceManager.Instance.state != DiceManager.State.AddingDiscoveredDie))
                            {
                                PixelsApp.Instance.ShowDialogBox("Are you sure?", "You have not paired any die, are you sure you want to leave the tutorial?", "Yes", "Cancel", res =>
                                {
                                    if (res)
                                    {
                                        NavigationManager.Instance.checkCanGoToPage = null;
                                        NavigationManager.Instance.onPageEntered -= onPageChanged;
                                        goToPage.Invoke();
                                    }
                                });
                            }
                            else
                            {
                                goToPage?.Invoke();
                            }
                        }

                        void onPageChanged(UIPage newPage, object context)
                        {
                            IEnumerator waitUntilIdleAgainAndContinue()
                            {
                                yield return new WaitUntil(() => DiceManager.Instance.state == DiceManager.State.Idle);

                                // Check that we DO in fact have dice in the list
                                if (DiceManager.Instance.allDice.Count() > 0)
                                {
                                    // Automatically assign dice!
                                    List<Dice.EditDie> userDice = new List<Dice.EditDie>(DiceManager.Instance.allDice);
                                    foreach (var preset in AppDataSet.Instance.presets)
                                    {
                                        foreach (var assignment in preset.dieAssignments)
                                        {
                                            if (assignment.die == null)
                                            {
                                                assignment.die = userDice[assignment.defaultDieAssignmentIndex % userDice.Count];
                                            }
                                        }
                                    }

                                    poolTutorialRoot.gameObject.SetActive(true);
                                    poolTutorialNext.onClick.RemoveAllListeners();
                                    poolTutorialNext.onClick.AddListener(() =>
                                    {
                                        IEnumerator waitAndDisplayHomeTutorial()
                                        {
                                            poolTutorialRoot.gameObject.SetActive(false);
                                            NavigationManager.Instance.GoToRoot(UIPage.PageId.Home);
                                            yield return new WaitForSeconds(0.25f);
                                            homeTutorialRoot.gameObject.SetActive(true);
                                            homeTutorialNext.onClick.RemoveAllListeners();
                                            homeTutorialNext.onClick.AddListener(() =>
                                            {
                                                homeTutorialRoot.gameObject.SetActive(false);
                                                home2TutorialRoot.gameObject.SetActive(true);
                                                home2TutorialNext.onClick.RemoveAllListeners();
                                                home2TutorialNext.onClick.AddListener(() =>
                                                {
                                                    home2TutorialRoot.gameObject.SetActive(false);
                                                    AppSettings.Instance.SetMainTutorialEnabled(false);
                                                });
                                            });
                                        }
                                        StartCoroutine(waitAndDisplayHomeTutorial());

                                    });
                                }
                            }
                            NavigationManager.Instance.onPageEntered -= onPageChanged;
                            NavigationManager.Instance.checkCanGoToPage = null;
                            StartCoroutine(waitUntilIdleAgainAndContinue());
                        }

                        NavigationManager.Instance.onPageEntered += onPageChanged;
                        NavigationManager.Instance.checkCanGoToPage = checkCanGoToPage;
                    });
                }

                StartCoroutine(waitAndDisplayScanningTutorial());
            });
        });
    }

    public void StartPresetsTutorial()
    {
        presetsTutorialRoot.gameObject.SetActive(true);
        presetsTutorialNext.onClick.RemoveAllListeners();
        presetsTutorialNext.onClick.AddListener(() =>
        {
            presetsTutorialRoot.gameObject.SetActive(false);
            presets2TutorialRoot.gameObject.SetActive(true);
            presets2TutorialNext.onClick.RemoveAllListeners();
            presets2TutorialNext.onClick.AddListener(() =>
            {
                presets2TutorialRoot.gameObject.SetActive(false);
                AppSettings.Instance.SetPresetsTutorialEnabled(false);
            });
        });
    }

    public void StartPresetTutorial()
    {
        presetTutorialRoot.gameObject.SetActive(true);
        presetTutorialNext.onClick.RemoveAllListeners();
        presetTutorialNext.onClick.AddListener(() =>
        {
            presetTutorialRoot.gameObject.SetActive(false);
            preset2TutorialRoot.gameObject.SetActive(true);
            preset2TutorialNext.onClick.RemoveAllListeners();
            preset2TutorialNext.onClick.AddListener(() =>
            {
                preset2TutorialRoot.gameObject.SetActive(false);
                AppSettings.Instance.SetPresetTutorialEnabled(false);
            });
        });
    }

    public void StartBehaviorTutorial()
    {
        behaviorTutorialRoot.gameObject.SetActive(true);
        behaviorTutorialNext.onClick.RemoveAllListeners();
        behaviorTutorialNext.onClick.AddListener(() =>
        {
            behaviorTutorialRoot.gameObject.SetActive(false);
            AppSettings.Instance.SetBehaviorTutorialEnabled(false);
        });
    }

    public void StartRuleTutorial()
    {
        ruleTutorialRoot.gameObject.SetActive(true);
        ruleTutorialNext.onClick.RemoveAllListeners();
        ruleTutorialNext.onClick.AddListener(() =>
        {
            ruleTutorialRoot.gameObject.SetActive(false);
            rule2TutorialRoot.gameObject.SetActive(true);
            rule2TutorialNext.onClick.RemoveAllListeners();
            rule2TutorialNext.onClick.AddListener(() =>
            {
                rule2TutorialRoot.gameObject.SetActive(false);
                AppSettings.Instance.SetRuleTutorialEnabled(false);
            });
        });
    }

    public void StartAnimationsTutorial()
    {
        animationsTutorialRoot.gameObject.SetActive(true);
        animationsTutorialNext.onClick.RemoveAllListeners();
        animationsTutorialNext.onClick.AddListener(() =>
        {
            animationsTutorialRoot.gameObject.SetActive(false);
            AppSettings.Instance.SetAnimationsTutorialEnabled(false);
        });
    }

    public void StartAnimationTutorial()
    {
        animationTutorialRoot.gameObject.SetActive(true);
        animationTutorialNext.onClick.RemoveAllListeners();
        animationTutorialNext.onClick.AddListener(() =>
        {
            animationTutorialRoot.gameObject.SetActive(false);
            animation2TutorialRoot.gameObject.SetActive(true);
            animation2TutorialNext.onClick.RemoveAllListeners();
            animation2TutorialNext.onClick.AddListener(() =>
            {
                animation2TutorialRoot.gameObject.SetActive(false);
                AppSettings.Instance.SetAnimationTutorialEnabled(false);
            });
        });
    }

}
