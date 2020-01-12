using ICSharpCode.TextEditor;
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor.Document
{
	public class Bookmark
	{
		private IDocument document;

		private TextAnchor anchor;

		private TextLocation location;

		private bool isEnabled = true;

		public TextAnchor Anchor
		{
			get
			{
				return this.anchor;
			}
		}

		public virtual bool CanToggle
		{
			get
			{
				return true;
			}
		}

		public int ColumnNumber
		{
			get
			{
				if (this.anchor != null)
				{
					return this.anchor.ColumnNumber;
				}
				return this.location.Column;
			}
		}

		public IDocument Document
		{
			get
			{
				return this.document;
			}
			set
			{
				if (this.document != value)
				{
					if (this.anchor != null)
					{
						this.location = this.anchor.Location;
						this.anchor = null;
					}
					this.document = value;
					this.CreateAnchor();
					this.OnDocumentChanged(EventArgs.Empty);
				}
			}
		}

		public bool IsEnabled
		{
			get
			{
				return this.isEnabled;
			}
			set
			{
				if (this.isEnabled != value)
				{
					this.isEnabled = value;
					if (this.document != null)
					{
						this.document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, this.LineNumber));
						this.document.CommitUpdate();
					}
					this.OnIsEnabledChanged(EventArgs.Empty);
				}
			}
		}

		public int LineNumber
		{
			get
			{
				if (this.anchor != null)
				{
					return this.anchor.LineNumber;
				}
				return this.location.Line;
			}
		}

		public TextLocation Location
		{
			get
			{
				if (this.anchor == null)
				{
					return this.location;
				}
				return this.anchor.Location;
			}
			set
			{
				this.location = value;
				this.CreateAnchor();
			}
		}

		public Bookmark(IDocument document, TextLocation location) : this(document, location, true)
		{
		}

		public Bookmark(IDocument document, TextLocation location, bool isEnabled)
		{
			this.document = document;
			this.isEnabled = isEnabled;
			this.Location = location;
		}

		private void AnchorDeleted(object sender, EventArgs e)
		{
			this.document.BookmarkManager.RemoveMark(this);
		}

		public virtual bool Click(Control parent, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left || !this.CanToggle)
			{
				return false;
			}
			this.document.BookmarkManager.RemoveMark(this);
			return true;
		}

		private void CreateAnchor()
		{
			if (this.document != null)
			{
				LineSegment lineSegment = this.document.GetLineSegment(Math.Max(0, Math.Min(this.location.Line, this.document.TotalNumberOfLines - 1)));
				this.anchor = lineSegment.CreateAnchor(Math.Max(0, Math.Min(this.location.Column, lineSegment.Length)));
				this.anchor.MovementType = AnchorMovementType.AfterInsertion;
				this.anchor.Deleted += new EventHandler(this.AnchorDeleted);
			}
		}

		public virtual void Draw(IconBarMargin margin, Graphics g, Point p)
		{
			margin.DrawBookmark(g, p.Y, this.isEnabled);
		}

		protected virtual void OnDocumentChanged(EventArgs e)
		{
			if (this.DocumentChanged != null)
			{
				this.DocumentChanged(this, e);
			}
		}

		protected virtual void OnIsEnabledChanged(EventArgs e)
		{
			if (this.IsEnabledChanged != null)
			{
				this.IsEnabledChanged(this, e);
			}
		}

		public event EventHandler DocumentChanged;

		public event EventHandler IsEnabledChanged;
	}
}