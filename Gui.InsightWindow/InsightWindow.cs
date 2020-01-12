using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using ICSharpCode.TextEditor.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor.Gui.InsightWindow
{
	public class InsightWindow : AbstractCompletionWindow
	{
		private MouseWheelHandler mouseWheelHandler = new MouseWheelHandler();

		private Stack<ICSharpCode.TextEditor.Gui.InsightWindow.InsightWindow.InsightDataProviderStackElement> insightDataProviderStack = new Stack<ICSharpCode.TextEditor.Gui.InsightWindow.InsightWindow.InsightDataProviderStackElement>();

		private int CurrentData
		{
			get
			{
				return this.insightDataProviderStack.Peek().currentData;
			}
			set
			{
				this.insightDataProviderStack.Peek().currentData = value;
			}
		}

		private IInsightDataProvider DataProvider
		{
			get
			{
				if (this.insightDataProviderStack.Count == 0)
				{
					return null;
				}
				return this.insightDataProviderStack.Peek().dataProvider;
			}
		}

		public InsightWindow(Form parentForm, TextEditorControl control) : base(parentForm, control)
		{
			base.SetStyle(ControlStyles.UserPaint, true);
			base.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
		}

		public void AddInsightDataProvider(IInsightDataProvider provider, string fileName)
		{
			provider.SetupDataProvider(fileName, this.control.ActiveTextAreaControl.TextArea);
			if (provider.InsightDataCount > 0)
			{
				this.insightDataProviderStack.Push(new ICSharpCode.TextEditor.Gui.InsightWindow.InsightWindow.InsightDataProviderStackElement(provider));
			}
		}

		protected override void CaretOffsetChanged(object sender, EventArgs e)
		{
			TextLocation position = this.control.ActiveTextAreaControl.Caret.Position;
			int y = position.Y;
			int fontHeight = this.control.ActiveTextAreaControl.TextArea.TextView.FontHeight;
			int num = this.control.ActiveTextAreaControl.TextArea.VirtualTop.Y;
			int y1 = this.control.ActiveTextAreaControl.TextArea.TextView.DrawingPosition.Y;
			int drawingXPos = this.control.ActiveTextAreaControl.TextArea.TextView.GetDrawingXPos(position.Y, position.X);
			int visibleLine = (this.control.ActiveTextAreaControl.Document.GetVisibleLine(position.Y) + 1) * this.control.ActiveTextAreaControl.TextArea.TextView.FontHeight;
			Point virtualTop = this.control.ActiveTextAreaControl.TextArea.VirtualTop;
			int num1 = visibleLine - virtualTop.Y;
			int num2 = (this.control.TextEditorProperties.ShowHorizontalRuler ? this.control.ActiveTextAreaControl.TextArea.TextView.FontHeight : 0);
			Point screen = this.control.ActiveTextAreaControl.PointToScreen(new Point(drawingXPos, num1 + num2));
			if (screen.Y != base.Location.Y)
			{
				base.Location = screen;
			}
			while (this.DataProvider != null && this.DataProvider.CaretOffsetChanged())
			{
				this.CloseCurrentDataProvider();
			}
		}

		private void CloseCurrentDataProvider()
		{
			this.insightDataProviderStack.Pop();
			if (this.insightDataProviderStack.Count == 0)
			{
				base.Close();
				return;
			}
			this.Refresh();
		}

		public void HandleMouseWheel(MouseEventArgs e)
		{
			if (this.DataProvider != null && this.DataProvider.InsightDataCount > 0)
			{
				int scrollAmount = this.mouseWheelHandler.GetScrollAmount(e);
				if (this.control.TextEditorProperties.MouseWheelScrollDown)
				{
					scrollAmount = -scrollAmount;
				}
				if (scrollAmount > 0)
				{
					this.CurrentData = (this.CurrentData + 1) % this.DataProvider.InsightDataCount;
				}
				else if (scrollAmount < 0)
				{
					this.CurrentData = (this.CurrentData + this.DataProvider.InsightDataCount - 1) % this.DataProvider.InsightDataCount;
				}
				this.Refresh();
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			this.control.ActiveTextAreaControl.TextArea.Focus();
			if (TipPainterTools.DrawingRectangle1.Contains(e.X, e.Y))
			{
				this.CurrentData = (this.CurrentData + this.DataProvider.InsightDataCount - 1) % this.DataProvider.InsightDataCount;
				this.Refresh();
			}
			if (TipPainterTools.DrawingRectangle2.Contains(e.X, e.Y))
			{
				this.CurrentData = (this.CurrentData + 1) % this.DataProvider.InsightDataCount;
				this.Refresh();
			}
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			string insightData;
			string rangeDescription = null;
			if (this.DataProvider == null || this.DataProvider.InsightDataCount < 1)
			{
				insightData = "Unknown Method";
			}
			else
			{
				if (this.DataProvider.InsightDataCount > 1)
				{
					rangeDescription = this.control.GetRangeDescription(this.CurrentData + 1, this.DataProvider.InsightDataCount);
				}
				insightData = this.DataProvider.GetInsightData(this.CurrentData);
			}
			this.drawingSize = TipPainterTools.GetDrawingSizeHelpTipFromCombinedDescription(this, pe.Graphics, this.Font, rangeDescription, insightData);
			if (this.drawingSize != base.Size)
			{
				this.SetLocation();
				return;
			}
			TipPainterTools.DrawHelpTipFromCombinedDescription(this, pe.Graphics, this.Font, rangeDescription, insightData);
		}

		protected override void OnPaintBackground(PaintEventArgs pe)
		{
			pe.Graphics.FillRectangle(SystemBrushes.Info, pe.ClipRectangle);
		}

		protected override bool ProcessTextAreaKey(Keys keyData)
		{
			if (!base.Visible)
			{
				return false;
			}
			switch (keyData)
			{
				case Keys.Up:
				{
					if (this.DataProvider != null && this.DataProvider.InsightDataCount > 0)
					{
						this.CurrentData = (this.CurrentData + this.DataProvider.InsightDataCount - 1) % this.DataProvider.InsightDataCount;
						this.Refresh();
					}
					return true;
				}
				case Keys.Right:
				{
					return base.ProcessTextAreaKey(keyData);
				}
				case Keys.Down:
				{
					if (this.DataProvider != null && this.DataProvider.InsightDataCount > 0)
					{
						this.CurrentData = (this.CurrentData + 1) % this.DataProvider.InsightDataCount;
						this.Refresh();
					}
					return true;
				}
				default:
				{
					return base.ProcessTextAreaKey(keyData);
				}
			}
		}

		public void ShowInsightWindow()
		{
			if (base.Visible)
			{
				this.Refresh();
			}
			else if (this.insightDataProviderStack.Count > 0)
			{
				base.ShowCompletionWindow();
				return;
			}
		}

		private class InsightDataProviderStackElement
		{
			public int currentData;

			public IInsightDataProvider dataProvider;

			public InsightDataProviderStackElement(IInsightDataProvider dataProvider)
			{
				this.currentData = Math.Max(dataProvider.DefaultIndex, 0);
				this.dataProvider = dataProvider;
			}
		}
	}
}