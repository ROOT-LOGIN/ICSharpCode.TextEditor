using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using ICSharpCode.TextEditor.Undo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor
{
	[ToolboxItem(false)]
	public class TextArea : Control
	{
		private bool hiddenMouseCursor;

		private Point mouseCursorHidePosition;

		private Point virtualTop = new Point(0, 0);

		private TextAreaControl motherTextAreaControl;

		private TextEditorControl motherTextEditorControl;

		private List<BracketHighlightingSheme> bracketshemes = new List<BracketHighlightingSheme>();

		private TextAreaClipboardHandler textAreaClipboardHandler;

		private bool autoClearSelection;

		private List<AbstractMargin> leftMargins = new List<AbstractMargin>();

		private ICSharpCode.TextEditor.TextView textView;

		private ICSharpCode.TextEditor.GutterMargin gutterMargin;

		private ICSharpCode.TextEditor.FoldMargin foldMargin;

		private ICSharpCode.TextEditor.IconBarMargin iconBarMargin;

		private ICSharpCode.TextEditor.Document.SelectionManager selectionManager;

		private ICSharpCode.TextEditor.Caret caret;

		internal Point mousepos = new Point(0, 0);

		private bool disposed;

		private AbstractMargin lastMouseInMargin;

		private static DeclarationViewWindow toolTip;

		private static string oldToolTip;

		private bool toolTipActive;

		private Rectangle toolTipRectangle;

		private AbstractMargin updateMargin;

		public bool AutoClearSelection
		{
			get
			{
				return this.autoClearSelection;
			}
			set
			{
				this.autoClearSelection = value;
			}
		}

		public ICSharpCode.TextEditor.Caret Caret
		{
			get
			{
				return this.caret;
			}
		}

		public TextAreaClipboardHandler ClipboardHandler
		{
			get
			{
				return this.textAreaClipboardHandler;
			}
		}

		[Browsable(false)]
		public IDocument Document
		{
			get
			{
				return this.motherTextEditorControl.Document;
			}
		}

		public bool EnableCutOrPaste
		{
			get
			{
				if (this.motherTextAreaControl == null)
				{
					return false;
				}
				if (this.SelectionManager.HasSomethingSelected)
				{
					return !this.SelectionManager.SelectionIsReadonly;
				}
				return !this.IsReadOnly(this.Caret.Offset);
			}
		}

		public System.Text.Encoding Encoding
		{
			get
			{
				return this.motherTextEditorControl.Encoding;
			}
		}

		private int FirstPhysicalLine
		{
			get
			{
				return this.VirtualTop.Y / this.textView.FontHeight;
			}
		}

		public ICSharpCode.TextEditor.FoldMargin FoldMargin
		{
			get
			{
				return this.foldMargin;
			}
		}

		public ICSharpCode.TextEditor.GutterMargin GutterMargin
		{
			get
			{
				return this.gutterMargin;
			}
		}

		public ICSharpCode.TextEditor.IconBarMargin IconBarMargin
		{
			get
			{
				return this.iconBarMargin;
			}
		}

		[Browsable(false)]
		public IList<AbstractMargin> LeftMargins
		{
			get
			{
				return this.leftMargins.AsReadOnly();
			}
		}

		public int MaxVScrollValue
		{
			get
			{
				return (this.Document.GetVisibleLine(this.Document.TotalNumberOfLines - 1) + 1 + this.TextView.VisibleLineCount * 2 / 3) * this.TextView.FontHeight;
			}
		}

		public TextAreaControl MotherTextAreaControl
		{
			get
			{
				return this.motherTextAreaControl;
			}
		}

		public TextEditorControl MotherTextEditorControl
		{
			get
			{
				return this.motherTextEditorControl;
			}
		}

		public ICSharpCode.TextEditor.Document.SelectionManager SelectionManager
		{
			get
			{
				return this.selectionManager;
			}
		}

		public ITextEditorProperties TextEditorProperties
		{
			get
			{
				return this.motherTextEditorControl.TextEditorProperties;
			}
		}

		public ICSharpCode.TextEditor.TextView TextView
		{
			get
			{
				return this.textView;
			}
		}

		public Point VirtualTop
		{
			get
			{
				return this.virtualTop;
			}
			set
			{
				Point point = new Point(value.X, Math.Min(this.MaxVScrollValue, Math.Max(0, value.Y)));
				if (this.virtualTop != point)
				{
					this.virtualTop = point;
					this.motherTextAreaControl.VScrollBar.Value = this.virtualTop.Y;
					base.Invalidate();
				}
				this.caret.UpdateCaretPosition();
			}
		}

		public TextArea(TextEditorControl motherTextEditorControl, TextAreaControl motherTextAreaControl)
		{
			this.motherTextAreaControl = motherTextAreaControl;
			this.motherTextEditorControl = motherTextEditorControl;
			this.caret = new ICSharpCode.TextEditor.Caret(this);
			this.selectionManager = new ICSharpCode.TextEditor.Document.SelectionManager(this.Document, this);
			this.textAreaClipboardHandler = new TextAreaClipboardHandler(this);
			base.ResizeRedraw = true;
			base.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			base.SetStyle(ControlStyles.Opaque, false);
			base.SetStyle(ControlStyles.ResizeRedraw, true);
			base.SetStyle(ControlStyles.Selectable, true);
			this.textView = new ICSharpCode.TextEditor.TextView(this);
			this.gutterMargin = new ICSharpCode.TextEditor.GutterMargin(this);
			this.foldMargin = new ICSharpCode.TextEditor.FoldMargin(this);
			this.iconBarMargin = new ICSharpCode.TextEditor.IconBarMargin(this);
			List<AbstractMargin> abstractMargins = this.leftMargins;
			AbstractMargin[] abstractMarginArray = new AbstractMargin[] { this.iconBarMargin, this.gutterMargin, this.foldMargin };
			abstractMargins.AddRange(abstractMarginArray);
			this.OptionsChanged();
			(new TextAreaMouseHandler(this)).Attach();
			(new TextAreaDragDropHandler()).Attach(this);
			this.bracketshemes.Add(new BracketHighlightingSheme('{', '}'));
			this.bracketshemes.Add(new BracketHighlightingSheme('(', ')'));
			this.bracketshemes.Add(new BracketHighlightingSheme('[', ']'));
			this.caret.PositionChanged += new EventHandler(this.SearchMatchingBracket);
			this.Document.TextContentChanged += new EventHandler(this.TextContentChanged);
			this.Document.FoldingManager.FoldingsChanged += new EventHandler(this.DocumentFoldingsChanged);
		}

		public void BeginUpdate()
		{
			this.motherTextEditorControl.BeginUpdate();
		}

		private void CloseToolTip()
		{
			if (this.toolTipActive)
			{
				this.toolTipActive = false;
				this.SetToolTip(null, -1);
			}
			base.ResetMouseEventArgs();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing && !this.disposed)
			{
				this.disposed = true;
				if (this.caret != null)
				{
					this.caret.PositionChanged -= new EventHandler(this.SearchMatchingBracket);
					this.caret.Dispose();
				}
				if (this.selectionManager != null)
				{
					this.selectionManager.Dispose();
				}
				this.Document.TextContentChanged -= new EventHandler(this.TextContentChanged);
				this.Document.FoldingManager.FoldingsChanged -= new EventHandler(this.DocumentFoldingsChanged);
				this.motherTextAreaControl = null;
				this.motherTextEditorControl = null;
				foreach (AbstractMargin leftMargin in this.leftMargins)
				{
					if (!(leftMargin is IDisposable))
					{
						continue;
					}
					(leftMargin as IDisposable).Dispose();
				}
				this.textView.Dispose();
			}
		}

		private void DocumentFoldingsChanged(object sender, EventArgs e)
		{
			this.Caret.UpdateCaretPosition();
			base.Invalidate();
			this.motherTextAreaControl.AdjustScrollBars();
		}

		public void EndUpdate()
		{
			this.motherTextEditorControl.EndUpdate();
		}

		public bool ExecuteDialogKey(Keys keyData)
		{
			if (this.DoProcessDialogKey != null && this.DoProcessDialogKey(keyData))
			{
				return true;
			}
			IEditAction editAction = this.motherTextEditorControl.GetEditAction(keyData);
			this.AutoClearSelection = true;
			if (editAction == null)
			{
				return false;
			}
			this.BeginUpdate();
			try
			{
				lock (this.Document)
				{
					editAction.Execute(this);
					if (this.SelectionManager.HasSomethingSelected && this.AutoClearSelection && this.Document.TextEditorProperties.DocumentSelectionMode == DocumentSelectionMode.Normal)
					{
						this.SelectionManager.ClearSelection();
					}
				}
			}
			finally
			{
				this.EndUpdate();
				this.Caret.UpdateCaretPosition();
			}
			return true;
		}

		public Highlight FindMatchingBracketHighlight()
		{
			Highlight highlight;
			if (this.Caret.Offset == 0)
			{
				return null;
			}
			List<BracketHighlightingSheme>.Enumerator enumerator = this.bracketshemes.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					BracketHighlightingSheme current = enumerator.Current;
					Highlight highlight1 = current.GetHighlight(this.Document, this.Caret.Offset - 1);
					if (highlight1 == null)
					{
						continue;
					}
					highlight = highlight1;
					return highlight;
				}
				return null;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return highlight;
		}

		private string GenerateWhitespaceString(int length)
		{
			return new string(' ', length);
		}

		protected internal virtual bool HandleKeyPress(char ch)
		{
			if (this.KeyEventHandler == null)
			{
				return false;
			}
			return this.KeyEventHandler(ch);
		}

		public void InsertChar(char ch)
		{
			bool isInUpdate = this.motherTextEditorControl.IsInUpdate;
			if (!isInUpdate)
			{
				this.BeginUpdate();
			}
			if (char.IsWhiteSpace(ch) && ch != '\t' && ch != '\n')
			{
				ch = ' ';
			}
			this.Document.UndoStack.StartUndoGroup();
			if (this.Document.TextEditorProperties.DocumentSelectionMode == DocumentSelectionMode.Normal && this.SelectionManager.SelectionCollection.Count > 0)
			{
				this.Caret.Position = this.SelectionManager.SelectionCollection[0].StartPosition;
				this.SelectionManager.RemoveSelectedText();
			}
			LineSegment lineSegment = this.Document.GetLineSegment(this.Caret.Line);
			int offset = this.Caret.Offset;
			int column = this.Caret.Column;
			if (lineSegment.Length >= column || ch == '\n')
			{
				this.Document.Insert(offset, ch.ToString());
			}
			else
			{
				this.Document.Insert(offset, string.Concat(this.GenerateWhitespaceString(column - lineSegment.Length), ch));
			}
			this.Document.UndoStack.EndUndoGroup();
			ICSharpCode.TextEditor.Caret caret = this.Caret;
			caret.Column = caret.Column + 1;
			if (!isInUpdate)
			{
				this.EndUpdate();
				this.UpdateLineToEnd(this.Caret.Line, this.Caret.Column);
			}
		}

		public void InsertLeftMargin(int index, AbstractMargin margin)
		{
			this.leftMargins.Insert(index, margin);
			this.Refresh();
		}

		public void InsertString(string str)
		{
			bool isInUpdate = this.motherTextEditorControl.IsInUpdate;
			if (!isInUpdate)
			{
				this.BeginUpdate();
			}
			try
			{
				this.Document.UndoStack.StartUndoGroup();
				if (this.Document.TextEditorProperties.DocumentSelectionMode == DocumentSelectionMode.Normal && this.SelectionManager.SelectionCollection.Count > 0)
				{
					this.Caret.Position = this.SelectionManager.SelectionCollection[0].StartPosition;
					this.SelectionManager.RemoveSelectedText();
				}
				int offset = this.Document.PositionToOffset(this.Caret.Position);
				int line = this.Caret.Line;
				LineSegment lineSegment = this.Document.GetLineSegment(this.Caret.Line);
				if (lineSegment.Length >= this.Caret.Column)
				{
					this.Document.Insert(offset, str);
					this.Caret.Position = this.Document.OffsetToPosition(offset + str.Length);
				}
				else
				{
					int column = this.Caret.Column - lineSegment.Length;
					this.Document.Insert(offset, string.Concat(this.GenerateWhitespaceString(column), str));
					this.Caret.Position = this.Document.OffsetToPosition(offset + str.Length + column);
				}
				this.Document.UndoStack.EndUndoGroup();
				if (line == this.Caret.Line)
				{
					this.UpdateLineToEnd(this.Caret.Line, this.Caret.Column);
				}
				else
				{
					this.UpdateToEnd(line);
				}
			}
			finally
			{
				if (!isInUpdate)
				{
					this.EndUpdate();
				}
			}
		}

		private void InvalidateLines(int xPos, int lineBegin, int lineEnd)
		{
			lineBegin = Math.Max(this.Document.GetVisibleLine(lineBegin), this.FirstPhysicalLine);
			lineEnd = Math.Min(this.Document.GetVisibleLine(lineEnd), this.FirstPhysicalLine + this.textView.VisibleLineCount);
			int num = Math.Max(0, lineBegin * this.textView.FontHeight);
			Rectangle drawingPosition = this.textView.DrawingPosition;
			int num1 = Math.Min(drawingPosition.Height, (1 + lineEnd - lineBegin) * (this.textView.FontHeight + 1));
			Rectangle rectangle = new Rectangle(0, num - 1 - this.virtualTop.Y, base.Width, num1 + 3);
			base.Invalidate(rectangle);
		}

		protected override bool IsInputChar(char charCode)
		{
			return true;
		}

		internal bool IsReadOnly(int offset)
		{
			if (this.Document.ReadOnly)
			{
				return true;
			}
			if (!this.TextEditorProperties.SupportReadOnlySegments)
			{
				return false;
			}
			return this.Document.MarkerStrategy.GetMarkers(offset).Exists((TextMarker m) => m.IsReadOnly);
		}

		internal bool IsReadOnly(int offset, int length)
		{
			if (this.Document.ReadOnly)
			{
				return true;
			}
			if (!this.TextEditorProperties.SupportReadOnlySegments)
			{
				return false;
			}
			return this.Document.MarkerStrategy.GetMarkers(offset, length).Exists((TextMarker m) => m.IsReadOnly);
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			this.SimulateKeyPress(e.KeyChar);
			e.Handled = true;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			this.mousepos = new Point(e.X, e.Y);
			base.OnMouseDown(e);
			this.CloseToolTip();
			foreach (AbstractMargin leftMargin in this.leftMargins)
			{
				if (!leftMargin.DrawingPosition.Contains(e.X, e.Y))
				{
					continue;
				}
				leftMargin.HandleMouseDown(new Point(e.X, e.Y), e.Button);
			}
		}

		protected override void OnMouseHover(EventArgs e)
		{
			base.OnMouseHover(e);
			if (Control.MouseButtons != System.Windows.Forms.MouseButtons.None)
			{
				this.CloseToolTip();
				return;
			}
			this.RequestToolTip(base.PointToClient(Control.MousePosition));
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			this.Cursor = Cursors.Default;
			if (this.lastMouseInMargin != null)
			{
				this.lastMouseInMargin.HandleMouseLeave(EventArgs.Empty);
				this.lastMouseInMargin = null;
			}
			this.CloseToolTip();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (!this.toolTipRectangle.Contains(e.Location))
			{
				this.toolTipRectangle = Rectangle.Empty;
				if (this.toolTipActive)
				{
					this.RequestToolTip(e.Location);
				}
			}
			foreach (AbstractMargin leftMargin in this.leftMargins)
			{
				if (!leftMargin.DrawingPosition.Contains(e.X, e.Y))
				{
					continue;
				}
				this.Cursor = leftMargin.Cursor;
				leftMargin.HandleMouseMove(new Point(e.X, e.Y), e.Button);
				if (this.lastMouseInMargin != leftMargin)
				{
					if (this.lastMouseInMargin != null)
					{
						this.lastMouseInMargin.HandleMouseLeave(EventArgs.Empty);
					}
					this.lastMouseInMargin = leftMargin;
				}
				return;
			}
			if (this.lastMouseInMargin != null)
			{
				this.lastMouseInMargin.HandleMouseLeave(EventArgs.Empty);
				this.lastMouseInMargin = null;
			}
			if (!this.textView.DrawingPosition.Contains(e.X, e.Y))
			{
				this.Cursor = Cursors.Default;
				return;
			}
			ICSharpCode.TextEditor.TextView textView = this.TextView;
			int x = e.X - this.TextView.DrawingPosition.X;
			int y = e.Y;
			Rectangle drawingPosition = this.TextView.DrawingPosition;
			TextLocation logicalPosition = textView.GetLogicalPosition(x, y - drawingPosition.Y);
			if (this.SelectionManager.IsSelected(this.Document.PositionToOffset(logicalPosition)) && Control.MouseButtons == System.Windows.Forms.MouseButtons.None)
			{
				this.Cursor = Cursors.Default;
				return;
			}
			this.Cursor = this.textView.Cursor;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			int width = 0;
			int num = 0;
			bool flag = false;
			Graphics graphics = e.Graphics;
			Rectangle clipRectangle = e.ClipRectangle;
			bool flag1 = (clipRectangle.X != 0 || clipRectangle.Y != 0 || clipRectangle.Width != base.Width ? false : clipRectangle.Height == base.Height);
			graphics.TextRenderingHint = this.TextEditorProperties.TextRenderingHint;
			if (this.updateMargin != null)
			{
				this.updateMargin.Paint(graphics, this.updateMargin.DrawingPosition);
			}
			if (clipRectangle.Width <= 0 || clipRectangle.Height <= 0)
			{
				return;
			}
			foreach (AbstractMargin leftMargin in this.leftMargins)
			{
				if (!leftMargin.IsVisible)
				{
					continue;
				}
				System.Drawing.Size size = leftMargin.Size;
				Rectangle rectangle = new Rectangle(width, num, size.Width, base.Height - num);
				if (rectangle != leftMargin.DrawingPosition)
				{
					if (!flag1 && !clipRectangle.Contains(rectangle))
					{
						base.Invalidate();
					}
					flag = true;
					leftMargin.DrawingPosition = rectangle;
				}
				width += leftMargin.DrawingPosition.Width;
				if (!clipRectangle.IntersectsWith(rectangle))
				{
					continue;
				}
				rectangle.Intersect(clipRectangle);
				if (rectangle.IsEmpty)
				{
					continue;
				}
				leftMargin.Paint(graphics, rectangle);
			}
			Rectangle rectangle1 = new Rectangle(width, num, base.Width - width, base.Height - num);
			if (rectangle1 != this.textView.DrawingPosition)
			{
				flag = true;
				this.textView.DrawingPosition = rectangle1;
				base.BeginInvoke(new MethodInvoker(this.caret.UpdateCaretPosition));
			}
			if (clipRectangle.IntersectsWith(rectangle1))
			{
				rectangle1.Intersect(clipRectangle);
				if (!rectangle1.IsEmpty)
				{
					this.textView.Paint(graphics, rectangle1);
				}
			}
			if (flag)
			{
				this.motherTextAreaControl.AdjustScrollBars();
			}
			base.OnPaint(e);
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
		}

		protected virtual void OnToolTipRequest(ToolTipRequestEventArgs e)
		{
			if (this.ToolTipRequest != null)
			{
				this.ToolTipRequest(this, e);
			}
		}

		public void OptionsChanged()
		{
			this.UpdateMatchingBracket();
			this.textView.OptionsChanged();
			this.caret.RecreateCaret();
			this.caret.UpdateCaretPosition();
			this.Refresh();
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (this.ExecuteDialogKey(keyData))
			{
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}

		internal void RaiseMouseMove(MouseEventArgs e)
		{
			this.OnMouseMove(e);
		}

		public void Refresh(AbstractMargin margin)
		{
			this.updateMargin = margin;
			base.Invalidate(this.updateMargin.DrawingPosition);
			base.Update();
			this.updateMargin = null;
		}

		public void ReplaceChar(char ch)
		{
			bool isInUpdate = this.motherTextEditorControl.IsInUpdate;
			if (!isInUpdate)
			{
				this.BeginUpdate();
			}
			if (this.Document.TextEditorProperties.DocumentSelectionMode == DocumentSelectionMode.Normal && this.SelectionManager.SelectionCollection.Count > 0)
			{
				this.Caret.Position = this.SelectionManager.SelectionCollection[0].StartPosition;
				this.SelectionManager.RemoveSelectedText();
			}
			int line = this.Caret.Line;
			LineSegment lineSegment = this.Document.GetLineSegment(line);
			int offset = this.Document.PositionToOffset(this.Caret.Position);
			if (offset >= lineSegment.Offset + lineSegment.Length)
			{
				this.Document.Insert(offset, ch.ToString());
			}
			else
			{
				this.Document.Replace(offset, 1, ch.ToString());
			}
			if (!isInUpdate)
			{
				this.EndUpdate();
				this.UpdateLineToEnd(line, this.Caret.Column);
			}
			ICSharpCode.TextEditor.Caret caret = this.Caret;
			caret.Column = caret.Column + 1;
		}

		protected void RequestToolTip(Point mousePos)
		{
			if (this.toolTipRectangle.Contains(mousePos))
			{
				if (!this.toolTipActive)
				{
					base.ResetMouseEventArgs();
				}
				return;
			}
			this.toolTipRectangle = new Rectangle(mousePos.X - 4, mousePos.Y - 4, 8, 8);
			ICSharpCode.TextEditor.TextView textView = this.textView;
			int x = mousePos.X - this.textView.DrawingPosition.Left;
			int y = mousePos.Y;
			Rectangle drawingPosition = this.textView.DrawingPosition;
			TextLocation logicalPosition = textView.GetLogicalPosition(x, y - drawingPosition.Top);
			bool flag = (!this.textView.DrawingPosition.Contains(mousePos) || logicalPosition.Y < 0 ? false : logicalPosition.Y < this.Document.TotalNumberOfLines);
			ToolTipRequestEventArgs toolTipRequestEventArg = new ToolTipRequestEventArgs(mousePos, logicalPosition, flag);
			this.OnToolTipRequest(toolTipRequestEventArg);
			if (!toolTipRequestEventArg.ToolTipShown)
			{
				this.CloseToolTip();
				return;
			}
			this.toolTipActive = true;
			this.SetToolTip(toolTipRequestEventArg.toolTipText, (flag ? logicalPosition.Y + 1 : -1));
		}

		public void ScrollTo(int line)
		{
			this.motherTextAreaControl.ScrollTo(line);
		}

		public void ScrollToCaret()
		{
			this.motherTextAreaControl.ScrollToCaret();
		}

		private void SearchMatchingBracket(object sender, EventArgs e)
		{
			if (!this.TextEditorProperties.ShowMatchingBracket)
			{
				this.textView.Highlight = null;
				return;
			}
			int y = -1;
			int num = -1;
			if (this.textView.Highlight != null && this.textView.Highlight.OpenBrace.Y >= 0 && this.textView.Highlight.OpenBrace.Y < this.Document.TotalNumberOfLines)
			{
				y = this.textView.Highlight.OpenBrace.Y;
			}
			if (this.textView.Highlight != null && this.textView.Highlight.CloseBrace.Y >= 0 && this.textView.Highlight.CloseBrace.Y < this.Document.TotalNumberOfLines)
			{
				num = this.textView.Highlight.CloseBrace.Y;
			}
			this.textView.Highlight = this.FindMatchingBracketHighlight();
			if (y >= 0)
			{
				this.UpdateLine(y);
			}
			if (num >= 0 && num != y)
			{
				this.UpdateLine(num);
			}
			if (this.textView.Highlight != null)
			{
				int y1 = this.textView.Highlight.OpenBrace.Y;
				int num1 = this.textView.Highlight.CloseBrace.Y;
				if (y1 != y && y1 != num)
				{
					this.UpdateLine(y1);
				}
				if (num1 != y && num1 != num && num1 != y1)
				{
					this.UpdateLine(num1);
				}
			}
		}

		public void SetCaretToDesiredColumn()
		{
			FoldMarker foldMarker;
			ICSharpCode.TextEditor.Caret caret = this.Caret;
			ICSharpCode.TextEditor.TextView textView = this.textView;
			int line = this.Caret.Line;
			int desiredColumn = this.Caret.DesiredColumn;
			Point virtualTop = this.VirtualTop;
			caret.Position = textView.GetLogicalColumn(line, desiredColumn + virtualTop.X, out foldMarker);
		}

		public void SetDesiredColumn()
		{
			this.Caret.DesiredColumn = this.TextView.GetDrawingXPos(this.Caret.Line, this.Caret.Column) + this.VirtualTop.X;
		}

		private void SetToolTip(string text, int lineNumber)
		{
			if (TextArea.toolTip == null || TextArea.toolTip.IsDisposed)
			{
				TextArea.toolTip = new DeclarationViewWindow(base.FindForm());
			}
			if (TextArea.oldToolTip == text)
			{
				return;
			}
			if (text != null)
			{
				Point mousePosition = Control.MousePosition;
				Point client = base.PointToClient(mousePosition);
				if (lineNumber >= 0)
				{
					lineNumber = this.Document.GetVisibleLine(lineNumber);
					mousePosition.Y = mousePosition.Y - client.Y + lineNumber * this.TextView.FontHeight - this.virtualTop.Y;
				}
				mousePosition.Offset(3, 3);
				TextArea.toolTip.Owner = base.FindForm();
				TextArea.toolTip.Location = mousePosition;
				TextArea.toolTip.Description = text;
				TextArea.toolTip.HideOnClick = true;
				TextArea.toolTip.Show();
			}
			else
			{
				TextArea.toolTip.Hide();
			}
			TextArea.oldToolTip = text;
		}

		internal void ShowHiddenCursor(bool forceShow)
		{
			if (this.hiddenMouseCursor && (this.mouseCursorHidePosition != System.Windows.Forms.Cursor.Position || forceShow))
			{
				System.Windows.Forms.Cursor.Show();
				this.hiddenMouseCursor = false;
			}
		}

		public void SimulateKeyPress(char ch)
		{
			if (this.SelectionManager.HasSomethingSelected)
			{
				if (this.SelectionManager.SelectionIsReadonly)
				{
					return;
				}
			}
			else if (this.IsReadOnly(this.Caret.Offset))
			{
				return;
			}
			if (ch < ' ')
			{
				return;
			}
			if (!this.hiddenMouseCursor && this.TextEditorProperties.HideMouseCursor && base.ClientRectangle.Contains(base.PointToClient(System.Windows.Forms.Cursor.Position)))
			{
				this.mouseCursorHidePosition = System.Windows.Forms.Cursor.Position;
				this.hiddenMouseCursor = true;
				System.Windows.Forms.Cursor.Hide();
			}
			this.CloseToolTip();
			this.BeginUpdate();
			this.Document.UndoStack.StartUndoGroup();
			try
			{
				if (!this.HandleKeyPress(ch))
				{
					switch (this.Caret.CaretMode)
					{
						case CaretMode.InsertMode:
						{
							this.InsertChar(ch);
							break;
						}
						case CaretMode.OverwriteMode:
						{
							this.ReplaceChar(ch);
							break;
						}
					}
				}
				int line = this.Caret.Line;
				this.Document.FormattingStrategy.FormatLine(this, line, this.Document.PositionToOffset(this.Caret.Position), ch);
				this.EndUpdate();
			}
			finally
			{
				this.Document.UndoStack.EndUndoGroup();
			}
		}

		private void TextContentChanged(object sender, EventArgs e)
		{
			this.Caret.Position = new TextLocation(0, 0);
			this.SelectionManager.SelectionCollection.Clear();
		}

		internal void UpdateLine(int line)
		{
			this.UpdateLines(0, line, line);
		}

		internal void UpdateLine(int line, int begin, int end)
		{
			this.UpdateLines(line, line);
		}

		internal void UpdateLines(int lineBegin, int lineEnd)
		{
			this.UpdateLines(0, lineBegin, lineEnd);
		}

		internal void UpdateLines(int xPos, int lineBegin, int lineEnd)
		{
			this.InvalidateLines(xPos * this.TextView.WideSpaceWidth, lineBegin, lineEnd);
		}

		internal void UpdateLineToEnd(int lineNr, int xStart)
		{
			this.UpdateLines(xStart, lineNr, lineNr);
		}

		public void UpdateMatchingBracket()
		{
			this.SearchMatchingBracket(null, null);
		}

		internal void UpdateToEnd(int lineBegin)
		{
			lineBegin = this.Document.GetVisibleLine(lineBegin);
			int num = Math.Max(0, lineBegin * this.textView.FontHeight);
			num = Math.Max(0, num - this.virtualTop.Y);
			Rectangle rectangle = new Rectangle(0, num, base.Width, base.Height - num);
			base.Invalidate(rectangle);
		}

		public event DialogKeyProcessor DoProcessDialogKey;

		public event ICSharpCode.TextEditor.KeyEventHandler KeyEventHandler;

		public event ToolTipRequestEventHandler ToolTipRequest;
	}
}