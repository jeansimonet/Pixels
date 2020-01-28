using UnityEngine;

namespace Michsky.UI.ModernUIPack
{
    public class DemoScrollManager : MonoBehaviour
    {
        [Header("RESOURCES")]
        public DemoManager panelManager;
        public float topValue;
        public float bottomValue;
        // public string testFloat;

        void Update()
        {
            if (panelManager.listScrollbar.value <= topValue && panelManager.listScrollbar.value >= bottomValue)
                panelManager.PanelAnim(gameObject.transform.GetSiblingIndex());

         //   testFloat = panelManager.listScrollbar.value.ToString("F2");
        }

        public void GoToPanel(float panelValue)
        {
            panelManager.enabeScrolling = false;
            panelManager.animValue = panelValue;
        }
    }
}