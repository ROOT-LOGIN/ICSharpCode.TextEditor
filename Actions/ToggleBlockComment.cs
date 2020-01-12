using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Undo;
using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Actions
{
	public class ToggleBlockComment : AbstractEditAction
	{
		public ToggleBlockComment()
		{
		}

		public override void Execute(TextArea textArea)
		{
			int offset;
			int endOffset;
			if (textArea.Document.ReadOnly)
			{
				return;
			}
			string str = null;
			if (textArea.Document.HighlightingStrategy.Properties.ContainsKey("BlockCommentBegin"))
			{
				str = textArea.Document.HighlightingStrategy.Properties["BlockCommentBegin"].ToString();
			}
			string str1 = null;
			if (textArea.Document.HighlightingStrategy.Properties.ContainsKey("BlockCommentEnd"))
			{
				str1 = textArea.Document.HighlightingStrategy.Properties["BlockCommentEnd"].ToString();
			}
			if (str == null || str.Length == 0 || str1 == null || str1.Length == 0)
			{
				return;
			}
			if (!textArea.SelectionManager.HasSomethingSelected)
			{
				offset = textArea.Caret.Offset;
				endOffset = offset;
			}
			else
			{
				offset = textArea.SelectionManager.SelectionCollection[0].Offset;
				endOffset = textArea.SelectionManager.SelectionCollection[textArea.SelectionManager.SelectionCollection.Count - 1].EndOffset;
			}
			BlockCommentRegion blockCommentRegion = ToggleBlockComment.FindSelectedCommentRegion(textArea.Document, str, str1, offset, endOffset);
			textArea.Document.UndoStack.StartUndoGroup();
			if (blockCommentRegion != null)
			{
				this.RemoveComment(textArea.Document, blockCommentRegion);
			}
			else if (textArea.SelectionManager.HasSomethingSelected)
			{
				this.SetCommentAt(textArea.Document, offset, endOffset, str, str1);
			}
			textArea.Document.UndoStack.EndUndoGroup();
			textArea.Document.CommitUpdate();
			textArea.AutoClearSelection = false;
		}

		public static BlockCommentRegion FindSelectedCommentRegion(IDocument document, string commentStart, string commentEnd, int selectionStartOffset, int selectionEndOffset)
		{
			if (document.TextLength == 0)
			{
				return null;
			}
			int num = -1;
			string text = document.GetText(selectionStartOffset, selectionEndOffset - selectionStartOffset);
			int num1 = text.IndexOf(commentStart);
			if (num1 >= 0)
			{
				num1 += selectionStartOffset;
			}
			num = (num1 < 0 ? text.IndexOf(commentEnd) : text.IndexOf(commentEnd, num1 + commentStart.Length - selectionStartOffset));
			if (num >= 0)
			{
				num += selectionStartOffset;
			}
			if (num1 == -1)
			{
				int textLength = selectionEndOffset + commentStart.Length - 1;
				if (textLength > document.TextLength)
				{
					textLength = document.TextLength;
				}
				string str = document.GetText(0, textLength);
				num1 = str.LastIndexOf(commentStart);
				if (num1 >= 0 && str.IndexOf(commentEnd, num1, selectionStartOffset - num1) > num1)
				{
					num1 = -1;
				}
			}
			if (num == -1)
			{
				int num2 = selectionStartOffset + 1 - commentEnd.Length;
				if (num2 < 0)
				{
					num2 = selectionStartOffset;
				}
				string text1 = document.GetText(num2, document.TextLength - num2);
				num = text1.IndexOf(commentEnd);
				if (num >= 0)
				{
					num += num2;
				}
			}
			if (num1 == -1 || num == -1)
			{
				return null;
			}
			return new BlockCommentRegion(commentStart, commentEnd, num1, num);
		}

		private void RemoveComment(IDocument document, BlockCommentRegion commentRegion)
		{
			document.Remove(commentRegion.EndOffset, commentRegion.CommentEnd.Length);
			document.Remove(commentRegion.StartOffset, commentRegion.CommentStart.Length);
		}

		private void SetCommentAt(IDocument document, int offsetStart, int offsetEnd, string commentStart, string commentEnd)
		{
			document.Insert(offsetEnd, commentEnd);
			document.Insert(offsetStart, commentStart);
		}
	}
}