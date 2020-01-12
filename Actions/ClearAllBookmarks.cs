using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class ClearAllBookmarks : AbstractEditAction
	{
		private Predicate<Bookmark> predicate;

		public ClearAllBookmarks(Predicate<Bookmark> predicate)
		{
			this.predicate = predicate;
		}

		public override void Execute(TextArea textArea)
		{
			textArea.Document.BookmarkManager.RemoveMarks(this.predicate);
			textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
			textArea.Document.CommitUpdate();
		}
	}
}