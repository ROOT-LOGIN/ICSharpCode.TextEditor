using System;
using System.Drawing;

namespace ICSharpCode.TextEditor.Util
{
	internal class TipSpacer : TipSection
	{
		private SizeF spacerSize;

		public TipSpacer(System.Drawing.Graphics graphics, SizeF size) : base(graphics)
		{
			this.spacerSize = size;
		}

		public override void Draw(PointF location)
		{
		}

		protected override void OnMaximumSizeChanged()
		{
			base.OnMaximumSizeChanged();
			SizeF maximumSize = base.MaximumSize;
			float single = Math.Min(maximumSize.Width, this.spacerSize.Width);
			SizeF sizeF = base.MaximumSize;
			base.SetRequiredSize(new SizeF(single, Math.Min(sizeF.Height, this.spacerSize.Height)));
		}
	}
}