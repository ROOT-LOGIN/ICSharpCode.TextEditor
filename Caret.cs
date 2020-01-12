using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor
{
	public class Caret : IDisposable
	{
		private int line;

		private int column;

		private int desiredXPos;

		private ICSharpCode.TextEditor.CaretMode caretMode;

		private static bool caretCreated;

		private bool hidden = true;

		private TextArea textArea;

		private Point currentPos = new Point(-1, -1);

		private Ime ime;

		private Caret.CaretImplementation caretImplementation;

		private int oldLine = -1;

		private bool outstandingUpdate;

		private bool firePositionChangedAfterUpdateEnd;

		public ICSharpCode.TextEditor.CaretMode CaretMode
		{
			get
			{
				return this.caretMode;
			}
			set
			{
				this.caretMode = value;
				this.OnCaretModeChanged(EventArgs.Empty);
			}
		}

		public int Column
		{
			get
			{
				return this.column;
			}
			set
			{
				this.column = value;
				this.ValidateCaretPos();
				this.UpdateCaretPosition();
				this.OnPositionChanged(EventArgs.Empty);
			}
		}

		public int DesiredColumn
		{
			get
			{
				return this.desiredXPos;
			}
			set
			{
				this.desiredXPos = value;
			}
		}

		public int Line
		{
			get
			{
				return this.line;
			}
			set
			{
				this.line = value;
				this.ValidateCaretPos();
				this.UpdateCaretPosition();
				this.OnPositionChanged(EventArgs.Empty);
			}
		}

		public int Offset
		{
			get
			{
				return this.textArea.Document.PositionToOffset(this.Position);
			}
		}

		public TextLocation Position
		{
			get
			{
				return new TextLocation(this.column, this.line);
			}
			set
			{
				this.line = value.Y;
				this.column = value.X;
				this.ValidateCaretPos();
				this.UpdateCaretPosition();
				this.OnPositionChanged(EventArgs.Empty);
			}
		}

		public Point ScreenPosition
		{
			get
			{
				int drawingXPos = this.textArea.TextView.GetDrawingXPos(this.line, this.column);
				Rectangle drawingPosition = this.textArea.TextView.DrawingPosition;
				Rectangle rectangle = this.textArea.TextView.DrawingPosition;
				int y = rectangle.Y + this.textArea.Document.GetVisibleLine(this.line) * this.textArea.TextView.FontHeight;
				Point virtualTop = this.textArea.TextView.TextArea.VirtualTop;
				return new Point(drawingPosition.X + drawingXPos, y - virtualTop.Y);
			}
		}

		static Caret()
		{
		}

		public Caret(TextArea textArea)
		{
			this.textArea = textArea;
			textArea.GotFocus += new EventHandler(this.GotFocus);
			textArea.LostFocus += new EventHandler(this.LostFocus);
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				this.caretImplementation = new Caret.ManagedCaret(this);
				return;
			}
			this.caretImplementation = new Caret.Win32Caret(this);
		}

		private void CreateCaret()
		{
			while (!Caret.caretCreated)
			{
				switch (this.caretMode)
				{
					case ICSharpCode.TextEditor.CaretMode.InsertMode:
					{
						Caret.caretCreated = this.caretImplementation.Create(2, this.textArea.TextView.FontHeight);
						continue;
					}
					case ICSharpCode.TextEditor.CaretMode.OverwriteMode:
					{
						Caret.caretCreated = this.caretImplementation.Create(this.textArea.TextView.SpaceWidth, this.textArea.TextView.FontHeight);
						continue;
					}
					default:
					{
						continue;
					}
				}
			}
			if (this.currentPos.X < 0)
			{
				this.ValidateCaretPos();
				this.currentPos = this.ScreenPosition;
			}
			this.caretImplementation.SetPosition(this.currentPos.X, this.currentPos.Y);
			this.caretImplementation.Show();
		}

		public void Dispose()
		{
			this.textArea.GotFocus -= new EventHandler(this.GotFocus);
			this.textArea.LostFocus -= new EventHandler(this.LostFocus);
			this.textArea = null;
			this.caretImplementation.Dispose();
		}

		private void DisposeCaret()
		{
			if (Caret.caretCreated)
			{
				Caret.caretCreated = false;
				this.caretImplementation.Hide();
				this.caretImplementation.Destroy();
			}
		}

		private void FirePositionChangedAfterUpdateEnd(object sender, EventArgs e)
		{
			this.OnPositionChanged(EventArgs.Empty);
		}

		private void GotFocus(object sender, EventArgs e)
		{
			this.hidden = false;
			if (!this.textArea.MotherTextEditorControl.IsInUpdate)
			{
				this.CreateCaret();
				this.UpdateCaretPosition();
			}
		}

		[Conditional("DEBUG")]
		private static void Log(string text)
		{
		}

		private void LostFocus(object sender, EventArgs e)
		{
			this.hidden = true;
			this.DisposeCaret();
		}

		protected virtual void OnCaretModeChanged(EventArgs e)
		{
			if (this.CaretModeChanged != null)
			{
				this.CaretModeChanged(this, e);
			}
			this.caretImplementation.Hide();
			this.caretImplementation.Destroy();
			Caret.caretCreated = false;
			this.CreateCaret();
			this.caretImplementation.Show();
		}

		internal void OnEndUpdate()
		{
			if (this.outstandingUpdate)
			{
				this.UpdateCaretPosition();
			}
		}

		protected virtual void OnPositionChanged(EventArgs e)
		{
			if (this.textArea.MotherTextEditorControl.IsInUpdate)
			{
				if (!this.firePositionChangedAfterUpdateEnd)
				{
					this.firePositionChangedAfterUpdateEnd = true;
					this.textArea.Document.UpdateCommited += new EventHandler(this.FirePositionChangedAfterUpdateEnd);
				}
				return;
			}
			if (this.firePositionChangedAfterUpdateEnd)
			{
				this.textArea.Document.UpdateCommited -= new EventHandler(this.FirePositionChangedAfterUpdateEnd);
				this.firePositionChangedAfterUpdateEnd = false;
			}
			List<FoldMarker> foldingsFromPosition = this.textArea.Document.FoldingManager.GetFoldingsFromPosition(this.line, this.column);
			bool isFolded = false;
			foreach (FoldMarker foldMarker in foldingsFromPosition)
			{
				isFolded |= foldMarker.IsFolded;
				foldMarker.IsFolded = false;
			}
			if (isFolded)
			{
				this.textArea.Document.FoldingManager.NotifyFoldingsChanged(EventArgs.Empty);
			}
			if (this.PositionChanged != null)
			{
				this.PositionChanged(this, e);
			}
			this.textArea.ScrollToCaret();
		}

		internal void PaintCaret(Graphics g)
		{
			this.caretImplementation.PaintCaret(g);
			this.PaintCaretLine(g);
		}

		private void PaintCaretLine(Graphics g)
		{
			if (!this.textArea.Document.TextEditorProperties.CaretLine)
			{
				return;
			}
			HighlightColor colorFor = this.textArea.Document.HighlightingStrategy.GetColorFor("CaretLine");
			Pen dotPen = BrushRegistry.GetDotPen(colorFor.Color);
			int x = this.currentPos.X;
			int num = this.currentPos.X;
			Rectangle displayRectangle = this.textArea.DisplayRectangle;
			g.DrawLine(dotPen, x, 0, num, displayRectangle.Height);
		}

		public void RecreateCaret()
		{
			this.DisposeCaret();
			if (!this.hidden)
			{
				this.CreateCaret();
			}
		}

		public void UpdateCaretPosition()
		{
			if (this.textArea.TextEditorProperties.CaretLine)
			{
				this.textArea.Invalidate();
			}
			else if (this.caretImplementation.RequireRedrawOnPositionChange)
			{
				this.textArea.UpdateLine(this.oldLine);
				if (this.line != this.oldLine)
				{
					this.textArea.UpdateLine(this.line);
				}
			}
			else if (this.textArea.MotherTextAreaControl.TextEditorProperties.LineViewerStyle == LineViewerStyle.FullRow && this.oldLine != this.line)
			{
				this.textArea.UpdateLine(this.oldLine);
				this.textArea.UpdateLine(this.line);
			}
			this.oldLine = this.line;
			if (this.hidden || this.textArea.MotherTextEditorControl.IsInUpdate)
			{
				this.outstandingUpdate = true;
				return;
			}
			this.outstandingUpdate = false;
			this.ValidateCaretPos();
			int num = this.line;
			int drawingXPos = this.textArea.TextView.GetDrawingXPos(num, this.column);
			Point screenPosition = this.ScreenPosition;
			if (drawingXPos < 0)
			{
				this.caretImplementation.Destroy();
			}
			else
			{
				this.CreateCaret();
				if (!this.caretImplementation.SetPosition(screenPosition.X, screenPosition.Y))
				{
					this.caretImplementation.Destroy();
					Caret.caretCreated = false;
					this.UpdateCaretPosition();
				}
			}
			if (this.ime != null)
			{
				this.ime.HWnd = this.textArea.Handle;
				this.ime.Font = this.textArea.Document.TextEditorProperties.Font;
			}
			else
			{
				this.ime = new Ime(this.textArea.Handle, this.textArea.Document.TextEditorProperties.Font);
			}
			this.ime.SetIMEWindowLocation(screenPosition.X, screenPosition.Y);
			this.currentPos = screenPosition;
		}

		public void ValidateCaretPos()
		{
			this.line = Math.Max(0, Math.Min(this.textArea.Document.TotalNumberOfLines - 1, this.line));
			this.column = Math.Max(0, this.column);
			if (this.column == 2147483647 || !this.textArea.TextEditorProperties.AllowCaretBeyondEOL)
			{
				LineSegment lineSegment = this.textArea.Document.GetLineSegment(this.line);
				this.column = Math.Min(this.column, lineSegment.Length);
			}
		}

		public TextLocation ValidatePosition(TextLocation pos)
		{
			int num = Math.Max(0, Math.Min(this.textArea.Document.TotalNumberOfLines - 1, pos.Y));
			int num1 = Math.Max(0, pos.X);
			if (num1 == 2147483647 || !this.textArea.TextEditorProperties.AllowCaretBeyondEOL)
			{
				LineSegment lineSegment = this.textArea.Document.GetLineSegment(num);
				num1 = Math.Min(num1, lineSegment.Length);
			}
			return new TextLocation(num1, num);
		}

		public event EventHandler CaretModeChanged;

		public event EventHandler PositionChanged;

		private abstract class CaretImplementation : IDisposable
		{
			public bool RequireRedrawOnPositionChange;

			protected CaretImplementation()
			{
			}

			public abstract bool Create(int width, int height);

			public abstract void Destroy();

			public virtual void Dispose()
			{
				this.Destroy();
			}

			public abstract void Hide();

			public abstract void PaintCaret(Graphics g);

			public abstract bool SetPosition(int x, int y);

			public abstract void Show();
		}

		private class ManagedCaret : Caret.CaretImplementation
		{
			private Timer timer;

			private bool visible;

			private bool blink;

			private int x;

			private int y;

			private int width;

			private int height;

			private TextArea textArea;

			private Caret parentCaret;

			public ManagedCaret(Caret caret)
			{
				this.RequireRedrawOnPositionChange = true;
				this.textArea = caret.textArea;
				this.parentCaret = caret;
				this.timer.Tick += new EventHandler(this.CaretTimerTick);
			}

			private void CaretTimerTick(object sender, EventArgs e)
			{
				this.blink = !this.blink;
				if (this.visible)
				{
					this.textArea.UpdateLine(this.parentCaret.Line);
				}
			}

			public override bool Create(int width, int height)
			{
				this.visible = true;
				this.width = width - 2;
				this.height = height;
				this.timer.Enabled = true;
				return true;
			}

			public override void Destroy()
			{
				this.visible = false;
				this.timer.Enabled = false;
			}

			public override void Dispose()
			{
				base.Dispose();
				this.timer.Dispose();
			}

			public override void Hide()
			{
				this.visible = false;
			}

			public override void PaintCaret(Graphics g)
			{
				if (this.visible && this.blink)
				{
					g.DrawRectangle(Pens.Gray, this.x, this.y, this.width, this.height);
				}
			}

			public override bool SetPosition(int x, int y)
			{
				this.x = x - 1;
				this.y = y;
				return true;
			}

			public override void Show()
			{
				this.visible = true;
			}
		}

		private class Win32Caret : Caret.CaretImplementation
		{
			private TextArea textArea;

			public Win32Caret(Caret caret)
			{
				this.textArea = caret.textArea;
			}

			public override bool Create(int width, int height)
			{
				return Caret.Win32Caret.CreateCaret(this.textArea.Handle, 0, width, height);
			}

			[DllImport("User32.dll", CharSet=CharSet.None, ExactSpelling=false)]
			private static extern bool CreateCaret(IntPtr hWnd, int hBitmap, int nWidth, int nHeight);

			public override void Destroy()
			{
				Caret.Win32Caret.DestroyCaret();
			}

			[DllImport("User32.dll", CharSet=CharSet.None, ExactSpelling=false)]
			private static extern bool DestroyCaret();

			public override void Hide()
			{
				Caret.Win32Caret.HideCaret(this.textArea.Handle);
			}

			[DllImport("User32.dll", CharSet=CharSet.None, ExactSpelling=false)]
			private static extern bool HideCaret(IntPtr hWnd);

			public override void PaintCaret(Graphics g)
			{
			}

			[DllImport("User32.dll", CharSet=CharSet.None, ExactSpelling=false)]
			private static extern bool SetCaretPos(int x, int y);

			public override bool SetPosition(int x, int y)
			{
				return Caret.Win32Caret.SetCaretPos(x, y);
			}

			public override void Show()
			{
				Caret.Win32Caret.ShowCaret(this.textArea.Handle);
			}

			[DllImport("User32.dll", CharSet=CharSet.None, ExactSpelling=false)]
			private static extern bool ShowCaret(IntPtr hWnd);
		}
	}
}