using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Actions
{
	public class CaretLeft : AbstractEditAction
	{
		public CaretLeft()
		{
		}

		public override void Execute(TextArea textArea)
		{
			TextLocation position = textArea.Caret.Position;
			List<FoldMarker> foldedFoldingsWithEnd = textArea.Document.FoldingManager.GetFoldedFoldingsWithEnd(position.Y);
			FoldMarker foldMarker = null;
			foreach (FoldMarker foldMarker1 in foldedFoldingsWithEnd)
			{
				if (foldMarker1.EndColumn != position.X)
				{
					continue;
				}
				foldMarker = foldMarker1;
				break;
			}
			if (foldMarker != null)
			{
				position.Y = foldMarker.StartLine;
				position.X = foldMarker.StartColumn;
			}
			else if (position.X > 0)
			{
				position.X = position.X - 1;
			}
			else if (position.Y > 0)
			{
				LineSegment lineSegment = textArea.Document.GetLineSegment(position.Y - 1);
				position = new TextLocation(lineSegment.Length, position.Y - 1);
			}
			textArea.Caret.Position = position;
			textArea.SetDesiredColumn();
		}
	}
}