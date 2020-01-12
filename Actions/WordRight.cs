using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Actions
{
	public class WordRight : CaretRight
	{
		public WordRight()
		{
		}

		public override void Execute(TextArea textArea)
		{
			TextLocation position;
			LineSegment lineSegment = textArea.Document.GetLineSegment(textArea.Caret.Position.Y);
			TextLocation textLocation = textArea.Caret.Position;
			if (textArea.Caret.Column < lineSegment.Length)
			{
				int num = TextUtilities.FindNextWordStart(textArea.Document, textArea.Caret.Offset);
				position = textArea.Document.OffsetToPosition(num);
			}
			else
			{
				position = new TextLocation(0, textArea.Caret.Line + 1);
			}
			foreach (FoldMarker foldingsFromPosition in textArea.Document.FoldingManager.GetFoldingsFromPosition(position.Y, position.X))
			{
				if (!foldingsFromPosition.IsFolded)
				{
					continue;
				}
				if (textLocation.X != foldingsFromPosition.StartColumn || textLocation.Y != foldingsFromPosition.StartLine)
				{
					position = new TextLocation(foldingsFromPosition.StartColumn, foldingsFromPosition.StartLine);
					break;
				}
				else
				{
					position = new TextLocation(foldingsFromPosition.EndColumn, foldingsFromPosition.EndLine);
					break;
				}
			}
			textArea.Caret.Position = position;
			textArea.SetDesiredColumn();
		}
	}
}