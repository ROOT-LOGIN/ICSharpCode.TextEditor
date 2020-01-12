using ICSharpCode.TextEditor.Util;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ICSharpCode.TextEditor.Document
{
	public sealed class LineSegment : ISegment
	{
		internal LineSegmentTree.Enumerator treeEntry;

		private int totalLength;

		private int delimiterLength;

		private List<TextWord> words;

		private SpanStack highlightSpanStack;

		private WeakCollection<TextAnchor> anchors;

		public int DelimiterLength
		{
			get
			{
				return this.delimiterLength;
			}
			internal set
			{
				this.delimiterLength = value;
			}
		}

		public SpanStack HighlightSpanStack
		{
			get
			{
				return this.highlightSpanStack;
			}
			set
			{
				this.highlightSpanStack = value;
			}
		}

		int ICSharpCode.TextEditor.Document.ISegment.Length
		{
			get
			{
				return this.Length;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		int ICSharpCode.TextEditor.Document.ISegment.Offset
		{
			get
			{
				return this.Offset;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public bool IsDeleted
		{
			get
			{
				return !this.treeEntry.IsValid;
			}
		}

		public int Length
		{
			get
			{
				return this.totalLength - this.delimiterLength;
			}
		}

		public int LineNumber
		{
			get
			{
				return this.treeEntry.CurrentIndex;
			}
		}

		public int Offset
		{
			get
			{
				return this.treeEntry.CurrentOffset;
			}
		}

		public int TotalLength
		{
			get
			{
				return this.totalLength;
			}
			internal set
			{
				this.totalLength = value;
			}
		}

		public List<TextWord> Words
		{
			get
			{
				return this.words;
			}
			set
			{
				this.words = value;
			}
		}

		public LineSegment()
		{
		}

		private void AddAnchor(TextAnchor anchor)
		{
			if (this.anchors == null)
			{
				this.anchors = new WeakCollection<TextAnchor>();
			}
			this.anchors.Add(anchor);
		}

		public TextAnchor CreateAnchor(int column)
		{
			if (column < 0 || column > this.Length)
			{
				throw new ArgumentOutOfRangeException("column");
			}
			TextAnchor textAnchor = new TextAnchor(this, column);
			this.AddAnchor(textAnchor);
			return textAnchor;
		}

		internal void Deleted(ref DeferredEventList deferredEventList)
		{
			this.treeEntry = LineSegmentTree.Enumerator.Invalid;
			if (this.anchors != null)
			{
				foreach (TextAnchor anchor in this.anchors)
				{
					anchor.Delete(ref deferredEventList);
				}
				this.anchors = null;
			}
		}

		public HighlightColor GetColorForPosition(int x)
		{
			HighlightColor syntaxColor;
			if (this.Words != null)
			{
				int length = 0;
				List<TextWord>.Enumerator enumerator = this.Words.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						TextWord current = enumerator.Current;
						if (x >= length + current.Length)
						{
							length += current.Length;
						}
						else
						{
							syntaxColor = current.SyntaxColor;
							return syntaxColor;
						}
					}
					return new HighlightColor(Color.Black, false, false);
				}
				finally
				{
					((IDisposable)enumerator).Dispose();
				}
				return syntaxColor;
			}
			return new HighlightColor(Color.Black, false, false);
		}

		public TextWord GetWord(int column)
		{
			TextWord textWord;
			int length = 0;
			List<TextWord>.Enumerator enumerator = this.words.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TextWord current = enumerator.Current;
					if (column >= length + current.Length)
					{
						length += current.Length;
					}
					else
					{
						textWord = current;
						return textWord;
					}
				}
				return null;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return textWord;
		}

		internal void InsertedLinePart(int startColumn, int length)
		{
			if (length == 0)
			{
				return;
			}
			if (this.anchors != null)
			{
				foreach (TextAnchor anchor in this.anchors)
				{
					if ((anchor.MovementType == AnchorMovementType.BeforeInsertion ? anchor.ColumnNumber <= startColumn : anchor.ColumnNumber < startColumn))
					{
						continue;
					}
					TextAnchor columnNumber = anchor;
					columnNumber.ColumnNumber = columnNumber.ColumnNumber + length;
				}
			}
		}

		internal void MergedWith(LineSegment deletedLine, int firstLineLength)
		{
			if (deletedLine.anchors != null)
			{
				foreach (TextAnchor anchor in deletedLine.anchors)
				{
					anchor.Line = this;
					this.AddAnchor(anchor);
					TextAnchor columnNumber = anchor;
					columnNumber.ColumnNumber = columnNumber.ColumnNumber + firstLineLength;
				}
				deletedLine.anchors = null;
			}
		}

		internal void RemovedLinePart(ref DeferredEventList deferredEventList, int startColumn, int length)
		{
			if (length == 0)
			{
				return;
			}
			if (this.anchors != null)
			{
				List<TextAnchor> textAnchors = null;
				foreach (TextAnchor anchor in this.anchors)
				{
					if (anchor.ColumnNumber <= startColumn)
					{
						continue;
					}
					if (anchor.ColumnNumber < startColumn + length)
					{
						if (textAnchors == null)
						{
							textAnchors = new List<TextAnchor>();
						}
						anchor.Delete(ref deferredEventList);
						textAnchors.Add(anchor);
					}
					else
					{
						TextAnchor columnNumber = anchor;
						columnNumber.ColumnNumber = columnNumber.ColumnNumber - length;
					}
				}
				if (textAnchors != null)
				{
					foreach (TextAnchor textAnchor in textAnchors)
					{
						this.anchors.Remove(textAnchor);
					}
				}
			}
		}

		internal void SplitTo(LineSegment followingLine)
		{
			if (this.anchors != null)
			{
				List<TextAnchor> textAnchors = null;
				foreach (TextAnchor anchor in this.anchors)
				{
					if ((anchor.MovementType == AnchorMovementType.BeforeInsertion ? anchor.ColumnNumber <= this.Length : anchor.ColumnNumber < this.Length))
					{
						continue;
					}
					anchor.Line = followingLine;
					followingLine.AddAnchor(anchor);
					TextAnchor columnNumber = anchor;
					columnNumber.ColumnNumber = columnNumber.ColumnNumber - this.Length;
					if (textAnchors == null)
					{
						textAnchors = new List<TextAnchor>();
					}
					textAnchors.Add(anchor);
				}
				if (textAnchors != null)
				{
					foreach (TextAnchor textAnchor in textAnchors)
					{
						this.anchors.Remove(textAnchor);
					}
				}
			}
		}

		public override string ToString()
		{
			if (this.IsDeleted)
			{
				object[] length = new object[] { "[LineSegment: (deleted) Length = ", this.Length, ", TotalLength = ", this.TotalLength, ", DelimiterLength = ", this.delimiterLength, "]" };
				return string.Concat(length);
			}
			object[] lineNumber = new object[] { "[LineSegment: LineNumber=", this.LineNumber, ", Offset = ", this.Offset, ", Length = ", this.Length, ", TotalLength = ", this.TotalLength, ", DelimiterLength = ", this.delimiterLength, "]" };
			return string.Concat(lineNumber);
		}
	}
}