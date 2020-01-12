using ICSharpCode.TextEditor;
using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Document
{
	public sealed class MarkerStrategy
	{
		private List<ICSharpCode.TextEditor.Document.TextMarker> textMarker = new List<ICSharpCode.TextEditor.Document.TextMarker>();

		private IDocument document;

		private Dictionary<int, List<ICSharpCode.TextEditor.Document.TextMarker>> markersTable = new Dictionary<int, List<ICSharpCode.TextEditor.Document.TextMarker>>();

		public IDocument Document
		{
			get
			{
				return this.document;
			}
		}

		public IEnumerable<ICSharpCode.TextEditor.Document.TextMarker> TextMarker
		{
			get
			{
				return this.textMarker.AsReadOnly();
			}
		}

		public MarkerStrategy(IDocument document)
		{
			this.document = document;
			document.DocumentChanged += new DocumentEventHandler(this.DocumentChanged);
		}

		public void AddMarker(ICSharpCode.TextEditor.Document.TextMarker item)
		{
			this.markersTable.Clear();
			this.textMarker.Add(item);
		}

		private void DocumentChanged(object sender, DocumentEventArgs e)
		{
			this.markersTable.Clear();
			this.document.UpdateSegmentListOnDocumentChange<ICSharpCode.TextEditor.Document.TextMarker>(this.textMarker, e);
		}

		public List<ICSharpCode.TextEditor.Document.TextMarker> GetMarkers(int offset)
		{
			if (!this.markersTable.ContainsKey(offset))
			{
				List<ICSharpCode.TextEditor.Document.TextMarker> textMarkers = new List<ICSharpCode.TextEditor.Document.TextMarker>();
				for (int i = 0; i < this.textMarker.Count; i++)
				{
					ICSharpCode.TextEditor.Document.TextMarker item = this.textMarker[i];
					if (item.Offset <= offset && offset <= item.EndOffset)
					{
						textMarkers.Add(item);
					}
				}
				this.markersTable[offset] = textMarkers;
			}
			return this.markersTable[offset];
		}

		public List<ICSharpCode.TextEditor.Document.TextMarker> GetMarkers(int offset, int length)
		{
			int num = offset + length - 1;
			List<ICSharpCode.TextEditor.Document.TextMarker> textMarkers = new List<ICSharpCode.TextEditor.Document.TextMarker>();
			for (int i = 0; i < this.textMarker.Count; i++)
			{
				ICSharpCode.TextEditor.Document.TextMarker item = this.textMarker[i];
				if (item.Offset <= offset && offset <= item.EndOffset || item.Offset <= num && num <= item.EndOffset || offset <= item.Offset && item.Offset <= num || offset <= item.EndOffset && item.EndOffset <= num)
				{
					textMarkers.Add(item);
				}
			}
			return textMarkers;
		}

		public List<ICSharpCode.TextEditor.Document.TextMarker> GetMarkers(TextLocation position)
		{
			if (position.Y >= this.document.TotalNumberOfLines || position.Y < 0)
			{
				return new List<ICSharpCode.TextEditor.Document.TextMarker>();
			}
			LineSegment lineSegment = this.document.GetLineSegment(position.Y);
			return this.GetMarkers(lineSegment.Offset + position.X);
		}

		public void InsertMarker(int index, ICSharpCode.TextEditor.Document.TextMarker item)
		{
			this.markersTable.Clear();
			this.textMarker.Insert(index, item);
		}

		public void RemoveAll(Predicate<ICSharpCode.TextEditor.Document.TextMarker> match)
		{
			this.markersTable.Clear();
			this.textMarker.RemoveAll(match);
		}

		public void RemoveMarker(ICSharpCode.TextEditor.Document.TextMarker item)
		{
			this.markersTable.Clear();
			this.textMarker.Remove(item);
		}
	}
}