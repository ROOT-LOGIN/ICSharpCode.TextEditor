using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Actions
{
	public class Delete : AbstractEditAction
	{
		public Delete()
		{
		}

		internal static void DeleteSelection(TextArea textArea)
		{
			if (textArea.SelectionManager.SelectionIsReadonly)
			{
				return;
			}
			textArea.BeginUpdate();
			textArea.Caret.Position = textArea.SelectionManager.SelectionCollection[0].StartPosition;
			textArea.SelectionManager.RemoveSelectedText();
			textArea.ScrollToCaret();
			textArea.EndUpdate();
		}

		public override void Execute(TextArea textArea)
		{
			if (textArea.SelectionManager.HasSomethingSelected)
			{
				Delete.DeleteSelection(textArea);
				return;
			}
			if (textArea.IsReadOnly(textArea.Caret.Offset))
			{
				return;
			}
			if (textArea.Caret.Offset < textArea.Document.TextLength)
			{
				textArea.BeginUpdate();
				int lineNumberForOffset = textArea.Document.GetLineNumberForOffset(textArea.Caret.Offset);
				LineSegment lineSegment = textArea.Document.GetLineSegment(lineNumberForOffset);
				if (lineSegment.Offset + lineSegment.Length != textArea.Caret.Offset)
				{
					textArea.Document.Remove(textArea.Caret.Offset, 1);
				}
				else if (lineNumberForOffset + 1 < textArea.Document.TotalNumberOfLines)
				{
					LineSegment lineSegment1 = textArea.Document.GetLineSegment(lineNumberForOffset + 1);
					textArea.Document.Remove(textArea.Caret.Offset, lineSegment1.Offset - textArea.Caret.Offset);
					textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToEnd, new TextLocation(0, lineNumberForOffset)));
				}
				textArea.UpdateMatchingBracket();
				textArea.EndUpdate();
			}
		}
	}
}