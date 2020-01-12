using ICSharpCode.TextEditor;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class Redo : AbstractEditAction
	{
		public Redo()
		{
		}

		public override void Execute(TextArea textArea)
		{
			textArea.MotherTextEditorControl.Redo();
		}
	}
}