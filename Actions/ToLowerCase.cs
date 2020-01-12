using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class ToLowerCase : AbstractSelectionFormatAction
	{
		public ToLowerCase()
		{
		}

		protected override void Convert(IDocument document, int startOffset, int length)
		{
			string lower = document.GetText(startOffset, length).ToLower();
			document.Replace(startOffset, length, lower);
		}
	}
}