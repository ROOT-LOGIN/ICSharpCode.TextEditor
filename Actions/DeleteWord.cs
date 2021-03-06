using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class DeleteWord : Delete
	{
		public DeleteWord()
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
			if (textArea.Caret.Offset != lineSegmentForOffset.Offset + lineSegmentForOffset.Length)
			{
				int num = TextUtilities.FindNextWordStart(textArea.Document, textArea.Caret.Offset);
				if (num > textArea.Caret.Offset && !textArea.IsReadOnly(textArea.Caret.Offset, num - textArea.Caret.Offset))
				{
					textArea.Document.Remove(textArea.Caret.Offset, num - textArea.Caret.Offset);
				}
			}
			else
			{
				base.Execute(textArea);
			}
			textArea.UpdateMatchingBracket();
			textArea.EndUpdate();
			textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToEnd, new TextLocation(0, textArea.Document.GetLineNumberForOffset(textArea.Caret.Offset))));
			textArea.Document.CommitUpdate();
		}
	}
}