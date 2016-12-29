#!/usr/bin/csexec

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Diagnostics;

public static class Program
{
	public static void Main (string [] args)
	{
		try {
			Console.WriteLine ("Removing empty entries...");
			var script = new RemoveEmptyEntries () {
				PackageName = args [1],
				CultureCode = args [2].Replace ("_", "-")
			};

			script.Run ();
			Console.WriteLine ("Done.");
		}
		catch (Exception ex) {
			Console.WriteLine (ex.Message);
		}
	}
}

internal class RemoveEmptyEntries
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
				var outFile = file + ".out";
				
				// invoke xlstproc
				var xsltproc = new Process ();
				xsltproc.StartInfo.FileName = "xsltproc";
				xsltproc.StartInfo.Arguments = string.Format (
					"--novalid -o \"{0}.out\" \"../../xslt/remove-empty-entries.xslt\" \"{0}\"", file);
				xsltproc.StartInfo.UseShellExecute = false;
				xsltproc.Start ();
				xsltproc.WaitForExit ();

				if (xsltproc.ExitCode == 0) {
					NormalizeLineEndings (outFile);
					
					// the output file should be shorter than the original one 
					// if something was removed during XSL transformation
					if (new FileInfo (outFile).Length < new FileInfo (file).Length) {
						File.Delete (file);
						File.Move (outFile, file);
					}	
				}
				else {
					Console.WriteLine ("Error processing '{0}' file.", file);
				}

				if (File.Exists (outFile)) {
					File.Delete (outFile);
				}
			}

			Directory.SetCurrentDirectory (Path.Combine ("..", ".."));
		}
		catch (Exception ex) {
			throw ex;
		}
	}

	private void NormalizeLineEndings (string file)
	{
		var lines = File.ReadAllLines (file);
		File.WriteAllText (file, string.Join ("\r\n", lines) + "\r\n");
	}
}
