using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Undo
{
	public class UndoableInsert : IUndoableOperation
	{
		private IDocument document;

		private int offset;

		private string text;

		public UndoableInsert(IDocument document, int offset, string text)
		{
			if (document == null)
			{
				throw new ArgumentNullException("document");
			}
			if (offset < 0 || offset > document.TextLength)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			this.document = document;
			this.offset = offset;
			this.text = text;
		}

		public void Redo()
		{
			this.document.UndoStack.AcceptChanges = false;
			this.document.Insert(this.offset, this.text);
			this.document.UndoStack.AcceptChanges = true;
		}

		public void Undo()
		{
			this.document.UndoStack.AcceptChanges = false;
			this.document.Remove(this.offset, this.text.Length);
			this.document.UndoStack.AcceptChanges = true;
		}
	}
}