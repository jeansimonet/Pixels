using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class AppSettings
{
    public static string StartSceneName
    {
        get
        {
            return PlayerPrefs.GetString(nameof(StartSceneName));
        }
        set
        {
            PlayerPrefs.SetString(nameof(StartSceneName), value);
        }
    }
}
