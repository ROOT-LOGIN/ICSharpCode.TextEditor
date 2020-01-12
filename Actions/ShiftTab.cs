using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Undo;
using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Actions
{
	public class ShiftTab : AbstractEditAction
	{
		public ShiftTab()
		{
		}

		public override void Execute(TextArea textArea)
		{
			if (textArea.SelectionManager.HasSomethingSelected)
			{
				foreach (ISelection selectionCollection in textArea.SelectionManager.SelectionCollection)
				{
					int y = selectionCollection.StartPosition.Y;
					int num = selectionCollection.EndPosition.Y;
					textArea.BeginUpdate();
					this.RemoveTabs(textArea.Document, selectionCollection, y, num);
					textArea.Document.UpdateQueue.Clear();
					textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.LinesBetween, y, num));
					textArea.EndUpdate();
				}
				textArea.AutoClearSelection = false;
				return;
			}
			LineSegment lineSegmentForOffset = textArea.Document.GetLineSegmentForOffset(textArea.Caret.Offset);
			textArea.Document.GetText(lineSegmentForOffset.Offset, textArea.Caret.Offset - lineSegmentForOffset.Offset);
			int indentationSize = textArea.Document.TextEditorProperties.IndentationSize;
			int column = textArea.Caret.Column;
			int num1 = column % indentationSize;
			if (num1 != 0)
			{
				textArea.Caret.DesiredColumn = Math.Max(0, column - num1);
			}
			else
			{
				textArea.Caret.DesiredColumn = Math.Max(0, column - indentationSize);
			}
			textArea.SetCaretToDesiredColumn();
		}

		private void RemoveTabs(IDocument document, ISelection selection, int y1, int y2)
		{
			document.UndoStack.StartUndoGroup();
			for (int i = y2; i >= y1; i--)
			{
				LineSegment lineSegment = document.GetLineSegment(i);
				if ((i != y2 || lineSegment.Offset != selection.EndOffset) && lineSegment.Length > 0 && lineSegment.Length > 0)
				{
					int num = 0;
					if (document.GetCharAt(lineSegment.Offset) == '\t')
					{
						num = 1;
					}
					else if (document.GetCharAt(lineSegment.Offset) == ' ')
					{
						int num1 = 1;
						int indentationSize = document.TextEditorProperties.IndentationSize;
						num1 = 1;
						while (num1 < lineSegment.Length && document.GetCharAt(lineSegment.Offset + num1) == ' ')
						{
							num1++;
						}
						if (num1 < indentationSize)
						{
							num = (lineSegment.Length <= num1 || document.GetCharAt(lineSegment.Offset + num1) != '\t' ? num1 : num1 + 1);
						}
						else
						{
							num = indentationSize;
						}
					}
					if (num > 0)
					{
						document.Remove(lineSegment.Offset, num);
					}
				}
			}
			document.UndoStack.EndUndoGroup();
		}
	}
}