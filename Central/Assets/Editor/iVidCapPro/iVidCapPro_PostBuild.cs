// iVidCapPro Copyright (c) 2012-2013 James Allen and eccentric Orbits entertainment (eOe)
//
/*
Permission is hereby granted, free of charge, to any person or organization obtaining a copy of 
the software and accompanying documentation covered by this license (the "Software") to use
and prepare derivative works of the Software, for commercial or other purposes, excepting that the Software
may not be repackaged for sale as a Unity asset.

The copyright notices in the Software and this entire statement, including the above license grant, 
this restriction and the following disclaimer, must be included in all copies of the Software, 
in whole or in part.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT. IN NO EVENT SHALL 
THE COPYRIGHT HOLDERS OR ANYONE DISTRIBUTING THE SOFTWARE BE LIABLE FOR ANY DAMAGES OR OTHER LIABILITY, WHETHER 
IN CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
IN THE SOFTWARE.
*/

/* ---------------------------------------------------------------------
 	-- iVidCapPro_PostBuild --
 	
 	This class performs the post build processing necessary to modify
 	the Unity generated Xcode project to work with iVidCapPro.
   
   Change History:
   
   - 23 Mar 13 - Created.
   - 10 Aug 13 - Update for Unity 4.2.  AppController.mm file renamed
   				 to UnityAppController.mm.
   - 28 Jun 14 - Update for Unity 4.5. 
   - 08 Nov 14 - Update for Unity 5.0.
   - 15 Feb 15 - Update for Unity 4.6.2. Same fix as for Unity 5.0.
   				 Use new UnityGetMainScreenContextGLES function to fetch
   				 the GL context. Change is necessary as of 4.6.2p2 to be
   				 compatible with new Metal graphics backend.
   --------------------------------------------------------------------- */

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Text.RegularExpressions;


public class iVidCapPro_PostBuild {
		
	[PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
        UnityEngine.Debug.Log("iVidCapPro_PostBuild processing starts... Unity Version = " 
			+ Application.unityVersion);
	
		// Set the name of the Unity AppController source file.
		string appControllerName = "UnityAppController.mm";
		
		string appControllerPath = pathToBuiltProject + Path.DirectorySeparatorChar + "Classes" +
			Path.DirectorySeparatorChar + appControllerName;
		string appControllerBackupPath = pathToBuiltProject + Path.DirectorySeparatorChar + "Classes" +
			Path.DirectorySeparatorChar + appControllerName + "_pre_ivcp_edit";
		
		// The regular expression pattern to be used for finding the place in AppController
		// that we want to insert our get context function.
		string insertionPointPattern = "// --- AppController";
		
		// The definition of the get context function for Unity versions prior to 4.1.
		string contextFunction_1 = 
			"// Added for use by iVidCapPro.\n" +
			"extern \"C\" EAGLContext* ivcp_UnityGetContext()\n" +
            "{\n" +
   			"    return _context;\n" +
            "}\n\n";
		
		// The definition of the get context function for Unity versions 4.1 -> 4.3.
		string contextFunction_2 = 
			"// Added for use by iVidCapPro.\n" +
			"extern \"C\" EAGLContext* ivcp_UnityGetContext()\n" +
            "{\n" +
   			"    return _mainDisplay->surface.context;\n" +
            "}\n\n";

		// The definition of the get context function for Unity versions 4.5 -> 4.6.2.
		string contextFunction_3 = 
			"\n" +
			"// Added for use by iVidCapPro.\n" +
			"extern \"C\" EAGLContext* ivcp_UnityGetContext()\n" +
			"{\n" +
			"	DisplayConnection* display = GetAppController().mainDisplay;\n" +
			"	return display->surface.context;\n" +
			"}\n\n";

		// The definition of the get context function for Unity versions 4.6 -> ?.
		string contextFunction_4 = 
			"\n" +
				"// Added for use by iVidCapPro.\n" +
				"extern \"C\" EAGLContext* ivcp_UnityGetContext()\n" +
				"{\n" +
				"	return UnityGetMainScreenContextGLES();\n" +
				"}\n\n";
		
		// Make a backup copy of the AppController.
		File.Copy(appControllerPath, appControllerBackupPath, true);
		if (!File.Exists(appControllerBackupPath)) {
			UnityEngine.Debug.LogError("iVidCapPro_PostBuild ERROR: Backup of the original " + appControllerName + " could not be created.");
			return;
		} else {
			UnityEngine.Debug.Log("iVidCapPro_PostBuild: Backup of the original " + appControllerName + " file created as: " + 
				Path.GetFileName(appControllerBackupPath));
		}
		
		// Read App Controller file into a string.
		string fileString = "";
		if (!ReadFileIntoString(appControllerPath, out fileString)) {
			// Read failed...
			UnityEngine.Debug.LogError("iVidCapPro_PostBuild ERROR: Could not read file " + appControllerName + ".");
			return;
		}
		
		// Update AppController data string.
		// Set the substitution string based on the Unity Version.
		string substString = contextFunction_4;
			
		// Check to see if AppController already contains our function.
	    if (!Regex.IsMatch(fileString, @"ivcp_UnityGetContext")) {
			// For 4.5 and later, just add our function at the end of the file.
			fileString += substString;

			// Write modified AppController back to file.
			WriteStringIntoFile(fileString, appControllerPath);
		} else {
			// The function is already present.  No action needed.
			UnityEngine.Debug.Log("iVidCapPro_PostBuild: ivcp_UnityGetContext function already present. Nothing done."); 
		}
		
		UnityEngine.Debug.Log("iVidCapPro_PostBuild processing completed successfully.");
		
		return;
	}
	
	private static bool ReadFileIntoString(string filePath, out string stringData) {
		
		bool result = true;
		stringData = "";
		if (File.Exists(filePath)) {
			stringData = File.ReadAllText(filePath);
		} else {
			result = false;
		}
		return result;
	}
	
	private static bool WriteStringIntoFile(string stringData, string filePath) {
		
		File.WriteAllText(filePath, stringData);
		return true;
	}
}
