using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Undo;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICSharpCode.TextEditor.Actions
{
	public class Tab : AbstractEditAction
	{
		public Tab()
		{
		}

		public override void Execute(TextArea textArea)
		{
			if (textArea.SelectionManager.SelectionIsReadonly)
			{
				return;
			}
			textArea.Document.UndoStack.StartUndoGroup();
			if (!textArea.SelectionManager.HasSomethingSelected)
			{
				this.InsertTabAtCaretPosition(textArea);
			}
			else
			{
				foreach (ISelection selectionCollection in textArea.SelectionManager.SelectionCollection)
				{
					int y = selectionCollection.StartPosition.Y;
					int num = selectionCollection.EndPosition.Y;
					if (y == num)
					{
						this.InsertTabAtCaretPosition(textArea);
						break;
					}
					else
					{
						textArea.BeginUpdate();
						this.InsertTabs(textArea.Document, selectionCollection, y, num);
						textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.LinesBetween, y, num));
						textArea.EndUpdate();
					}
				}
				textArea.Document.CommitUpdate();
				textArea.AutoClearSelection = false;
			}
			textArea.Document.UndoStack.EndUndoGroup();
		}

		public static string GetIndentationString(IDocument document)
		{
			return Tab.GetIndentationString(document, null);
		}

		public static string GetIndentationString(IDocument document, TextArea textArea)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (!document.TextEditorProperties.ConvertTabsToSpaces)
			{
				stringBuilder.Append('\t');
			}
			else
			{
				int indentationSize = document.TextEditorProperties.IndentationSize;
				if (textArea == null)
				{
					stringBuilder.Append(new string(' ', indentationSize));
				}
				else
				{
					int visualColumn = textArea.TextView.GetVisualColumn(textArea.Caret.Line, textArea.Caret.Column);
					stringBuilder.Append(new string(' ', indentationSize - visualColumn % indentationSize));
				}
			}
			return stringBuilder.ToString();
		}

		private void InsertTabAtCaretPosition(TextArea textArea)
		{
			switch (textArea.Caret.CaretMode)
			{
				case CaretMode.InsertMode:
				{
					textArea.InsertString(Tab.GetIndentationString(textArea.Document, textArea));
					break;
				}
				case CaretMode.OverwriteMode:
				{
					string indentationString = Tab.GetIndentationString(textArea.Document, textArea);
					textArea.ReplaceChar(indentationString[0]);
					if (indentationString.Length <= 1)
					{
						break;
					}
					textArea.InsertString(indentationString.Substring(1));
					break;
				}
			}
			textArea.SetDesiredColumn();
		}

		private void InsertTabs(IDocument document, ISelection selection, int y1, int y2)
		{
			string indentationString = Tab.GetIndentationString(document);
			for (int i = y2; i >= y1; i--)
			{
				LineSegment lineSegment = document.GetLineSegment(i);
				if (i != y2 || i != selection.EndPosition.Y || selection.EndPosition.X != 0)
				{
					document.Insert(lineSegment.Offset, indentationString);
				}
			}
		}
	}
}