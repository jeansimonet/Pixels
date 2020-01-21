using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.ModernUIPack
{
    public class DemoTopListShadow : MonoBehaviour
    {
        public Scrollbar listScrollbar;
        public bool isRight;

        bool enableAnim = false;
        Animator shadowAnimator;

        private void Start()
        {
            shadowAnimator = gameObject.GetComponent<Animator>();
            listScrollbar.value = 0;

            if (isRight == false)
                shadowAnimator.Play("Out");
            else
                shadowAnimator.Play("In");
        }

        void Update()
        {
            if (isRight == true)
            {
                if (listScrollbar.value != 1 && enableAnim == true)
                {
                    shadowAnimator.Play("In");
                    listScrollbar.value = Mathf.Lerp(listScrollbar.value, 1, 0.25f);
                }

                if (listScrollbar.value == 1 || listScrollbar.value >= 0.99f)
                {
                    listScrollbar.value = 1;
                    shadowAnimator.Play("Out");
                    enableAnim = false;
                }

                else if(listScrollbar.value != 1)
                    shadowAnimator.Play("In");

            }

            else
            {
                if (listScrollbar.value != 0 && enableAnim == true)
                {
                    shadowAnimator.Play("In");
                    listScrollbar.value = Mathf.Lerp(listScrollbar.value, 0, 0.25f);
                }

                if (listScrollbar.value == 0 || listScrollbar.value <= 0.01f)
                {
                    listScrollbar.value = 0;
                    shadowAnimator.Play("Out");
                    enableAnim = false;
                }

                else if (listScrollbar.value != 0)
                    shadowAnimator.Play("In");
            }
        }

        public void ScrollUp()
        {
            enableAnim = true;
        }

        public void ScrollDown()
        {
            enableAnim = true;
        }
    }
}