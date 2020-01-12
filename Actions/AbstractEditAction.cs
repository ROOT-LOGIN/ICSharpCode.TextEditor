using ICSharpCode.TextEditor;
using System;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor.Actions
{
	public abstract class AbstractEditAction : IEditAction
	{
		private System.Windows.Forms.Keys[] keys;

		public System.Windows.Forms.Keys[] Keys
		{
			get
			{
				return this.keys;
			}
			set
			{
				this.keys = value;
			}
		}

		protected AbstractEditAction()
		{
		}

		public abstract void Execute(TextArea textArea);
	}
}