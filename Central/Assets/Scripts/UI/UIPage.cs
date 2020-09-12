using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPage
    : MonoBehaviour
{

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
        LiveView,
        GradientPatterns,
        AudioClips,
    }

    public bool pageDirty
    {
        get { return _pageDirty; }
        set
        {
            _pageDirty = value;
            NavigationManager.Instance.header.EnableSaveButton(_pageDirty);
        }
    }
    bool _pageDirty;

    public virtual void Enter(object context)
    {
        gameObject.SetActive(true);
    }
    public virtual void OnBack()
    {
        if (pageDirty)
        {
            PixelsApp.Instance.ShowDialogBox(
                "Discard Changes",
                "You have unsaved changes, are you sure you want to discard them?",
                "Discard",
                "Cancel", discard => 
                {
                    if (discard)
                    {
                        // Reload from file
                        AppDataSet.Instance.LoadData();
                        pageDirty = false;
                        NavigationManager.Instance.GoBack();
                    }
                });
        }
        else
        {
            NavigationManager.Instance.GoBack();
        }
    }
    public virtual void OnSave()
    {
        pageDirty = false;
        AppDataSet.Instance.SaveData(); // Not sure about this one!
        NavigationManager.Instance.GoBack();
    }
    public virtual void Leave()
    {
        gameObject.SetActive(false);
    }
    protected void SetupHeader(bool root, bool home, string title, System.Action<string> onTitleChanged)
    {
        NavigationManager.Instance.header.Setup(root, home, pageDirty, title, onTitleChanged);
    }
    protected void EnableSaveButton()
    {
        NavigationManager.Instance.header.EnableSaveButton(true);
    }
}

