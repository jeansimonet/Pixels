using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FileUtils
{
	public static string GetPath()
	{
		if (Application.isMobilePlatform || Application.isConsolePlatform)
			return Application.persistentDataPath;
		else // For standalone player or editor
			return "file://" + Application.persistentDataPath + "/";
	}
}
