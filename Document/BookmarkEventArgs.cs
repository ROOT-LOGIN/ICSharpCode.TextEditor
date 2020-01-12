using System;

namespace ICSharpCode.TextEditor.Document
{
	public class BookmarkEventArgs : EventArgs
	{
		private ICSharpCode.TextEditor.Document.Bookmark bookmark;

		public ICSharpCode.TextEditor.Document.Bookmark Bookmark
		{
			get
			{
				return this.bookmark;
			}
		}

		public BookmarkEventArgs(ICSharpCode.TextEditor.Document.Bookmark bookmark)
		{
			this.bookmark = bookmark;
		}
	}
}