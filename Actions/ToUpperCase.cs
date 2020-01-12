using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class ToUpperCase : AbstractSelectionFormatAction
	{
		public ToUpperCase()
		{
		}

		protected override void Convert(IDocument document, int startOffset, int length)
		{
			string upper = document.GetText(startOffset, length).ToUpper();
			document.Replace(startOffset, length, upper);
		}
	}
}