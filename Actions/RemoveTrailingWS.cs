using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class RemoveTrailingWS : AbstractLineFormatAction
	{
		public RemoveTrailingWS()
		{
		}

		protected override void Convert(IDocument document, int y1, int y2)
		{
			for (int i = y2 - 1; i >= y1; i--)
			{
				LineSegment lineSegment = document.GetLineSegment(i);
				int num = 0;
				for (int j = lineSegment.Offset + lineSegment.Length - 1; j >= lineSegment.Offset && char.IsWhiteSpace(document.GetCharAt(j)); j--)
				{
					num++;
				}
				if (num > 0)
				{
					document.Remove(lineSegment.Offset + lineSegment.Length - num, num);
				}
			}
		}
	}
}