using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class ConvertSpacesToTabs : AbstractSelectionFormatAction
	{
		public ConvertSpacesToTabs()
		{
		}

		protected override void Convert(IDocument document, int startOffset, int length)
		{
			string text = document.GetText(startOffset, length);
			string str = new string(' ', document.TextEditorProperties.TabIndent);
			document.Replace(startOffset, length, text.Replace(str, "\t"));
		}
	}
}