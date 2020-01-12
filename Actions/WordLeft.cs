using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Actions
{
	public class WordLeft : CaretLeft
	{
		public WordLeft()
		{
		}

		public override void Execute(TextArea textArea)
		{
			TextLocation position = textArea.Caret.Position;
			if (textArea.Caret.Column == 0)
			{
				base.Execute(textArea);
				return;
			}
			textArea.Document.GetLineSegment(textArea.Caret.Position.Y);
			int num = TextUtilities.FindPrevWordStart(textArea.Document, textArea.Caret.Offset);
			TextLocation textLocation = textArea.Document.OffsetToPosition(num);
			foreach (FoldMarker foldingsFromPosition in textArea.Document.FoldingManager.GetFoldingsFromPosition(textLocation.Y, textLocation.X))
			{
				if (!foldingsFromPosition.IsFolded)
				{
					continue;
				}
				if (position.X != foldingsFromPosition.EndColumn || position.Y != foldingsFromPosition.EndLine)
				{
					textLocation = new TextLocation(foldingsFromPosition.EndColumn, foldingsFromPosition.EndLine);
					break;
				}
				else
				{
					textLocation = new TextLocation(foldingsFromPosition.StartColumn, foldingsFromPosition.StartLine);
					break;
				}
			}
			textArea.Caret.Position = textLocation;
			textArea.SetDesiredColumn();
		}
	}
}