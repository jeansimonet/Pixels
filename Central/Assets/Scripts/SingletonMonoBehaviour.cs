using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T: MonoBehaviour
{
	static T _instance;

	public static T Instance => GetInstance();

	public static bool HasInstance => _instance != null;

	static T GetInstance()
	{
        if (_instance == null)
        {
			_instance = Utils.FindObjectOfTypeIncludingDisabled<T>();
        }
        return _instance;
	}

	protected virtual void OnDestroy()
	{
		_instance = null;
	}
}
