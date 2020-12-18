using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnDevice : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
        gameObject.SetActive(false);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
