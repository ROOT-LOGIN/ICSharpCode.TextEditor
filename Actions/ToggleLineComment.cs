using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Undo;
using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Actions
{
	public class ToggleLineComment : AbstractEditAction
	{
		private int firstLine;

		private int lastLine;

		public ToggleLineComment()
		{
		}

		public override void Execute(TextArea textArea)
		{
			if (textArea.Document.ReadOnly)
			{
				return;
			}
			string str = null;
			if (textArea.Document.HighlightingStrategy.Properties.ContainsKey("LineComment"))
			{
				str = textArea.Document.HighlightingStrategy.Properties["LineComment"].ToString();
			}
			if (str == null || str.Length == 0)
			{
				return;
			}
			textArea.Document.UndoStack.StartUndoGroup();
			if (!textArea.SelectionManager.HasSomethingSelected)
			{
				textArea.BeginUpdate();
				int line = textArea.Caret.Line;
				if (!this.ShouldComment(textArea.Document, str, null, line, line))
				{
					this.RemoveCommentAt(textArea.Document, str, null, line, line);
				}
				else
				{
					this.SetCommentAt(textArea.Document, str, null, line, line);
				}
				textArea.Document.UpdateQueue.Clear();
				textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, line));
				textArea.EndUpdate();
			}
			else
			{
				bool flag = true;
				foreach (ISelection selectionCollection in textArea.SelectionManager.SelectionCollection)
				{
					if (this.ShouldComment(textArea.Document, str, selectionCollection, selectionCollection.StartPosition.Y, selectionCollection.EndPosition.Y))
					{
						continue;
					}
					flag = false;
					break;
				}
				foreach (ISelection selection in textArea.SelectionManager.SelectionCollection)
				{
					textArea.BeginUpdate();
					if (!flag)
					{
						IDocument document = textArea.Document;
						int y = selection.StartPosition.Y;
						TextLocation endPosition = selection.EndPosition;
						this.RemoveCommentAt(document, str, selection, y, endPosition.Y);
					}
					else
					{
						IDocument document1 = textArea.Document;
						int num = selection.StartPosition.Y;
						TextLocation textLocation = selection.EndPosition;
						this.SetCommentAt(document1, str, selection, num, textLocation.Y);
					}
					textArea.Document.UpdateQueue.Clear();
					textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.LinesBetween, this.firstLine, this.lastLine));
					textArea.EndUpdate();
				}
				textArea.Document.CommitUpdate();
				textArea.AutoClearSelection = false;
			}
			textArea.Document.UndoStack.EndUndoGroup();
		}

		private void RemoveCommentAt(IDocument document, string comment, ISelection selection, int y1, int y2)
		{
			this.firstLine = y1;
			this.lastLine = y2;
			for (int i = y2; i >= y1; i--)
			{
				LineSegment lineSegment = document.GetLineSegment(i);
				if (selection == null || i != y2 || lineSegment.Offset != selection.Offset + selection.Length)
				{
					string text = document.GetText(lineSegment.Offset, lineSegment.Length);
					if (text.Trim().StartsWith(comment))
					{
						document.Remove(lineSegment.Offset + text.IndexOf(comment), comment.Length);
					}
				}
				else
				{
					this.lastLine--;
				}
			}
		}

		private void SetCommentAt(IDocument document, string comment, ISelection selection, int y1, int y2)
		{
			this.firstLine = y1;
			this.lastLine = y2;
			for (int i = y2; i >= y1; i--)
			{
				LineSegment lineSegment = document.GetLineSegment(i);
				if (selection == null || i != y2 || lineSegment.Offset != selection.Offset + selection.Length)
				{
					document.GetText(lineSegment.Offset, lineSegment.Length);
					document.Insert(lineSegment.Offset, comment);
				}
				else
				{
					this.lastLine--;
				}
			}
		}

		private bool ShouldComment(IDocument document, string comment, ISelection selection, int startLine, int endLine)
		{
			for (int i = endLine; i >= startLine; i--)
			{
				LineSegment lineSegment = document.GetLineSegment(i);
				if (selection != null && i == endLine && lineSegment.Offset == selection.Offset + selection.Length)
				{
					this.lastLine--;
				}
				else if (!document.GetText(lineSegment.Offset, lineSegment.Length).Trim().StartsWith(comment))
				{
					return true;
				}
			}
			return false;
		}
	}
}