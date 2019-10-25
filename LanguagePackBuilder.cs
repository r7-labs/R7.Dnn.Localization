#!/usr/bin/csexec

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;

public static class Program
{
	public static void Main (string [] args)
	{
		try
		{
			var script = new LanguagePackBuilder () {
				PackageName = args [1],
				CultureCode = args [2].Replace ("_", "-")
			};
			script.Run ();
		}
		catch (Exception ex)
		{
			Console.WriteLine (ex.Message);
		}
	}
}

internal class LanguagePackBuilder
{
 	#region Parameters

	public string PackageName { get; set; }
	public string CultureCode { get; set; }
	public string PackageVersion = Environment.GetEnvironmentVariable ("LPB_PACKAGE_VERSION");
	public string PackageType = Environment.GetEnvironmentVariable ("LPB_PACKAGE_TYPE");
	public string SourceVersion = Environment.GetEnvironmentVariable ("LPB_SOURCE_VERSION");
	public string TranslationDir = Environment.GetEnvironmentVariable ("LPB_TRANSLATION_DIR");
	public string PlatformType = Environment.GetEnvironmentVariable ("LBP_PLATFORM_TYPE");
	public string ExtensionPackage = Environment.GetEnvironmentVariable ("LBP_EXTENSION_PACKAGE");
	public string BuildDir = "_build";
	public string ManifestFileNameTemplate = "R7_${PlatformType}_${PackageType}_${PackageName}_${CultureCode}.dnn";
	public string PackFileNameTemplate = "ResourcePack.R7.${PlatformType}.${PackageType}.${PackageName}.${SourceVersion}-${PackageVersion}.${CultureCode}.zip";

	#endregion

	string ScriptDirectory;

	public void Run ()
	{
		try
		{
			ScriptDirectory = Directory.GetCurrentDirectory ();
			CreatePackage (GenerateManifest ());
			Directory.SetCurrentDirectory (ScriptDirectory);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	string GenerateManifest ()
	{
		try
		{
			// TODO: Support for package-specific manifest template
			var manifestTemplate = File.ReadAllText ("manifest_template.xml");

			var startTag = "<languageFile>";
			var endTag = "</languageFile>";

			var begin = manifestTemplate.IndexOf (startTag);
			var end = manifestTemplate.IndexOf (endTag) + endTag.Length;

			// get template for languageFile entries
			var langFileTemplate = manifestTemplate.Substring (begin, end - begin);
            
            // get translation files
			CdToTranslationDirectory ();
			var files = Directory.GetFiles (".", "*." + CultureCode + ".resx", SearchOption.AllDirectories);

			// for better formatting
			var line = 0;

			var langFiles = new StringBuilder (files.Length);
			foreach (var file in files)
			{
				if (line++ > 0)
					langFiles.Append ("\n\t\t\t\t\t\t");

				var langEntry =	langFileTemplate
					.Replace ("${FilePath}", Path.GetDirectoryName(file)
					 	.Remove (0, 2)) // remove "./" prefix
					.Replace ("${FileName}", Path.GetFileName (file));

				langFiles.Append (langEntry);
			}
                
			CdToPackageDirectory ();
            
			var manifest = manifestTemplate;

			// must remove and insert before any other replacement done
			manifest = manifest.Remove (begin, end - begin);
			manifest = manifest.Insert (begin, langFiles.ToString());

			manifest = ReplaceTags (manifest);

			// add license
			if (File.Exists ("license.txt"))
				manifest = manifest.Replace ("${License}", "<license src=\"license.txt\" />");
			else
				manifest = manifest.Replace ("${License}", "<license />");

			// add release notes
			if (File.Exists ("releaseNotes.txt"))
				manifest = manifest.Replace ("${ReleaseNotes}", "<releaseNotes src=\"releaseNotes.txt\" />");
			else
				manifest = manifest.Replace ("${ReleaseNotes}", "<releaseNotes />");
    
			if (!string.IsNullOrEmpty (manifest)) {
                Console.WriteLine ("Package manifest generated.");
            }

			return manifest;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

    void CdToPackageDirectory ()
	{
		Directory.SetCurrentDirectory (ScriptDirectory);
		Directory.SetCurrentDirectory (PackageName);
	}

	void CdToTranslationDirectory ()
	{
		CdToPackageDirectory ();
		Directory.SetCurrentDirectory (GetTranslationDirectory ());
	}

    string GetTranslationDirectory ()
    {
        if (!string.IsNullOrEmpty (TranslationDir)) {
            return TranslationDir;    
        }
        return CultureCode;
    }

	void CdToBuildDirectory ()
	{
		CdToPackageDirectory ();
		Directory.SetCurrentDirectory (BuildDir);
	}

	void CreatePackage (string manifestText)
	{
		try
		{
			CdToPackageDirectory ();
            
            // recreate build dir
            if (Directory.Exists (BuildDir)) {
                Directory.Delete (BuildDir, true);
            }
            Directory.CreateDirectory (BuildDir);

            Console.WriteLine ("Created build directory.");
            
			// TODO: Support more license/releaseNotes formats
			var packFileName = ReplaceTags (PackFileNameTemplate);
			var licenseFileName = "license.txt";
			var releaseNotesFileName = "releaseNotes.txt";

            // create manifest file
			var manifestFileName = ReplaceTags (ManifestFileNameTemplate);
            File.WriteAllText (Path.Combine (BuildDir, manifestFileName), manifestText);

			// copy license and release notes
			if (File.Exists (licenseFileName)) {
				File.Copy (licenseFileName, Path.Combine (BuildDir, licenseFileName));
                Console.WriteLine ("License file copied.");
            }

			if (File.Exists (releaseNotesFileName)) {
				File.Copy (releaseNotesFileName, Path.Combine (BuildDir, releaseNotesFileName));
                Console.WriteLine ("Release notes file copied.");
            }
            
            // copy translation files
            var buildDirAbsolute = Path.GetFullPath (BuildDir);
			CdToTranslationDirectory ();
            var files = Directory.GetFiles (".", "*." + CultureCode + ".resx", SearchOption.AllDirectories);
            foreach (var file in files) {
                CopyFilePreserveHierarchy (file, buildDirAbsolute);
            }

            Console.WriteLine ("Translation files copied.");

			// create package
            CdToBuildDirectory ();
			var zip = new Process ();
			zip.StartInfo.FileName = "zip";
			zip.StartInfo.Arguments = string.Format (@"-q -r -9 -i \*.resx \*.dnn \*.txt -UN=UTF8 ""{0}"" .", packFileName);
			zip.Start ();
			zip.WaitForExit ();
            
            Console.WriteLine ("Language pack created.");
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

    void CopyFilePreserveHierarchy (string srcFile, string destDir)
    {
        var cp = new Process ();
    	cp.StartInfo.FileName = "cp";
        cp.StartInfo.Arguments = string.Format (@"-r --parents ""{0}"" ""{1}""", srcFile, destDir);
		cp.Start ();
		cp.WaitForExit ();
    }

	string ReplaceTags (string template)
	{
		var result = template;

		if (PackageType == "Core")
		{
			result = result.Replace ("${PackageType}.${PackageName}", PackageType);
			result = result.Replace ("${PackageType}_${PackageName}", PackageType);
			result = result.Replace ("${ExtensionPackage}", string.Empty);
			result = result.Replace ("${PackageDescription}", "For DNN Core version " + SourceVersion);
			result = result.Replace ("${Dependencies}",
				"<dependency type=\"CoreVersion\">" + SourceVersion + "</dependency>");
		}
		else
		{
			result = result.Replace ("${ExtensionPackage}", "<package>" + ExtensionPackage + "</package>");
			result = result.Replace ("${PackageDescription}", "For " + ExtensionPackage + " version " + SourceVersion);
			result = result.Replace ("${Dependencies}",
				"<dependency type=\"Package\">" + ExtensionPackage + "</dependency>");
		}
		
		// Extension language pack name
		result = result.Replace ("${ExtensionName}",
			(PackageType == "Extension")? PackageName + " " : string.Empty);

		result = result.Replace ("${CultureCode}", CultureCode);
		result = result.Replace ("${CultureNameNative}", CultureInfo.GetCultureInfoByIetfLanguageTag (CultureCode).NativeName);
		result = result.Replace ("${PackageType}", PackageType);
		result = result.Replace ("${PackageName}", PackageName);
		result = result.Replace ("${PlatformType}", PlatformType);
		result = result.Replace ("${SourceVersion}", SourceVersion);
		result = result.Replace ("${PackageVersion}", PackageVersion);

		return result;
	}

	List<string> RunToLines (string command, string arguments)
	{
		var result = new List<string> ();

		var process = new Process ();
		process.StartInfo.FileName = command;
		process.StartInfo.Arguments = arguments;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;

		try
		{
			process.Start ();
			process.WaitForExit ();

			while (!process.StandardOutput.EndOfStream) {
				result.Add (process.StandardOutput.ReadLine ());
            }
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			process.Close ();
		}

		return result;
	}
}
