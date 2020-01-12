using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class ShiftMovePageDown : MovePageDown
	{
		public ShiftMovePageDown()
		{
		}

		public override void Execute(TextArea textArea)
		{
			TextLocation position = textArea.Caret.Position;
			base.Execute(textArea);
			textArea.AutoClearSelection = false;
			textArea.SelectionManager.ExtendSelection(position, textArea.Caret.Position);
		}
	}
}