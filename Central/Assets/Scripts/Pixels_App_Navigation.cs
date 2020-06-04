using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Pixels_App_Navigation : MonoBehaviour
{
    [System.Serializable]
    public struct PageAndToggle
    {
        public RectTransform Page;
        public Toggle Toggle;
    }
    public PageAndToggle[] PagesAndToggles;

    ToggleGroup toggleGroup;
    Dictionary<Toggle, RectTransform> toggleToPageMap;

    // Start is called before the first frame update
    void Awake()
    {
        // Grab the group from the first toggle
        if (PagesAndToggles.Length > 0)
        {
            toggleGroup = PagesAndToggles[0].Toggle.group;

            toggleToPageMap = new Dictionary<Toggle, RectTransform>();
            foreach (var pat in PagesAndToggles)
            {
                toggleToPageMap.Add(pat.Toggle, pat.Page);
                Debug.Assert(pat.Toggle.group == toggleGroup);
                pat.Page.gameObject.SetActive(false);
                pat.Toggle.isOn = false;
            }
            PagesAndToggles[0].Page.gameObject.SetActive(true);
            PagesAndToggles[0].Toggle.isOn = true;
        }
    }

    void Start()
    {
        foreach (var pat in PagesAndToggles)
        {
            pat.Toggle.onValueChanged.AddListener(onToggleChanged);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void onToggleChanged(bool isOn)
    {
        var activeToggle = toggleGroup.ActiveToggles().FirstOrDefault();

        // Turn off all other pages
        foreach (var pat in PagesAndToggles)
        {
            pat.Page.gameObject.SetActive(false);
        }

        // And the new page on
        RectTransform page = null;
        if (toggleToPageMap.TryGetValue(activeToggle, out page))
        {
            page.gameObject.SetActive(true);
        }
    }
}
