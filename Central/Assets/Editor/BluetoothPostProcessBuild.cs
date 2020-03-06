#if UNITY_IOS
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using System.IO;

public class BluetoothPostProcessBuild
{
	[PostProcessBuild]
	public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
	{
		if (buildTarget == BuildTarget.iOS)
		{
			// Get plist
			string plistPath = pathToBuiltProject + "/Info.plist";
			PlistDocument plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(plistPath));

			// Get root
			PlistElementDict rootDict = plist.root;

            rootDict.SetString("NSBluetoothAlwaysUsageDescription", "Uses BLE to communicate with Pixels dice.");
            rootDict.SetString("NSBluetoothPeripheralUsageDescription", "Uses BLE to communicate with Pixels dice.");

			// Write to file
			File.WriteAllText(plistPath, plist.WriteToString());
		}
	}
}
#endif
