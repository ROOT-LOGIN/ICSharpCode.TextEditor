using ICSharpCode.TextEditor;
using System;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor.Actions
{
	public interface IEditAction
	{
		System.Windows.Forms.Keys[] Keys
		{
			get;
			set;
		}

		void Execute(TextArea textArea);
	}
}