#!/usr/bin/csexec

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
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
	public string PlatformType = Environment.GetEnvironmentVariable ("LBP_PLATFORM_TYPE");
	public string ExtensionAssembly = Environment.GetEnvironmentVariable ("LBP_EXTENSION_ASSEMBLY");

	public string ManifestFileNameTemplate = "R7_${PlatformType}_${PackageType}_${PackageName}_${CultureCode}.dnn";
	public string PackFileNameTemplate = "ResourcePack.R7.${PlatformType}.${PackageType}.${PackageName}.${SourceVersion}-${PackageVersion}.${CultureCode}.zip";

	#endregion

	private Dictionary<string,string> NativeCultureNames;

	public LanguagePackBuilder ()
	{
		NativeCultureNames = new Dictionary<string,string> ();
		NativeCultureNames.Add ("en-US", "English (United States)");
		NativeCultureNames.Add ("ru-RU", "Русский (Россия)");
	}

	public void Run ()
	{
		try
		{
			CreatePackage (GenerateManifest ());
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private string GenerateManifest ()
	{
		try
		{
			var manifestTemplate = File.ReadAllText ("manifest_template.xml");

			var startTag = "<languageFile>";
			var endTag = "</languageFile>";

			var begin = manifestTemplate.IndexOf (startTag);
			var end = manifestTemplate.IndexOf (endTag) + endTag.Length;

			// get template for languageFile entries
			var langFileTemplate = manifestTemplate.Substring (begin, end - begin);

			// get translation files
			Directory.SetCurrentDirectory (Path.Combine (PackageName, CultureCode));
			var files = Directory.GetFiles (".", "*.ru-RU.resx", SearchOption.AllDirectories);

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

			var manifest = manifestTemplate;

			// must remove and insert before any other replacement done
			manifest = manifest.Remove (begin, end - begin);
			manifest = manifest.Insert (begin, langFiles.ToString());

			manifest = ReplaceTags (manifest);

			// add license
			if (File.Exists (Path.Combine ("..", "license.txt")))
				manifest = manifest.Replace ("${License}", "<license src=\"license.txt\" />");
			else
				manifest = manifest.Replace ("${License}", "<license />");

			// add release notes
			if (File.Exists (Path.Combine ("..", "releaseNotes.txt")))
				manifest = manifest.Replace ("${ReleaseNotes}", "<releaseNotes src=\"releaseNotes.txt\" />");
			else
				manifest = manifest.Replace ("${ReleaseNotes}", "<releaseNotes />");

			var manifestFileName = ReplaceTags (ManifestFileNameTemplate);

			File.WriteAllText (manifestFileName, manifest);

			Directory.SetCurrentDirectory (Path.Combine ("..", ".."));

			return manifestFileName;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void CreatePackage (string manifestFileName)
	{
		try
		{
			var packFileName = Path.Combine ("..", ReplaceTags (PackFileNameTemplate));
			var licenseFileName = "license.txt";
			var releaseNotesFileName = "releaseNotes.txt";

			Console.WriteLine (Directory.GetCurrentDirectory());

			Directory.SetCurrentDirectory (Path.Combine (PackageName, CultureCode));

			// delete old package
			if (File.Exists (packFileName))
				File.Delete (packFileName);

			// copy license and release notes
			if (File.Exists (Path.Combine ("..", licenseFileName)))
				File.Copy (Path.Combine ("..", licenseFileName), licenseFileName);

			if (File.Exists (Path.Combine ("..", releaseNotesFileName)))
				File.Copy (Path.Combine ("..", releaseNotesFileName), releaseNotesFileName);

			// create package
			var zip = new Process ();
			zip.StartInfo.FileName = "zip";
			zip.StartInfo.Arguments = string.Format (@"-q -r -9 -i \*.resx \*.dnn \*.txt -UN=UTF8 ""{0}"" .", packFileName);
			zip.Start ();
			zip.WaitForExit ();

			// delete manifest and related files
			File.Delete (manifestFileName);
			File.Delete (licenseFileName);
			File.Delete (releaseNotesFileName);

			Directory.SetCurrentDirectory (Path.Combine ("..", ".."));
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private string ReplaceTags (string template)
	{
		var result = template;

		// "Core[\._]Core" => "Core"
		if (PackageType == PackageName)
		{
			result = result.Replace ("${PackageType}.${PackageName}", PackageName);
			result = result.Replace ("${PackageType}_${PackageName}", PackageName);
		}

		if (PackageType == "Extension")
		{
			result = result.Replace ("${ExtensionAssembly}", "<package>" + ExtensionAssembly + "</package>");
		}
		else
		{
			result = result.Replace ("${ExtensionAssembly}", string.Empty);
		}

		// Extension language pack name
		result = result.Replace ("${ExtensionName}",
			(PackageType == "Extension")? PackageName + " " : string.Empty);

		result = result.Replace ("${CultureCode}", CultureCode);
		result = result.Replace ("${CultureNameNative}", NativeCultureNames [CultureCode]);
		result = result.Replace ("${PackageType}", PackageType);
		result = result.Replace ("${PackageName}", PackageName);
		result = result.Replace ("${PlatformType}", PlatformType);
		result = result.Replace ("${SourceVersion}", SourceVersion);
		result = result.Replace ("${PackageVersion}", PackageVersion);

		return result;
	}

	private List<string> RunToLines (string command, string arguments)
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

			while (!process.StandardOutput.EndOfStream)
				result.Add (process.StandardOutput.ReadLine ());
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
