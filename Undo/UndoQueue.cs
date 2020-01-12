using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Undo
{
	internal sealed class UndoQueue : IUndoableOperation
	{
		private List<IUndoableOperation> undolist = new List<IUndoableOperation>();

		public UndoQueue(Stack<IUndoableOperation> stack, int numops)
		{
			if (stack == null)
			{
				throw new ArgumentNullException("stack");
			}
			if (numops > stack.Count)
			{
				numops = stack.Count;
			}
			for (int i = 0; i < numops; i++)
			{
				this.undolist.Add(stack.Pop());
			}
		}

		public void Redo()
		{
			for (int i = this.undolist.Count - 1; i >= 0; i--)
			{
				this.undolist[i].Redo();
			}
		}

		public void Undo()
		{
			for (int i = 0; i < this.undolist.Count; i++)
			{
				this.undolist[i].Undo();
			}
		}
	}
}