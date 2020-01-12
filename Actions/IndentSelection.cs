using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class IndentSelection : AbstractLineFormatAction
	{
		public IndentSelection()
		{
		}

		protected override void Convert(IDocument document, int startLine, int endLine)
		{
			document.FormattingStrategy.IndentLines(this.textArea, startLine, endLine);
		}
	}
}