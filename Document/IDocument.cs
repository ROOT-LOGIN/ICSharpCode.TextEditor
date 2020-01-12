using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Undo;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ICSharpCode.TextEditor.Document
{
	public interface IDocument
	{
		ICSharpCode.TextEditor.Document.BookmarkManager BookmarkManager
		{
			get;
		}

		ICSharpCode.TextEditor.Document.FoldingManager FoldingManager
		{
			get;
		}

		IFormattingStrategy FormattingStrategy
		{
			get;
			set;
		}

		IHighlightingStrategy HighlightingStrategy
		{
			get;
			set;
		}

		IList<LineSegment> LineSegmentCollection
		{
			get;
		}

		ICSharpCode.TextEditor.Document.MarkerStrategy MarkerStrategy
		{
			get;
		}

		bool ReadOnly
		{
			get;
			set;
		}

		ITextBufferStrategy TextBufferStrategy
		{
			get;
		}

		string TextContent
		{
			get;
			set;
		}

		ITextEditorProperties TextEditorProperties
		{
			get;
			set;
		}

		int TextLength
		{
			get;
		}

		int TotalNumberOfLines
		{
			get;
		}

		ICSharpCode.TextEditor.Undo.UndoStack UndoStack
		{
			get;
		}

		List<TextAreaUpdate> UpdateQueue
		{
			get;
		}

		void CommitUpdate();

		char GetCharAt(int offset);

		int GetFirstLogicalLine(int lineNumber);

		int GetLastLogicalLine(int lineNumber);

		int GetLineNumberForOffset(int offset);

		LineSegment GetLineSegment(int lineNumber);

		LineSegment GetLineSegmentForOffset(int offset);

		int GetNextVisibleLineAbove(int lineNumber, int lineCount);

		int GetNextVisibleLineBelow(int lineNumber, int lineCount);

		string GetText(int offset, int length);

		string GetText(ISegment segment);

		int GetVisibleLine(int lineNumber);

		void Insert(int offset, string text);

		TextLocation OffsetToPosition(int offset);

		int PositionToOffset(TextLocation p);

		void Remove(int offset, int length);

		void Replace(int offset, int length, string text);

		void RequestUpdate(TextAreaUpdate update);

		void UpdateSegmentListOnDocumentChange<T>(List<T> list, DocumentEventArgs e)
		where T : ISegment;

		event DocumentEventHandler DocumentAboutToBeChanged;

		event DocumentEventHandler DocumentChanged;

		event EventHandler<LineCountChangeEventArgs> LineCountChanged;

		event EventHandler<LineEventArgs> LineDeleted;

		event EventHandler<LineLengthChangeEventArgs> LineLengthChanged;

		event EventHandler TextContentChanged;

		event EventHandler UpdateCommited;
	}
}