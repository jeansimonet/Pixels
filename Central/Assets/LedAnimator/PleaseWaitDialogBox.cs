using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PleaseWaitDialogBox : MonoBehaviour
{
    [SerializeField]
    Text _messageField = null;
    [SerializeField]
    Text _pctField = null;

    public void Show(string message)
    {
        _messageField.text = message;
        _pctField.text = "";
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void UpdatePct(float percent)
    {
        _pctField.text = (percent * 100).ToString("F1") + "%";
    }
}
