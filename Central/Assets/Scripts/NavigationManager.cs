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
            public PixelsApp.Page page;
            public object context;
        }

        public delegate void PageEnterEvent(PixelsApp.Page page, object context);
        public PageEnterEvent onPageEntered;
        public delegate void PageLeavingEvent(PixelsApp.Page page);
        public PageLeavingEvent onLeavingPage;

        public Page currentRoot => pages.FirstOrDefault();
        public Page currentPage => pages.LastOrDefault();

        List<Page> pages = new List<Page>();

        public void GoToRoot(PixelsApp.Page newRootPage)
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

        public void GoTo(PixelsApp.Page newPage, object context)
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

    [System.Serializable]
    public class PageAndToggle
    {
        public PixelsApp.Page page;
        public MainNavigationButton button;
        public PixelsApp.PageId pageId;
    }
    public PageAndToggle[] pages;

    PageHistory history;

    // Start is called before the first frame update
    void Awake()
    {
        foreach (var pat in pages)
        {
            pat.page.Leave();
            if (pat.button != null)
            {
                pat.button.onClick.AddListener(() => GoToRoot(pat.page));
                pat.button.SetCurrent(false);
            }
        }

        // Start the page navigation history
        history = new PageHistory();
        history.onPageEntered += onPageEntered;

        // Go to the home page
        history.GoToRoot(pages[0].page);
    }

    /// <summary>
    /// Go to a new page
    /// </sumary>
    public void GoToPage(PixelsApp.PageId pageId, object context)
    {
        var newPage = pages.FirstOrDefault(pat => pat.pageId == pageId);
        if (newPage != null && history.currentPage.page != newPage.page)
        {
            history.GoTo(newPage.page, context);
        }
        // Else we're already there
    }

    public void GoToRoot(PixelsApp.PageId pageId)
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
    public void GoToRoot(PixelsApp.Page newRoot)
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
    void onPageEntered(PixelsApp.Page newPage, object context)
    {
        // Update the buttons to reflect whether the page is one of the roots
        foreach (var p in pages)
        {
            if (p.button != null)
            {
                p.button.SetCurrent(p.page == newPage);
            }
        }
    }
}
