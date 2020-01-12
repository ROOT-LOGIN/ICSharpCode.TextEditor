using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Util;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor
{
	[ToolboxItem(false)]
	public class TextAreaControl : Panel
	{
		private const int LineLengthCacheAdditionalSize = 100;

		private TextEditorControl motherTextEditorControl;

		private HRuler hRuler;

		private System.Windows.Forms.VScrollBar vScrollBar = new System.Windows.Forms.VScrollBar();

		private System.Windows.Forms.HScrollBar hScrollBar = new System.Windows.Forms.HScrollBar();

		private ICSharpCode.TextEditor.TextArea textArea;

		private bool doHandleMousewheel = true;

		private bool disposed;

		private bool adjustScrollBarsOnNextUpdate;

		private Point scrollToPosOnNextUpdate;

		private int[] lineLengthCache;

		private MouseWheelHandler mouseWheelHandler = new MouseWheelHandler();

		private int scrollMarginHeight = 3;

		public ICSharpCode.TextEditor.Caret Caret
		{
			get
			{
				return this.textArea.Caret;
			}
		}

		[Browsable(false)]
		public IDocument Document
		{
			get
			{
				if (this.motherTextEditorControl == null)
				{
					return null;
				}
				return this.motherTextEditorControl.Document;
			}
		}

		public bool DoHandleMousewheel
		{
			get
			{
				return this.doHandleMousewheel;
			}
			set
			{
				this.doHandleMousewheel = value;
			}
		}

		public System.Windows.Forms.HScrollBar HScrollBar
		{
			get
			{
				return this.hScrollBar;
			}
		}

		public ICSharpCode.TextEditor.Document.SelectionManager SelectionManager
		{
			get
			{
				return this.textArea.SelectionManager;
			}
		}

		public ICSharpCode.TextEditor.TextArea TextArea
		{
			get
			{
				return this.textArea;
			}
		}

		public ITextEditorProperties TextEditorProperties
		{
			get
			{
				if (this.motherTextEditorControl == null)
				{
					return null;
				}
				return this.motherTextEditorControl.TextEditorProperties;
			}
		}

		public System.Windows.Forms.VScrollBar VScrollBar
		{
			get
			{
				return this.vScrollBar;
			}
		}

		public TextAreaControl(TextEditorControl motherTextEditorControl)
		{
			this.motherTextEditorControl = motherTextEditorControl;
			this.textArea = new ICSharpCode.TextEditor.TextArea(motherTextEditorControl, this);
			base.Controls.Add(this.textArea);
			this.vScrollBar.ValueChanged += new EventHandler(this.VScrollBarValueChanged);
			base.Controls.Add(this.vScrollBar);
			this.hScrollBar.ValueChanged += new EventHandler(this.HScrollBarValueChanged);
			base.Controls.Add(this.hScrollBar);
			base.ResizeRedraw = true;
			this.Document.TextContentChanged += new EventHandler(this.DocumentTextContentChanged);
			this.Document.DocumentChanged += new DocumentEventHandler(this.AdjustScrollBarsOnDocumentChange);
			this.Document.UpdateCommited += new EventHandler(this.DocumentUpdateCommitted);
		}

		public void AdjustScrollBars()
		{
			this.adjustScrollBarsOnNextUpdate = false;
			this.vScrollBar.Minimum = 0;
			this.vScrollBar.Maximum = this.textArea.MaxVScrollValue;
			int num = 0;
			int firstVisibleLine = this.textArea.TextView.FirstVisibleLine;
			int firstLogicalLine = this.Document.GetFirstLogicalLine(this.textArea.TextView.FirstPhysicalLine + this.textArea.TextView.VisibleLineCount);
			if (firstLogicalLine >= this.Document.TotalNumberOfLines)
			{
				firstLogicalLine = this.Document.TotalNumberOfLines - 1;
			}
			if (this.lineLengthCache == null || (int)this.lineLengthCache.Length <= firstLogicalLine)
			{
				this.lineLengthCache = new int[firstLogicalLine + 100];
			}
			for (int i = firstVisibleLine; i <= firstLogicalLine; i++)
			{
				LineSegment lineSegment = this.Document.GetLineSegment(i);
				if (this.Document.FoldingManager.IsLineVisible(i))
				{
					if (this.lineLengthCache[i] <= 0)
					{
						int visualColumnFast = this.textArea.TextView.GetVisualColumnFast(lineSegment, lineSegment.Length);
						this.lineLengthCache[i] = Math.Max(1, visualColumnFast);
						num = Math.Max(num, visualColumnFast);
					}
					else
					{
						num = Math.Max(num, this.lineLengthCache[i]);
					}
				}
			}
			this.hScrollBar.Minimum = 0;
			this.hScrollBar.Maximum = Math.Max(num + 20, this.textArea.TextView.VisibleColumnCount - 1);
			System.Windows.Forms.VScrollBar vScrollBar = this.vScrollBar;
			Rectangle drawingPosition = this.textArea.TextView.DrawingPosition;
			vScrollBar.LargeChange = Math.Max(0, drawingPosition.Height);
			this.vScrollBar.SmallChange = Math.Max(0, this.textArea.TextView.FontHeight);
			this.hScrollBar.LargeChange = Math.Max(0, this.textArea.TextView.VisibleColumnCount - 1);
			this.hScrollBar.SmallChange = Math.Max(0, this.textArea.TextView.SpaceWidth);
		}

		private void AdjustScrollBarsClearCache()
		{
			if (this.lineLengthCache != null)
			{
				if ((int)this.lineLengthCache.Length < this.Document.TotalNumberOfLines + 200)
				{
					this.lineLengthCache = null;
					return;
				}
				Array.Clear(this.lineLengthCache, 0, (int)this.lineLengthCache.Length);
			}
		}

		private void AdjustScrollBarsOnDocumentChange(object sender, DocumentEventArgs e)
		{
			if (this.motherTextEditorControl.IsInUpdate)
			{
				this.adjustScrollBarsOnNextUpdate = true;
				return;
			}
			this.AdjustScrollBarsClearCache();
			this.AdjustScrollBars();
		}

		public void CenterViewOn(int line, int treshold)
		{
			line = Math.Max(0, Math.Min(this.Document.TotalNumberOfLines - 1, line));
			line = this.Document.GetVisibleLine(line);
			line = line - this.textArea.TextView.VisibleLineCount / 2;
			int firstPhysicalLine = this.textArea.TextView.FirstPhysicalLine;
			if (this.textArea.TextView.LineHeightRemainder > 0)
			{
				firstPhysicalLine++;
			}
			if (Math.Abs(firstPhysicalLine - line) > treshold)
			{
				this.vScrollBar.Value = Math.Max(0, Math.Min(this.vScrollBar.Maximum, (line - this.scrollMarginHeight + 3) * this.textArea.TextView.FontHeight));
				this.VScrollBarValueChanged(this, EventArgs.Empty);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !this.disposed)
			{
				this.disposed = true;
				this.Document.TextContentChanged -= new EventHandler(this.DocumentTextContentChanged);
				this.Document.DocumentChanged -= new DocumentEventHandler(this.AdjustScrollBarsOnDocumentChange);
				this.Document.UpdateCommited -= new EventHandler(this.DocumentUpdateCommitted);
				this.motherTextEditorControl = null;
				if (this.vScrollBar != null)
				{
					this.vScrollBar.Dispose();
					this.vScrollBar = null;
				}
				if (this.hScrollBar != null)
				{
					this.hScrollBar.Dispose();
					this.hScrollBar = null;
				}
				if (this.hRuler != null)
				{
					this.hRuler.Dispose();
					this.hRuler = null;
				}
			}
			base.Dispose(disposing);
		}

		private void DocumentTextContentChanged(object sender, EventArgs e)
		{
			this.Caret.ValidateCaretPos();
		}

		private void DocumentUpdateCommitted(object sender, EventArgs e)
		{
			if (!this.motherTextEditorControl.IsInUpdate)
			{
				this.Caret.ValidateCaretPos();
				if (!this.scrollToPosOnNextUpdate.IsEmpty)
				{
					this.ScrollTo(this.scrollToPosOnNextUpdate.Y, this.scrollToPosOnNextUpdate.X);
				}
				if (this.adjustScrollBarsOnNextUpdate)
				{
					this.AdjustScrollBarsClearCache();
					this.AdjustScrollBars();
				}
			}
		}

		public void HandleMouseWheel(MouseEventArgs e)
		{
			int scrollAmount = this.mouseWheelHandler.GetScrollAmount(e);
			if (scrollAmount == 0)
			{
				return;
			}
			if ((Control.ModifierKeys & Keys.Control) != Keys.None && this.TextEditorProperties.MouseWheelTextZoom)
			{
				if (scrollAmount > 0)
				{
					this.motherTextEditorControl.Font = new System.Drawing.Font(this.motherTextEditorControl.Font.Name, this.motherTextEditorControl.Font.Size + 1f);
					return;
				}
				this.motherTextEditorControl.Font = new System.Drawing.Font(this.motherTextEditorControl.Font.Name, Math.Max(6f, this.motherTextEditorControl.Font.Size - 1f));
				return;
			}
			if (this.TextEditorProperties.MouseWheelScrollDown)
			{
				scrollAmount = -scrollAmount;
			}
			int value = this.vScrollBar.Value + this.vScrollBar.SmallChange * scrollAmount;
			this.vScrollBar.Value = Math.Max(this.vScrollBar.Minimum, Math.Min(this.vScrollBar.Maximum - this.vScrollBar.LargeChange + 1, value));
		}

		private void HScrollBarValueChanged(object sender, EventArgs e)
		{
			ICSharpCode.TextEditor.TextArea point = this.textArea;
			int value = this.hScrollBar.Value * this.textArea.TextView.WideSpaceWidth;
			Point virtualTop = this.textArea.VirtualTop;
			point.VirtualTop = new Point(value, virtualTop.Y);
			this.textArea.Invalidate();
		}

		public void JumpTo(int line)
		{
			line = Math.Max(0, Math.Min(line, this.Document.TotalNumberOfLines - 1));
			string text = this.Document.GetText(this.Document.GetLineSegment(line));
			this.JumpTo(line, text.Length - text.TrimStart(new char[0]).Length);
		}

		public void JumpTo(int line, int column)
		{
			this.textArea.Focus();
			this.textArea.SelectionManager.ClearSelection();
			this.textArea.Caret.Position = new TextLocation(column, line);
			this.textArea.SetDesiredColumn();
			this.ScrollToCaret();
		}

		protected override void OnEnter(EventArgs e)
		{
			this.Caret.ValidateCaretPos();
			base.OnEnter(e);
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (this.DoHandleMousewheel)
			{
				this.HandleMouseWheel(e);
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			this.ResizeTextArea();
		}

		public void OptionsChanged()
		{
			this.textArea.OptionsChanged();
			if (this.textArea.TextEditorProperties.ShowHorizontalRuler)
			{
				if (this.hRuler != null)
				{
					this.hRuler.Invalidate();
				}
				else
				{
					this.hRuler = new HRuler(this.textArea);
					base.Controls.Add(this.hRuler);
					this.ResizeTextArea();
				}
			}
			else if (this.hRuler != null)
			{
				base.Controls.Remove(this.hRuler);
				this.hRuler.Dispose();
				this.hRuler = null;
				this.ResizeTextArea();
			}
			this.AdjustScrollBars();
		}

		public void ResizeTextArea()
		{
			int bottom = 0;
			int height = 0;
			if (this.hRuler != null)
			{
				this.hRuler.Bounds = new Rectangle(0, 0, base.Width - SystemInformation.HorizontalScrollBarArrowWidth, this.textArea.TextView.FontHeight);
				bottom = this.hRuler.Bounds.Bottom;
				height = this.hRuler.Bounds.Height;
			}
			this.textArea.Bounds = new Rectangle(0, bottom, base.Width - SystemInformation.HorizontalScrollBarArrowWidth, base.Height - SystemInformation.VerticalScrollBarArrowHeight - height);
			this.SetScrollBarBounds();
		}

		public void ScrollTo(int line, int column)
		{
			if (this.motherTextEditorControl.IsInUpdate)
			{
				this.scrollToPosOnNextUpdate = new Point(column, line);
				return;
			}
			this.scrollToPosOnNextUpdate = Point.Empty;
			this.ScrollTo(line);
			int value = this.hScrollBar.Value - this.hScrollBar.Minimum;
			int visibleColumnCount = value + this.textArea.TextView.VisibleColumnCount;
			int visualColumn = this.textArea.TextView.GetVisualColumn(line, column);
			if (this.textArea.TextView.VisibleColumnCount < 0)
			{
				this.hScrollBar.Value = 0;
				return;
			}
			if (visualColumn < value)
			{
				this.hScrollBar.Value = Math.Max(0, visualColumn - this.scrollMarginHeight);
				return;
			}
			if (visualColumn > visibleColumnCount)
			{
				this.hScrollBar.Value = Math.Max(0, Math.Min(this.hScrollBar.Maximum, visualColumn - this.textArea.TextView.VisibleColumnCount + this.scrollMarginHeight));
			}
		}

		public void ScrollTo(int line)
		{
			line = Math.Max(0, Math.Min(this.Document.TotalNumberOfLines - 1, line));
			line = this.Document.GetVisibleLine(line);
			int firstPhysicalLine = this.textArea.TextView.FirstPhysicalLine;
			if (this.textArea.TextView.LineHeightRemainder > 0)
			{
				firstPhysicalLine++;
			}
			if (line - this.scrollMarginHeight + 3 < firstPhysicalLine)
			{
				this.vScrollBar.Value = Math.Max(0, Math.Min(this.vScrollBar.Maximum, (line - this.scrollMarginHeight + 3) * this.textArea.TextView.FontHeight));
				this.VScrollBarValueChanged(this, EventArgs.Empty);
				return;
			}
			int visibleLineCount = firstPhysicalLine + this.textArea.TextView.VisibleLineCount;
			if (line + this.scrollMarginHeight - 1 > visibleLineCount)
			{
				if (this.textArea.TextView.VisibleLineCount != 1)
				{
					this.vScrollBar.Value = Math.Min(this.vScrollBar.Maximum, (line - this.textArea.TextView.VisibleLineCount + this.scrollMarginHeight - 1) * this.textArea.TextView.FontHeight);
				}
				else
				{
					this.vScrollBar.Value = Math.Max(0, Math.Min(this.vScrollBar.Maximum, (line - this.scrollMarginHeight - 1) * this.textArea.TextView.FontHeight));
				}
				this.VScrollBarValueChanged(this, EventArgs.Empty);
			}
		}

		public void ScrollToCaret()
		{
			this.ScrollTo(this.textArea.Caret.Line, this.textArea.Caret.Column);
		}

		public void SetScrollBarBounds()
		{
			System.Windows.Forms.VScrollBar rectangle = this.vScrollBar;
			Rectangle bounds = this.textArea.Bounds;
			rectangle.Bounds = new Rectangle(bounds.Right, 0, SystemInformation.HorizontalScrollBarArrowWidth, base.Height - SystemInformation.VerticalScrollBarArrowHeight);
			System.Windows.Forms.HScrollBar hScrollBar = this.hScrollBar;
			Rectangle bounds1 = this.textArea.Bounds;
			hScrollBar.Bounds = new Rectangle(0, bounds1.Bottom, base.Width - SystemInformation.HorizontalScrollBarArrowWidth, SystemInformation.VerticalScrollBarArrowHeight);
		}

		private void VScrollBarValueChanged(object sender, EventArgs e)
		{
			ICSharpCode.TextEditor.TextArea point = this.textArea;
			Point virtualTop = this.textArea.VirtualTop;
			point.VirtualTop = new Point(virtualTop.X, this.vScrollBar.Value);
			this.textArea.Invalidate();
			this.AdjustScrollBars();
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 123 && this.ShowContextMenu != null)
			{
				long num = m.LParam.ToInt64();
				int num1 = (short)(num & 0xFFFF);
                int num2 = (short)((num >> 16) & 0xFFFF);
				if (num1 != -1 || num2 != -1)
				{
					Point client = base.PointToClient(new Point(num1, num2));
					this.ShowContextMenu(this, new MouseEventArgs(System.Windows.Forms.MouseButtons.Right, 1, client.X, client.Y, 0));
				}
				else
				{
					Point screenPosition = this.Caret.ScreenPosition;
					this.ShowContextMenu(this, new MouseEventArgs(System.Windows.Forms.MouseButtons.None, 0, screenPosition.X, screenPosition.Y + this.textArea.TextView.FontHeight, 0));
				}
			}
			base.WndProc(ref m);
		}

		public event MouseEventHandler ShowContextMenu;
	}
}