using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class DeleteLine : AbstractEditAction
	{
		public DeleteLine()
		{
		}

		public override void Execute(TextArea textArea)
		{
			int line = textArea.Caret.Line;
			LineSegment lineSegment = textArea.Document.GetLineSegment(line);
			if (textArea.IsReadOnly(lineSegment.Offset, lineSegment.Length))
			{
				return;
			}
			textArea.Document.Remove(lineSegment.Offset, lineSegment.TotalLength);
			textArea.Caret.Position = textArea.Document.OffsetToPosition(lineSegment.Offset);
			textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToEnd, new TextLocation(0, line)));
			textArea.UpdateMatchingBracket();
			textArea.Document.CommitUpdate();
		}
	}
}