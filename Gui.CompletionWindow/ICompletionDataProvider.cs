using ICSharpCode.TextEditor;
using System;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor.Gui.CompletionWindow
{
	public interface ICompletionDataProvider
	{
		int DefaultIndex
		{
			get;
		}

		System.Windows.Forms.ImageList ImageList
		{
			get;
		}

		string PreSelection
		{
			get;
		}

		ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped);

		bool InsertAction(ICompletionData data, TextArea textArea, int insertionOffset, char key);

		CompletionDataProviderKeyResult ProcessKey(char key);
	}
}