using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchStartScene : MonoBehaviour
{
    public void SwitchToScene(string sceneName)
    {
        AppSettings.StartSceneName = sceneName;
        Application.Quit();
    }
}