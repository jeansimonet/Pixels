using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
	// https://gist.github.com/SoylentGraham/bef991c9cd38f9b9c39e549bfcfb05a9
	public static T[] FindObjectsOfTypeIncludingDisabled<T>() where T: Component
	{
		var ActiveScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene ();
		var RootObjects = ActiveScene.GetRootGameObjects ();
		var MatchObjects = new List<T> ();

		foreach (var ro in RootObjects) {
			var Matches = ro.GetComponentsInChildren<T> (true);
			MatchObjects.AddRange (Matches);
		}

		return MatchObjects.ToArray ();
	}

	public static T FindObjectOfTypeIncludingDisabled<T>() where T: Component
	{
		var ActiveScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene ();
		var RootObjects = ActiveScene.GetRootGameObjects ();
		var MatchObjects = new List<T> ();

		foreach (var ro in RootObjects) {
			var Match = ro.GetComponentInChildren<T> (true);
			if (Match != null) return Match;
		}

		return null;
	}
}