using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoolDieDevOnlyMenuEntry : MonoBehaviour
{
    public Color disabledColor;
    public Text menuText;
    public Image menuIcon;

    // Start is called before the first frame update
    void Awake()
    {
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
        menuText.color = disabledColor;
        menuIcon.color = disabledColor;
        GetComponent<Button>().interactable = false;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
