using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Actions
{
	public class ToggleComment : AbstractEditAction
	{
		public ToggleComment()
		{
		}

		public override void Execute(TextArea textArea)
		{
			if (textArea.Document.ReadOnly)
			{
				return;
			}
			if (textArea.Document.HighlightingStrategy.Properties.ContainsKey("LineComment"))
			{
				(new ToggleLineComment()).Execute(textArea);
				return;
			}
			if (textArea.Document.HighlightingStrategy.Properties.ContainsKey("BlockCommentBegin") && textArea.Document.HighlightingStrategy.Properties.ContainsKey("BlockCommentBegin"))
			{
				(new ToggleBlockComment()).Execute(textArea);
			}
		}
	}
}