using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Actions
{
	public class End : AbstractEditAction
	{
		public End()
		{
		}

		public override void Execute(TextArea textArea)
		{
			TextLocation position = textArea.Caret.Position;
			bool flag = false;
			do
			{
				LineSegment lineSegment = textArea.Document.GetLineSegment(position.Y);
				position.X = lineSegment.Length;
				List<FoldMarker> foldingsFromPosition = textArea.Document.FoldingManager.GetFoldingsFromPosition(position.Y, position.X);
				flag = false;
				foreach (FoldMarker foldMarker in foldingsFromPosition)
				{
					if (!foldMarker.IsFolded)
					{
						continue;
					}
					position = new TextLocation(foldMarker.EndColumn, foldMarker.EndLine);
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