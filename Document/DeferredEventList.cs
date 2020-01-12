using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Document
{
	internal struct DeferredEventList
	{
		internal List<LineSegment> removedLines;

		internal List<TextAnchor> textAnchor;

		public void AddDeletedAnchor(TextAnchor anchor)
		{
			if (this.textAnchor == null)
			{
				this.textAnchor = new List<TextAnchor>();
			}
			this.textAnchor.Add(anchor);
		}

		public void AddRemovedLine(LineSegment line)
		{
			if (this.removedLines == null)
			{
				this.removedLines = new List<LineSegment>();
			}
			this.removedLines.Add(line);
		}

		public void RaiseEvents()
		{
			if (this.textAnchor != null)
			{
				foreach (TextAnchor textAnchor in this.textAnchor)
				{
					textAnchor.RaiseDeleted();
				}
			}
		}
	}
}