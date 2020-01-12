using System;

namespace ICSharpCode.TextEditor.Document
{
	public class HighlightInfo
	{
		public bool BlockSpanOn;

		public bool Span;

		public ICSharpCode.TextEditor.Document.Span CurSpan;

		public HighlightInfo(ICSharpCode.TextEditor.Document.Span curSpan, bool span, bool blockSpanOn)
		{
			this.CurSpan = curSpan;
			this.Span = span;
			this.BlockSpanOn = blockSpanOn;
		}
	}
}