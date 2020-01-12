using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor
{
	public class FoldMargin : AbstractMargin
	{
		private int selectedFoldLine = -1;

		public override bool IsVisible
		{
			get
			{
				return this.textArea.TextEditorProperties.EnableFolding;
			}
		}

		public override System.Drawing.Size Size
		{
			get
			{
				return new System.Drawing.Size(this.textArea.TextView.FontHeight, -1);
			}
		}

		public FoldMargin(ICSharpCode.TextEditor.TextArea textArea) : base(textArea)
		{
		}

		private void DrawFoldMarker(Graphics g, RectangleF rectangle, bool isOpened, bool isSelected)
		{
			HighlightColor colorFor = this.textArea.Document.HighlightingStrategy.GetColorFor("FoldMarker");
			HighlightColor highlightColor = this.textArea.Document.HighlightingStrategy.GetColorFor("FoldLine");
			HighlightColor colorFor1 = this.textArea.Document.HighlightingStrategy.GetColorFor("SelectedFoldLine");
			Rectangle rectangle1 = new Rectangle((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
			g.FillRectangle(BrushRegistry.GetBrush(colorFor.BackgroundColor), rectangle1);
			g.DrawRectangle(BrushRegistry.GetPen((isSelected ? colorFor1.Color : highlightColor.Color)), rectangle1);
			int num = (int)Math.Round((double)rectangle.Height / 8) + 1;
			int height = rectangle1.Height / 2 + rectangle1.Height % 2;
			g.DrawLine(BrushRegistry.GetPen(colorFor.Color), rectangle.X + (float)num, rectangle.Y + (float)height, rectangle.X + rectangle.Width - (float)num, rectangle.Y + (float)height);
			if (!isOpened)
			{
				g.DrawLine(BrushRegistry.GetPen(colorFor.Color), rectangle.X + (float)height, rectangle.Y + (float)num, rectangle.X + (float)height, rectangle.Y + rectangle.Height - (float)num);
			}
		}

		public override void HandleMouseDown(Point mousepos, MouseButtons mouseButtons)
		{
			bool enableFolding = this.textArea.Document.TextEditorProperties.EnableFolding;
			int y = mousepos.Y;
			Point virtualTop = this.textArea.VirtualTop;
			int num = (y + virtualTop.Y) / this.textArea.TextView.FontHeight;
			int firstLogicalLine = this.textArea.Document.GetFirstLogicalLine(num);
			this.textArea.Focus();
			if (!enableFolding || firstLogicalLine < 0 || firstLogicalLine + 1 >= this.textArea.Document.TotalNumberOfLines)
			{
				return;
			}
			foreach (FoldMarker foldingsWithStart in this.textArea.Document.FoldingManager.GetFoldingsWithStart(firstLogicalLine))
			{
				foldingsWithStart.IsFolded = !foldingsWithStart.IsFolded;
			}
			this.textArea.Document.FoldingManager.NotifyFoldingsChanged(EventArgs.Empty);
		}

		public override void HandleMouseLeave(EventArgs e)
		{
			if (this.selectedFoldLine != -1)
			{
				this.selectedFoldLine = -1;
				this.textArea.Refresh(this);
			}
		}

		public override void HandleMouseMove(Point mousepos, MouseButtons mouseButtons)
		{
			bool enableFolding = this.textArea.Document.TextEditorProperties.EnableFolding;
			int y = mousepos.Y;
			Point virtualTop = this.textArea.VirtualTop;
			int num = (y + virtualTop.Y) / this.textArea.TextView.FontHeight;
			int firstLogicalLine = this.textArea.Document.GetFirstLogicalLine(num);
			if (!enableFolding || firstLogicalLine < 0 || firstLogicalLine + 1 >= this.textArea.Document.TotalNumberOfLines)
			{
				return;
			}
			List<FoldMarker> foldingsWithStart = this.textArea.Document.FoldingManager.GetFoldingsWithStart(firstLogicalLine);
			int num1 = this.selectedFoldLine;
			if (foldingsWithStart.Count <= 0)
			{
				this.selectedFoldLine = -1;
			}
			else
			{
				this.selectedFoldLine = firstLogicalLine;
			}
			if (num1 != this.selectedFoldLine)
			{
				this.textArea.Refresh(this);
			}
		}

		public override void Paint(Graphics g, Rectangle rect)
		{
			if (rect.Width <= 0 || rect.Height <= 0)
			{
				return;
			}
			HighlightColor colorFor = this.textArea.Document.HighlightingStrategy.GetColorFor("LineNumbers");
			for (int i = 0; i < (base.DrawingPosition.Height + this.textArea.TextView.VisibleLineDrawingRemainder) / this.textArea.TextView.FontHeight + 1; i++)
			{
				int x = base.DrawingPosition.X;
				Rectangle drawingPosition = base.DrawingPosition;
				int top = drawingPosition.Top + i * this.textArea.TextView.FontHeight - this.textArea.TextView.VisibleLineDrawingRemainder;
				Rectangle rectangle = base.DrawingPosition;
				Rectangle rectangle1 = new Rectangle(x, top, rectangle.Width, this.textArea.TextView.FontHeight);
				if (rect.IntersectsWith(rectangle1))
				{
					if (!this.textArea.Document.TextEditorProperties.ShowLineNumbers)
					{
						g.FillRectangle(BrushRegistry.GetBrush((this.textArea.Enabled ? colorFor.BackgroundColor : SystemColors.InactiveBorder)), rectangle1);
					}
					else
					{
						g.FillRectangle(BrushRegistry.GetBrush((this.textArea.Enabled ? colorFor.BackgroundColor : SystemColors.InactiveBorder)), rectangle1);
						g.DrawLine(BrushRegistry.GetDotPen(colorFor.Color), this.drawingPosition.X, rectangle1.Y, this.drawingPosition.X, rectangle1.Bottom);
					}
					int firstLogicalLine = this.textArea.Document.GetFirstLogicalLine(this.textArea.TextView.FirstPhysicalLine + i);
					if (firstLogicalLine < this.textArea.Document.TotalNumberOfLines)
					{
						this.PaintFoldMarker(g, firstLogicalLine, rectangle1);
					}
				}
			}
		}

		private void PaintFoldMarker(Graphics g, int lineNumber, Rectangle drawingRectangle)
		{
			HighlightColor colorFor = this.textArea.Document.HighlightingStrategy.GetColorFor("FoldLine");
			HighlightColor highlightColor = this.textArea.Document.HighlightingStrategy.GetColorFor("SelectedFoldLine");
			List<FoldMarker> foldingsWithStart = this.textArea.Document.FoldingManager.GetFoldingsWithStart(lineNumber);
			List<FoldMarker> foldingsContainsLineNumber = this.textArea.Document.FoldingManager.GetFoldingsContainsLineNumber(lineNumber);
			List<FoldMarker> foldingsWithEnd = this.textArea.Document.FoldingManager.GetFoldingsWithEnd(lineNumber);
			bool count = foldingsWithStart.Count > 0;
			bool flag = foldingsContainsLineNumber.Count > 0;
			bool count1 = foldingsWithEnd.Count > 0;
			bool flag1 = this.SelectedFoldingFrom(foldingsWithStart);
			bool flag2 = this.SelectedFoldingFrom(foldingsContainsLineNumber);
			bool flag3 = this.SelectedFoldingFrom(foldingsWithEnd);
			int num = (int)Math.Round((double)((float)this.textArea.TextView.FontHeight * 0.57f));
			num = num - num % 2;
			int y = drawingRectangle.Y + (drawingRectangle.Height - num) / 2;
			int x = drawingRectangle.X + (drawingRectangle.Width - num) / 2 + num / 2;
			if (count)
			{
				bool flag4 = true;
				bool endLine = false;
				foreach (FoldMarker foldMarker in foldingsWithStart)
				{
					if (!foldMarker.IsFolded)
					{
						endLine = foldMarker.EndLine > foldMarker.StartLine;
					}
					else
					{
						flag4 = false;
					}
				}
				bool flag5 = false;
				foreach (FoldMarker foldMarker1 in foldingsWithEnd)
				{
					if (foldMarker1.EndLine <= foldMarker1.StartLine || foldMarker1.IsFolded)
					{
						continue;
					}
					flag5 = true;
				}
				this.DrawFoldMarker(g, new RectangleF((float)(drawingRectangle.X + (drawingRectangle.Width - num) / 2), (float)y, (float)num, (float)num), flag4, flag1);
				if (flag || flag5)
				{
					g.DrawLine(BrushRegistry.GetPen((flag2 ? highlightColor.Color : colorFor.Color)), x, drawingRectangle.Top, x, y - 1);
				}
				if (flag || endLine)
				{
					g.DrawLine(BrushRegistry.GetPen((flag3 || flag1 && flag4 || flag2 ? highlightColor.Color : colorFor.Color)), x, y + num + 1, x, drawingRectangle.Bottom);
					return;
				}
			}
			else if (count1)
			{
				int top = drawingRectangle.Top + drawingRectangle.Height / 2;
				g.DrawLine(BrushRegistry.GetPen((flag3 ? highlightColor.Color : colorFor.Color)), x, top, x + num / 2, top);
				g.DrawLine(BrushRegistry.GetPen((flag2 || flag3 ? highlightColor.Color : colorFor.Color)), x, drawingRectangle.Top, x, top);
				if (flag)
				{
					g.DrawLine(BrushRegistry.GetPen((flag2 ? highlightColor.Color : colorFor.Color)), x, top + 1, x, drawingRectangle.Bottom);
					return;
				}
			}
			else if (flag)
			{
				g.DrawLine(BrushRegistry.GetPen((flag2 ? highlightColor.Color : colorFor.Color)), x, drawingRectangle.Top, x, drawingRectangle.Bottom);
			}
		}

		private bool SelectedFoldingFrom(List<FoldMarker> list)
		{
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (this.selectedFoldLine == list[i].StartLine)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}