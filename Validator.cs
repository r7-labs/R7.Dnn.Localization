#!/usr/bin/csexec -r:System.Windows.Forms.dll

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Resources;
using System.Collections;
using System.Collections.Generic;

public static class Program
{
	public static void Main (string [] args)
	{
		try {
			var script = new ResxValidator () {
				PackageName = args [1],
				CultureCode = args [2].Replace ("_", "-")
			};

			script.Run ();
		}
		catch (Exception ex) {
			Console.WriteLine (ex.Message);
		}
	}
}

internal class ResxValidator
{
	#region Parameters

	public string CultureCode { get; set; }
	public string PackageName { get; set; }

	#endregion

	public void Run ()
	{
		try {
			// get source files
			var sourceFiles = Directory.GetFiles (Path.Combine (PackageName, "en-US"), "*.resx", SearchOption.AllDirectories);

			foreach (var sourceFile in sourceFiles) {
				ValidateResxFile (sourceFile, sourceFile
					.Replace (".en-US.resx", ".resx")
					.Replace ("en-US", CultureCode)
					.Replace (".resx", "." + CultureCode + ".resx"));
			}
		}
		catch (Exception ex) {
			throw ex;
		}
	}

	private int ValidateResxFile (string sourceFile, string translationFile)
	{
		if (!File.Exists (translationFile)) {
			Console.WriteLine ("Warning: {0} - the translation file does not exists.", sourceFile);
			return 2;
		}

		var sd = LoadResxFile (sourceFile);
		if (sd == null) {
			Console.WriteLine ("Error: {0} - cannot load source file!", sourceFile);
			return 1;
		}

		var td = LoadResxFile (translationFile);
		if (td == null) {
			Console.WriteLine ("Error: {0} - cannot load translation file!", sourceFile);
			return 1;
		}

		var exitCode = 0;

		foreach (var key in sd.Keys) {
		
			if (!string.IsNullOrWhiteSpace (sd [key]) && !td.ContainsKey (key)) {
				Console.WriteLine ("Warning: {0} {1} - not translated.", sourceFile, key);
				exitCode = 2;
				continue;
			}

			if (!td.ContainsKey (key)) {
				continue;
			}

			if (td [key] == sd [key]) {
				Console.WriteLine ("Warning: {0} {1} - values are the same.", sourceFile, key);
				exitCode = 2;
				continue;
			}
			
			if (!string.IsNullOrWhiteSpace (sd [key]) && string.IsNullOrWhiteSpace (td [key])) {
				Console.WriteLine ("Warning: {0} {1} - translation is blank.", sourceFile, key);
				exitCode = 2;
				continue;
			}
			
			if (!string.IsNullOrWhiteSpace (sd [key]) && !string.IsNullOrWhiteSpace (td [key])) {
				var sourceLastChar = sd [key] [sd [key].Length - 1];
				var translationLastChar = td [key] [td [key].Length - 1];
				if (!char.IsLetter (sourceLastChar) && !char.IsLetter (translationLastChar)) {
					if (sourceLastChar != translationLastChar) {
						Console.WriteLine ("Warning: {0} {1} - different ending chars '{2}' and '{3}'.", 
							sourceFile, key, VisibleWhiteSpace (sourceLastChar), VisibleWhiteSpace (translationLastChar));
						exitCode = 2;
						continue;
					}
				}
			}
		}

		return exitCode;
	}

	private string VisibleWhiteSpace (char c)
	{
		if (c == '\n') {
			return "\\n";
		}

		if (c == '\r') {
			return "\\r";
		}

		if (c == '\t') {
			return "\\t";
		}

		return c.ToString ();
	}

	private Dictionary<string,string> LoadResxFile (string file)
	{
		var dictionary = new Dictionary<string,string> ();
		
		try {
			using (ResXResourceReader resxReader = new ResXResourceReader(file)) {
				foreach (DictionaryEntry entry in resxReader) {
					dictionary.Add ((string) entry.Key, (string) entry.Value);
				}
			}
		}
		catch {
			return null;
		}

		return dictionary;
	}
}
