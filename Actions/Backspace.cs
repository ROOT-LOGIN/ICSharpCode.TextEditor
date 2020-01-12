using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class Backspace : AbstractEditAction
	{
		public Backspace()
		{
		}

		public override void Execute(TextArea textArea)
		{
			if (textArea.SelectionManager.HasSomethingSelected)
			{
				Delete.DeleteSelection(textArea);
				return;
			}
			if (textArea.Caret.Offset > 0 && !textArea.IsReadOnly(textArea.Caret.Offset - 1))
			{
				textArea.BeginUpdate();
				int lineNumberForOffset = textArea.Document.GetLineNumberForOffset(textArea.Caret.Offset);
				int offset = textArea.Document.GetLineSegment(lineNumberForOffset).Offset;
				if (offset != textArea.Caret.Offset)
				{
					int num = textArea.Caret.Offset - 1;
					textArea.Caret.Position = textArea.Document.OffsetToPosition(num);
					textArea.Document.Remove(num, 1);
					textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToLineEnd, new TextLocation(textArea.Caret.Offset - textArea.Document.GetLineSegment(lineNumberForOffset).Offset, lineNumberForOffset)));
				}
				else
				{
					LineSegment lineSegment = textArea.Document.GetLineSegment(lineNumberForOffset - 1);
					int totalNumberOfLines = textArea.Document.TotalNumberOfLines;
					int offset1 = lineSegment.Offset + lineSegment.Length;
					int length = lineSegment.Length;
					textArea.Document.Remove(offset1, offset - offset1);
					textArea.Caret.Position = new TextLocation(length, lineNumberForOffset - 1);
					textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToEnd, new TextLocation(0, lineNumberForOffset - 1)));
				}
				textArea.EndUpdate();
			}
		}
	}
}