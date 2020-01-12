using ICSharpCode.TextEditor;
using System;

namespace ICSharpCode.TextEditor.Document
{
	public class DefaultSelection : ISelection
	{
		private IDocument document;

		private bool isRectangularSelection;

		private TextLocation startPosition;

		private TextLocation endPosition;

		public int EndOffset
		{
			get
			{
				return this.document.PositionToOffset(this.endPosition);
			}
		}

		public TextLocation EndPosition
		{
			get
			{
				return this.endPosition;
			}
			set
			{
				this.endPosition = value;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return this.startPosition == this.endPosition;
			}
		}

		public bool IsRectangularSelection
		{
			get
			{
				return JustDecompileGenerated_get_IsRectangularSelection();
			}
			set
			{
				JustDecompileGenerated_set_IsRectangularSelection(value);
			}
		}

		public bool JustDecompileGenerated_get_IsRectangularSelection()
		{
			return this.isRectangularSelection;
		}

		public void JustDecompileGenerated_set_IsRectangularSelection(bool value)
		{
			this.isRectangularSelection = value;
		}

		public int Length
		{
			get
			{
				return this.EndOffset - this.Offset;
			}
		}

		public int Offset
		{
			get
			{
				return this.document.PositionToOffset(this.startPosition);
			}
		}

		public string SelectedText
		{
			get
			{
				if (this.document == null)
				{
					return null;
				}
				if (this.Length < 0)
				{
					return null;
				}
				return this.document.GetText(this.Offset, this.Length);
			}
		}

		public TextLocation StartPosition
		{
			get
			{
				return this.startPosition;
			}
			set
			{
				this.startPosition = value;
			}
		}

		public DefaultSelection(IDocument document, TextLocation startPosition, TextLocation endPosition)
		{
			this.document = document;
			this.startPosition = startPosition;
			this.endPosition = endPosition;
		}

		public bool ContainsOffset(int offset)
		{
			if (this.Offset > offset)
			{
				return false;
			}
			return offset <= this.EndOffset;
		}

		public bool ContainsPosition(TextLocation position)
		{
			if (this.IsEmpty)
			{
				return false;
			}
			if (this.startPosition.Y < position.Y && position.Y < this.endPosition.Y || this.startPosition.Y == position.Y && this.startPosition.X <= position.X && (this.startPosition.Y != this.endPosition.Y || position.X <= this.endPosition.X))
			{
				return true;
			}
			if (this.endPosition.Y != position.Y || this.startPosition.Y == this.endPosition.Y)
			{
				return false;
			}
			return position.X <= this.endPosition.X;
		}

		public override string ToString()
		{
			return string.Format("[DefaultSelection : StartPosition={0}, EndPosition={1}]", this.startPosition, this.endPosition);
		}
	}
}