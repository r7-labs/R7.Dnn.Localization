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

	public const string CultureCode = "ru-RU";
	public const string CultureNameNative = "Русский (Россия)";
	public const string PackType = "Core"; // DNN supports "Core" and "Extension", but not "Full" anymore
	public const string PlatformType = "DNNCE";
	public const string Version = "07.01.02";
	public const string PackVersion = "1";
	public const string ManifestFileNameTemplate = "R7_${PlatformType}_${CultureCode}.dnn";
	public const string PackFileNameTemplate = "ResourcePack.R7.${PlatformType}.${PackType}.${Version}-${PackVersion}.${CultureCode}.zip";

	#endregion

	public void Main ()
	{
		try 
		{
			GenerateManifest ();
			CreatePackage ();
		}
		catch (Exception ex)
		{
			Console.WriteLine (ex.Message);
		}
	}

	private void GenerateManifest ()
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

			var manifestFileName = ReplaceTags (ManifestFileNameTemplate);
		
			File.WriteAllText (Path.Combine (CultureCode, manifestFileName), manifest);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void CreatePackage ()
	{
		try
		{ 
			Directory.SetCurrentDirectory (CultureCode);
			
			var packFileName = ReplaceTags (PackFileNameTemplate);
		
			// delete old package
			if (File.Exists (packFileName))
				File.Delete (packFileName);	

			var zip = new Process ();
			zip.StartInfo.FileName = "zip";
			zip.StartInfo.Arguments = string.Format (
				@"-q -r -9 -i \*.resx \*.dnn -UN=UTF8 ""{0}"" .", packFileName);

			zip.Start ();
			zip.WaitForExit ();

			Directory.SetCurrentDirectory ("..");
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
}

new LanguagePackBuilder().Main();





