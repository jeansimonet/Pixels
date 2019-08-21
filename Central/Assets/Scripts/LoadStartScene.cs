using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadStartScene : MonoBehaviour
{
    [SerializeField]
    string _defaultSceneName = "LedAnimator";

    // Start is called before the first frame update
    void Start()
    {
        string scene = AppSettings.StartSceneName;
        if (string.IsNullOrWhiteSpace(scene))
        {
            scene = _defaultSceneName;
        }
        Debug.Log("Loading start scene: " + scene);
        SceneManager.LoadScene(scene);
    }
}
