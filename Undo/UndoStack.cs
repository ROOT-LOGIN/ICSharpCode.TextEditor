using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ICSharpCode.TextEditor.Undo
{
	public class UndoStack
	{
		private Stack<IUndoableOperation> undostack = new Stack<IUndoableOperation>();

		private Stack<IUndoableOperation> redostack = new Stack<IUndoableOperation>();

		public TextEditorControlBase TextEditorControl;

		internal bool AcceptChanges = true;

		private int undoGroupDepth;

		private int actionCountInUndoGroup;

		public bool CanRedo
		{
			get
			{
				return this.redostack.Count > 0;
			}
		}

		public bool CanUndo
		{
			get
			{
				return this.undostack.Count > 0;
			}
		}

		public int RedoItemCount
		{
			get
			{
				return this.redostack.Count;
			}
		}

		public int UndoItemCount
		{
			get
			{
				return this.undostack.Count;
			}
		}

		public UndoStack()
		{
		}

		public void AssertNoUndoGroupOpen()
		{
			if (this.undoGroupDepth != 0)
			{
				this.undoGroupDepth = 0;
				throw new InvalidOperationException("No undo group should be open at this point");
			}
		}

		public void ClearAll()
		{
			this.AssertNoUndoGroupOpen();
			this.undostack.Clear();
			this.redostack.Clear();
			this.actionCountInUndoGroup = 0;
		}

		public void ClearRedoStack()
		{
			this.redostack.Clear();
		}

		public void EndUndoGroup()
		{
			if (this.undoGroupDepth == 0)
			{
				throw new InvalidOperationException("There are no open undo groups");
			}
			this.undoGroupDepth--;
			if (this.undoGroupDepth == 0 && this.actionCountInUndoGroup > 1)
			{
				UndoQueue undoQueue = new UndoQueue(this.undostack, this.actionCountInUndoGroup);
				this.undostack.Push(undoQueue);
				if (this.OperationPushed != null)
				{
					this.OperationPushed(this, new OperationEventArgs(undoQueue));
				}
			}
		}

		protected void OnActionRedone()
		{
			if (this.ActionRedone != null)
			{
				this.ActionRedone(null, null);
			}
		}

		protected void OnActionUndone()
		{
			if (this.ActionUndone != null)
			{
				this.ActionUndone(null, null);
			}
		}

		public void Push(IUndoableOperation operation)
		{
			if (operation == null)
			{
				throw new ArgumentNullException("operation");
			}
			if (this.AcceptChanges)
			{
				this.StartUndoGroup();
				this.undostack.Push(operation);
				this.actionCountInUndoGroup++;
				if (this.TextEditorControl != null)
				{
					this.undostack.Push(new UndoStack.UndoableSetCaretPosition(this, this.TextEditorControl.ActiveTextAreaControl.Caret.Position));
					this.actionCountInUndoGroup++;
				}
				this.EndUndoGroup();
				this.ClearRedoStack();
			}
		}

		public void Redo()
		{
			this.AssertNoUndoGroupOpen();
			if (this.redostack.Count > 0)
			{
				IUndoableOperation undoableOperation = this.redostack.Pop();
				this.undostack.Push(undoableOperation);
				undoableOperation.Redo();
				this.OnActionRedone();
			}
		}

		public void StartUndoGroup()
		{
			if (this.undoGroupDepth == 0)
			{
				this.actionCountInUndoGroup = 0;
			}
			this.undoGroupDepth++;
		}

		public void Undo()
		{
			this.AssertNoUndoGroupOpen();
			if (this.undostack.Count > 0)
			{
				IUndoableOperation undoableOperation = this.undostack.Pop();
				this.redostack.Push(undoableOperation);
				undoableOperation.Undo();
				this.OnActionUndone();
			}
		}

		public event EventHandler ActionRedone;

		public event EventHandler ActionUndone;

		public event OperationEventHandler OperationPushed;

		private class UndoableSetCaretPosition : IUndoableOperation
		{
			private UndoStack stack;

			private TextLocation pos;

			private TextLocation redoPos;

			public UndoableSetCaretPosition(UndoStack stack, TextLocation pos)
			{
				this.stack = stack;
				this.pos = pos;
			}

			public void Redo()
			{
				this.stack.TextEditorControl.ActiveTextAreaControl.Caret.Position = this.redoPos;
				this.stack.TextEditorControl.ActiveTextAreaControl.SelectionManager.ClearSelection();
			}

			public void Undo()
			{
				this.redoPos = this.stack.TextEditorControl.ActiveTextAreaControl.Caret.Position;
				this.stack.TextEditorControl.ActiveTextAreaControl.Caret.Position = this.pos;
				this.stack.TextEditorControl.ActiveTextAreaControl.SelectionManager.ClearSelection();
			}
		}
	}
}