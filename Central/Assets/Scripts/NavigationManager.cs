using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class NavigationManager : SingletonMonoBehaviour<NavigationManager>
{
    public class PageHistory
    {
        public delegate void PageEnterLeaveEvent(PixelsApp.Page page);
        public PageEnterLeaveEvent onPageEntered;
        public PageEnterLeaveEvent onLeavingPage;

        public PixelsApp.Page currentRoot => pages.FirstOrDefault();
        public PixelsApp.Page currentPage => pages.LastOrDefault();

        List<PixelsApp.Page> pages = new List<PixelsApp.Page>();

        public void GoToRoot(PixelsApp.Page newRootPage)
        {
            if (pages.Count > 0)
            {
                LeavePage(pages[pages.Count - 1]);
                pages.Clear();
            }
            // Else no page to leave obviously
            pages.Add(newRootPage);
            EnterPage(newRootPage);
        }

        public void GoTo(PixelsApp.Page newPage)
        {
            if (pages.Count > 0)
            {
                LeavePage(pages[pages.Count - 1]);
            }
            // Else no page to leave obviously
            pages.Add(newPage);
            EnterPage(newPage);
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

        void EnterPage(PixelsApp.Page page)
        {
            page.Enter();
            onPageEntered?.Invoke(page);
        }

        void LeavePage(PixelsApp.Page page)
        {
            onLeavingPage?.Invoke(page);
            page.Leave();
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
                pat.button.onClick.AddListener(() => GoToRoot(pat.page, null));
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
    public void GoToPage(PixelsApp.PageId pageId, System.Action<PixelsApp.Page> afterEnterAction)
    {
        var newPage = pages.FirstOrDefault(pat => pat.pageId == pageId);
        if (newPage != null && history.currentPage != newPage.page)
        {
            history.GoTo(newPage.page);
            afterEnterAction?.Invoke(newPage.page);
        }
        // Else we're already there
    }

    public void GoToRoot(PixelsApp.PageId pageId, System.Action<PixelsApp.Page> afterEnterAction)
    {
        var newPage = pages.FirstOrDefault(pat => pat.pageId == pageId);
        if (newPage != null && history.currentPage != newPage.page)
        {
            GoToRoot(newPage.page, afterEnterAction);
        }
        // Else we're already there
    }

    /// <summary>
    /// Go to a new page as root (clearing history)
    /// </sumary>
    public void GoToRoot(PixelsApp.Page newRoot, System.Action<PixelsApp.Page> afterEnterAction)
    {
        if (history.currentPage != newRoot)
        {
            history.GoToRoot(newRoot);
            afterEnterAction?.Invoke(newRoot);
        }
        // Else we're already there
    }

    /// <summary>
    /// Go back
    /// </sumary>
    public void GoBack(System.Action<PixelsApp.Page> afterEnterAction)
    {
        if (history.GoBack())
        {
            afterEnterAction?.Invoke(history.currentPage);
        }
    }

    /// <summary>
    /// Called when the user clicks on of the root toggles
    /// </sumary>
    void onPageEntered(PixelsApp.Page newPage)
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
