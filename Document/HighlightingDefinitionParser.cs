using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace ICSharpCode.TextEditor.Document
{
	public static class HighlightingDefinitionParser
	{
		public static DefaultHighlightingStrategy Parse(SyntaxMode syntaxMode, XmlReader xmlReader)
		{
			return HighlightingDefinitionParser.Parse(null, syntaxMode, xmlReader);
		}

		public static DefaultHighlightingStrategy Parse(DefaultHighlightingStrategy highlighter, SyntaxMode syntaxMode, XmlReader xmlReader)
		{
			DefaultHighlightingStrategy defaultHighlightingStrategy;
			HighlightColor highlightBackground;
			HighlightColor highlightColor;
			if (syntaxMode == null)
			{
				throw new ArgumentNullException("syntaxMode");
			}
			if (xmlReader == null)
			{
				throw new ArgumentNullException("xmlTextReader");
			}
			try
			{
				List<ValidationEventArgs> validationEventArgs = null;
				XmlReaderSettings xmlReaderSetting = new XmlReaderSettings();
				Stream manifestResourceStream = typeof(HighlightingDefinitionParser).Assembly.GetManifestResourceStream("BigBug.ICSharpCode.TextEditor.xshd.Mode.xsd");
				xmlReaderSetting.Schemas.Add("", new XmlTextReader(manifestResourceStream));
				xmlReaderSetting.Schemas.ValidationEventHandler += new ValidationEventHandler((object sender, ValidationEventArgs args) => {
					if (validationEventArgs == null)
					{
						validationEventArgs = new List<ValidationEventArgs>();
					}
					validationEventArgs.Add(args);
				});
				xmlReaderSetting.ValidationType = ValidationType.Schema;
				XmlReader xmlReader1 = XmlReader.Create(xmlReader, xmlReaderSetting);
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(xmlReader1);
				if (highlighter == null)
				{
					highlighter = new DefaultHighlightingStrategy(xmlDocument.DocumentElement.Attributes["name"].InnerText);
				}
				if (xmlDocument.DocumentElement.HasAttribute("extends"))
				{
					KeyValuePair<SyntaxMode, ISyntaxModeFileProvider> keyValuePair = HighlightingManager.Manager.FindHighlighterEntry(xmlDocument.DocumentElement.GetAttribute("extends"));
					if (keyValuePair.Key == null)
					{
						throw new HighlightingDefinitionInvalidException(string.Concat("Cannot find referenced highlighting source ", xmlDocument.DocumentElement.GetAttribute("extends")));
					}
					highlighter = HighlightingDefinitionParser.Parse(highlighter, keyValuePair.Key, keyValuePair.Value.GetSyntaxModeFile(keyValuePair.Key));
					if (highlighter == null)
					{
						defaultHighlightingStrategy = null;
						return defaultHighlightingStrategy;
					}
				}
				if (xmlDocument.DocumentElement.HasAttribute("extensions"))
				{
					string attribute = xmlDocument.DocumentElement.GetAttribute("extensions");
					char[] chrArray = new char[] { ';', '|' };
					highlighter.Extensions = attribute.Split(chrArray);
				}
				XmlElement item = xmlDocument.DocumentElement["Environment"];
				if (item != null)
				{
					foreach (XmlNode childNode in item.ChildNodes)
					{
						if (!(childNode is XmlElement))
						{
							continue;
						}
						XmlElement xmlElement = (XmlElement)childNode;
						if (xmlElement.Name != "Custom")
						{
							DefaultHighlightingStrategy defaultHighlightingStrategy1 = highlighter;
							string name = xmlElement.Name;
							if (xmlElement.HasAttribute("bgcolor"))
							{
								highlightBackground = new HighlightBackground(xmlElement);
							}
							else
							{
								highlightBackground = new HighlightColor(xmlElement);
							}
							defaultHighlightingStrategy1.SetColorFor(name, highlightBackground);
						}
						else
						{
							DefaultHighlightingStrategy defaultHighlightingStrategy2 = highlighter;
							string str = xmlElement.GetAttribute("name");
							if (xmlElement.HasAttribute("bgcolor"))
							{
								highlightColor = new HighlightBackground(xmlElement);
							}
							else
							{
								highlightColor = new HighlightColor(xmlElement);
							}
							defaultHighlightingStrategy2.SetColorFor(str, highlightColor);
						}
					}
				}
				if (xmlDocument.DocumentElement["Properties"] != null)
				{
					foreach (XmlElement innerText in xmlDocument.DocumentElement["Properties"].ChildNodes)
					{
						highlighter.Properties[innerText.Attributes["name"].InnerText] = innerText.Attributes["value"].InnerText;
					}
				}
				if (xmlDocument.DocumentElement["Digits"] != null)
				{
					highlighter.DigitColor = new HighlightColor(xmlDocument.DocumentElement["Digits"]);
				}
				foreach (XmlElement elementsByTagName in xmlDocument.DocumentElement.GetElementsByTagName("RuleSet"))
				{
					highlighter.AddRuleSet(new HighlightRuleSet(elementsByTagName));
				}
				xmlReader.Close();
				if (validationEventArgs != null)
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (ValidationEventArgs validationEventArg in validationEventArgs)
					{
						stringBuilder.AppendLine(validationEventArg.Message);
					}
					throw new HighlightingDefinitionInvalidException(stringBuilder.ToString());
				}
				defaultHighlightingStrategy = highlighter;
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				throw new HighlightingDefinitionInvalidException(string.Concat("Could not load mode definition file '", syntaxMode.FileName, "'.\n"), exception);
			}
			return defaultHighlightingStrategy;
		}
	}
}