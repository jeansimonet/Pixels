using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PleaseWaitDialogBox : MonoBehaviour
{
    [SerializeField]
    Text _messageField = null;

    public void Show(string message)
    {
        _messageField.text = message;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
