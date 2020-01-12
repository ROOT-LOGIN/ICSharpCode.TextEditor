using ICSharpCode.TextEditor;
using System;
using System.Runtime.CompilerServices;

namespace ICSharpCode.TextEditor.Document
{
	public sealed class TextAnchor
	{
		private LineSegment lineSegment;

		private int columnNumber;

		public int ColumnNumber
		{
			get
			{
				if (this.lineSegment == null)
				{
					throw TextAnchor.AnchorDeletedError();
				}
				return this.columnNumber;
			}
			internal set
			{
				this.columnNumber = value;
			}
		}

		public bool IsDeleted
		{
			get
			{
				return this.lineSegment == null;
			}
		}

		public LineSegment Line
		{
			get
			{
				if (this.lineSegment == null)
				{
					throw TextAnchor.AnchorDeletedError();
				}
				return this.lineSegment;
			}
			internal set
			{
				this.lineSegment = value;
			}
		}

		public int LineNumber
		{
			get
			{
				return this.Line.LineNumber;
			}
		}

		public TextLocation Location
		{
			get
			{
				return new TextLocation(this.ColumnNumber, this.LineNumber);
			}
		}

		public AnchorMovementType MovementType
		{
			get;
			set;
		}

		public int Offset
		{
			get
			{
				return this.Line.Offset + this.columnNumber;
			}
		}

		internal TextAnchor(LineSegment lineSegment, int columnNumber)
		{
			this.lineSegment = lineSegment;
			this.columnNumber = columnNumber;
		}

		private static Exception AnchorDeletedError()
		{
			return new InvalidOperationException("The text containing the anchor was deleted");
		}

		internal void Delete(ref DeferredEventList deferredEventList)
		{
			this.lineSegment = null;
			deferredEventList.AddDeletedAnchor(this);
		}

		internal void RaiseDeleted()
		{
			if (this.Deleted != null)
			{
				this.Deleted(this, EventArgs.Empty);
			}
		}

		public override string ToString()
		{
			if (this.IsDeleted)
			{
				return "[TextAnchor (deleted)]";
			}
			TextLocation location = this.Location;
			return string.Concat("[TextAnchor ", location.ToString(), "]");
		}

		public event EventHandler Deleted;
	}
}