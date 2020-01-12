using System;

namespace ICSharpCode.TextEditor.Document
{
	public class LineEventArgs : EventArgs
	{
		private IDocument document;

		private ICSharpCode.TextEditor.Document.LineSegment lineSegment;

		public IDocument Document
		{
			get
			{
				return this.document;
			}
		}

		public ICSharpCode.TextEditor.Document.LineSegment LineSegment
		{
			get
			{
				return this.lineSegment;
			}
		}

		public LineEventArgs(IDocument document, ICSharpCode.TextEditor.Document.LineSegment lineSegment)
		{
			this.document = document;
			this.lineSegment = lineSegment;
		}

		public override string ToString()
		{
			return string.Format("[LineEventArgs Document={0} LineSegment={1}]", this.document, this.lineSegment);
		}
	}
}