using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class ConvertLeadingTabsToSpaces : AbstractLineFormatAction
	{
		public ConvertLeadingTabsToSpaces()
		{
		}

		protected override void Convert(IDocument document, int y1, int y2)
		{
			for (int i = y2; i >= y1; i--)
			{
				LineSegment lineSegment = document.GetLineSegment(i);
				if (lineSegment.Length > 0)
				{
					int num = 0;
					num = 0;
					while (num < lineSegment.Length && char.IsWhiteSpace(document.GetCharAt(lineSegment.Offset + num)))
					{
						num++;
					}
					if (num > 0)
					{
						string text = document.GetText(lineSegment.Offset, num);
						string str = text.Replace("\t", new string(' ', document.TextEditorProperties.TabIndent));
						document.Replace(lineSegment.Offset, num, str);
					}
				}
			}
		}
	}
}