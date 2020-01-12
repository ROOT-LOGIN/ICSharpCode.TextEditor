using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Actions
{
	public class Home : AbstractEditAction
	{
		public Home()
		{
		}

		public override void Execute(TextArea textArea)
		{
			TextLocation position = textArea.Caret.Position;
			bool flag = false;
			do
			{
				LineSegment lineSegment = textArea.Document.GetLineSegment(position.Y);
				if (!TextUtilities.IsEmptyLine(textArea.Document, position.Y))
				{
					int firstNonWSChar = TextUtilities.GetFirstNonWSChar(textArea.Document, lineSegment.Offset);
					int offset = firstNonWSChar - lineSegment.Offset;
					if (position.X != offset)
					{
						position.X = offset;
					}
					else
					{
						position.X = 0;
					}
				}
				else if (position.X == 0)
				{
					position.X = lineSegment.Length;
				}
				else
				{
					position.X = 0;
				}
				List<FoldMarker> foldingsFromPosition = textArea.Document.FoldingManager.GetFoldingsFromPosition(position.Y, position.X);
				flag = false;
				foreach (FoldMarker foldMarker in foldingsFromPosition)
				{
					if (!foldMarker.IsFolded)
					{
						continue;
					}
					position = new TextLocation(foldMarker.StartColumn, foldMarker.StartLine);
					flag = true;
					break;
				}
			}
			while (flag);
			if (position != textArea.Caret.Position)
			{
				textArea.Caret.Position = position;
				textArea.SetDesiredColumn();
			}
		}
	}
}