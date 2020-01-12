using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Util;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor.Gui.CompletionWindow
{
	public class CodeCompletionWindow : AbstractCompletionWindow
	{
		private const int ScrollbarWidth = 16;

		private const int MaxListLength = 10;

		private ICompletionData[] completionData;

		private CodeCompletionListView codeCompletionListView;

		private VScrollBar vScrollBar = new VScrollBar();

		private ICompletionDataProvider dataProvider;

		private IDocument document;

		private bool showDeclarationWindow = true;

		private bool fixedListViewWidth = true;

		private int startOffset;

		private int endOffset;

		private DeclarationViewWindow declarationViewWindow;

		private Rectangle workingScreen;

		private bool inScrollUpdate;

		private MouseWheelHandler mouseWheelHandler = new MouseWheelHandler();

		public bool CloseWhenCaretAtBeginning
		{
			get;
			set;
		}

		private CodeCompletionWindow(ICompletionDataProvider completionDataProvider, ICompletionData[] completionData, Form parentForm, TextEditorControl control, bool showDeclarationWindow, bool fixedListViewWidth) : base(parentForm, control)
		{
			this.dataProvider = completionDataProvider;
			this.completionData = completionData;
			this.document = control.Document;
			this.showDeclarationWindow = showDeclarationWindow;
			this.fixedListViewWidth = fixedListViewWidth;
			this.workingScreen = Screen.GetWorkingArea(base.Location);
			this.startOffset = control.ActiveTextAreaControl.Caret.Offset + 1;
			this.endOffset = this.startOffset;
			if (completionDataProvider.PreSelection != null)
			{
				CodeCompletionWindow length = this;
				length.startOffset = length.startOffset - (completionDataProvider.PreSelection.Length + 1);
				this.endOffset--;
			}
			this.codeCompletionListView = new CodeCompletionListView(completionData)
			{
				ImageList = completionDataProvider.ImageList,
				Dock = DockStyle.Fill
			};
			this.codeCompletionListView.SelectedItemChanged += new EventHandler(this.CodeCompletionListViewSelectedItemChanged);
			this.codeCompletionListView.DoubleClick += new EventHandler(this.CodeCompletionListViewDoubleClick);
			this.codeCompletionListView.Click += new EventHandler(this.CodeCompletionListViewClick);
			base.Controls.Add(this.codeCompletionListView);
			if ((int)completionData.Length > 10)
			{
				this.vScrollBar.Dock = DockStyle.Right;
				this.vScrollBar.Minimum = 0;
				this.vScrollBar.Maximum = (int)completionData.Length - 1;
				this.vScrollBar.SmallChange = 1;
				this.vScrollBar.LargeChange = 10;
				this.codeCompletionListView.FirstItemChanged += new EventHandler(this.CodeCompletionListViewFirstItemChanged);
				base.Controls.Add(this.vScrollBar);
			}
			this.drawingSize = this.GetListViewSize();
			this.SetLocation();
			if (this.declarationViewWindow == null)
			{
				this.declarationViewWindow = new DeclarationViewWindow(parentForm);
			}
			this.SetDeclarationViewLocation();
			this.declarationViewWindow.ShowDeclarationViewWindow();
			this.declarationViewWindow.MouseMove += new MouseEventHandler(this.ControlMouseMove);
			control.Focus();
			this.CodeCompletionListViewSelectedItemChanged(this, EventArgs.Empty);
			if (completionDataProvider.DefaultIndex >= 0)
			{
				this.codeCompletionListView.SelectIndex(completionDataProvider.DefaultIndex);
			}
			if (completionDataProvider.PreSelection != null)
			{
				this.CaretOffsetChanged(this, EventArgs.Empty);
			}
			this.vScrollBar.ValueChanged += new EventHandler(this.VScrollBarValueChanged);
			this.document.DocumentAboutToBeChanged += new DocumentEventHandler(this.DocumentAboutToBeChanged);
		}

		protected override void CaretOffsetChanged(object sender, EventArgs e)
		{
			int offset = this.control.ActiveTextAreaControl.Caret.Offset;
			if (offset == this.startOffset)
			{
				if (this.CloseWhenCaretAtBeginning)
				{
					base.Close();
				}
				return;
			}
			if (offset < this.startOffset || offset > this.endOffset)
			{
				base.Close();
				return;
			}
			this.codeCompletionListView.SelectItemWithStart(this.control.Document.GetText(this.startOffset, offset - this.startOffset));
		}

		private void CodeCompletionListViewClick(object sender, EventArgs e)
		{
			this.control.ActiveTextAreaControl.TextArea.Focus();
		}

		private void CodeCompletionListViewDoubleClick(object sender, EventArgs e)
		{
			this.InsertSelectedItem('\0');
		}

		private void CodeCompletionListViewFirstItemChanged(object sender, EventArgs e)
		{
			if (this.inScrollUpdate)
			{
				return;
			}
			this.inScrollUpdate = true;
			this.vScrollBar.Value = Math.Min(this.vScrollBar.Maximum, this.codeCompletionListView.FirstItem);
			this.inScrollUpdate = false;
		}

		private void CodeCompletionListViewSelectedItemChanged(object sender, EventArgs e)
		{
			ICompletionData selectedCompletionData = this.codeCompletionListView.SelectedCompletionData;
			if (!this.showDeclarationWindow || selectedCompletionData == null || selectedCompletionData.Description == null || selectedCompletionData.Description.Length <= 0)
			{
				this.declarationViewWindow.Description = null;
				return;
			}
			this.declarationViewWindow.Description = selectedCompletionData.Description;
			this.SetDeclarationViewLocation();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.document.DocumentAboutToBeChanged -= new DocumentEventHandler(this.DocumentAboutToBeChanged);
				if (this.codeCompletionListView != null)
				{
					this.codeCompletionListView.Dispose();
					this.codeCompletionListView = null;
				}
				if (this.declarationViewWindow != null)
				{
					this.declarationViewWindow.Dispose();
					this.declarationViewWindow = null;
				}
			}
			base.Dispose(disposing);
		}

		private void DocumentAboutToBeChanged(object sender, DocumentEventArgs e)
		{
			if (e.Offset >= this.startOffset && e.Offset <= this.endOffset)
			{
				if (e.Length > 0)
				{
					this.endOffset -= e.Length;
				}
				if (!string.IsNullOrEmpty(e.Text))
				{
					this.endOffset += e.Text.Length;
				}
			}
		}

		private System.Drawing.Size GetListViewSize()
		{
			int itemHeight = this.codeCompletionListView.ItemHeight * Math.Min(10, (int)this.completionData.Length);
			int listViewWidth = this.codeCompletionListView.ItemHeight * 10;
			if (!this.fixedListViewWidth)
			{
				listViewWidth = this.GetListViewWidth(listViewWidth, itemHeight);
			}
			return new System.Drawing.Size(listViewWidth, itemHeight);
		}

		private int GetListViewWidth(int defaultWidth, int height)
		{
			float single = (float)defaultWidth;
			using (Graphics graphic = this.codeCompletionListView.CreateGraphics())
			{
				for (int i = 0; i < (int)this.completionData.Length; i++)
				{
					SizeF sizeF = graphic.MeasureString(this.completionData[i].Text.ToString(), this.codeCompletionListView.Font);
					float width = sizeF.Width;
					if (width > single)
					{
						single = width;
					}
				}
			}
			if ((float)(this.codeCompletionListView.ItemHeight * (int)this.completionData.Length) > (float)height)
			{
				single += 16f;
			}
			return (int)single;
		}

		public void HandleMouseWheel(MouseEventArgs e)
		{
			int scrollAmount = this.mouseWheelHandler.GetScrollAmount(e);
			if (scrollAmount == 0)
			{
				return;
			}
			if (this.control.TextEditorProperties.MouseWheelScrollDown)
			{
				scrollAmount = -scrollAmount;
			}
			int value = this.vScrollBar.Value + this.vScrollBar.SmallChange * scrollAmount;
			this.vScrollBar.Value = Math.Max(this.vScrollBar.Minimum, Math.Min(this.vScrollBar.Maximum - this.vScrollBar.LargeChange + 1, value));
		}

		private bool InsertSelectedItem(char ch)
		{
			this.document.DocumentAboutToBeChanged -= new DocumentEventHandler(this.DocumentAboutToBeChanged);
			ICompletionData selectedCompletionData = this.codeCompletionListView.SelectedCompletionData;
			bool flag = false;
			if (selectedCompletionData != null)
			{
				this.control.BeginUpdate();
				try
				{
					if (this.endOffset - this.startOffset > 0)
					{
						this.control.Document.Remove(this.startOffset, this.endOffset - this.startOffset);
					}
					flag = this.dataProvider.InsertAction(selectedCompletionData, this.control.ActiveTextAreaControl.TextArea, this.startOffset, ch);
				}
				finally
				{
					this.control.EndUpdate();
				}
			}
			base.Close();
			return flag;
		}

		public override bool ProcessKeyEvent(char ch)
		{
			switch (this.dataProvider.ProcessKey(ch))
			{
				case CompletionDataProviderKeyResult.NormalKey:
				{
					return base.ProcessKeyEvent(ch);
				}
				case CompletionDataProviderKeyResult.InsertionKey:
				{
					return this.InsertSelectedItem(ch);
				}
				case CompletionDataProviderKeyResult.BeforeStartKey:
				{
					this.startOffset++;
					this.endOffset++;
					return base.ProcessKeyEvent(ch);
				}
			}
			throw new InvalidOperationException("Invalid return value of dataProvider.ProcessKey");
		}

		protected override bool ProcessTextAreaKey(Keys keyData)
		{
			if (!base.Visible)
			{
				return false;
			}
			Keys key = keyData;
			if (key == Keys.Tab)
			{
				this.InsertSelectedItem('\t');
				return true;
			}
			if (key == Keys.Return)
			{
				this.InsertSelectedItem('\n');
				return true;
			}
			switch (key)
			{
				case Keys.Prior:
				{
					this.codeCompletionListView.PageUp();
					return true;
				}
				case Keys.Next:
				{
					this.codeCompletionListView.PageDown();
					return true;
				}
				case Keys.End:
				{
					this.codeCompletionListView.SelectIndex((int)this.completionData.Length - 1);
					return true;
				}
				case Keys.Home:
				{
					this.codeCompletionListView.SelectIndex(0);
					return true;
				}
				case Keys.Left:
				case Keys.Right:
				{
					return base.ProcessTextAreaKey(keyData);
				}
				case Keys.Up:
				{
					this.codeCompletionListView.SelectPrevItem();
					return true;
				}
				case Keys.Down:
				{
					this.codeCompletionListView.SelectNextItem();
					return true;
				}
				default:
				{
					return base.ProcessTextAreaKey(keyData);
				}
			}
		}

		private void SetDeclarationViewLocation()
		{
			Point point;
			int left = base.Bounds.Left - this.workingScreen.Left;
			if ((this.workingScreen.Right - base.Bounds.Right) * 2 <= left)
			{
				DeclarationViewWindow requiredLeftHandSideWidth = this.declarationViewWindow;
				DeclarationViewWindow declarationViewWindow = this.declarationViewWindow;
				int num = base.Bounds.Left;
				Rectangle bounds = base.Bounds;
				requiredLeftHandSideWidth.Width = declarationViewWindow.GetRequiredLeftHandSideWidth(new Point(num, bounds.Top));
				this.declarationViewWindow.FixedWidth = true;
				if (base.Bounds.Left >= this.declarationViewWindow.Width)
				{
					Rectangle rectangle = base.Bounds;
					point = new Point(rectangle.Left - this.declarationViewWindow.Width, base.Bounds.Top);
				}
				else
				{
					point = new Point(0, base.Bounds.Top);
				}
				if (this.declarationViewWindow.Location != point)
				{
					this.declarationViewWindow.Location = point;
				}
				this.declarationViewWindow.Refresh();
			}
			else
			{
				this.declarationViewWindow.FixedWidth = false;
				point = new Point(base.Bounds.Right, base.Bounds.Top);
				if (this.declarationViewWindow.Location != point)
				{
					this.declarationViewWindow.Location = point;
					return;
				}
			}
		}

		protected override void SetLocation()
		{
			base.SetLocation();
			if (this.declarationViewWindow != null)
			{
				this.SetDeclarationViewLocation();
			}
		}

		public static CodeCompletionWindow ShowCompletionWindow(Form parent, TextEditorControl control, string fileName, ICompletionDataProvider completionDataProvider, char firstChar)
		{
			return CodeCompletionWindow.ShowCompletionWindow(parent, control, fileName, completionDataProvider, firstChar, true, true);
		}

		public static CodeCompletionWindow ShowCompletionWindow(Form parent, TextEditorControl control, string fileName, ICompletionDataProvider completionDataProvider, char firstChar, bool showDeclarationWindow, bool fixedListViewWidth)
		{
			ICompletionData[] completionDataArray = completionDataProvider.GenerateCompletionData(fileName, control.ActiveTextAreaControl.TextArea, firstChar);
			if (completionDataArray == null || (int)completionDataArray.Length == 0)
			{
				return null;
			}
			CodeCompletionWindow codeCompletionWindow = new CodeCompletionWindow(completionDataProvider, completionDataArray, parent, control, showDeclarationWindow, fixedListViewWidth)
			{
				CloseWhenCaretAtBeginning = firstChar == '\0'
			};
			codeCompletionWindow.ShowCompletionWindow();
			return codeCompletionWindow;
		}

		private void VScrollBarValueChanged(object sender, EventArgs e)
		{
			if (this.inScrollUpdate)
			{
				return;
			}
			this.inScrollUpdate = true;
			this.codeCompletionListView.FirstItem = this.vScrollBar.Value;
			this.codeCompletionListView.Refresh();
			this.control.ActiveTextAreaControl.TextArea.Focus();
			this.inScrollUpdate = false;
		}
	}
}