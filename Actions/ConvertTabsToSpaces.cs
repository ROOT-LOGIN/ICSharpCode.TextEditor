using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class ConvertTabsToSpaces : AbstractSelectionFormatAction
	{
		public ConvertTabsToSpaces()
		{
		}

		protected override void Convert(IDocument document, int startOffset, int length)
		{
			string text = document.GetText(startOffset, length);
			string str = new string(' ', document.TextEditorProperties.TabIndent);
			document.Replace(startOffset, length, text.Replace("\t", str));
		}
	}
}