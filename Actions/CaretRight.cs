using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Actions
{
	public class CaretRight : AbstractEditAction
	{
		public CaretRight()
		{
		}

		public override void Execute(TextArea textArea)
		{
			LineSegment lineSegment = textArea.Document.GetLineSegment(textArea.Caret.Line);
			TextLocation position = textArea.Caret.Position;
			List<FoldMarker> foldedFoldingsWithStart = textArea.Document.FoldingManager.GetFoldedFoldingsWithStart(position.Y);
			FoldMarker foldMarker = null;
			foreach (FoldMarker foldMarker1 in foldedFoldingsWithStart)
			{
				if (foldMarker1.StartColumn != position.X)
				{
					continue;
				}
				foldMarker = foldMarker1;
				break;
			}
			if (foldMarker != null)
			{
				position.Y = foldMarker.EndLine;
				position.X = foldMarker.EndColumn;
			}
			else if (position.X < lineSegment.Length || textArea.TextEditorProperties.AllowCaretBeyondEOL)
			{
				position.X = position.X + 1;
			}
			else if (position.Y + 1 < textArea.Document.TotalNumberOfLines)
			{
				position.Y = position.Y + 1;
				position.X = 0;
			}
			textArea.Caret.Position = position;
			textArea.SetDesiredColumn();
		}
	}
}