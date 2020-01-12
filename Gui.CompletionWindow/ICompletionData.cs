using ICSharpCode.TextEditor;
using System;

namespace ICSharpCode.TextEditor.Gui.CompletionWindow
{
	public interface ICompletionData
	{
		string Description
		{
			get;
		}

		int ImageIndex
		{
			get;
		}

		double Priority
		{
			get;
		}

		string Text
		{
			get;
			set;
		}

		bool InsertAction(TextArea textArea, char ch);
	}
}