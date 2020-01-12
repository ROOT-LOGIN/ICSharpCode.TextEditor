using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class BlockCommentRegion
	{
		private string commentStart = string.Empty;

		private string commentEnd = string.Empty;

		private int startOffset = -1;

		private int endOffset = -1;

		public string CommentEnd
		{
			get
			{
				return this.commentEnd;
			}
		}

		public string CommentStart
		{
			get
			{
				return this.commentStart;
			}
		}

		public int EndOffset
		{
			get
			{
				return this.endOffset;
			}
		}

		public int StartOffset
		{
			get
			{
				return this.startOffset;
			}
		}

		public BlockCommentRegion(string commentStart, string commentEnd, int startOffset, int endOffset)
		{
			this.commentStart = commentStart;
			this.commentEnd = commentEnd;
			this.startOffset = startOffset;
			this.endOffset = endOffset;
		}

		public override bool Equals(object obj)
		{
			BlockCommentRegion blockCommentRegion = obj as BlockCommentRegion;
			if (blockCommentRegion == null)
			{
				return false;
			}
			if (!(this.commentStart == blockCommentRegion.commentStart) || !(this.commentEnd == blockCommentRegion.commentEnd) || this.startOffset != blockCommentRegion.startOffset)
			{
				return false;
			}
			return this.endOffset == blockCommentRegion.endOffset;
		}

		public override int GetHashCode()
		{
			int hashCode = 0;
			if (this.commentStart != null)
			{
				hashCode = hashCode + 1000000007 * this.commentStart.GetHashCode();
			}
			if (this.commentEnd != null)
			{
				hashCode = hashCode + 1000000009 * this.commentEnd.GetHashCode();
			}
			hashCode = hashCode + 1000000021 * this.startOffset.GetHashCode();
			hashCode = hashCode + 1000000033 * this.endOffset.GetHashCode();
			return hashCode;
		}
	}
}