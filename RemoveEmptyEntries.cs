#!/usr/bin/csexec -r:R7.Scripting.dll

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

public class RemoveEmptyEntries
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
				Console.WriteLine (file);

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
				//	"--novalid -o \"{0}.out\" \"../../xslt/remove-empty-entries.xslt\" \"{0}\"", file);
					"--novalid -o \"{0}\" \"../../xslt/remove-empty-entries.xslt\" \"{0}\"", file);
				xsltproc.StartInfo.UseShellExecute = false;

				xsltproc.Start ();
				xsltproc.WaitForExit ();
			}

			Directory.SetCurrentDirectory (Path.Combine ("..", ".."));
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}
}
