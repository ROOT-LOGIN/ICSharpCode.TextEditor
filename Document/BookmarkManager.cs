using ICSharpCode.TextEditor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace ICSharpCode.TextEditor.Document
{
	public class BookmarkManager
	{
		private IDocument document;

		private List<Bookmark> bookmark = new List<Bookmark>();

		public IDocument Document
		{
			get
			{
				return this.document;
			}
		}

		public IBookmarkFactory Factory
		{
			get;
			set;
		}

		public ReadOnlyCollection<Bookmark> Marks
		{
			get
			{
				return new ReadOnlyCollection<Bookmark>(this.bookmark);
			}
		}

		internal BookmarkManager(IDocument document, LineManager lineTracker)
		{
			this.document = document;
		}

		private bool AcceptAnyMarkPredicate(Bookmark mark)
		{
			return true;
		}

		public void AddMark(Bookmark mark)
		{
			this.bookmark.Add(mark);
			this.OnAdded(new BookmarkEventArgs(mark));
		}

		public void Clear()
		{
			foreach (Bookmark bookmark in this.bookmark)
			{
				this.OnRemoved(new BookmarkEventArgs(bookmark));
			}
			this.bookmark.Clear();
		}

		public Bookmark GetFirstMark(Predicate<Bookmark> predicate)
		{
			if (this.bookmark.Count < 1)
			{
				return null;
			}
			Bookmark item = null;
			for (int i = 0; i < this.bookmark.Count; i++)
			{
				if (predicate(this.bookmark[i]) && this.bookmark[i].IsEnabled && (item == null || this.bookmark[i].LineNumber < item.LineNumber))
				{
					item = this.bookmark[i];
				}
			}
			return item;
		}

		public Bookmark GetLastMark(Predicate<Bookmark> predicate)
		{
			if (this.bookmark.Count < 1)
			{
				return null;
			}
			Bookmark item = null;
			for (int i = 0; i < this.bookmark.Count; i++)
			{
				if (predicate(this.bookmark[i]) && this.bookmark[i].IsEnabled && (item == null || this.bookmark[i].LineNumber > item.LineNumber))
				{
					item = this.bookmark[i];
				}
			}
			return item;
		}

		public Bookmark GetNextMark(int curLineNr)
		{
			return this.GetNextMark(curLineNr, new Predicate<Bookmark>(this.AcceptAnyMarkPredicate));
		}

		public Bookmark GetNextMark(int curLineNr, Predicate<Bookmark> predicate)
		{
			if (this.bookmark.Count == 0)
			{
				return null;
			}
			Bookmark firstMark = this.GetFirstMark(predicate);
			foreach (Bookmark bookmark in this.bookmark)
			{
				if (!predicate(bookmark) || !bookmark.IsEnabled || bookmark.LineNumber <= curLineNr || bookmark.LineNumber >= firstMark.LineNumber && firstMark.LineNumber > curLineNr)
				{
					continue;
				}
				firstMark = bookmark;
			}
			return firstMark;
		}

		public Bookmark GetPrevMark(int curLineNr)
		{
			return this.GetPrevMark(curLineNr, new Predicate<Bookmark>(this.AcceptAnyMarkPredicate));
		}

		public Bookmark GetPrevMark(int curLineNr, Predicate<Bookmark> predicate)
		{
			if (this.bookmark.Count == 0)
			{
				return null;
			}
			Bookmark lastMark = this.GetLastMark(predicate);
			foreach (Bookmark bookmark in this.bookmark)
			{
				if (!predicate(bookmark) || !bookmark.IsEnabled || bookmark.LineNumber >= curLineNr || bookmark.LineNumber <= lastMark.LineNumber && lastMark.LineNumber < curLineNr)
				{
					continue;
				}
				lastMark = bookmark;
			}
			return lastMark;
		}

		public bool IsMarked(int lineNr)
		{
			for (int i = 0; i < this.bookmark.Count; i++)
			{
				if (this.bookmark[i].LineNumber == lineNr)
				{
					return true;
				}
			}
			return false;
		}

		protected virtual void OnAdded(BookmarkEventArgs e)
		{
			if (this.Added != null)
			{
				this.Added(this, e);
			}
		}

		protected virtual void OnRemoved(BookmarkEventArgs e)
		{
			if (this.Removed != null)
			{
				this.Removed(this, e);
			}
		}

		public void RemoveMark(Bookmark mark)
		{
			this.bookmark.Remove(mark);
			this.OnRemoved(new BookmarkEventArgs(mark));
		}

		public void RemoveMarks(Predicate<Bookmark> predicate)
		{
			for (int i = 0; i < this.bookmark.Count; i++)
			{
				Bookmark item = this.bookmark[i];
				if (predicate(item))
				{
					int num = i;
					i = num - 1;
					this.bookmark.RemoveAt(num);
					this.OnRemoved(new BookmarkEventArgs(item));
				}
			}
		}

		public void ToggleMarkAt(TextLocation location)
		{
			Bookmark bookmark;
			bookmark = (this.Factory == null ? new Bookmark(this.document, location) : this.Factory.CreateBookmark(this.document, location));
			Type type = bookmark.GetType();
			for (int i = 0; i < this.bookmark.Count; i++)
			{
				Bookmark item = this.bookmark[i];
				if (item.LineNumber == location.Line && item.CanToggle && item.GetType() == type)
				{
					this.bookmark.RemoveAt(i);
					this.OnRemoved(new BookmarkEventArgs(item));
					return;
				}
			}
			this.bookmark.Add(bookmark);
			this.OnAdded(new BookmarkEventArgs(bookmark));
		}

		public event BookmarkEventHandler Added;

		public event BookmarkEventHandler Removed;
	}
}