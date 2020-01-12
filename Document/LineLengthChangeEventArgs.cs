using System;

namespace ICSharpCode.TextEditor.Document
{
	public class LineLengthChangeEventArgs : LineEventArgs
	{
		private int lengthDelta;

		public int LengthDelta
		{
			get
			{
				return this.lengthDelta;
			}
		}

		public LineLengthChangeEventArgs(IDocument document, ICSharpCode.TextEditor.Document.LineSegment lineSegment, int moved) : base(document, lineSegment)
		{
			this.lengthDelta = moved;
		}

		public override string ToString()
		{
			return string.Format("[LineLengthEventArgs Document={0} LineSegment={1} LengthDelta={2}]", base.Document, base.LineSegment, this.lengthDelta);
		}
	}
}