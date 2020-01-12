using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class ConvertLeadingSpacesToTabs : AbstractLineFormatAction
	{
		public ConvertLeadingSpacesToTabs()
		{
		}

		protected override void Convert(IDocument document, int y1, int y2)
		{
			for (int i = y2; i >= y1; i--)
			{
				LineSegment lineSegment = document.GetLineSegment(i);
				if (lineSegment.Length > 0)
				{
					string tabs = TextUtilities.LeadingWhiteSpaceToTabs(document.GetText(lineSegment.Offset, lineSegment.Length), document.TextEditorProperties.TabIndent);
					document.Replace(lineSegment.Offset, lineSegment.Length, tabs);
				}
			}
		}
	}
}