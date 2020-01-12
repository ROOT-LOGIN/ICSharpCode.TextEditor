using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Undo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ICSharpCode.TextEditor.Document
{
	internal sealed class DefaultDocument : IDocument
	{
		private bool readOnly;

		private ICSharpCode.TextEditor.Document.LineManager lineTrackingStrategy;

		private ICSharpCode.TextEditor.Document.BookmarkManager bookmarkManager;

		private ITextBufferStrategy textBufferStrategy;

		private IFormattingStrategy formattingStrategy;

		private ICSharpCode.TextEditor.Document.FoldingManager foldingManager;

		private ICSharpCode.TextEditor.Undo.UndoStack undoStack = new ICSharpCode.TextEditor.Undo.UndoStack();

		private ITextEditorProperties textEditorProperties = new DefaultTextEditorProperties();

		private ICSharpCode.TextEditor.Document.MarkerStrategy markerStrategy;

		private List<TextAreaUpdate> updateQueue = new List<TextAreaUpdate>();

		public ICSharpCode.TextEditor.Document.BookmarkManager BookmarkManager
		{
			get
			{
				return JustDecompileGenerated_get_BookmarkManager();
			}
			set
			{
				JustDecompileGenerated_set_BookmarkManager(value);
			}
		}

		public ICSharpCode.TextEditor.Document.BookmarkManager JustDecompileGenerated_get_BookmarkManager()
		{
			return this.bookmarkManager;
		}

		public void JustDecompileGenerated_set_BookmarkManager(ICSharpCode.TextEditor.Document.BookmarkManager value)
		{
			this.bookmarkManager = value;
		}

		public ICSharpCode.TextEditor.Document.FoldingManager FoldingManager
		{
			get
			{
				return JustDecompileGenerated_get_FoldingManager();
			}
			set
			{
				JustDecompileGenerated_set_FoldingManager(value);
			}
		}

		public ICSharpCode.TextEditor.Document.FoldingManager JustDecompileGenerated_get_FoldingManager()
		{
			return this.foldingManager;
		}

		public void JustDecompileGenerated_set_FoldingManager(ICSharpCode.TextEditor.Document.FoldingManager value)
		{
			this.foldingManager = value;
		}

		public IFormattingStrategy FormattingStrategy
		{
			get
			{
				return this.formattingStrategy;
			}
			set
			{
				this.formattingStrategy = value;
			}
		}

		public IHighlightingStrategy HighlightingStrategy
		{
			get
			{
				return this.lineTrackingStrategy.HighlightingStrategy;
			}
			set
			{
				this.lineTrackingStrategy.HighlightingStrategy = value;
			}
		}

		public ICSharpCode.TextEditor.Document.LineManager LineManager
		{
			get
			{
				return this.lineTrackingStrategy;
			}
			set
			{
				this.lineTrackingStrategy = value;
			}
		}

		public IList<LineSegment> LineSegmentCollection
		{
			get
			{
				return this.lineTrackingStrategy.LineSegmentCollection;
			}
		}

		public ICSharpCode.TextEditor.Document.MarkerStrategy MarkerStrategy
		{
			get
			{
				return JustDecompileGenerated_get_MarkerStrategy();
			}
			set
			{
				JustDecompileGenerated_set_MarkerStrategy(value);
			}
		}

		public ICSharpCode.TextEditor.Document.MarkerStrategy JustDecompileGenerated_get_MarkerStrategy()
		{
			return this.markerStrategy;
		}

		public void JustDecompileGenerated_set_MarkerStrategy(ICSharpCode.TextEditor.Document.MarkerStrategy value)
		{
			this.markerStrategy = value;
		}

		public bool ReadOnly
		{
			get
			{
				return this.readOnly;
			}
			set
			{
				this.readOnly = value;
			}
		}

		public ITextBufferStrategy TextBufferStrategy
		{
			get
			{
				return JustDecompileGenerated_get_TextBufferStrategy();
			}
			set
			{
				JustDecompileGenerated_set_TextBufferStrategy(value);
			}
		}

		public ITextBufferStrategy JustDecompileGenerated_get_TextBufferStrategy()
		{
			return this.textBufferStrategy;
		}

		public void JustDecompileGenerated_set_TextBufferStrategy(ITextBufferStrategy value)
		{
			this.textBufferStrategy = value;
		}

		public string TextContent
		{
			get
			{
				return this.GetText(0, this.textBufferStrategy.Length);
			}
			set
			{
				this.OnDocumentAboutToBeChanged(new DocumentEventArgs(this, 0, 0, value));
				this.textBufferStrategy.SetContent(value);
				this.lineTrackingStrategy.SetContent(value);
				this.undoStack.ClearAll();
				this.OnDocumentChanged(new DocumentEventArgs(this, 0, 0, value));
				this.OnTextContentChanged(EventArgs.Empty);
			}
		}

		public ITextEditorProperties TextEditorProperties
		{
			get
			{
				return this.textEditorProperties;
			}
			set
			{
				this.textEditorProperties = value;
			}
		}

		public int TextLength
		{
			get
			{
				return this.textBufferStrategy.Length;
			}
		}

		public int TotalNumberOfLines
		{
			get
			{
				return this.lineTrackingStrategy.TotalNumberOfLines;
			}
		}

		public ICSharpCode.TextEditor.Undo.UndoStack UndoStack
		{
			get
			{
				return this.undoStack;
			}
		}

		public List<TextAreaUpdate> UpdateQueue
		{
			get
			{
				return this.updateQueue;
			}
		}

		public DefaultDocument()
		{
		}

		public void CommitUpdate()
		{
			if (this.UpdateCommited != null)
			{
				this.UpdateCommited(this, EventArgs.Empty);
			}
		}

		public char GetCharAt(int offset)
		{
			return this.textBufferStrategy.GetCharAt(offset);
		}

		public int GetFirstLogicalLine(int lineNumber)
		{
			return this.lineTrackingStrategy.GetFirstLogicalLine(lineNumber);
		}

		public int GetLastLogicalLine(int lineNumber)
		{
			return this.lineTrackingStrategy.GetLastLogicalLine(lineNumber);
		}

		public int GetLineNumberForOffset(int offset)
		{
			return this.lineTrackingStrategy.GetLineNumberForOffset(offset);
		}

		public LineSegment GetLineSegment(int line)
		{
			return this.lineTrackingStrategy.GetLineSegment(line);
		}

		public LineSegment GetLineSegmentForOffset(int offset)
		{
			return this.lineTrackingStrategy.GetLineSegmentForOffset(offset);
		}

		public int GetNextVisibleLineAbove(int lineNumber, int lineCount)
		{
			return this.lineTrackingStrategy.GetNextVisibleLineAbove(lineNumber, lineCount);
		}

		public int GetNextVisibleLineBelow(int lineNumber, int lineCount)
		{
			return this.lineTrackingStrategy.GetNextVisibleLineBelow(lineNumber, lineCount);
		}

		public string GetText(int offset, int length)
		{
			return this.textBufferStrategy.GetText(offset, length);
		}

		public string GetText(ISegment segment)
		{
			return this.GetText(segment.Offset, segment.Length);
		}

		public int GetVisibleLine(int lineNumber)
		{
			return this.lineTrackingStrategy.GetVisibleLine(lineNumber);
		}

		public void Insert(int offset, string text)
		{
			if (this.readOnly)
			{
				return;
			}
			this.OnDocumentAboutToBeChanged(new DocumentEventArgs(this, offset, -1, text));
			this.textBufferStrategy.Insert(offset, text);
			this.lineTrackingStrategy.Insert(offset, text);
			this.undoStack.Push(new UndoableInsert(this, offset, text));
			this.OnDocumentChanged(new DocumentEventArgs(this, offset, -1, text));
		}

		public TextLocation OffsetToPosition(int offset)
		{
			int lineNumberForOffset = this.GetLineNumberForOffset(offset);
			LineSegment lineSegment = this.GetLineSegment(lineNumberForOffset);
			return new TextLocation(offset - lineSegment.Offset, lineNumberForOffset);
		}

		private void OnDocumentAboutToBeChanged(DocumentEventArgs e)
		{
			if (this.DocumentAboutToBeChanged != null)
			{
				this.DocumentAboutToBeChanged(this, e);
			}
		}

		private void OnDocumentChanged(DocumentEventArgs e)
		{
			if (this.DocumentChanged != null)
			{
				this.DocumentChanged(this, e);
			}
		}

		private void OnTextContentChanged(EventArgs e)
		{
			if (this.TextContentChanged != null)
			{
				this.TextContentChanged(this, e);
			}
		}

		public int PositionToOffset(TextLocation p)
		{
			if (p.Y >= this.TotalNumberOfLines)
			{
				return 0;
			}
			LineSegment lineSegment = this.GetLineSegment(p.Y);
			return Math.Min(this.TextLength, lineSegment.Offset + Math.Min(lineSegment.Length, p.X));
		}

		public void Remove(int offset, int length)
		{
			if (this.readOnly)
			{
				return;
			}
			this.OnDocumentAboutToBeChanged(new DocumentEventArgs(this, offset, length));
			this.undoStack.Push(new UndoableDelete(this, offset, this.GetText(offset, length)));
			this.textBufferStrategy.Remove(offset, length);
			this.lineTrackingStrategy.Remove(offset, length);
			this.OnDocumentChanged(new DocumentEventArgs(this, offset, length));
		}

		public void Replace(int offset, int length, string text)
		{
			if (this.readOnly)
			{
				return;
			}
			this.OnDocumentAboutToBeChanged(new DocumentEventArgs(this, offset, length, text));
			this.undoStack.Push(new UndoableReplace(this, offset, this.GetText(offset, length), text));
			this.textBufferStrategy.Replace(offset, length, text);
			this.lineTrackingStrategy.Replace(offset, length, text);
			this.OnDocumentChanged(new DocumentEventArgs(this, offset, length, text));
		}

		public void RequestUpdate(TextAreaUpdate update)
		{
			if (this.updateQueue.Count == 1 && this.updateQueue[0].TextAreaUpdateType == TextAreaUpdateType.WholeTextArea)
			{
				return;
			}
			if (update.TextAreaUpdateType == TextAreaUpdateType.WholeTextArea)
			{
				this.updateQueue.Clear();
			}
			this.updateQueue.Add(update);
		}

		public void UpdateSegmentListOnDocumentChange<T>(List<T> list, DocumentEventArgs e)
		where T : ISegment
		{
			int num = (e.Length > 0 ? e.Length : 0);
			int num1 = (e.Text != null ? e.Text.Length : 0);
			for (int i = 0; i < list.Count; i++)
			{
				ISegment item = list[i];
				int offset = item.Offset;
				int offset1 = item.Offset + item.Length;
				if (e.Offset <= offset)
				{
					offset -= num;
					if (offset < e.Offset)
					{
						offset = e.Offset;
					}
				}
				if (e.Offset < offset1)
				{
					offset1 -= num;
					if (offset1 < e.Offset)
					{
						offset1 = e.Offset;
					}
				}
				if (offset != offset1)
				{
					if (e.Offset <= offset)
					{
						offset += num1;
					}
					if (e.Offset < offset1)
					{
						offset1 += num1;
					}
					item.Offset = offset;
					item.Length = offset1 - offset;
				}
				else
				{
					list.RemoveAt(i);
					i--;
				}
			}
		}

		[Conditional("DEBUG")]
		internal static void ValidatePosition(IDocument document, TextLocation position)
		{
			document.GetLineSegment(position.Line);
		}

		public event DocumentEventHandler DocumentAboutToBeChanged;

		public event DocumentEventHandler DocumentChanged;

		public event EventHandler<LineCountChangeEventArgs> LineCountChanged
		{
			add
			{
				this.lineTrackingStrategy.LineCountChanged += value;
			}
			remove
			{
				this.lineTrackingStrategy.LineCountChanged -= value;
			}
		}

		public event EventHandler<LineEventArgs> LineDeleted
		{
			add
			{
				this.lineTrackingStrategy.LineDeleted += value;
			}
			remove
			{
				this.lineTrackingStrategy.LineDeleted -= value;
			}
		}

		public event EventHandler<LineLengthChangeEventArgs> LineLengthChanged
		{
			add
			{
				this.lineTrackingStrategy.LineLengthChanged += value;
			}
			remove
			{
				this.lineTrackingStrategy.LineLengthChanged -= value;
			}
		}

		public event EventHandler TextContentChanged;

		public event EventHandler UpdateCommited;
	}
}