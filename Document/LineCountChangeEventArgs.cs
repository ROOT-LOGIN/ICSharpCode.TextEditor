using System;

namespace ICSharpCode.TextEditor.Document
{
	public class LineCountChangeEventArgs : EventArgs
	{
		private IDocument document;

		private int start;

		private int moved;

		public IDocument Document
		{
			get
			{
				return this.document;
			}
		}

		public int LinesMoved
		{
			get
			{
				return this.moved;
			}
		}

		public int LineStart
		{
			get
			{
				return this.start;
			}
		}

		public LineCountChangeEventArgs(IDocument document, int lineStart, int linesMoved)
		{
			this.document = document;
			this.start = lineStart;
			this.moved = linesMoved;
		}
	}
}