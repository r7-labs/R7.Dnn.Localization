#!/usr/bin/csharp

// already defined:
// using System;
// using System.Collections.Generic;

using System.IO;
using System.Text;
using System.Diagnostics;

class LanguagePackBuilder
{
	#region Parameters

	public string PackageVersion = Environment.GetEnvironmentVariable ("LPB_PACKAGE_VERSION");
	public string CultureCode = Environment.GetEnvironmentVariable ("LPB_CULTURE_CODE");
	public string CultureNameNative = Environment.GetEnvironmentVariable ("LPB_CULTURE_NAME_NATIVE");
	public string PackageType = Environment.GetEnvironmentVariable ("LPB_PACKAGE_TYPE");
	public string PackageName = Environment.GetEnvironmentVariable ("LPB_PACKAGE_NAME");
	public string SourceVersion = Environment.GetEnvironmentVariable ("LPB_SOURCE_VERSION");	
	public string PlatformType = Environment.GetEnvironmentVariable ("LBP_PLATFORM_TYPE");
	
	public string ManifestFileNameTemplate = "R7_${PlatformType}_${PackageType}_${PackageName}_${CultureCode}.dnn";
	public string PackFileNameTemplate = "ResourcePack.R7.${PlatformType}.${PackageType}.${PackageName}.${SourceVersion}-${PackageVersion}.${CultureCode}.zip";

	#endregion

	public void Main ()
	{
		try 
		{
			CreatePackage (GenerateManifest ());
		}
		catch (Exception ex)
		{
			Console.WriteLine (ex.Message);
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

			// copy translations to tmp folder
			
			Console.WriteLine (Directory.GetCurrentDirectory());
			
			Directory.SetCurrentDirectory (Path.Combine (PackageName, CultureCode));
		
			// delete old package
			if (File.Exists (packFileName))
				File.Delete (packFileName);	

			// create package
			var zip = new Process ();
			zip.StartInfo.FileName = "zip";
			zip.StartInfo.Arguments = string.Format (@"-q -r -9 -i \*.resx \*.dnn -UN=UTF8 ""{0}"" .", packFileName);
			zip.Start ();
			zip.WaitForExit ();
			
			// delete manifest file
			File.Delete (manifestFileName);
			
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

		// Extension language pack name
		result = result.Replace ("${ExtensionName}", 
			(PackageType == "Extension")? PackageName + " " : string.Empty);
			
		result = result.Replace ("${CultureCode}", CultureCode);
		result = result.Replace ("${CultureNameNative}", CultureNameNative);
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

	private static string SafeGetEnvironmentVariable (string variableName, string defaultValue)
	{
		var variableValue = Environment.GetEnvironmentVariable (variableName);

		return !string.IsNullOrWhiteSpace (variableValue)? variableValue : defaultValue;
	}
}

new LanguagePackBuilder().Main();





