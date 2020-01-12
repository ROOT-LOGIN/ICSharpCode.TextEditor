using ICSharpCode.TextEditor;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class Undo : AbstractEditAction
	{
		public Undo()
		{
		}

		public override void Execute(TextArea textArea)
		{
			textArea.MotherTextEditorControl.Undo();
		}
	}
}