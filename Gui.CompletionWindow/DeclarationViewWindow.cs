using ICSharpCode.TextEditor.Util;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor.Gui.CompletionWindow
{
	public class DeclarationViewWindow : Form, IDeclarationViewWindow
	{
		private string description = string.Empty;

		private bool fixedWidth;

		public bool HideOnClick;

		protected override System.Windows.Forms.CreateParams CreateParams
		{
			get
			{
				System.Windows.Forms.CreateParams createParams = base.CreateParams;
				AbstractCompletionWindow.AddShadowToWindow(createParams);
				return createParams;
			}
		}

		public string Description
		{
			get
			{
				return this.description;
			}
			set
			{
				this.description = value;
				if (value == null && base.Visible)
				{
					base.Visible = false;
					return;
				}
				if (value != null)
				{
					if (!base.Visible)
					{
						this.ShowDeclarationViewWindow();
					}
					this.Refresh();
				}
			}
		}

		public bool FixedWidth
		{
			get
			{
				return this.fixedWidth;
			}
			set
			{
				this.fixedWidth = value;
			}
		}

		protected override bool ShowWithoutActivation
		{
			get
			{
				return true;
			}
		}

		public DeclarationViewWindow(Form parent)
		{
			base.SetStyle(ControlStyles.Selectable, false);
			base.StartPosition = FormStartPosition.Manual;
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			base.Owner = parent;
			base.ShowInTaskbar = false;
			base.Size = new System.Drawing.Size(0, 0);
			base.CreateHandle();
		}

		public void CloseDeclarationViewWindow()
		{
			base.Close();
			base.Dispose();
		}

		public int GetRequiredLeftHandSideWidth(Point p)
		{
			int width;
			if (this.description == null || this.description.Length <= 0)
			{
				return 0;
			}
			using (Graphics graphic = base.CreateGraphics())
			{
				System.Drawing.Size leftHandSideDrawingSizeHelpTipFromCombinedDescription = TipPainterTools.GetLeftHandSideDrawingSizeHelpTipFromCombinedDescription(this, graphic, this.Font, null, this.description, p);
				width = leftHandSideDrawingSizeHelpTipFromCombinedDescription.Width;
			}
			return width;
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			if (this.HideOnClick)
			{
				base.Hide();
			}
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			if (this.description != null && this.description.Length > 0)
			{
				if (this.fixedWidth)
				{
					TipPainterTools.DrawFixedWidthHelpTipFromCombinedDescription(this, pe.Graphics, this.Font, null, this.description);
					return;
				}
				TipPainterTools.DrawHelpTipFromCombinedDescription(this, pe.Graphics, this.Font, null, this.description);
			}
		}

		protected override void OnPaintBackground(PaintEventArgs pe)
		{
			pe.Graphics.FillRectangle(SystemBrushes.Info, pe.ClipRectangle);
		}

		public void ShowDeclarationViewWindow()
		{
			base.Show();
		}
	}
}