using ICSharpCode.TextEditor.Util;
using System;
using System.Collections;
using System.Xml;

namespace ICSharpCode.TextEditor.Document
{
	public class HighlightRuleSet
	{
		private LookupTable keyWords;

		private ArrayList spans = new ArrayList();

		private LookupTable prevMarkers;

		private LookupTable nextMarkers;

		private char escapeCharacter;

		private bool ignoreCase;

		private string name;

		private bool[] delimiters = new bool[256];

		private string reference;

		internal IHighlightingStrategyUsingRuleSets Highlighter;

		public bool[] Delimiters
		{
			get
			{
				return this.delimiters;
			}
		}

		public char EscapeCharacter
		{
			get
			{
				return this.escapeCharacter;
			}
		}

		public bool IgnoreCase
		{
			get
			{
				return this.ignoreCase;
			}
		}

		public LookupTable KeyWords
		{
			get
			{
				return this.keyWords;
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

		public LookupTable NextMarkers
		{
			get
			{
				return this.nextMarkers;
			}
		}

		public LookupTable PrevMarkers
		{
			get
			{
				return this.prevMarkers;
			}
		}

		public string Reference
		{
			get
			{
				return this.reference;
			}
		}

		public ArrayList Spans
		{
			get
			{
				return this.spans;
			}
		}

		public HighlightRuleSet()
		{
			this.keyWords = new LookupTable(false);
			this.prevMarkers = new LookupTable(false);
			this.nextMarkers = new LookupTable(false);
		}

		public HighlightRuleSet(XmlElement el)
		{
			if (el.Attributes["name"] != null)
			{
				this.Name = el.Attributes["name"].InnerText;
			}
			if (el.HasAttribute("escapecharacter"))
			{
				this.escapeCharacter = el.GetAttribute("escapecharacter")[0];
			}
			if (el.Attributes["reference"] != null)
			{
				this.reference = el.Attributes["reference"].InnerText;
			}
			if (el.Attributes["ignorecase"] != null)
			{
				this.ignoreCase = bool.Parse(el.Attributes["ignorecase"].InnerText);
			}
			for (int i = 0; i < (int)this.Delimiters.Length; i++)
			{
				this.delimiters[i] = false;
			}
			if (el["Delimiters"] != null)
			{
				string innerText = el["Delimiters"].InnerText;
				for (int j = 0; j < innerText.Length; j++)
				{
					char chr = innerText[j];
					this.delimiters[chr] = true;
				}
			}
			this.keyWords = new LookupTable(!this.IgnoreCase);
			this.prevMarkers = new LookupTable(!this.IgnoreCase);
			this.nextMarkers = new LookupTable(!this.IgnoreCase);
			foreach (XmlElement elementsByTagName in el.GetElementsByTagName("KeyWords"))
			{
				HighlightColor highlightColor = new HighlightColor(elementsByTagName);
				foreach (XmlElement xmlElement in elementsByTagName.GetElementsByTagName("Key"))
				{
					this.keyWords[xmlElement.Attributes["word"].InnerText] = highlightColor;
				}
			}
			foreach (XmlElement elementsByTagName1 in el.GetElementsByTagName("Span"))
			{
				this.Spans.Add(new Span(elementsByTagName1));
			}
			foreach (XmlElement xmlElement1 in el.GetElementsByTagName("MarkPrevious"))
			{
				PrevMarker prevMarker = new PrevMarker(xmlElement1);
				this.prevMarkers[prevMarker.What] = prevMarker;
			}
			foreach (XmlElement elementsByTagName2 in el.GetElementsByTagName("MarkFollowing"))
			{
				NextMarker nextMarker = new NextMarker(elementsByTagName2);
				this.nextMarkers[nextMarker.What] = nextMarker;
			}
		}

		public void MergeFrom(HighlightRuleSet ruleSet)
		{
			for (int i = 0; i < (int)this.delimiters.Length; i++)
			{
				this.delimiters[i] |= ruleSet.delimiters[i];
			}
			ArrayList arrayLists = this.spans;
			this.spans = (ArrayList)ruleSet.spans.Clone();
			this.spans.AddRange(arrayLists);
		}
	}
}