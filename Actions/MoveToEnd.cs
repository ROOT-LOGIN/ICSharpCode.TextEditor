using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class MoveToEnd : AbstractEditAction
	{
		public MoveToEnd()
		{
		}

		public override void Execute(TextArea textArea)
		{
			TextLocation position = textArea.Document.OffsetToPosition(textArea.Document.TextLength);
			if (textArea.Caret.Position != position)
			{
				textArea.Caret.Position = position;
				textArea.SetDesiredColumn();
			}
		}
	}
}