using ICSharpCode.TextEditor;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class Copy : AbstractEditAction
	{
		public Copy()
		{
		}

		public override void Execute(TextArea textArea)
		{
			textArea.AutoClearSelection = false;
			textArea.ClipboardHandler.Copy(null, null);
		}
	}
}