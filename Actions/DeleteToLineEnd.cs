using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class DeleteToLineEnd : AbstractEditAction
	{
		public DeleteToLineEnd()
		{
		}

		public override void Execute(TextArea textArea)
		{
			int line = textArea.Caret.Line;
			LineSegment lineSegment = textArea.Document.GetLineSegment(line);
			int offset = lineSegment.Offset + lineSegment.Length - textArea.Caret.Offset;
			if (offset > 0 && !textArea.IsReadOnly(textArea.Caret.Offset, offset))
			{
				textArea.Document.Remove(textArea.Caret.Offset, offset);
				textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, new TextLocation(0, line)));
				textArea.Document.CommitUpdate();
			}
		}
	}
}