using System;

namespace ICSharpCode.TextEditor.Document
{
	public class FoldMarker : AbstractSegment, IComparable
	{
		private bool isFolded;

		private string foldText = "...";

		private ICSharpCode.TextEditor.Document.FoldType foldType;

		private IDocument document;

		private int startLine = -1;

		private int startColumn;

		private int endLine = -1;

		private int endColumn;

		public int EndColumn
		{
			get
			{
				if (this.endLine < 0)
				{
					FoldMarker.GetPointForOffset(this.document, this.offset + this.length, out this.endLine, out this.endColumn);
				}
				return this.endColumn;
			}
		}

		public int EndLine
		{
			get
			{
				if (this.endLine < 0)
				{
					FoldMarker.GetPointForOffset(this.document, this.offset + this.length, out this.endLine, out this.endColumn);
				}
				return this.endLine;
			}
		}

		public string FoldText
		{
			get
			{
				return this.foldText;
			}
		}

		public ICSharpCode.TextEditor.Document.FoldType FoldType
		{
			get
			{
				return this.foldType;
			}
			set
			{
				this.foldType = value;
			}
		}

		public string InnerText
		{
			get
			{
				return this.document.GetText(this.offset, this.length);
			}
		}

		public bool IsFolded
		{
			get
			{
				return this.isFolded;
			}
			set
			{
				this.isFolded = value;
			}
		}

		public override int Length
		{
			get
			{
				return base.Length;
			}
			set
			{
				base.Length = value;
				this.endLine = -1;
			}
		}

		public override int Offset
		{
			get
			{
				return base.Offset;
			}
			set
			{
				base.Offset = value;
				this.startLine = -1;
				this.endLine = -1;
			}
		}

		public int StartColumn
		{
			get
			{
				if (this.startLine < 0)
				{
					FoldMarker.GetPointForOffset(this.document, this.offset, out this.startLine, out this.startColumn);
				}
				return this.startColumn;
			}
		}

		public int StartLine
		{
			get
			{
				if (this.startLine < 0)
				{
					FoldMarker.GetPointForOffset(this.document, this.offset, out this.startLine, out this.startColumn);
				}
				return this.startLine;
			}
		}

		public FoldMarker(IDocument document, int offset, int length, string foldText, bool isFolded)
		{
			this.document = document;
			this.offset = offset;
			this.length = length;
			this.foldText = foldText;
			this.isFolded = isFolded;
		}

		public FoldMarker(IDocument document, int startLine, int startColumn, int endLine, int endColumn) : this(document, startLine, startColumn, endLine, endColumn, ICSharpCode.TextEditor.Document.FoldType.Unspecified)
		{
		}

		public FoldMarker(IDocument document, int startLine, int startColumn, int endLine, int endColumn, ICSharpCode.TextEditor.Document.FoldType foldType) : this(document, startLine, startColumn, endLine, endColumn, foldType, "...")
		{
		}

		public FoldMarker(IDocument document, int startLine, int startColumn, int endLine, int endColumn, ICSharpCode.TextEditor.Document.FoldType foldType, string foldText) : this(document, startLine, startColumn, endLine, endColumn, foldType, foldText, false)
		{
		}

		public FoldMarker(IDocument document, int startLine, int startColumn, int endLine, int endColumn, ICSharpCode.TextEditor.Document.FoldType foldType, string foldText, bool isFolded)
		{
			this.document = document;
			startLine = Math.Min(document.TotalNumberOfLines - 1, Math.Max(startLine, 0));
			ISegment lineSegment = document.GetLineSegment(startLine);
			endLine = Math.Min(document.TotalNumberOfLines - 1, Math.Max(endLine, 0));
			ISegment segment = document.GetLineSegment(endLine);
			if (string.IsNullOrEmpty(foldText))
			{
				foldText = "...";
			}
			this.FoldType = foldType;
			this.foldText = foldText;
			this.offset = lineSegment.Offset + Math.Min(startColumn, lineSegment.Length);
			this.length = segment.Offset + Math.Min(endColumn, segment.Length) - this.offset;
			this.isFolded = isFolded;
		}

		public int CompareTo(object o)
		{
			if (!(o is FoldMarker))
			{
				throw new ArgumentException();
			}
			FoldMarker foldMarker = (FoldMarker)o;
			if (this.offset != foldMarker.offset)
			{
				return this.offset.CompareTo(foldMarker.offset);
			}
			return this.length.CompareTo(foldMarker.length);
		}

		private static void GetPointForOffset(IDocument document, int offset, out int line, out int column)
		{
			if (offset > document.TextLength)
			{
				line = document.TotalNumberOfLines + 1;
				column = 1;
				return;
			}
			if (offset < 0)
			{
				line = -1;
				column = -1;
				return;
			}
			line = document.GetLineNumberForOffset(offset);
			column = offset - document.GetLineSegment(line).Offset;
		}
	}
}