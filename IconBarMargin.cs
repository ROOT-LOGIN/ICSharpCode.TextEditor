using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor
{
	public class IconBarMargin : AbstractMargin
	{
		private const int iconBarWidth = 18;

		private readonly static System.Drawing.Size iconBarSize;

		public override bool IsVisible
		{
			get
			{
				return this.textArea.TextEditorProperties.IsIconBarVisible;
			}
		}

		public override System.Drawing.Size Size
		{
			get
			{
				return IconBarMargin.iconBarSize;
			}
		}

		static IconBarMargin()
		{
			IconBarMargin.iconBarSize = new System.Drawing.Size(18, -1);
		}

		public IconBarMargin(ICSharpCode.TextEditor.TextArea textArea) : base(textArea)
		{
		}

		private GraphicsPath CreateArrowGraphicsPath(Rectangle r)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			int width = r.Width / 2;
			int height = r.Height / 2;
			graphicsPath.AddLine(r.X, r.Y + height / 2, r.X + width, r.Y + height / 2);
			graphicsPath.AddLine(r.X + width, r.Y + height / 2, r.X + width, r.Y);
			graphicsPath.AddLine(r.X + width, r.Y, r.Right, r.Y + height);
			graphicsPath.AddLine(r.Right, r.Y + height, r.X + width, r.Bottom);
			graphicsPath.AddLine(r.X + width, r.Bottom, r.X + width, r.Bottom - height / 2);
			graphicsPath.AddLine(r.X + width, r.Bottom - height / 2, r.X, r.Bottom - height / 2);
			graphicsPath.AddLine(r.X, r.Bottom - height / 2, r.X, r.Y + height / 2);
			graphicsPath.CloseFigure();
			return graphicsPath;
		}

		private GraphicsPath CreateRoundRectGraphicsPath(Rectangle r)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			int width = r.Width / 2;
			graphicsPath.AddLine(r.X + width, r.Y, r.Right - width, r.Y);
			graphicsPath.AddArc(r.Right - width, r.Y, width, width, 270f, 90f);
			graphicsPath.AddLine(r.Right, r.Y + width, r.Right, r.Bottom - width);
			graphicsPath.AddArc(r.Right - width, r.Bottom - width, width, width, 0f, 90f);
			graphicsPath.AddLine(r.Right - width, r.Bottom, r.X + width, r.Bottom);
			graphicsPath.AddArc(r.X, r.Bottom - width, width, width, 90f, 90f);
			graphicsPath.AddLine(r.X, r.Bottom - width, r.X, r.Y + width);
			graphicsPath.AddArc(r.X, r.Y, width, width, 180f, 90f);
			graphicsPath.CloseFigure();
			return graphicsPath;
		}

		public void DrawArrow(Graphics g, int y)
		{
			int fontHeight = this.textArea.TextView.FontHeight / 8;
			Rectangle rectangle = new Rectangle(1, y + fontHeight, this.drawingPosition.Width - 4, this.textArea.TextView.FontHeight - fontHeight * 2);
			using (Brush linearGradientBrush = new LinearGradientBrush(new Point(rectangle.Left, rectangle.Top), new Point(rectangle.Right, rectangle.Bottom), Color.LightYellow, Color.Yellow))
			{
				this.FillArrow(g, linearGradientBrush, rectangle);
			}
			using (Brush brush = new LinearGradientBrush(new Point(rectangle.Left, rectangle.Top), new Point(rectangle.Right, rectangle.Bottom), Color.Yellow, Color.Brown))
			{
				using (Pen pen = new Pen(brush))
				{
					this.DrawArrow(g, pen, rectangle);
				}
			}
		}

		private void DrawArrow(Graphics g, Pen p, Rectangle r)
		{
			using (GraphicsPath graphicsPath = this.CreateArrowGraphicsPath(r))
			{
				g.DrawPath(p, graphicsPath);
			}
		}

		public void DrawBookmark(Graphics g, int y, bool isEnabled)
		{
			int fontHeight = this.textArea.TextView.FontHeight / 8;
			Rectangle rectangle = new Rectangle(1, y + fontHeight, this.drawingPosition.Width - 4, this.textArea.TextView.FontHeight - fontHeight * 2);
			if (!isEnabled)
			{
				this.FillRoundRect(g, Brushes.White, rectangle);
			}
			else
			{
				using (Brush linearGradientBrush = new LinearGradientBrush(new Point(rectangle.Left, rectangle.Top), new Point(rectangle.Right, rectangle.Bottom), Color.SkyBlue, Color.White))
				{
					this.FillRoundRect(g, linearGradientBrush, rectangle);
				}
			}
			using (Brush brush = new LinearGradientBrush(new Point(rectangle.Left, rectangle.Top), new Point(rectangle.Right, rectangle.Bottom), Color.SkyBlue, Color.Blue))
			{
				using (Pen pen = new Pen(brush))
				{
					this.DrawRoundRect(g, pen, rectangle);
				}
			}
		}

		public void DrawBreakpoint(Graphics g, int y, bool isEnabled, bool isHealthy)
		{
			Color color;
			int num = Math.Min(16, this.textArea.TextView.FontHeight);
			Rectangle rectangle = new Rectangle(1, y + (this.textArea.TextView.FontHeight - num) / 2, num, num);
			using (GraphicsPath graphicsPath = new GraphicsPath())
			{
				graphicsPath.AddEllipse(rectangle);
				using (PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath))
				{
					pathGradientBrush.CenterPoint = new PointF((float)(rectangle.Left + rectangle.Width / 3), (float)(rectangle.Top + rectangle.Height / 3));
					pathGradientBrush.CenterColor = Color.MistyRose;
					Color[] colorArray = new Color[1];
					color = (isHealthy ? Color.Firebrick : Color.Olive);
					colorArray[0] = color;
					pathGradientBrush.SurroundColors = colorArray;
					if (!isEnabled)
					{
						g.FillEllipse(SystemBrushes.Control, rectangle);
						using (Pen pen = new Pen(pathGradientBrush))
						{
							g.DrawEllipse(pen, new Rectangle(rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2));
						}
					}
					else
					{
						g.FillEllipse(pathGradientBrush, rectangle);
					}
				}
			}
		}

		private void DrawRoundRect(Graphics g, Pen p, Rectangle r)
		{
			using (GraphicsPath graphicsPath = this.CreateRoundRectGraphicsPath(r))
			{
				g.DrawPath(p, graphicsPath);
			}
		}

		private void FillArrow(Graphics g, Brush b, Rectangle r)
		{
			using (GraphicsPath graphicsPath = this.CreateArrowGraphicsPath(r))
			{
				g.FillPath(b, graphicsPath);
			}
		}

		private void FillRoundRect(Graphics g, Brush b, Rectangle r)
		{
			using (GraphicsPath graphicsPath = this.CreateRoundRectGraphicsPath(r))
			{
				g.FillPath(b, graphicsPath);
			}
		}

		public override void HandleMouseDown(Point mousePos, MouseButtons mouseButtons)
		{
			int y = mousePos.Y;
			Point virtualTop = this.textArea.VirtualTop;
			int num = (y + virtualTop.Y) / this.textArea.TextView.FontHeight;
			int firstLogicalLine = this.textArea.Document.GetFirstLogicalLine(num);
			if ((mouseButtons & MouseButtons.Right) == MouseButtons.Right && this.textArea.Caret.Line != firstLogicalLine)
			{
				this.textArea.Caret.Line = firstLogicalLine;
			}
			IList<Bookmark> marks = this.textArea.Document.BookmarkManager.Marks;
			List<Bookmark> bookmarks = new List<Bookmark>();
			int count = marks.Count;
			foreach (Bookmark mark in marks)
			{
				if (mark.LineNumber != firstLogicalLine)
				{
					continue;
				}
				bookmarks.Add(mark);
			}
			for (int i = bookmarks.Count - 1; i >= 0; i--)
			{
				if (bookmarks[i].Click(this.textArea, new MouseEventArgs(mouseButtons, 1, mousePos.X, mousePos.Y, 0)))
				{
					if (count != marks.Count)
					{
						this.textArea.UpdateLine(firstLogicalLine);
					}
					return;
				}
			}
			base.HandleMouseDown(mousePos, mouseButtons);
		}

		private static bool IsLineInsideRegion(int top, int bottom, int regionTop, int regionBottom)
		{
			if (top >= regionTop && top <= regionBottom)
			{
				return true;
			}
			if (regionTop > top && regionTop < bottom)
			{
				return true;
			}
			return false;
		}

		public override void Paint(Graphics g, Rectangle rect)
		{
			if (rect.Width <= 0 || rect.Height <= 0)
			{
				return;
			}
			g.FillRectangle(SystemBrushes.Control, new Rectangle(this.drawingPosition.X, rect.Top, this.drawingPosition.Width - 1, rect.Height));
			g.DrawLine(SystemPens.ControlDark, this.drawingPosition.Right - 1, rect.Top, this.drawingPosition.Right - 1, rect.Bottom);
			foreach (Bookmark mark in this.textArea.Document.BookmarkManager.Marks)
			{
				int visibleLine = this.textArea.Document.GetVisibleLine(mark.LineNumber);
				int fontHeight = this.textArea.TextView.FontHeight;
				Point virtualTop = this.textArea.VirtualTop;
				int y = visibleLine * fontHeight - virtualTop.Y;
				if (!IconBarMargin.IsLineInsideRegion(y, y + fontHeight, rect.Y, rect.Bottom) || visibleLine == this.textArea.Document.GetVisibleLine(mark.LineNumber - 1))
				{
					continue;
				}
				mark.Draw(this, g, new Point(0, y));
			}
			base.Paint(g, rect);
		}
	}
}