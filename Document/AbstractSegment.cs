using System;

namespace ICSharpCode.TextEditor.Document
{
	public class AbstractSegment : ISegment
	{
		[CLSCompliant(false)]
		protected int offset = -1;

		[CLSCompliant(false)]
		protected int length = -1;

		public virtual int Length
		{
			get
			{
				return this.length;
			}
			set
			{
				this.length = value;
			}
		}

		public virtual int Offset
		{
			get
			{
				return this.offset;
			}
			set
			{
				this.offset = value;
			}
		}

		public AbstractSegment()
		{
		}

		public override string ToString()
		{
			return string.Format("[AbstractSegment: Offset = {0}, Length = {1}]", this.Offset, this.Length);
		}
	}
}