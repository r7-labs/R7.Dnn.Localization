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
		try
		{
			var script = new RemoveEmptyEntries () {
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

internal class RemoveEmptyEntries
{
	#region Parameters

	public string CultureCode { get; set; }
	public string PackageName { get; set; }

	#endregion

	public void Run ()
	{
		try
		{
			// get translation files
			Directory.SetCurrentDirectory (Path.Combine (PackageName, CultureCode));
			var files = Directory.GetFiles (".",
				string.Format ("*.{0}.resx", CultureCode), SearchOption.AllDirectories);

			foreach (var file in files)
			{
				/*
				var doc = new XPathDocument (file);
				var writer = XmlWriter.Create (file + ".out");
				writer.Settings.Indent = true;
				writer.Settings.NewLineHandling = NewLineHandling.None;

				var transform = new XslCompiledTransform ();
				var settings = new XsltSettings ();

				transform.Load("../../xslt/remove-empty-entries.xslt", settings, null);

				transform.Transform (doc, writer);
				*/

				// invoke xlstproc
				var xsltproc = new Process ();
				xsltproc.StartInfo.FileName = "xsltproc";
				xsltproc.StartInfo.Arguments = string.Format (
					"--novalid -o \"{0}.out\" \"../../xslt/remove-empty-entries.xslt\" \"{0}\"", file);
				xsltproc.StartInfo.UseShellExecute = false;
				xsltproc.Start ();
				xsltproc.WaitForExit ();

				if (xsltproc.ExitCode == 0) {
					var diff = new Process ();
					diff.StartInfo.FileName = "diff";
					diff.StartInfo.Arguments = string.Format ("-w \"{0}\" \"{0}.out\"", file);
					diff.StartInfo.UseShellExecute = false;
					diff.Start ();
					diff.WaitForExit ();

					if (diff.ExitCode == 0) {
						// no difference except whitespace, keep original file
						File.Delete (file + ".out");
					}
					else {
						// TODO: Detect comments and schema removal
						// replace original file
						Console.WriteLine ("Empty entries removed from: {0}", file);
						File.Delete (file);
						File.Move (file + ".out", file);
					}
				}
			}

			Directory.SetCurrentDirectory (Path.Combine ("..", ".."));
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}
}
