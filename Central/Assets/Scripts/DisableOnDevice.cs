using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnDevice : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        if (Application.platform != RuntimePlatform.WindowsEditor)
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
