using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor.Gui.CompletionWindow
{
	public abstract class AbstractCompletionWindow : Form
	{
		protected TextEditorControl control;

		protected System.Drawing.Size drawingSize;

		private Rectangle workingScreen;

		private Form parentForm;

		private static int shadowStatus;

		protected override System.Windows.Forms.CreateParams CreateParams
		{
			get
			{
				System.Windows.Forms.CreateParams createParams = base.CreateParams;
				AbstractCompletionWindow.AddShadowToWindow(createParams);
				return createParams;
			}
		}

		protected override bool ShowWithoutActivation
		{
			get
			{
				return true;
			}
		}

		protected AbstractCompletionWindow(Form parentForm, TextEditorControl control)
		{
			this.workingScreen = Screen.GetWorkingArea(parentForm);
			this.parentForm = parentForm;
			this.control = control;
			this.SetLocation();
			base.StartPosition = FormStartPosition.Manual;
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			base.ShowInTaskbar = false;
			this.MinimumSize = new System.Drawing.Size(1, 1);
			base.Size = new System.Drawing.Size(1, 1);
		}

		public static void AddShadowToWindow(System.Windows.Forms.CreateParams createParams)
		{
			if (AbstractCompletionWindow.shadowStatus == 0)
			{
				AbstractCompletionWindow.shadowStatus = -1;
				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				{
					Version version = Environment.OSVersion.Version;
					if (version.Major > 5 || version.Major == 5 && version.Minor >= 1)
					{
						AbstractCompletionWindow.shadowStatus = 1;
					}
				}
			}
			if (AbstractCompletionWindow.shadowStatus == 1)
			{
				System.Windows.Forms.CreateParams classStyle = createParams;
				classStyle.ClassStyle = classStyle.ClassStyle | 131072;
			}
		}

		protected virtual void CaretOffsetChanged(object sender, EventArgs e)
		{
		}

		protected void ControlMouseMove(object sender, MouseEventArgs e)
		{
			this.control.ActiveTextAreaControl.TextArea.ShowHiddenCursor(false);
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			this.parentForm.LocationChanged -= new EventHandler(this.ParentFormLocationChanged);
			foreach (Control control in base.Controls)
			{
				control.MouseMove -= new MouseEventHandler(this.ControlMouseMove);
			}
			if (this.control.ActiveTextAreaControl.VScrollBar != null)
			{
				this.control.ActiveTextAreaControl.VScrollBar.ValueChanged -= new EventHandler(this.ParentFormLocationChanged);
			}
			if (this.control.ActiveTextAreaControl.HScrollBar != null)
			{
				this.control.ActiveTextAreaControl.HScrollBar.ValueChanged -= new EventHandler(this.ParentFormLocationChanged);
			}
			this.control.ActiveTextAreaControl.TextArea.LostFocus -= new EventHandler(this.TextEditorLostFocus);
			AbstractCompletionWindow abstractCompletionWindow = this;
			this.control.ActiveTextAreaControl.Caret.PositionChanged -= new EventHandler(abstractCompletionWindow.CaretOffsetChanged);
			AbstractCompletionWindow abstractCompletionWindow1 = this;
			this.control.ActiveTextAreaControl.TextArea.DoProcessDialogKey -= new DialogKeyProcessor(abstractCompletionWindow1.ProcessTextAreaKey);
			this.control.Resize -= new EventHandler(this.ParentFormLocationChanged);
			base.Dispose();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			this.ControlMouseMove(this, e);
		}

		private void ParentFormLocationChanged(object sender, EventArgs e)
		{
			this.SetLocation();
		}

		public virtual bool ProcessKeyEvent(char ch)
		{
			return false;
		}

		protected virtual bool ProcessTextAreaKey(Keys keyData)
		{
			if (!base.Visible)
			{
				return false;
			}
			if (keyData != Keys.Escape)
			{
				return false;
			}
			base.Close();
			return true;
		}

		protected virtual void SetLocation()
		{
			TextArea textArea = this.control.ActiveTextAreaControl.TextArea;
			TextLocation position = textArea.Caret.Position;
			int drawingXPos = textArea.TextView.GetDrawingXPos(position.Y, position.X);
			int num = (textArea.TextEditorProperties.ShowHorizontalRuler ? textArea.TextView.FontHeight : 0);
			Rectangle drawingPosition = textArea.TextView.DrawingPosition;
			Rectangle rectangle = textArea.TextView.DrawingPosition;
			int y = rectangle.Y + textArea.Document.GetVisibleLine(position.Y) * textArea.TextView.FontHeight;
			Point virtualTop = textArea.TextView.TextArea.VirtualTop;
			Point point = new Point(drawingPosition.X + drawingXPos, y - virtualTop.Y + textArea.TextView.FontHeight + num);
			Point screen = this.control.ActiveTextAreaControl.PointToScreen(point);
			Rectangle right = new Rectangle(screen, this.drawingSize);
			if (!this.workingScreen.Contains(right))
			{
				if (right.Right > this.workingScreen.Right)
				{
					right.X = this.workingScreen.Right - right.Width;
				}
				if (right.Left < this.workingScreen.Left)
				{
					right.X = this.workingScreen.Left;
				}
				if (right.Top < this.workingScreen.Top)
				{
					right.Y = this.workingScreen.Top;
				}
				if (right.Bottom > this.workingScreen.Bottom)
				{
					right.Y = right.Y - right.Height - this.control.ActiveTextAreaControl.TextArea.TextView.FontHeight;
					if (right.Bottom > this.workingScreen.Bottom)
					{
						right.Y = this.workingScreen.Bottom - right.Height;
					}
				}
			}
			base.Bounds = right;
		}

		protected void ShowCompletionWindow()
		{
			base.Owner = this.parentForm;
			base.Enabled = true;
			base.Show();
			this.control.Focus();
			if (this.parentForm != null)
			{
				this.parentForm.LocationChanged += new EventHandler(this.ParentFormLocationChanged);
			}
			this.control.ActiveTextAreaControl.VScrollBar.ValueChanged += new EventHandler(this.ParentFormLocationChanged);
			this.control.ActiveTextAreaControl.HScrollBar.ValueChanged += new EventHandler(this.ParentFormLocationChanged);
			AbstractCompletionWindow abstractCompletionWindow = this;
			this.control.ActiveTextAreaControl.TextArea.DoProcessDialogKey += new DialogKeyProcessor(abstractCompletionWindow.ProcessTextAreaKey);
			AbstractCompletionWindow abstractCompletionWindow1 = this;
			this.control.ActiveTextAreaControl.Caret.PositionChanged += new EventHandler(abstractCompletionWindow1.CaretOffsetChanged);
			this.control.ActiveTextAreaControl.TextArea.LostFocus += new EventHandler(this.TextEditorLostFocus);
			this.control.Resize += new EventHandler(this.ParentFormLocationChanged);
			foreach (Control control in base.Controls)
			{
				control.MouseMove += new MouseEventHandler(this.ControlMouseMove);
			}
		}

		protected void TextEditorLostFocus(object sender, EventArgs e)
		{
			if (!this.control.ActiveTextAreaControl.TextArea.Focused && !base.ContainsFocus)
			{
				base.Close();
			}
		}
	}
}