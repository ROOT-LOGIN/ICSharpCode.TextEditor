using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class Cut : AbstractEditAction
	{
		public Cut()
		{
		}

		public override void Execute(TextArea textArea)
		{
			if (textArea.Document.ReadOnly)
			{
				return;
			}
			textArea.ClipboardHandler.Cut(null, null);
		}
	}
}