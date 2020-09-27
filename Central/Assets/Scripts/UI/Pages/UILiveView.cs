using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Linq;

public class UILiveView
    : UIPage
{
    [Header("Controls")]
    public Transform contentRoot;

    [Header("Prefabs")]
    public UILiveDieEntry dieEntryPrefab;

    // The list of entries we have created to display behaviors
    List<UILiveDieEntry> entries = new List<UILiveDieEntry>();

    void OnEnable()
    {
        base.SetupHeader(true, false, "Live View", null);
    }

    void OnDisable()
    {
        if (AppDataSet.Instance != null) // When quiting the app, it may be null
        {
            foreach (var uientry in entries)
            {
                GameObject.Destroy(uientry.gameObject);
            }
            entries.Clear();
        }
    }

    UILiveDieEntry CreateEntry(Dice.EditDie die, int roll)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UILiveDieEntry>(dieEntryPrefab, Vector3.zero, Quaternion.identity, contentRoot.transform);

        // Initialize it
        ret.Setup(die, roll, Time.time);
        return ret;
    }

}
