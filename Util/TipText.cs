using ICSharpCode.TextEditor;
using System;
using System.Drawing;

namespace ICSharpCode.TextEditor.Util
{
	internal class TipText : TipSection
	{
		protected StringAlignment horzAlign;

		protected StringAlignment vertAlign;

		protected System.Drawing.Color tipColor;

		protected Font tipFont;

		protected StringFormat tipFormat;

		protected string tipText;

		public System.Drawing.Color Color
		{
			get
			{
				return this.tipColor;
			}
			set
			{
				this.tipColor = value;
			}
		}

		public StringAlignment HorizontalAlignment
		{
			get
			{
				return this.horzAlign;
			}
			set
			{
				this.horzAlign = value;
				this.tipFormat = null;
			}
		}

		public StringAlignment VerticalAlignment
		{
			get
			{
				return this.vertAlign;
			}
			set
			{
				this.vertAlign = value;
				this.tipFormat = null;
			}
		}

		public TipText(System.Drawing.Graphics graphics, Font font, string text) : base(graphics)
		{
			this.tipFont = font;
			this.tipText = text;
			if (text != null && text.Length > 32767)
			{
				throw new ArgumentException(string.Concat("TipText: text too long (max. is ", (short)32767, " characters)"), "text");
			}
			this.Color = SystemColors.InfoText;
			this.HorizontalAlignment = StringAlignment.Near;
			this.VerticalAlignment = StringAlignment.Near;
		}

		private static StringFormat CreateTipStringFormat(StringAlignment horizontalAlignment, StringAlignment verticalAlignment)
		{
			StringFormat stringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
			stringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.MeasureTrailingSpaces;
			stringFormat.Alignment = horizontalAlignment;
			stringFormat.LineAlignment = verticalAlignment;
			return stringFormat;
		}

		public override void Draw(PointF location)
		{
			if (this.IsTextVisible())
			{
				RectangleF rectangleF = new RectangleF(location, base.AllocatedSize);
				base.Graphics.DrawString(this.tipText, this.tipFont, BrushRegistry.GetBrush(this.Color), rectangleF, this.GetInternalStringFormat());
			}
		}

		protected StringFormat GetInternalStringFormat()
		{
			if (this.tipFormat == null)
			{
				this.tipFormat = TipText.CreateTipStringFormat(this.horzAlign, this.vertAlign);
			}
			return this.tipFormat;
		}

		protected bool IsTextVisible()
		{
			if (this.tipText == null)
			{
				return false;
			}
			return this.tipText.Length > 0;
		}

		protected override void OnMaximumSizeChanged()
		{
			base.OnMaximumSizeChanged();
			if (!this.IsTextVisible())
			{
				base.SetRequiredSize(SizeF.Empty);
				return;
			}
			SizeF sizeF = base.Graphics.MeasureString(this.tipText, this.tipFont, base.MaximumSize, this.GetInternalStringFormat());
			base.SetRequiredSize(sizeF);
		}
	}
}