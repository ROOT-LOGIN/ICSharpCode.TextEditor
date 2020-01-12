using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ICSharpCode.TextEditor.Document
{
	public class FileSyntaxModeProvider : ISyntaxModeFileProvider
	{
		private string directory;

		private List<SyntaxMode> syntaxModes;

		public ICollection<SyntaxMode> SyntaxModes
		{
			get
			{
				return this.syntaxModes;
			}
		}

		public FileSyntaxModeProvider(string directory)
		{
			this.directory = directory;
			this.UpdateSyntaxModeList();
		}

		public XmlTextReader GetSyntaxModeFile(SyntaxMode syntaxMode)
		{
			string str = Path.Combine(this.directory, syntaxMode.FileName);
			if (!File.Exists(str))
			{
				throw new HighlightingDefinitionInvalidException(string.Concat("Can't load highlighting definition ", str, " (file not found)!"));
			}
			return new XmlTextReader(File.OpenRead(str));
		}

		private List<SyntaxMode> ScanDirectory(string directory)
		{
			string[] files = Directory.GetFiles(directory);
			List<SyntaxMode> syntaxModes = new List<SyntaxMode>();
			string[] strArrays = files;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				if (Path.GetExtension(str).Equals(".XSHD", StringComparison.OrdinalIgnoreCase))
				{
					XmlTextReader xmlTextReader = new XmlTextReader(str);
					while (xmlTextReader.Read())
					{
						if (xmlTextReader.NodeType != XmlNodeType.Element)
						{
							continue;
						}
						string name = xmlTextReader.Name;
						if (name == null || !(name == "SyntaxDefinition"))
						{
							throw new HighlightingDefinitionInvalidException(string.Concat("Unknown root node in syntax highlighting file :", xmlTextReader.Name));
						}
						string attribute = xmlTextReader.GetAttribute("name");
						string attribute1 = xmlTextReader.GetAttribute("extensions");
						syntaxModes.Add(new SyntaxMode(Path.GetFileName(str), attribute, attribute1));
						break;
					}
					xmlTextReader.Close();
				}
			}
			return syntaxModes;
		}

		public void UpdateSyntaxModeList()
		{
			string str = Path.Combine(this.directory, "SyntaxModes.xml");
			if (!File.Exists(str))
			{
				this.syntaxModes = this.ScanDirectory(this.directory);
				return;
			}
			Stream stream = File.OpenRead(str);
			this.syntaxModes = SyntaxMode.GetSyntaxModes(stream);
			stream.Close();
		}
	}
}