using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ICSharpCode.TextEditor.Document
{
	internal sealed class LineManager
	{
		private LineSegmentTree lineCollection = new LineSegmentTree();

		private IDocument document;

		private IHighlightingStrategy highlightingStrategy;

		private LineManager.DelimiterSegment delimiterSegment = new LineManager.DelimiterSegment();

		public IHighlightingStrategy HighlightingStrategy
		{
			get
			{
				return this.highlightingStrategy;
			}
			set
			{
				if (this.highlightingStrategy != value)
				{
					this.highlightingStrategy = value;
					if (this.highlightingStrategy != null)
					{
						this.highlightingStrategy.MarkTokens(this.document);
					}
				}
			}
		}

		public IList<LineSegment> LineSegmentCollection
		{
			get
			{
				return this.lineCollection;
			}
		}

		public int TotalNumberOfLines
		{
			get
			{
				return this.lineCollection.Count;
			}
		}

		public LineManager(IDocument document, IHighlightingStrategy highlightingStrategy)
		{
			this.document = document;
			this.highlightingStrategy = highlightingStrategy;
		}

		public int GetFirstLogicalLine(int visibleLineNumber)
		{
			if (!this.document.TextEditorProperties.EnableFolding)
			{
				return visibleLineNumber;
			}
			int startLine = 0;
			int endLine = 0;
			List<FoldMarker> topLevelFoldedFoldings = this.document.FoldingManager.GetTopLevelFoldedFoldings();
			foreach (FoldMarker topLevelFoldedFolding in topLevelFoldedFoldings)
			{
				if (topLevelFoldedFolding.StartLine < endLine)
				{
					continue;
				}
				if (startLine + topLevelFoldedFolding.StartLine - endLine >= visibleLineNumber)
				{
					break;
				}
				startLine = startLine + (topLevelFoldedFolding.StartLine - endLine);
				endLine = topLevelFoldedFolding.EndLine;
			}
			topLevelFoldedFoldings.Clear();
			topLevelFoldedFoldings = null;
			return endLine + visibleLineNumber - startLine;
		}

		public int GetLastLogicalLine(int visibleLineNumber)
		{
			if (!this.document.TextEditorProperties.EnableFolding)
			{
				return visibleLineNumber;
			}
			return this.GetFirstLogicalLine(visibleLineNumber + 1) - 1;
		}

		public int GetLineNumberForOffset(int offset)
		{
			return this.GetLineSegmentForOffset(offset).LineNumber;
		}

		public LineSegment GetLineSegment(int lineNr)
		{
			return this.lineCollection[lineNr];
		}

		public LineSegment GetLineSegmentForOffset(int offset)
		{
			return this.lineCollection.GetByOffset(offset);
		}

		public int GetNextVisibleLineAbove(int lineNumber, int lineCount)
		{
			int num = lineNumber;
			if (!this.document.TextEditorProperties.EnableFolding)
			{
				num += lineCount;
			}
			else
			{
				int num1 = 0;
				while (num1 < lineCount)
				{
					if (num < this.TotalNumberOfLines)
					{
						num++;
						while (num < this.TotalNumberOfLines && (num >= this.lineCollection.Count || !this.document.FoldingManager.IsLineVisible(num)))
						{
							num++;
						}
						num1++;
					}
					else
					{
						break;
					}
				}
			}
			return Math.Min(this.TotalNumberOfLines - 1, num);
		}

		public int GetNextVisibleLineBelow(int lineNumber, int lineCount)
		{
			int num = lineNumber;
			if (!this.document.TextEditorProperties.EnableFolding)
			{
				num -= lineCount;
			}
			else
			{
				for (int i = 0; i < lineCount; i++)
				{
					num--;
					while (num >= 0 && !this.document.FoldingManager.IsLineVisible(num))
					{
						num--;
					}
				}
			}
			return Math.Max(0, num);
		}

		public int GetVisibleLine(int logicalLineNumber)
		{
			int num;
			if (!this.document.TextEditorProperties.EnableFolding)
			{
				return logicalLineNumber;
			}
			int startLine = 0;
			int endLine = 0;
			List<FoldMarker>.Enumerator enumerator = this.document.FoldingManager.GetTopLevelFoldedFoldings().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					FoldMarker current = enumerator.Current;
					if (current.StartLine >= logicalLineNumber)
					{
						break;
					}
					if (current.StartLine < endLine)
					{
						continue;
					}
					startLine = startLine + (current.StartLine - endLine);
					if (current.EndLine <= logicalLineNumber)
					{
						endLine = current.EndLine;
					}
					else
					{
						num = startLine;
						return num;
					}
				}
				startLine = startLine + (logicalLineNumber - endLine);
				return startLine;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return num;
		}

		public void Insert(int offset, string text)
		{
			this.Replace(offset, 0, text);
		}

		private void InsertInternal(int offset, string text)
		{
			LineSegment byOffset = this.lineCollection.GetByOffset(offset);
			LineManager.DelimiterSegment delimiterSegment = this.NextDelimiter(text, 0);
			if (delimiterSegment == null)
			{
				byOffset.InsertedLinePart(offset - byOffset.Offset, text.Length);
				this.SetSegmentLength(byOffset, byOffset.TotalLength + text.Length);
				return;
			}
			LineSegment lineSegment = byOffset;
			lineSegment.InsertedLinePart(offset - lineSegment.Offset, delimiterSegment.Offset);
			int num = 0;
			while (delimiterSegment != null)
			{
				int num1 = offset + delimiterSegment.Offset + delimiterSegment.Length;
				int num2 = byOffset.Offset;
				int totalLength = num2 + byOffset.TotalLength - (offset + num);
				this.lineCollection.SetSegmentLength(byOffset, num1 - num2);
				LineSegment lineSegment1 = this.lineCollection.InsertSegmentAfter(byOffset, totalLength);
				byOffset.DelimiterLength = delimiterSegment.Length;
				byOffset = lineSegment1;
				num = delimiterSegment.Offset + delimiterSegment.Length;
				delimiterSegment = this.NextDelimiter(text, num);
			}
			lineSegment.SplitTo(byOffset);
			if (num != text.Length)
			{
				byOffset.InsertedLinePart(0, text.Length - num);
				this.SetSegmentLength(byOffset, byOffset.TotalLength + text.Length - num);
			}
		}

		private LineManager.DelimiterSegment NextDelimiter(string text, int offset)
		{
			int num = offset;
			while (true)
			{
				if (num >= text.Length)
				{
					return null;
				}
				char chr = text[num];
				if (chr == '\n')
				{
					break;
				}
				if (chr != '\r')
				{
					num++;
				}
				else
				{
					if (num + 1 >= text.Length || text[num + 1] != '\n')
					{
						break;
					}
					this.delimiterSegment.Offset = num;
					this.delimiterSegment.Length = 2;
					return this.delimiterSegment;
				}
			}
			this.delimiterSegment.Offset = num;
			this.delimiterSegment.Length = 1;
			return this.delimiterSegment;
		}

		private void OnLineCountChanged(LineCountChangeEventArgs e)
		{
			if (this.LineCountChanged != null)
			{
				this.LineCountChanged(this, e);
			}
		}

		private void OnLineDeleted(LineEventArgs e)
		{
			if (this.LineDeleted != null)
			{
				this.LineDeleted(this, e);
			}
		}

		private void OnLineLengthChanged(LineLengthChangeEventArgs e)
		{
			if (this.LineLengthChanged != null)
			{
				this.LineLengthChanged(this, e);
			}
		}

		public void Remove(int offset, int length)
		{
			this.Replace(offset, length, string.Empty);
		}

		private void RemoveInternal(ref DeferredEventList deferredEventList, int offset, int length)
		{
			LineSegment current;
			if (length == 0)
			{
				return;
			}
			LineSegmentTree.Enumerator enumeratorForOffset = this.lineCollection.GetEnumeratorForOffset(offset);
			LineSegment delimiterLength = enumeratorForOffset.Current;
			int num = delimiterLength.Offset;
			if (offset + length < num + delimiterLength.TotalLength)
			{
				delimiterLength.RemovedLinePart(ref deferredEventList, offset - num, length);
				this.SetSegmentLength(delimiterLength, delimiterLength.TotalLength - length);
				return;
			}
			int totalLength = num + delimiterLength.TotalLength - offset;
			delimiterLength.RemovedLinePart(ref deferredEventList, offset - num, totalLength);
			LineSegment byOffset = this.lineCollection.GetByOffset(offset + length);
			if (byOffset == delimiterLength)
			{
				this.SetSegmentLength(delimiterLength, delimiterLength.TotalLength - length);
				return;
			}
			int num1 = byOffset.Offset;
			int totalLength1 = num1 + byOffset.TotalLength - (offset + length);
			byOffset.RemovedLinePart(ref deferredEventList, 0, byOffset.TotalLength - totalLength1);
			delimiterLength.MergedWith(byOffset, offset - num);
			this.SetSegmentLength(delimiterLength, delimiterLength.TotalLength - totalLength + totalLength1);
			delimiterLength.DelimiterLength = byOffset.DelimiterLength;
			enumeratorForOffset.MoveNext();
			do
			{
				current = enumeratorForOffset.Current;
				enumeratorForOffset.MoveNext();
				this.lineCollection.RemoveSegment(current);
				current.Deleted(ref deferredEventList);
			}
			while (current != byOffset);
		}

		public void Replace(int offset, int length, string text)
		{
			int lineNumberForOffset = this.GetLineNumberForOffset(offset);
			int totalNumberOfLines = this.TotalNumberOfLines;
			DeferredEventList deferredEventList = new DeferredEventList();
			this.RemoveInternal(ref deferredEventList, offset, length);
			int num = this.TotalNumberOfLines;
			if (!string.IsNullOrEmpty(text))
			{
				this.InsertInternal(offset, text);
			}
			this.RunHighlighter(lineNumberForOffset, 1 + Math.Max(0, this.TotalNumberOfLines - num));
			if (deferredEventList.removedLines != null)
			{
				foreach (LineSegment removedLine in deferredEventList.removedLines)
				{
					this.OnLineDeleted(new LineEventArgs(this.document, removedLine));
				}
			}
			deferredEventList.RaiseEvents();
			if (this.TotalNumberOfLines != totalNumberOfLines)
			{
				this.OnLineCountChanged(new LineCountChangeEventArgs(this.document, lineNumberForOffset, this.TotalNumberOfLines - totalNumberOfLines));
			}
		}

		private void RunHighlighter(int firstLine, int lineCount)
		{
			if (this.highlightingStrategy != null)
			{
				List<LineSegment> lineSegments = new List<LineSegment>();
				LineSegmentTree.Enumerator enumeratorForIndex = this.lineCollection.GetEnumeratorForIndex(firstLine);
				for (int i = 0; i < lineCount && enumeratorForIndex.IsValid; i++)
				{
					lineSegments.Add(enumeratorForIndex.Current);
					enumeratorForIndex.MoveNext();
				}
				this.highlightingStrategy.MarkTokens(this.document, lineSegments);
			}
		}

		public void SetContent(string text)
		{
			this.lineCollection.Clear();
			if (text != null)
			{
				this.Replace(0, 0, text);
			}
		}

		private void SetSegmentLength(LineSegment segment, int newTotalLength)
		{
			int num = newTotalLength - segment.TotalLength;
			if (num != 0)
			{
				this.lineCollection.SetSegmentLength(segment, newTotalLength);
				this.OnLineLengthChanged(new LineLengthChangeEventArgs(this.document, segment, num));
			}
		}

		public event EventHandler<LineCountChangeEventArgs> LineCountChanged;

		public event EventHandler<LineEventArgs> LineDeleted;

		public event EventHandler<LineLengthChangeEventArgs> LineLengthChanged;

		private sealed class DelimiterSegment
		{
			internal int Offset;

			internal int Length;

			public DelimiterSegment()
			{
			}
		}
	}
}