using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Drawing;

namespace ICSharpCode.TextEditor.Actions
{
	public class CaretUp : AbstractEditAction
	{
		public CaretUp()
		{
		}

		public override void Execute(TextArea textArea)
		{
			TextLocation position = textArea.Caret.Position;
			int y = position.Y;
			int visibleLine = textArea.Document.GetVisibleLine(y);
			if (visibleLine > 0)
			{
				int drawingXPos = textArea.TextView.GetDrawingXPos(y, position.X);
				Rectangle drawingPosition = textArea.TextView.DrawingPosition;
				int num = drawingPosition.Y + (visibleLine - 1) * textArea.TextView.FontHeight;
				Point virtualTop = textArea.TextView.TextArea.VirtualTop;
				Point point = new Point(drawingXPos, num - virtualTop.Y);
				textArea.Caret.Position = textArea.TextView.GetLogicalPosition(point);
				textArea.SetCaretToDesiredColumn();
			}
		}
	}
}