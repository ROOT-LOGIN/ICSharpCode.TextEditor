using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;

namespace ICSharpCode.TextEditor.Actions
{
	public class GotoNextBookmark : AbstractEditAction
	{
		private Predicate<Bookmark> predicate;

		public GotoNextBookmark(Predicate<Bookmark> predicate)
		{
			this.predicate = predicate;
		}

		public override void Execute(TextArea textArea)
		{
			Bookmark nextMark = textArea.Document.BookmarkManager.GetNextMark(textArea.Caret.Line, this.predicate);
			if (nextMark != null)
			{
				textArea.Caret.Position = nextMark.Location;
				textArea.SelectionManager.ClearSelection();
				textArea.SetDesiredColumn();
			}
		}
	}
}