using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class NavigationManager : SingletonMonoBehaviour<NavigationManager>
{
    public class PageHistory
    {
        public class Page
        {
            public UIPage page;
            public object context;
        }

        public System.Action<UIPage, object, System.Action> checkCanGoToPage;

        public delegate void PageEnterEvent(UIPage page, object context);
        public PageEnterEvent onPageEntered;
        public delegate void PageLeavingEvent(UIPage page);
        public PageLeavingEvent onLeavingPage;

        public Page currentRoot => pages.FirstOrDefault();
        public Page currentPage => pages.LastOrDefault();

        List<Page> pages = new List<Page>();

        public void GoToRoot(UIPage newRootPage)
        {
            void goToNewRoot()
            {
                if (pages.Count > 0)
                {
                    LeavePage(pages[pages.Count - 1]);
                    pages.Clear();
                }
                // Else no page to leave obviously
                var p = new Page() { page = newRootPage, context = null };
                pages.Add(p);
                EnterPage(p);
            }

            if (checkCanGoToPage != null)
            {
                checkCanGoToPage(newRootPage, null, goToNewRoot);
            }
            else
            {
                goToNewRoot();
            }
        }

        public void GoTo(UIPage newPage, object context)
        {
            void goToNewPage()
            {
                if (pages.Count > 0)
                {
                    LeavePage(pages[pages.Count - 1]);
                }
                // Else no page to leave obviously
                var p = new Page() { page = newPage, context = context };
                pages.Add(p);
                EnterPage(p);
            }

            if (checkCanGoToPage != null)
            {
                checkCanGoToPage(newPage, context, goToNewPage);
            }
            else
            {
                goToNewPage();
            }
        }

        public bool GoBack()
        {
            bool ret = (pages.Count > 1);
            if (ret)
            {
                LeavePage(pages[pages.Count - 1]);
                pages.RemoveAt(pages.Count - 1);
                EnterPage(pages[pages.Count - 1]);
            }
            // Else no page to go back to
            return ret;
        }

        void EnterPage(Page page)
        {
            page.page.Enter(page.context);
            onPageEntered?.Invoke(page.page, page.context);
        }

        void LeavePage(Page page)
        {
            onLeavingPage?.Invoke(page.page);
            page.page.Leave();
        }
    }

    public UIPageHeader header;

    [System.Serializable]
    public class PageAndToggle
    {
        public UIPage page;
        public MainNavigationButton button;
        public UIPage.PageId pageId;
    }
    public PageAndToggle[] pages;

    public delegate void PageEnterEvent(UIPage page, object context);
    public PageEnterEvent onPageEntered;
    public delegate void PageLeavingEvent(UIPage page);
    public PageLeavingEvent onLeavingPage;

    public System.Action<UIPage, object, System.Action> checkCanGoToPage
    {
        get { return history.checkCanGoToPage; }
        set { history.checkCanGoToPage = value; }
    }

    PageHistory history;

    // Start is called before the first frame update
    void Awake()
    {
        header.onBackClicked.AddListener(OnBack);
        header.onMenuClicked.AddListener(OnMenu);
        header.onSaveClicked.AddListener(OnSave);

        foreach (var pat in pages)
        {
            pat.page.gameObject.SetActive(false);
            if (pat.button != null)
            {
                pat.button.onClick.AddListener(() => GoToRoot(pat.page));
                pat.button.SetCurrent(false);
            }
        }

        // Start the page navigation history
        history = new PageHistory();
        history.onPageEntered += onHistoryPageEntered;
        history.onLeavingPage += onHistoryLeavingPage;

        // Go to the home page
        history.GoToRoot(pages[0].page);
    }

    /// <summary>
    /// Go to a new page
    /// </sumary>
    public void GoToPage(UIPage.PageId pageId, object context)
    {
        var newPage = pages.FirstOrDefault(pat => pat.pageId == pageId);
        if (newPage != null && history.currentPage.page != newPage.page)
        {
            history.GoTo(newPage.page, context);
        }
        // Else we're already there
    }

    public void GoToRoot(UIPage.PageId pageId)
    {
        var newPage = pages.FirstOrDefault(pat => pat.pageId == pageId);
        if (newPage != null && history.currentPage.page != newPage.page)
        {
            GoToRoot(newPage.page);
        }
        // Else we're already there
    }

    /// <summary>
    /// Go to a new page as root (clearing history)
    /// </sumary>
    public void GoToRoot(UIPage newRoot)
    {
        if (history.currentPage.page != newRoot)
        {
            history.GoToRoot(newRoot);
        }
        // Else we're already there
    }

    /// <summary>
    /// Go back
    /// </sumary>
    public void GoBack()
    {
        history.GoBack();
    }

    /// <summary>
    /// Called when the user clicks on of the root toggles
    /// </sumary>
    void onHistoryPageEntered(UIPage newPage, object context)
    {
        // Update the buttons to reflect whether the page is one of the roots
        foreach (var p in pages)
        {
            if (p.button != null)
            {
                p.button.SetCurrent(p.page == newPage);
            }
        }
        onPageEntered?.Invoke(newPage, context);
    }

    void onHistoryLeavingPage(UIPage page)
    {
        onLeavingPage?.Invoke(page);
    }

    void OnBack()
    {
        history.currentPage.page.OnBack();
    }
    void OnMenu()
    {

    }
    void OnSave()
    {
        history.currentPage.page.OnSave();
    }

}
