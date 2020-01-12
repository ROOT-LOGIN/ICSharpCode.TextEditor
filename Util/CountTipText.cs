using ICSharpCode.TextEditor;
using System;
using System.Drawing;

namespace ICSharpCode.TextEditor.Util
{
	internal class CountTipText : TipText
	{
		private float triHeight = 10f;

		private float triWidth = 10f;

		public Rectangle DrawingRectangle1;

		public Rectangle DrawingRectangle2;

		public CountTipText(System.Drawing.Graphics graphics, Font font, string text) : base(graphics, font, text)
		{
		}

		public override void Draw(PointF location)
		{
			if (this.tipText != null && this.tipText.Length > 0)
			{
				base.Draw(new PointF(location.X + this.triWidth + 4f, location.Y));
				this.DrawingRectangle1 = new Rectangle((int)location.X + 2, (int)location.Y + 2, (int)this.triWidth, (int)this.triHeight);
				float x = location.X;
				SizeF allocatedSize = base.AllocatedSize;
				this.DrawingRectangle2 = new Rectangle((int)(x + allocatedSize.Width - this.triWidth - 2f), (int)location.Y + 2, (int)this.triWidth, (int)this.triHeight);
				this.DrawTriangle(location.X + 2f, location.Y + 2f, false);
				float single = location.X;
				SizeF sizeF = base.AllocatedSize;
				this.DrawTriangle(single + sizeF.Width - this.triWidth - 2f, location.Y + 2f, true);
			}
		}

		private void DrawTriangle(float x, float y, bool flipped)
		{
			Brush brush = BrushRegistry.GetBrush(System.Drawing.Color.FromArgb(192, 192, 192));
			base.Graphics.FillRectangle(brush, new RectangleF(x, y, this.triHeight, this.triHeight));
			float single = this.triHeight / 2f;
			float single1 = this.triHeight / 4f;
			brush = Brushes.Black;
			if (flipped)
			{
				System.Drawing.Graphics graphics = base.Graphics;
				PointF[] pointF = new PointF[] { new PointF(x, y + single - single1), new PointF(x + this.triWidth / 2f, y + single + single1), new PointF(x + this.triWidth, y + single - single1) };
				graphics.FillPolygon(brush, pointF);
				return;
			}
			System.Drawing.Graphics graphic = base.Graphics;
			PointF[] pointFArray = new PointF[] { new PointF(x, y + single + single1), new PointF(x + this.triWidth / 2f, y + single - single1), new PointF(x + this.triWidth, y + single + single1) };
			graphic.FillPolygon(brush, pointFArray);
		}

		protected override void OnMaximumSizeChanged()
		{
			if (!base.IsTextVisible())
			{
				base.SetRequiredSize(SizeF.Empty);
				return;
			}
			SizeF width = base.Graphics.MeasureString(this.tipText, this.tipFont, base.MaximumSize, base.GetInternalStringFormat());
			width.Width = width.Width + (this.triWidth * 2f + 8f);
			base.SetRequiredSize(width);
		}
	}
}