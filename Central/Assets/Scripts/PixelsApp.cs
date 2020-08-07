using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelsApp : SingletonMonoBehaviour<PixelsApp>
{
    [Header("Panels")]
    public UIDialogBox dialogBox;
    public UIColorPicker colorPicker;

    public enum PageId
    {
        Home,
        DicePool,
        DicePoolScanning,
        Patterns,
        Pattern,
        Presets,
        Preset,
        Behaviors,
        Behavior,
        Rule,
        LiveView
    }

    public class Page
        : MonoBehaviour
    {
        public virtual void Enter(object context)
        {
            gameObject.SetActive(true);
        }
        public virtual void Leave()
        {
            gameObject.SetActive(false);
        }
    }

    public bool ShowDialogBox(string title, string message, string okMessage, string cancelMessage, System.Action<bool> closeAction)
    {
        bool ret = !dialogBox.isShown;
        if (ret)
        {
            dialogBox.Show(title, message, okMessage, cancelMessage, closeAction);
        }
        return ret;
    }

    public bool ShowColorPicker(string title, Color previousColor, System.Action<bool, Color> closeAction)
    {
        bool ret = !colorPicker.isShown;
        if (ret)
        {
            colorPicker.Show(title, previousColor, closeAction);
        }
        return ret;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
