using System;

namespace ICSharpCode.TextEditor.Document
{
	public class ColumnRange
	{
		public readonly static ColumnRange NoColumn;

		public readonly static ColumnRange WholeColumn;

		private int startColumn;

		private int endColumn;

		public int EndColumn
		{
			get
			{
				return this.endColumn;
			}
			set
			{
				this.endColumn = value;
			}
		}

		public int StartColumn
		{
			get
			{
				return this.startColumn;
			}
			set
			{
				this.startColumn = value;
			}
		}

		static ColumnRange()
		{
			ColumnRange.NoColumn = new ColumnRange(-2, -2);
			ColumnRange.WholeColumn = new ColumnRange(-1, -1);
		}

		public ColumnRange(int startColumn, int endColumn)
		{
			this.startColumn = startColumn;
			this.endColumn = endColumn;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ColumnRange))
			{
				return false;
			}
			if (((ColumnRange)obj).startColumn != this.startColumn)
			{
				return false;
			}
			return ((ColumnRange)obj).endColumn == this.endColumn;
		}

		public override int GetHashCode()
		{
			return this.startColumn + (this.endColumn << 16);
		}

		public override string ToString()
		{
			return string.Format("[ColumnRange: StartColumn={0}, EndColumn={1}]", this.startColumn, this.endColumn);
		}
	}
}