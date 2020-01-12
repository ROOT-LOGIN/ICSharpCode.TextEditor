using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class MovePageDown : AbstractEditAction
	{
		public MovePageDown()
		{
		}

		public override void Execute(TextArea textArea)
		{
			int line = textArea.Caret.Line;
			int num = Math.Min(textArea.Document.GetNextVisibleLineAbove(line, textArea.TextView.VisibleLineCount), textArea.Document.TotalNumberOfLines - 1);
			if (line != num)
			{
				textArea.Caret.Position = new TextLocation(0, num);
				textArea.SetCaretToDesiredColumn();
			}
		}
	}
}