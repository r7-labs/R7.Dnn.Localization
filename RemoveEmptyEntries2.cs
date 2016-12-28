#!/usr/bin/csexec -r:System.Windows.Forms.dll

using System;
using System.IO;
using System.Resources;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;

public static class Program
{
	public static void Main (string [] args)
	{
		try {
			var script = new EmptyEntriesRemover () {
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

internal class EmptyEntriesRemover
{
	#region Parameters

	public string CultureCode { get; set; }
	public string PackageName { get; set; }

	#endregion

	public void Run ()
	{
		try {
			// get translation files
			Directory.SetCurrentDirectory (Path.Combine (PackageName, CultureCode));
			var files = Directory.GetFiles (".",
				string.Format ("*.{0}.resx", CultureCode), SearchOption.AllDirectories);

			foreach (var file in files) {
				RemoveEmptyEntries (file);	
			}
		}
		catch (Exception ex) {
			throw ex;
		}
	}

	private void RemoveEmptyEntries (string file)
	{
		try {
			var newFile = file + ".out";
			using (ResXResourceWriter resxWriter = new ResXResourceWriter (newFile)) {
				var resxReader = new ResXResourceReader(file);
				resxReader.UseResXDataNodes = true;
				foreach (DictionaryEntry entry in resxReader) {
					var node = (ResXDataNode) entry.Value;
					if (!string.IsNullOrEmpty (node.GetValue((ITypeResolutionService) null).ToString ())) {
						resxWriter.AddResource (node);
					}
				}
				resxReader.Close ();
			}

			if (new FileInfo (newFile).Length < new FileInfo (file).Length) {
				File.Delete (file);
				File.Move (newFile, file);
			}
			else {
				File.Delete (newFile);
			}
		}
		catch (Exception ex) {
			Console.WriteLine ("{0}: {1}", file, ex.Message);
		}
	}
}
