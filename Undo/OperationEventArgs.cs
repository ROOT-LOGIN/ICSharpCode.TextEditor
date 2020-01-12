using System;

namespace ICSharpCode.TextEditor.Undo
{
	public class OperationEventArgs : EventArgs
	{
		private IUndoableOperation op;

		public IUndoableOperation Operation
		{
			get
			{
				return this.op;
			}
		}

		public OperationEventArgs(IUndoableOperation op)
		{
			this.op = op;
		}
	}
}