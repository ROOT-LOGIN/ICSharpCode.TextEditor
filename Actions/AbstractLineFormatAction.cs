using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Undo;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor.Actions
{
	public abstract class AbstractLineFormatAction : AbstractEditAction
	{
		protected TextArea textArea;

		protected AbstractLineFormatAction()
		{
		}

		protected abstract void Convert(IDocument document, int startLine, int endLine);

		public override void Execute(TextArea textArea)
		{
			if (textArea.SelectionManager.SelectionIsReadonly)
			{
				return;
			}
			this.textArea = textArea;
			textArea.BeginUpdate();
			textArea.Document.UndoStack.StartUndoGroup();
			if (!textArea.SelectionManager.HasSomethingSelected)
			{
				this.Convert(textArea.Document, 0, textArea.Document.TotalNumberOfLines - 1);
			}
			else
			{
				foreach (ISelection selectionCollection in textArea.SelectionManager.SelectionCollection)
				{
					this.Convert(textArea.Document, selectionCollection.StartPosition.Y, selectionCollection.EndPosition.Y);
				}
			}
			textArea.Document.UndoStack.EndUndoGroup();
			textArea.Caret.ValidateCaretPos();
			textArea.EndUpdate();
			textArea.Refresh();
		}
	}
}