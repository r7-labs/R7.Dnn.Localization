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

	public string PackVersion = SafeGetEnvironmentVariable ("LPB_PACKVERSION", "0");

	public string CultureCode = "ru-RU";
	public string CultureNameNative = "Русский (Россия)";
	public string PackType = "Core"; // DNN supports "Core" and "Extension", but not "Full" anymore
	public string PlatformType = "DNNCE";
	public string Version = "07.01.02";
	public string ManifestFileNameTemplate = "R7_${PlatformType}_${CultureCode}.dnn";
	public string PackFileNameTemplate = "ResourcePack.R7.${PlatformType}.${PackType}.${Version}-${PackVersion}.${CultureCode}.zip";

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

			// run git ls-files
			var gitFiles = RunToLines ("git", "ls-files");

			// for better formatting
			var line = 0;

			var langFiles = new StringBuilder (gitFiles.Count);
			foreach (var file in gitFiles)
			{
				if (Path.GetExtension (file).ToLowerInvariant () == ".resx")
				{
					if (line++ > 0)
						langFiles.Append ("\n\t\t\t\t\t\t");

					var langEntry =	langFileTemplate
						.Replace ("${FilePath}", Path.GetDirectoryName(file)
						 	.Remove (0, CultureCode.Length + 1)) // remove "xx-YY/" prefix
						.Replace ("${FileName}", Path.GetFileName (file));

					langFiles.Append (langEntry);
				}
			}

			var manifest = manifestTemplate;

			// must remove and insert before any other replacement done
			manifest = manifest.Remove (begin, end - begin);
			manifest = manifest.Insert (begin, langFiles.ToString());

			manifest = ReplaceTags (manifest);

			var manifestFileName = Path.Combine (CultureCode, ReplaceTags (ManifestFileNameTemplate));
		
			File.WriteAllText (manifestFileName, manifest);

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
			var packFileName = ReplaceTags (PackFileNameTemplate);

			// delete old tmp folder
			if (Directory.Exists ("tmp"))
				Directory.Delete ("tmp", true);	

			// clone repository to local folder
			// to get rid of untracked files
			var git = new Process ();
			git.StartInfo.FileName = "git";
			git.StartInfo.Arguments = "clone . tmp";
			git.Start ();
			git.WaitForExit ();

			// copy manifest to tmp folder
			File.Copy (manifestFileName, Path.Combine ("tmp", manifestFileName));

			// switch to tmp folder
			Directory.SetCurrentDirectory (Path.Combine ("tmp", CultureCode));
		
			// delete old package
			if (File.Exists (packFileName))
				File.Delete (packFileName);	

			// create package
			var zip = new Process ();
			zip.StartInfo.FileName = "zip";
			zip.StartInfo.Arguments = string.Format (@"-q -r -9 -i \*.resx \*.dnn -UN=UTF8 ""{0}"" .", packFileName);
			zip.Start ();
			zip.WaitForExit ();

			// copy package file to original folder
			File.Copy (packFileName, Path.Combine ("..", "..", packFileName), true);
			
			Directory.SetCurrentDirectory (Path.Combine ("..", ".."));
			Directory.Delete ("tmp", true);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private string ReplaceTags (string template)
	{
		var result = template;

		result = result.Replace ("${CultureCode}", CultureCode);
		result = result.Replace ("${CultureNameNative}", CultureNameNative);
		result = result.Replace ("${PackType}", PackType);
		result = result.Replace ("${PlatformType}", PlatformType);
		result = result.Replace ("${Version}", Version);
		result = result.Replace ("${PackVersion}", PackVersion);
	
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





