using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor.Actions
{
	public abstract class AbstractSelectionFormatAction : AbstractEditAction
	{
		protected TextArea textArea;

		protected AbstractSelectionFormatAction()
		{
		}

		protected abstract void Convert(IDocument document, int offset, int length);

		public override void Execute(TextArea textArea)
		{
			if (textArea.SelectionManager.SelectionIsReadonly)
			{
				return;
			}
			this.textArea = textArea;
			textArea.BeginUpdate();
			if (!textArea.SelectionManager.HasSomethingSelected)
			{
				this.Convert(textArea.Document, 0, textArea.Document.TextLength);
			}
			else
			{
				foreach (ISelection selectionCollection in textArea.SelectionManager.SelectionCollection)
				{
					this.Convert(textArea.Document, selectionCollection.Offset, selectionCollection.Length);
				}
			}
			textArea.Caret.ValidateCaretPos();
			textArea.EndUpdate();
			textArea.Refresh();
		}
	}
}