using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Actions
{
	public class ShowDefinitionsOnly : AbstractEditAction
	{
		public ShowDefinitionsOnly()
		{
		}

		public override void Execute(TextArea textArea)
		{
			foreach (FoldMarker foldMarker in textArea.Document.FoldingManager.FoldMarker)
			{
				foldMarker.IsFolded = (foldMarker.FoldType == FoldType.MemberBody ? true : foldMarker.FoldType == FoldType.Region);
			}
			textArea.Document.FoldingManager.NotifyFoldingsChanged(EventArgs.Empty);
		}
	}
}