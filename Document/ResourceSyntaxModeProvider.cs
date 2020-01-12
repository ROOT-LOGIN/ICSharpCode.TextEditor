using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace ICSharpCode.TextEditor.Document
{
	public class ResourceSyntaxModeProvider : ISyntaxModeFileProvider
	{
		private List<SyntaxMode> syntaxModes;

		public ICollection<SyntaxMode> SyntaxModes
		{
			get
			{
				return this.syntaxModes;
			}
		}

		public ResourceSyntaxModeProvider()
		{
            Stream manifestResourceStream = typeof(SyntaxMode).Assembly.GetManifestResourceStream("BigBug.ICSharpCode.TextEditor.xshd.SyntaxModes.xml");
			if (manifestResourceStream == null)
			{
				this.syntaxModes = new List<SyntaxMode>();
				return;
			}
			this.syntaxModes = SyntaxMode.GetSyntaxModes(manifestResourceStream);
		}

		public XmlTextReader GetSyntaxModeFile(SyntaxMode syntaxMode)
		{
			Assembly assembly = typeof(SyntaxMode).Assembly;
			return new XmlTextReader(assembly.GetManifestResourceStream(string.Concat("BigBug.ICSharpCode.TextEditor.xshd.", syntaxMode.FileName)));
		}

		public void UpdateSyntaxModeList()
		{
		}
	}
}