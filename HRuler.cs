using System;
using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor
{
	public class HRuler : Control
	{
		private TextArea textArea;

		public HRuler(TextArea textArea)
		{
			this.textArea = textArea;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics graphics = e.Graphics;
			int num = 0;
			for (float i = (float)this.textArea.TextView.DrawingPosition.Left; i < (float)this.textArea.TextView.DrawingPosition.Right; i += (float)this.textArea.TextView.WideSpaceWidth)
			{
				int height = base.Height * 2 / 3;
				if (num % 5 == 0)
				{
					height = base.Height * 4 / 5;
				}
				if (num % 10 == 0)
				{
					height = 1;
				}
				num++;
				graphics.DrawLine(Pens.Black, (int)i, height, (int)i, base.Height - height);
			}
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			e.Graphics.FillRectangle(Brushes.White, new Rectangle(0, 0, base.Width, base.Height));
		}
	}
}