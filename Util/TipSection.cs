using System;
using System.Drawing;

namespace ICSharpCode.TextEditor.Util
{
	internal abstract class TipSection
	{
		private SizeF tipAllocatedSize;

		private System.Drawing.Graphics tipGraphics;

		private SizeF tipMaxSize;

		private SizeF tipRequiredSize;

		protected SizeF AllocatedSize
		{
			get
			{
				return this.tipAllocatedSize;
			}
		}

		protected System.Drawing.Graphics Graphics
		{
			get
			{
				return this.tipGraphics;
			}
		}

		protected SizeF MaximumSize
		{
			get
			{
				return this.tipMaxSize;
			}
		}

		protected TipSection(System.Drawing.Graphics graphics)
		{
			this.tipGraphics = graphics;
		}

		public abstract void Draw(PointF location);

		public SizeF GetRequiredSize()
		{
			return this.tipRequiredSize;
		}

		protected virtual void OnAllocatedSizeChanged()
		{
		}

		protected virtual void OnMaximumSizeChanged()
		{
		}

		public void SetAllocatedSize(SizeF allocatedSize)
		{
			this.tipAllocatedSize = allocatedSize;
			this.OnAllocatedSizeChanged();
		}

		public void SetMaximumSize(SizeF maximumSize)
		{
			this.tipMaxSize = maximumSize;
			this.OnMaximumSizeChanged();
		}

		protected void SetRequiredSize(SizeF requiredSize)
		{
			requiredSize.Width = Math.Max(0f, requiredSize.Width);
			requiredSize.Height = Math.Max(0f, requiredSize.Height);
			requiredSize.Width = Math.Min(this.tipMaxSize.Width, requiredSize.Width);
			requiredSize.Height = Math.Min(this.tipMaxSize.Height, requiredSize.Height);
			this.tipRequiredSize = requiredSize;
		}
	}
}