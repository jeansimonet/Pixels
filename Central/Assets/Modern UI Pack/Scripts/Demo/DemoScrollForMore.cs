using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.ModernUIPack
{
    public class DemoScrollForMore : MonoBehaviour
    {
        [Header("RESOURCES")]
        public Scrollbar listScrollbar;
        public Animator SFMAnimator;

        [Header("SETTINGS")]
        public float fadeOutValue;

        void Update()
        {
            if (listScrollbar.value >= fadeOutValue)
            {
                SFMAnimator.Play("SFM In");
            }

            else
            {
                SFMAnimator.Play("SFM Out");
            }
        }
    }
}
