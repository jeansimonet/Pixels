using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation
{
    public abstract class Page
    {
        public abstract void enter();
        public abstract void leave();
    }

    public class History
    {
        List<Page> pages;

        public History()
        {
            pages = new List<Page>();
        }

        public void GoToRoot(Page newRootPage)
        {
            pages[pages.Count - 1].leave();
            pages.Clear();
            pages.Add(newRootPage);
            newRootPage.enter();
        }
    }
}