using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.ModernUIPack
{
    public class ToggleAnim : MonoBehaviour
    {
        Toggle toggleObject;
        Animator toggleAnimator;

        void Start()
        {
            toggleObject = gameObject.GetComponent<Toggle>();
            toggleAnimator = gameObject.GetComponent<Animator>();
            toggleObject.onValueChanged.AddListener(TaskOnClick);

            if (toggleObject.isOn)
                toggleAnimator.Play("Toggle On");

            else
                toggleAnimator.Play("Toggle Off");
        }

        void TaskOnClick(bool value)
        {
            if (toggleObject.isOn)
                toggleAnimator.Play("Toggle On");

            else
                toggleAnimator.Play("Toggle Off");
        }
    }
}