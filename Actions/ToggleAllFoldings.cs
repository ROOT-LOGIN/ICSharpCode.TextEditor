using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Actions
{
	public class ToggleAllFoldings : AbstractEditAction
	{
		public ToggleAllFoldings()
		{
		}

		public override void Execute(TextArea textArea)
		{
			bool flag = true;
			foreach (FoldMarker foldMarker in textArea.Document.FoldingManager.FoldMarker)
			{
				if (!foldMarker.IsFolded)
				{
					continue;
				}
				flag = false;
				break;
			}
			foreach (FoldMarker foldMarker1 in textArea.Document.FoldingManager.FoldMarker)
			{
				foldMarker1.IsFolded = flag;
			}
			textArea.Document.FoldingManager.NotifyFoldingsChanged(EventArgs.Empty);
		}
	}
}