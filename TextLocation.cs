using System;

namespace ICSharpCode.TextEditor
{
	public struct TextLocation : IComparable<TextLocation>, IEquatable<TextLocation>
	{
		public readonly static TextLocation Empty;

		private int x;

		private int y;

		public int Column
		{
			get
			{
				return this.x;
			}
			set
			{
				this.x = value;
			}
		}

		public bool IsEmpty
		{
			get
			{
				if (this.x > 0)
				{
					return false;
				}
				return this.y <= 0;
			}
		}

		public int Line
		{
			get
			{
				return this.y;
			}
			set
			{
				this.y = value;
			}
		}

		public int X
		{
			get
			{
				return this.x;
			}
			set
			{
				this.x = value;
			}
		}

		public int Y
		{
			get
			{
				return this.y;
			}
			set
			{
				this.y = value;
			}
		}

		static TextLocation()
		{
			TextLocation.Empty = new TextLocation(-1, -1);
		}

		public TextLocation(int column, int line)
		{
			this.x = column;
			this.y = line;
		}

		public int CompareTo(TextLocation other)
		{
			if (this == other)
			{
				return 0;
			}
			if (this < other)
			{
				return -1;
			}
			return 1;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is TextLocation))
			{
				return false;
			}
			return (TextLocation)obj == this;
		}

		public bool Equals(TextLocation other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return 87 * this.x.GetHashCode() ^ this.y.GetHashCode();
		}

		public static bool operator ==(TextLocation a, TextLocation b)
		{
			if (a.x != b.x)
			{
				return false;
			}
			return a.y == b.y;
		}

		public static bool operator >(TextLocation a, TextLocation b)
		{
			if (a.y > b.y)
			{
				return true;
			}
			if (a.y != b.y)
			{
				return false;
			}
			return a.x > b.x;
		}

		public static bool operator >=(TextLocation a, TextLocation b)
		{
			return !(a < b);
		}

		public static bool operator !=(TextLocation a, TextLocation b)
		{
			if (a.x != b.x)
			{
				return true;
			}
			return a.y != b.y;
		}

		public static bool operator <(TextLocation a, TextLocation b)
		{
			if (a.y < b.y)
			{
				return true;
			}
			if (a.y != b.y)
			{
				return false;
			}
			return a.x < b.x;
		}

		public static bool operator <=(TextLocation a, TextLocation b)
		{
			return !(a > b);
		}

		public override string ToString()
		{
			return string.Format("(Line {1}, Col {0})", this.x, this.y);
		}
	}
}