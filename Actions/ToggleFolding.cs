using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Actions
{
	public class ToggleFolding : AbstractEditAction
	{
		public ToggleFolding()
		{
		}

		public override void Execute(TextArea textArea)
		{
			List<FoldMarker> foldingsWithStart = textArea.Document.FoldingManager.GetFoldingsWithStart(textArea.Caret.Line);
			if (foldingsWithStart.Count == 0)
			{
				foldingsWithStart = textArea.Document.FoldingManager.GetFoldingsContainsLineNumber(textArea.Caret.Line);
				if (foldingsWithStart.Count != 0)
				{
					FoldMarker item = foldingsWithStart[0];
					for (int i = 1; i < foldingsWithStart.Count; i++)
					{
						if (new TextLocation(foldingsWithStart[i].StartColumn, foldingsWithStart[i].StartLine) > new TextLocation(item.StartColumn, item.StartLine))
						{
							item = foldingsWithStart[i];
						}
					}
					item.IsFolded = !item.IsFolded;
				}
			}
			else
			{
				foreach (FoldMarker isFolded in foldingsWithStart)
				{
					isFolded.IsFolded = !isFolded.IsFolded;
				}
			}
			textArea.Document.FoldingManager.NotifyFoldingsChanged(EventArgs.Empty);
		}
	}
}