using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class Paste : AbstractEditAction
	{
		public Paste()
		{
		}

		public override void Execute(TextArea textArea)
		{
			if (textArea.Document.ReadOnly)
			{
				return;
			}
			textArea.ClipboardHandler.Paste(null, null);
		}
	}
}