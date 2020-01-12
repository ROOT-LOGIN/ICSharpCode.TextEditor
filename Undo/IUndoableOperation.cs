using System;

namespace ICSharpCode.TextEditor.Undo
{
	public interface IUndoableOperation
	{
		void Redo();

		void Undo();
	}
}