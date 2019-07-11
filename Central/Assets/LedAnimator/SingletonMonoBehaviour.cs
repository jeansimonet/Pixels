using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T: class
{
	static T _instance;

	public static T Instance => GetInstance();

	static T GetInstance()
	{
        if (_instance == null)
        {
    		_instance = Object.FindObjectOfType<Canvas>().rootCanvas.GetComponentInChildren<T>(includeInactive: true);
        }
        return _instance;
	}

	void OnDestroy()
	{
		_instance = null;
	}
}
