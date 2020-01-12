using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class WordBackspace : AbstractEditAction
	{
		public WordBackspace()
		{
		}

		public override void Execute(TextArea textArea)
		{
			if (textArea.SelectionManager.HasSomethingSelected)
			{
				Delete.DeleteSelection(textArea);
				return;
			}
			textArea.BeginUpdate();
			LineSegment lineSegmentForOffset = textArea.Document.GetLineSegmentForOffset(textArea.Caret.Offset);
			if (textArea.Caret.Offset > lineSegmentForOffset.Offset)
			{
				int num = TextUtilities.FindPrevWordStart(textArea.Document, textArea.Caret.Offset);
				if (num < textArea.Caret.Offset && !textArea.IsReadOnly(num, textArea.Caret.Offset - num))
				{
					textArea.Document.Remove(num, textArea.Caret.Offset - num);
					textArea.Caret.Position = textArea.Document.OffsetToPosition(num);
				}
			}
			if (textArea.Caret.Offset == lineSegmentForOffset.Offset)
			{
				int lineNumberForOffset = textArea.Document.GetLineNumberForOffset(textArea.Caret.Offset);
				if (lineNumberForOffset > 0)
				{
					LineSegment lineSegment = textArea.Document.GetLineSegment(lineNumberForOffset - 1);
					int offset = lineSegment.Offset + lineSegment.Length;
					int offset1 = textArea.Caret.Offset - offset;
					if (!textArea.IsReadOnly(offset, offset1))
					{
						textArea.Document.Remove(offset, offset1);
						textArea.Caret.Position = textArea.Document.OffsetToPosition(offset);
					}
				}
			}
			textArea.SetDesiredColumn();
			textArea.EndUpdate();
			textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToEnd, new TextLocation(0, textArea.Document.GetLineNumberForOffset(textArea.Caret.Offset))));
			textArea.Document.CommitUpdate();
		}
	}
}