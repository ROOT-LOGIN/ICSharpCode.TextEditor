using ICSharpCode.TextEditor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;

namespace ICSharpCode.TextEditor.Document
{
	public class SelectionManager : IDisposable
	{
		private TextLocation selectionStart;

		private IDocument document;

		private TextArea textArea;

		internal SelectFrom selectFrom = new SelectFrom();

		internal List<ISelection> selectionCollection = new List<ISelection>();

		public bool HasSomethingSelected
		{
			get
			{
				return this.selectionCollection.Count > 0;
			}
		}

		public string SelectedText
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (ISelection selection in this.selectionCollection)
				{
					stringBuilder.Append(selection.SelectedText);
				}
				return stringBuilder.ToString();
			}
		}

		public List<ISelection> SelectionCollection
		{
			get
			{
				return this.selectionCollection;
			}
		}

		public bool SelectionIsReadonly
		{
			get
			{
				bool flag;
				if (this.document.ReadOnly)
				{
					return true;
				}
				List<ISelection>.Enumerator enumerator = this.selectionCollection.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						ISelection current = enumerator.Current;
						if (!SelectionManager.SelectionIsReadOnly(this.document, current))
						{
							continue;
						}
						flag = true;
						return flag;
					}
					return false;
				}
				finally
				{
					((IDisposable)enumerator).Dispose();
				}
				return flag;
			}
		}

		internal TextLocation SelectionStart
		{
			get
			{
				return this.selectionStart;
			}
			set
			{
				this.selectionStart = value;
			}
		}

		public SelectionManager(IDocument document)
		{
			this.document = document;
			document.DocumentChanged += new DocumentEventHandler(this.DocumentChanged);
		}

		public SelectionManager(IDocument document, TextArea textArea)
		{
			this.document = document;
			this.textArea = textArea;
			document.DocumentChanged += new DocumentEventHandler(this.DocumentChanged);
		}

		public void ClearSelection()
		{
			Point point = this.textArea.mousepos;
			this.selectFrom.first = this.selectFrom.@where;
			TextView textView = this.textArea.TextView;
			int x = point.X - this.textArea.TextView.DrawingPosition.X;
			int y = point.Y;
			Rectangle drawingPosition = this.textArea.TextView.DrawingPosition;
			TextLocation logicalPosition = textView.GetLogicalPosition(x, y - drawingPosition.Y);
			if (this.selectFrom.@where == 1)
			{
				logicalPosition.X = 0;
			}
			if (logicalPosition.Line >= this.document.TotalNumberOfLines)
			{
				logicalPosition.Line = this.document.TotalNumberOfLines - 1;
				logicalPosition.Column = this.document.GetLineSegment(this.document.TotalNumberOfLines - 1).Length;
			}
			this.SelectionStart = logicalPosition;
			this.ClearWithoutUpdate();
			this.document.CommitUpdate();
		}

		private void ClearWithoutUpdate()
		{
			while (this.selectionCollection.Count > 0)
			{
				ISelection item = this.selectionCollection[this.selectionCollection.Count - 1];
				this.selectionCollection.RemoveAt(this.selectionCollection.Count - 1);
				IDocument document = this.document;
				int y = item.StartPosition.Y;
				TextLocation endPosition = item.EndPosition;
				document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.LinesBetween, y, endPosition.Y));
				this.OnSelectionChanged(EventArgs.Empty);
			}
		}

		public void Dispose()
		{
			if (this.document != null)
			{
				this.document.DocumentChanged -= new DocumentEventHandler(this.DocumentChanged);
				this.document = null;
			}
		}

		private void DocumentChanged(object sender, DocumentEventArgs e)
		{
			if (e.Text == null)
			{
				this.Remove(e.Offset, e.Length);
				return;
			}
			if (e.Length < 0)
			{
				this.Insert(e.Offset, e.Text);
				return;
			}
			this.Replace(e.Offset, e.Length, e.Text);
		}

		public void ExtendSelection(TextLocation oldPosition, TextLocation newPosition)
		{
			TextLocation textLocation;
			TextLocation textLocation1;
			if (oldPosition == newPosition)
			{
				return;
			}
			int x = newPosition.X;
			if (!this.GreaterEqPos(oldPosition, newPosition))
			{
				textLocation = oldPosition;
				textLocation1 = newPosition;
			}
			else
			{
				textLocation = newPosition;
				textLocation1 = oldPosition;
			}
			if (textLocation == textLocation1)
			{
				return;
			}
			if (!this.HasSomethingSelected)
			{
				this.SetSelection(new DefaultSelection(this.document, textLocation, textLocation1));
				if (this.selectFrom.@where == 0)
				{
					this.SelectionStart = oldPosition;
				}
				return;
			}
			ISelection item = this.selectionCollection[0];
			if (textLocation == textLocation1)
			{
				return;
			}
			if (this.selectFrom.@where == 1)
			{
				newPosition.X = 0;
			}
			if (!this.GreaterEqPos(newPosition, this.SelectionStart))
			{
				if (this.selectFrom.@where != 1 || this.selectFrom.first != 1)
				{
					item.EndPosition = this.SelectionStart;
				}
				else
				{
					item.EndPosition = this.NextValidPosition(this.SelectionStart.Y);
				}
				item.StartPosition = newPosition;
			}
			else
			{
				item.StartPosition = this.SelectionStart;
				if (this.selectFrom.@where != 1)
				{
					newPosition.X = x;
					item.EndPosition = newPosition;
				}
				else
				{
					item.EndPosition = new TextLocation(this.textArea.Caret.Column, this.textArea.Caret.Line);
				}
			}
			this.document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.LinesBetween, textLocation.Y, textLocation1.Y));
			this.document.CommitUpdate();
			this.OnSelectionChanged(EventArgs.Empty);
		}

		public void FireSelectionChanged()
		{
			this.OnSelectionChanged(EventArgs.Empty);
		}

		public ISelection GetSelectionAt(int offset)
		{
			ISelection selection;
			List<ISelection>.Enumerator enumerator = this.selectionCollection.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ISelection current = enumerator.Current;
					if (!current.ContainsOffset(offset))
					{
						continue;
					}
					selection = current;
					return selection;
				}
				return null;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return selection;
		}

		public ColumnRange GetSelectionAtLine(int lineNumber)
		{
			ColumnRange wholeColumn;
			List<ISelection>.Enumerator enumerator = this.selectionCollection.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ISelection current = enumerator.Current;
					int y = current.StartPosition.Y;
					int num = current.EndPosition.Y;
					if (y < lineNumber && lineNumber < num)
					{
						wholeColumn = ColumnRange.WholeColumn;
						return wholeColumn;
					}
					else if (y != lineNumber)
					{
						if (num != lineNumber)
						{
							continue;
						}
						wholeColumn = new ColumnRange(0, current.EndPosition.X);
						return wholeColumn;
					}
					else
					{
						LineSegment lineSegment = this.document.GetLineSegment(y);
						int x = current.StartPosition.X;
						wholeColumn = new ColumnRange(x, (num == lineNumber ? current.EndPosition.X : lineSegment.Length + 1));
						return wholeColumn;
					}
				}
				return ColumnRange.NoColumn;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return wholeColumn;
		}

		public bool GreaterEqPos(TextLocation p1, TextLocation p2)
		{
			if (p1.Y > p2.Y)
			{
				return true;
			}
			if (p1.Y != p2.Y)
			{
				return false;
			}
			return p1.X >= p2.X;
		}

		internal void Insert(int offset, string text)
		{
		}

		public bool IsSelected(int offset)
		{
			return this.GetSelectionAt(offset) != null;
		}

		public TextLocation NextValidPosition(int line)
		{
			if (line < this.document.TotalNumberOfLines - 1)
			{
				return new TextLocation(0, line + 1);
			}
			return new TextLocation(this.document.GetLineSegment(this.document.TotalNumberOfLines - 1).Length + 1, line);
		}

		protected virtual void OnSelectionChanged(EventArgs e)
		{
			if (this.SelectionChanged != null)
			{
				this.SelectionChanged(this, e);
			}
		}

		internal void Remove(int offset, int length)
		{
		}

		public void RemoveSelectedText()
		{
			if (this.SelectionIsReadonly)
			{
				this.ClearSelection();
				return;
			}
			List<int> nums = new List<int>();
			int offset = -1;
			bool flag = true;
			foreach (ISelection selection in this.selectionCollection)
			{
				if (flag)
				{
					int y = selection.StartPosition.Y;
					if (y == selection.EndPosition.Y)
					{
						nums.Add(y);
					}
					else
					{
						flag = false;
					}
				}
				offset = selection.Offset;
				this.document.Remove(selection.Offset, selection.Length);
			}
			this.ClearSelection();
			if (offset != -1)
			{
				if (!flag)
				{
					this.document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
				}
				else
				{
					foreach (int num in nums)
					{
						this.document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, num));
					}
				}
				this.document.CommitUpdate();
			}
		}

		internal void Replace(int offset, int length, string text)
		{
		}

		internal static bool SelectionIsReadOnly(IDocument document, ISelection sel)
		{
			if (!document.TextEditorProperties.SupportReadOnlySegments)
			{
				return false;
			}
			return document.MarkerStrategy.GetMarkers(sel.Offset, sel.Length).Exists((TextMarker m) => m.IsReadOnly);
		}

		private bool SelectionsOverlap(ISelection s1, ISelection s2)
		{
			if (s1.Offset <= s2.Offset && s2.Offset <= s1.Offset + s1.Length || s1.Offset <= s2.Offset + s2.Length && s2.Offset + s2.Length <= s1.Offset + s1.Length)
			{
				return true;
			}
			if (s1.Offset < s2.Offset)
			{
				return false;
			}
			return s1.Offset + s1.Length <= s2.Offset + s2.Length;
		}

		public void SetSelection(ISelection selection)
		{
			if (selection == null)
			{
				this.ClearSelection();
				return;
			}
			if (this.SelectionCollection.Count == 1 && selection.StartPosition == this.SelectionCollection[0].StartPosition && selection.EndPosition == this.SelectionCollection[0].EndPosition)
			{
				return;
			}
			this.ClearWithoutUpdate();
			this.selectionCollection.Add(selection);
			IDocument document = this.document;
			int y = selection.StartPosition.Y;
			TextLocation endPosition = selection.EndPosition;
			document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.LinesBetween, y, endPosition.Y));
			this.document.CommitUpdate();
			this.OnSelectionChanged(EventArgs.Empty);
		}

		public void SetSelection(TextLocation startPosition, TextLocation endPosition)
		{
			this.SetSelection(new DefaultSelection(this.document, startPosition, endPosition));
		}

		public event EventHandler SelectionChanged;
	}
}