using ICSharpCode.TextEditor.Document;
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor
{
	public abstract class AbstractMargin
	{
		private System.Windows.Forms.Cursor cursor = Cursors.Default;

		[CLSCompliant(false)]
		protected Rectangle drawingPosition = new Rectangle(0, 0, 0, 0);

		[CLSCompliant(false)]
		protected ICSharpCode.TextEditor.TextArea textArea;

		public virtual System.Windows.Forms.Cursor Cursor
		{
			get
			{
				return this.cursor;
			}
			set
			{
				this.cursor = value;
			}
		}

		public IDocument Document
		{
			get
			{
				return this.textArea.Document;
			}
		}

		public Rectangle DrawingPosition
		{
			get
			{
				return this.drawingPosition;
			}
			set
			{
				this.drawingPosition = value;
			}
		}

		public virtual bool IsVisible
		{
			get
			{
				return true;
			}
		}

		public virtual System.Drawing.Size Size
		{
			get
			{
				return new System.Drawing.Size(-1, -1);
			}
		}

		public ICSharpCode.TextEditor.TextArea TextArea
		{
			get
			{
				return this.textArea;
			}
		}

		public ITextEditorProperties TextEditorProperties
		{
			get
			{
				return this.textArea.Document.TextEditorProperties;
			}
		}

		protected AbstractMargin(ICSharpCode.TextEditor.TextArea textArea)
		{
			this.textArea = textArea;
		}

		public virtual void HandleMouseDown(Point mousepos, MouseButtons mouseButtons)
		{
			if (this.MouseDown != null)
			{
				this.MouseDown(this, mousepos, mouseButtons);
			}
		}

		public virtual void HandleMouseLeave(EventArgs e)
		{
			if (this.MouseLeave != null)
			{
				this.MouseLeave(this, e);
			}
		}

		public virtual void HandleMouseMove(Point mousepos, MouseButtons mouseButtons)
		{
			if (this.MouseMove != null)
			{
				this.MouseMove(this, mousepos, mouseButtons);
			}
		}

		public virtual void Paint(Graphics g, Rectangle rect)
		{
			if (this.Painted != null)
			{
				this.Painted(this, g, rect);
			}
		}

		public event MarginMouseEventHandler MouseDown;

		public event EventHandler MouseLeave;

		public event MarginMouseEventHandler MouseMove;

		public event MarginPaintEventHandler Painted;
	}
}