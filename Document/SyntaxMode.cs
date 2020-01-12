using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ICSharpCode.TextEditor.Document
{
	public class SyntaxMode
	{
		private string fileName;

		private string name;

		private string[] extensions;

		public string[] Extensions
		{
			get
			{
				return this.extensions;
			}
			set
			{
				this.extensions = value;
			}
		}

		public string FileName
		{
			get
			{
				return this.fileName;
			}
			set
			{
				this.fileName = value;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				this.name = value;
			}
		}

		public SyntaxMode(string fileName, string name, string extensions)
		{
			this.fileName = fileName;
			this.name = name;
			char[] chrArray = new char[] { ';', '|', ',' };
			this.extensions = extensions.Split(chrArray);
		}

		public SyntaxMode(string fileName, string name, string[] extensions)
		{
			this.fileName = fileName;
			this.name = name;
			this.extensions = extensions;
		}

		public static List<SyntaxMode> GetSyntaxModes(Stream xmlSyntaxModeStream)
		{
			XmlTextReader xmlTextReader = new XmlTextReader(xmlSyntaxModeStream);
			List<SyntaxMode> syntaxModes = new List<SyntaxMode>();
			while (true)
			{
				if (!xmlTextReader.Read())
				{
					xmlTextReader.Close();
					return syntaxModes;
				}
				if (xmlTextReader.NodeType == XmlNodeType.Element)
				{
					string name = xmlTextReader.Name;
					string str = name;
					if (name == null)
					{
						break;
					}
					if (str == "SyntaxModes")
					{
						string attribute = xmlTextReader.GetAttribute("version");
						if (attribute != "1.0")
						{
							throw new HighlightingDefinitionInvalidException(string.Concat("Unknown syntax mode file defininition with version ", attribute));
						}
					}
					else if (str == "Mode")
					{
						syntaxModes.Add(new SyntaxMode(xmlTextReader.GetAttribute("file"), xmlTextReader.GetAttribute("name"), xmlTextReader.GetAttribute("extensions")));
					}
					else
					{
						break;
					}
				}
			}
			throw new HighlightingDefinitionInvalidException(string.Concat("Unknown node in syntax mode file :", xmlTextReader.Name));
		}

		public override string ToString()
		{
			return string.Format("[SyntaxMode: FileName={0}, Name={1}, Extensions=({2})]", this.fileName, this.name, string.Join(",", this.extensions));
		}
	}
}