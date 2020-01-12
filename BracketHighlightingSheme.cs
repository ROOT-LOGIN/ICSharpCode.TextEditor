using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor
{
	public class BracketHighlightingSheme
	{
		private char opentag;

		private char closingtag;

		public char ClosingTag
		{
			get
			{
				return this.closingtag;
			}
			set
			{
				this.closingtag = value;
			}
		}

		public char OpenTag
		{
			get
			{
				return this.opentag;
			}
			set
			{
				this.opentag = value;
			}
		}

		public BracketHighlightingSheme(char opentag, char closingtag)
		{
			this.opentag = opentag;
			this.closingtag = closingtag;
		}

		public Highlight GetHighlight(IDocument document, int offset)
		{
			int num;
			num = (document.TextEditorProperties.BracketMatchingStyle != BracketMatchingStyle.After ? offset + 1 : offset);
			char charAt = document.GetCharAt(Math.Max(0, Math.Min(document.TextLength - 1, num)));
			TextLocation position = document.OffsetToPosition(num);
			if (charAt == this.opentag)
			{
				if (num < document.TextLength)
				{
					int num1 = TextUtilities.SearchBracketForward(document, num + 1, this.opentag, this.closingtag);
					if (num1 >= 0)
					{
						return new Highlight(document.OffsetToPosition(num1), position);
					}
				}
			}
			else if (charAt == this.closingtag && num > 0)
			{
				int num2 = TextUtilities.SearchBracketBackward(document, num - 1, this.opentag, this.closingtag);
				if (num2 >= 0)
				{
					return new Highlight(document.OffsetToPosition(num2), position);
				}
			}
			return null;
		}
	}
}