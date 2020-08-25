using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A simple button that controls an animator to show a rotating icon
/// </sumary>
public class UIDicePoolRefreshButton : MonoBehaviour
{
    [Header("Controls")]
    public Image refreshImage;
    public Animator refreshImageAnimator;

    public Button.ButtonClickedEvent onClick => GetComponent<Button>().onClick;

    enum State
    {
        Idle,
        Rotating
    }
    State state = State.Idle;

    public void StartRotating()
    {
        Debug.Assert(state == State.Idle);
        state = State.Rotating;
        refreshImageAnimator.SetBool("Rotating", true);
    }

    public void StopRotating()
    {
        Debug.Assert(state == State.Rotating);
        state = State.Idle;

        // Reset the rotation of the image
        refreshImageAnimator.SetBool("Rotating", false);
    }
}
