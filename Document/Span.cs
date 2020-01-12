using System;
using System.Xml;

namespace ICSharpCode.TextEditor.Document
{
	public sealed class Span
	{
		private bool stopEOL;

		private HighlightColor color;

		private HighlightColor beginColor;

		private HighlightColor endColor;

		private char[] begin;

		private char[] end;

		private string name;

		private string rule;

		private HighlightRuleSet ruleSet;

		private char escapeCharacter;

		private bool ignoreCase;

		private bool isBeginSingleWord;

		private bool? isBeginStartOfLine;

		private bool isEndSingleWord;

		public char[] Begin
		{
			get
			{
				return this.begin;
			}
		}

		public HighlightColor BeginColor
		{
			get
			{
				if (this.beginColor != null)
				{
					return this.beginColor;
				}
				return this.color;
			}
		}

		public HighlightColor Color
		{
			get
			{
				return this.color;
			}
		}

		public char[] End
		{
			get
			{
				return this.end;
			}
		}

		public HighlightColor EndColor
		{
			get
			{
				if (this.endColor == null)
				{
					return this.color;
				}
				return this.endColor;
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
			set
			{
				this.ignoreCase = value;
			}
		}

		public bool IsBeginSingleWord
		{
			get
			{
				return this.isBeginSingleWord;
			}
		}

		public bool? IsBeginStartOfLine
		{
			get
			{
				return this.isBeginStartOfLine;
			}
		}

		public bool IsEndSingleWord
		{
			get
			{
				return this.isEndSingleWord;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public string Rule
		{
			get
			{
				return this.rule;
			}
		}

		internal HighlightRuleSet RuleSet
		{
			get
			{
				return this.ruleSet;
			}
			set
			{
				this.ruleSet = value;
			}
		}

		public bool StopEOL
		{
			get
			{
				return this.stopEOL;
			}
		}

		public Span(XmlElement span)
		{
			this.color = new HighlightColor(span);
			if (span.HasAttribute("rule"))
			{
				this.rule = span.GetAttribute("rule");
			}
			if (span.HasAttribute("escapecharacter"))
			{
				this.escapeCharacter = span.GetAttribute("escapecharacter")[0];
			}
			this.name = span.GetAttribute("name");
			if (span.HasAttribute("stopateol"))
			{
				this.stopEOL = bool.Parse(span.GetAttribute("stopateol"));
			}
			this.begin = span["Begin"].InnerText.ToCharArray();
			this.beginColor = new HighlightColor(span["Begin"], this.color);
			if (span["Begin"].HasAttribute("singleword"))
			{
				this.isBeginSingleWord = bool.Parse(span["Begin"].GetAttribute("singleword"));
			}
			if (span["Begin"].HasAttribute("startofline"))
			{
				this.isBeginStartOfLine = new bool?(bool.Parse(span["Begin"].GetAttribute("startofline")));
			}
			if (span["End"] != null)
			{
				this.end = span["End"].InnerText.ToCharArray();
				this.endColor = new HighlightColor(span["End"], this.color);
				if (span["End"].HasAttribute("singleword"))
				{
					this.isEndSingleWord = bool.Parse(span["End"].GetAttribute("singleword"));
				}
			}
		}
	}
}