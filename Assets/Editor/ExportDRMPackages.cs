//
// This script adds menu items to the editor in "Menu->Assets->PSVita" which can be used to build and install a
// PSVita DRM content package containing an asset bundle created from the selected objects in the project view.
//
// The script demonstrates how to create a download-able content package that can be used in
// the Playstation Store, purchased, installed, and then loaded into your application.
//
// NOTES:
//
// Content ID
//
// Each content package requires a unique ContentID, the contentID comprises the serviceID of the application
// that the content will be used with plus a unique 16 character content label.
// For example...
//
// ED1633-NPXB01864_00-ROCK100000000000 where ED1633-NPXB01864_00 is the serviceID of the application and ROCK100000000000 is the unique content label.
//
// Pass-code
//
// Each content package must use the same 32 character pass code as the application package that the content will be used with.
//

using UnityEngine;
using UnityEditor;
using UnityEditor.PSP2;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class ExportDRMPackages
{
	static string serviceID = "ED1633-NPXB01864_00";				// Must match the serviceID of the application that the content will be used with.
	static string passcode = "7Y8mR4qlSL6qQ7IlF9z2wurxxZvZjhpt";	// Must match the application package's pass-code. We could get this from PlayerSettings.PSVita.packagePassword
	static string stagingArea = "Temp/DRMContent";					// Temp folder for placing files while building the package.

	// Structure defining a DRM package and it's contents.
	struct DRMPackageDesc
	{
		public DRMPackageDesc(string title, string label, string[] bundlePaths, string[] rawPaths, bool free)
		{
			contentTitle = title;
			contentLabel = label;
			bundleAssetPaths = bundlePaths;
			rawAssetPaths = rawPaths;
			isFree = free;
		}
		public string contentTitle;			// The content packages title.
		public string contentLabel;			// Unique content label, corresponds to the directory that the content will be installed in.
		public string[] bundleAssetPaths;	// Array of assets that will be added to asset bundles, one bundle per asset (+ dependencies).
		public string[] rawAssetPaths;		// Array of assets that will be added to the package but not bundled, typically metadata of some kind.
		public bool isFree;					// Will the content be free or paid-for ?
	}

	static DRMPackageDesc []packages =
	{
		new DRMPackageDesc("Rock1", "ROCK100000000000", new string[] { "Assets/DRMContent/Rock1.prefab" }, new string[] { "Assets/DRMContent/Rock1_desc.txt" }, false ),
		new DRMPackageDesc("Rock2", "ROCK200000000000", new string[] { "Assets/DRMContent/Rock2.prefab" }, new string[] { "Assets/DRMContent/Rock2_desc.txt" }, false ),
	};
	
	[MenuItem("PSVita Tools/Export DRM Content Packages")]
	static void ExportDRMContentPackages()
	{
		if (!VitaSDKTools.CheckSDKToolsExist(false))
		{
			Debug.LogError("PSVita SDK tools not found\n");
			return;
		}

		string packagePath = EditorUtility.OpenFolderPanel("Select Package Save Location", "", "");
		if (packagePath.Length == 0)
		{
			return;
		}

		FileUtil.DeleteFileOrDirectory(stagingArea);
		Directory.CreateDirectory(stagingArea);

		foreach(DRMPackageDesc desc in packages)
		{
			string[] bundles = BuildAssetBundles(desc.bundleAssetPaths);
			string[] assets = CopyRawAssets(desc.rawAssetPaths);

			string contentID = serviceID + "-" + desc.contentLabel;
			CreateDRMPackage(desc.contentTitle, contentID, passcode, desc.isFree, bundles, assets, packagePath);
		}
	}
	
	[MenuItem("PSVita Tools/Install DRM Content Package")]
	static void InstallDRMContentPackage()
	{
		if (!VitaSDKTools.CheckSDKToolsExist(false))
		{
			Debug.LogError("PSVita SDK tools not found\n");
			return;
		}

		string packagePath = EditorUtility.OpenFilePanel("Select Package To Install", "", "pkg");
		if (packagePath.Length == 0)
		{
			return;
		}

		InstallDRMPackage(packagePath);
	}

	// Create a DRM content package.
	//
	// string contentTitle		The content packages title/name.
	// string contentID			The content packages unique contentID, incorporating the serviceID of the app that the content will be used with.
	// string passcode			The content packages 32 character pass-code, this must match the application packages pass-code
	// bool isFree				Will the content be a free or paid-for download.
	// string[] bundles			Array of asset bundle file names that will be added to the package
	// string[] rawAssets		Array of raw asset file names that will be added to the package
	// string packagePath		The path where the package will be saved.
	//
	static void CreateDRMPackage(string contentTitle, string contentID, string passcode, bool isFree, string[] bundles, string[] rawAssets, string packagePath)
	{
		string stagingAreaSceSys = stagingArea + "/sce_sys";
		string paramSfx = stagingArea + "/param.sfx";
		string paramSfo = stagingAreaSceSys + "/param.sfo";

		Directory.CreateDirectory(stagingArea);
		Directory.CreateDirectory(stagingAreaSceSys);

		// Build the param.sfo
		if (!BuildParamFile(contentTitle, contentID, paramSfx, paramSfo))
		{
			Debug.LogError("Failed building param.sfo!\n");
			return;
		}

		List<string> fileNames = new List<string>();
		fileNames.Add(paramSfo);
		foreach (string file in bundles)
		{
			fileNames.Add(file);
		}

		foreach (string file in rawAssets)
		{
			fileNames.Add(file);
		}

		// Build the package
		if (!BuildPackage(stagingArea, fileNames.ToArray(), contentID, passcode, isFree, packagePath))
		{
			Debug.LogError("Failed building package!\n");
			return;
		}
	}
	
	static bool BuildParamFile(string contentTitle, string contentID, string paramSfx, string paramSfo)
	{
		StringBuilder errors = new System.Text.StringBuilder("");
		
		if (!VitaSDKTools.ValidateContentID(contentID))
		{
			Debug.LogError("Invalid contend ID: " + contentID + "\n");
			return false;
		}

		ParamFile sfxParams = new ParamFile();
		sfxParams.SetInt("ATTRIBUTE", 0);
		sfxParams.Set("CATEGORY", "ac");
		sfxParams.Set("CONTENT_ID", contentID);
		sfxParams.Set("TITLE", contentTitle);
		sfxParams.Set("TITLE_ID", contentID.Substring(7, 9));
		sfxParams.Set("VERSION", "01.00");
		sfxParams.Write(paramSfx);

		var psi = new System.Diagnostics.ProcessStartInfo();
		psi.FileName = VitaSDKTools.GetTool("psp2pubcmd");
		psi.UseShellExecute = false;
		psi.CreateNoWindow = true;
		psi.WorkingDirectory = Directory.GetCurrentDirectory();
		psi.RedirectStandardError = true;

		if(!VitaSDKTools.RunCommand2(psi, string.Format("-sc \"{0}\" \"{1}\"", paramSfx, paramSfo), errors, null))
		{
			LogErrors("Failed to build param.sfo file...\n", errors);
			return false;
		}

		if (!File.Exists(paramSfo))
		{
			return false;
		}
		
		return true;
	}

	static bool BuildPackage(string stagingArea, string[] files, string contentID, string passcode, bool isFree, string packagePath)
	{
		var psi = new System.Diagnostics.ProcessStartInfo();
		psi.FileName = VitaSDKTools.GetTool("psp2pubcmd");
		psi.UseShellExecute = false;
		psi.CreateNoWindow = true;
		psi.WorkingDirectory = Directory.GetCurrentDirectory();
		psi.RedirectStandardError = true;

		string package = packagePath + "/" + contentID + ".pkg";
		string project = Path.Combine(stagingArea, "project.gp4p");
		string drmType = isFree ? "Free" : "Local";
		string masterVersion = "01.00";
		string projectType = "psp2_addcont";
		StringBuilder errors = new System.Text.StringBuilder("");

		// Delete existing package.
		File.Delete(package);

		// Generate the gp4p project.
		if(!VitaSDKTools.RunCommand2(psi, string.Format("-gc --proj_type {0} --content_id {1} --pub_ver {2} --drm_type {3} --passcode {4} {5}",
						projectType, contentID, masterVersion, drmType, passcode, project), errors, null))
		{
			LogErrors("Failed to build package project...\n", errors);
			return false;
		}

		// Add the files.
		foreach (string file in files)
		{
			string strippedFile = file.Substring(stagingArea.Length);
			if(!VitaSDKTools.RunCommand2(psi, string.Format("-gfa \"{0}\" {1} {2}", file, strippedFile, project), errors, null))
			{
				LogErrors("Failed to add files to package project...\n", errors);
				return false;
			}
		}

		// Build the package.
		if (!VitaSDKTools.RunCommand2(psi, string.Format("-c \"{0}\" \"{1}\"", project, package), errors, null))
		{
			LogErrors("Failed to build package...\n", errors);
			return false;
		}
		
		return true;
	}

	static void InstallDRMPackage(string packageName)
	{
		StringBuilder errors = new System.Text.StringBuilder("");
		var psi = new System.Diagnostics.ProcessStartInfo();
		psi.FileName = VitaSDKTools.GetTool("psp2ctrl");
		psi.UseShellExecute = false;
		psi.CreateNoWindow = true;
		psi.WorkingDirectory = Directory.GetCurrentDirectory();
		psi.RedirectStandardError = true;
		EditorUtility.DisplayProgressBar("Installing DRM Package", "", 0.5F);
		if (!VitaSDKTools.RunCommand2(psi, string.Format("pkg-install \"{0}\"", packageName), errors, null))
		{
			LogErrors("Failed to install package...\n", errors);
		}
		EditorUtility.ClearProgressBar();
	}

	static string[] BuildAssetBundles(string[] assetPaths)
	{
		string[] bundles = new string[assetPaths.Length];
		Object[] objects = new Object[1];

		for (int i = 0; i < assetPaths.Length; i++)
		{
			bundles[i] = stagingArea + "/" + Path.GetFileNameWithoutExtension(assetPaths[i]) + ".unity3d";
			objects[0] = AssetDatabase.LoadAssetAtPath(assetPaths[i], typeof(Object));
			if (objects[0] == null)
			{
				Debug.LogError("Failed to build asset bundle, asset not found: " + assetPaths[i] + "\n");
				return null;
			}

			BuildPipeline.BuildAssetBundle(objects[0], objects, bundles[i],
								BuildAssetBundleOptions.UncompressedAssetBundle |
								BuildAssetBundleOptions.CollectDependencies |
								BuildAssetBundleOptions.CompleteAssets,
								BuildTarget.PSP2);
		}

		return bundles;
	}

	static string[] CopyRawAssets(string[] assetPaths)
	{
		string[] assets = new string[assetPaths.Length];

		for (int i = 0; i < assetPaths.Length; i++)
		{
			assets[i] = stagingArea + "/" + Path.GetFileName(assetPaths[i]);
			File.Copy(assetPaths[i], assets[i]);
		}

		return assets;
	}

	static void LogErrors(string heading, StringBuilder details)
	{
		string message = heading;
		string[] lines = details.ToString().Split('\n');
		foreach (string line in lines)
		{
			if (line.ToLower().Contains("error"))
			{
				message += line + "\n";
			}
		}
		Debug.LogError(message);
	}

}
