using System;
using System.Drawing;

namespace ICSharpCode.TextEditor.Util
{
	internal class TipSplitter : TipSection
	{
		private bool isHorizontal;

		private float[] offsets;

		private TipSection[] tipSections;

		public TipSplitter(System.Drawing.Graphics graphics, bool horizontal, params TipSection[] sections) : base(graphics)
		{
			this.isHorizontal = horizontal;
			this.offsets = new float[(int)sections.Length];
			this.tipSections = (TipSection[])sections.Clone();
		}

		public override void Draw(PointF location)
		{
			if (this.isHorizontal)
			{
				for (int i = 0; i < (int)this.tipSections.Length; i++)
				{
					this.tipSections[i].Draw(new PointF(location.X + this.offsets[i], location.Y));
				}
				return;
			}
			for (int j = 0; j < (int)this.tipSections.Length; j++)
			{
				this.tipSections[j].Draw(new PointF(location.X, location.Y + this.offsets[j]));
			}
		}

		protected override void OnMaximumSizeChanged()
		{
			float single;
			base.OnMaximumSizeChanged();
			float single1 = 0f;
			float single2 = 0f;
			SizeF maximumSize = base.MaximumSize;
			for (int i = 0; i < (int)this.tipSections.Length; i++)
			{
				TipSection tipSection = this.tipSections[i];
				tipSection.SetMaximumSize(maximumSize);
				SizeF requiredSize = tipSection.GetRequiredSize();
				this.offsets[i] = single1;
				if (!this.isHorizontal)
				{
					single = (float)Math.Ceiling((double)requiredSize.Height);
					single1 += single;
					maximumSize.Height = Math.Max(0f, maximumSize.Height - single);
					single2 = Math.Max(single2, requiredSize.Width);
				}
				else
				{
					single = (float)Math.Ceiling((double)requiredSize.Width);
					single1 += single;
					maximumSize.Width = Math.Max(0f, maximumSize.Width - single);
					single2 = Math.Max(single2, requiredSize.Height);
				}
			}
			TipSection[] tipSectionArray = this.tipSections;
			for (int j = 0; j < (int)tipSectionArray.Length; j++)
			{
				TipSection tipSection1 = tipSectionArray[j];
				if (!this.isHorizontal)
				{
					SizeF sizeF = tipSection1.GetRequiredSize();
					tipSection1.SetAllocatedSize(new SizeF(single2, sizeF.Height));
				}
				else
				{
					SizeF requiredSize1 = tipSection1.GetRequiredSize();
					tipSection1.SetAllocatedSize(new SizeF(requiredSize1.Width, single2));
				}
			}
			if (this.isHorizontal)
			{
				base.SetRequiredSize(new SizeF(single1, single2));
				return;
			}
			base.SetRequiredSize(new SizeF(single2, single1));
		}
	}
}