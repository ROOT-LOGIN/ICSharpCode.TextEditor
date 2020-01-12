using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class GotoMatchingBrace : AbstractEditAction
	{
		public GotoMatchingBrace()
		{
		}

		public override void Execute(TextArea textArea)
		{
			Highlight highlight = textArea.FindMatchingBracketHighlight();
			if (highlight != null)
			{
				TextLocation closeBrace = highlight.CloseBrace;
				TextLocation textLocation = highlight.CloseBrace;
				TextLocation textLocation1 = new TextLocation(closeBrace.X + 1, textLocation.Y);
				TextLocation openBrace = highlight.OpenBrace;
				TextLocation openBrace1 = highlight.OpenBrace;
				TextLocation textLocation2 = new TextLocation(openBrace.X + 1, openBrace1.Y);
				if (textLocation1 == textArea.Caret.Position)
				{
					if (textArea.Document.TextEditorProperties.BracketMatchingStyle != BracketMatchingStyle.After)
					{
						textArea.Caret.Position = new TextLocation(textLocation2.X - 1, textLocation2.Y);
					}
					else
					{
						textArea.Caret.Position = textLocation2;
					}
				}
				else if (textArea.Document.TextEditorProperties.BracketMatchingStyle != BracketMatchingStyle.After)
				{
					textArea.Caret.Position = new TextLocation(textLocation1.X - 1, textLocation1.Y);
				}
				else
				{
					textArea.Caret.Position = textLocation1;
				}
				textArea.SetDesiredColumn();
			}
		}
	}
}