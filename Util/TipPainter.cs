using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor.Util
{
	internal static class TipPainter
	{
		private const float HorizontalBorder = 2f;

		private const float VerticalBorder = 1f;

		public static Size DrawFixedWidthTip(Control control, Graphics graphics, TipSection tipData)
		{
			Size empty = Size.Empty;
			SizeF requiredSize = SizeF.Empty;
			PointF screen = control.PointToScreen(new Point(control.Width, 0));
			RectangleF workingArea = TipPainter.GetWorkingArea(control);
			SizeF sizeF = new SizeF(screen.X - 4f, workingArea.Bottom - screen.Y - 2f);
			if (sizeF.Width > 0f && sizeF.Height > 0f)
			{
				graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
				tipData.SetMaximumSize(sizeF);
				requiredSize = tipData.GetRequiredSize();
				tipData.SetAllocatedSize(requiredSize);
				requiredSize += new SizeF(4f, 2f);
				empty = Size.Ceiling(requiredSize);
			}
			if (control.Height != empty.Height)
			{
				control.Height = empty.Height;
			}
			if (empty != Size.Empty)
			{
				Rectangle rectangle = new Rectangle(Point.Empty, control.Size - new Size(1, 1));
				RectangleF rectangleF = new RectangleF(2f, 1f, requiredSize.Width - 4f, requiredSize.Height - 2f);
				graphics.DrawRectangle(SystemPens.WindowFrame, rectangle);
				tipData.Draw(new PointF(2f, 1f));
			}
			return empty;
		}

		public static Size DrawTip(Control control, Graphics graphics, Font font, string description)
		{
			return TipPainter.DrawTip(control, graphics, new TipText(graphics, font, description));
		}

		public static Size DrawTip(Control control, Graphics graphics, TipSection tipData)
		{
			Size empty = Size.Empty;
			SizeF requiredSize = SizeF.Empty;
			PointF screen = control.PointToScreen(Point.Empty);
			RectangleF workingArea = TipPainter.GetWorkingArea(control);
			SizeF sizeF = new SizeF(workingArea.Right - screen.X - 4f, workingArea.Bottom - screen.Y - 2f);
			if (sizeF.Width > 0f && sizeF.Height > 0f)
			{
				graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
				tipData.SetMaximumSize(sizeF);
				requiredSize = tipData.GetRequiredSize();
				tipData.SetAllocatedSize(requiredSize);
				requiredSize += new SizeF(4f, 2f);
				empty = Size.Ceiling(requiredSize);
			}
			if (control.ClientSize != empty)
			{
				control.ClientSize = empty;
			}
			if (empty != Size.Empty)
			{
				Rectangle rectangle = new Rectangle(Point.Empty, empty - new Size(1, 1));
				RectangleF rectangleF = new RectangleF(2f, 1f, requiredSize.Width - 4f, requiredSize.Height - 2f);
				graphics.DrawRectangle(SystemPens.WindowFrame, rectangle);
				tipData.Draw(new PointF(2f, 1f));
			}
			return empty;
		}

		public static Size GetLeftHandSideTipSize(Control control, Graphics graphics, TipSection tipData, Point p)
		{
			Size empty = Size.Empty;
			SizeF requiredSize = SizeF.Empty;
			RectangleF workingArea = TipPainter.GetWorkingArea(control);
			PointF pointF = p;
			SizeF sizeF = new SizeF(pointF.X - 4f, workingArea.Bottom - pointF.Y - 2f);
			if (sizeF.Width > 0f && sizeF.Height > 0f)
			{
				graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
				tipData.SetMaximumSize(sizeF);
				requiredSize = tipData.GetRequiredSize();
				tipData.SetAllocatedSize(requiredSize);
				requiredSize += new SizeF(4f, 2f);
				empty = Size.Ceiling(requiredSize);
			}
			return empty;
		}

		public static Size GetTipSize(Control control, Graphics graphics, Font font, string description)
		{
			return TipPainter.GetTipSize(control, graphics, new TipText(graphics, font, description));
		}

		public static Size GetTipSize(Control control, Graphics graphics, TipSection tipData)
		{
			Size empty = Size.Empty;
			SizeF requiredSize = SizeF.Empty;
			RectangleF workingArea = TipPainter.GetWorkingArea(control);
			PointF screen = control.PointToScreen(Point.Empty);
			SizeF sizeF = new SizeF(workingArea.Right - screen.X - 4f, workingArea.Bottom - screen.Y - 2f);
			if (sizeF.Width > 0f && sizeF.Height > 0f)
			{
				graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
				tipData.SetMaximumSize(sizeF);
				requiredSize = tipData.GetRequiredSize();
				tipData.SetAllocatedSize(requiredSize);
				requiredSize += new SizeF(4f, 2f);
				empty = Size.Ceiling(requiredSize);
			}
			if (control.ClientSize != empty)
			{
				control.ClientSize = empty;
			}
			return empty;
		}

		private static Rectangle GetWorkingArea(Control control)
		{
			Form owner = control.FindForm();
			if (owner.Owner != null)
			{
				owner = owner.Owner;
			}
			return Screen.GetWorkingArea(owner);
		}
	}
}