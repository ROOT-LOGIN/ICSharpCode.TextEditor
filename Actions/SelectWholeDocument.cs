using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Actions
{
	public class SelectWholeDocument : AbstractEditAction
	{
		public SelectWholeDocument()
		{
		}

		public override void Execute(TextArea textArea)
		{
			textArea.AutoClearSelection = false;
			TextLocation textLocation = new TextLocation(0, 0);
			TextLocation position = textArea.Document.OffsetToPosition(textArea.Document.TextLength);
			if (textArea.SelectionManager.HasSomethingSelected && textArea.SelectionManager.SelectionCollection[0].StartPosition == textLocation && textArea.SelectionManager.SelectionCollection[0].EndPosition == position)
			{
				return;
			}
			textArea.Caret.Position = textArea.SelectionManager.NextValidPosition(position.Y);
			textArea.SelectionManager.ExtendSelection(textLocation, position);
			textArea.SetDesiredColumn();
		}
	}
}