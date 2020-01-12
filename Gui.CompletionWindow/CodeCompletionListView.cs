using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor.Gui.CompletionWindow
{
	public class CodeCompletionListView : UserControl
	{
		private ICompletionData[] completionData;

		private int firstItem;

		private int selectedItem = -1;

		private System.Windows.Forms.ImageList imageList;

		public int FirstItem
		{
			get
			{
				return this.firstItem;
			}
			set
			{
				if (this.firstItem != value)
				{
					this.firstItem = value;
					this.OnFirstItemChanged(EventArgs.Empty);
				}
			}
		}

		public System.Windows.Forms.ImageList ImageList
		{
			get
			{
				return this.imageList;
			}
			set
			{
				this.imageList = value;
			}
		}

		public int ItemHeight
		{
			get
			{
				System.Drawing.Size imageSize = this.imageList.ImageSize;
				return Math.Max(imageSize.Height, (int)((double)this.Font.Height * 1.25));
			}
		}

		public int MaxVisibleItem
		{
			get
			{
				return base.Height / this.ItemHeight;
			}
		}

		public ICompletionData SelectedCompletionData
		{
			get
			{
				if (this.selectedItem < 0)
				{
					return null;
				}
				return this.completionData[this.selectedItem];
			}
		}

		public CodeCompletionListView(ICompletionData[] completionData)
		{
			Array.Sort<ICompletionData>(completionData, new Comparison<ICompletionData>(DefaultCompletionData.Compare));
			this.completionData = completionData;
		}

		public void CenterViewOn(int index)
		{
			int firstItem = this.FirstItem;
			int num = index - this.MaxVisibleItem / 2;
			if (num < 0)
			{
				this.FirstItem = 0;
			}
			else if (num < (int)this.completionData.Length - this.MaxVisibleItem)
			{
				this.FirstItem = num;
			}
			else
			{
				this.FirstItem = (int)this.completionData.Length - this.MaxVisibleItem;
			}
			if (this.FirstItem != firstItem)
			{
				base.Invalidate();
			}
		}

		public void ClearSelection()
		{
			if (this.selectedItem < 0)
			{
				return;
			}
			int num = this.selectedItem - this.firstItem;
			this.selectedItem = -1;
			base.Invalidate(new Rectangle(0, num * this.ItemHeight, base.Width, (num + 1) * this.ItemHeight + 1));
			base.Update();
			this.OnSelectedItemChanged(EventArgs.Empty);
		}

		public void Close()
		{
			if (this.completionData != null)
			{
				Array.Clear(this.completionData, 0, (int)this.completionData.Length);
			}
			base.Dispose();
		}

		protected virtual void OnFirstItemChanged(EventArgs e)
		{
			if (this.FirstItemChanged != null)
			{
				this.FirstItemChanged(this, e);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			float single = 1f;
			int num = this.firstItem;
			float itemHeight = (float)this.ItemHeight;
			while (num < (int)this.completionData.Length && single < (float)base.Height)
			{
				RectangleF rectangleF = new RectangleF(1f, single, (float)(base.Width - 2), itemHeight);
				if (rectangleF.Contains((float)e.X, (float)e.Y))
				{
					this.SelectIndex(num);
					return;
				}
				single += itemHeight;
				num++;
			}
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			float single = 1f;
			float itemHeight = (float)this.ItemHeight;
			float width = itemHeight * (float)this.imageList.ImageSize.Width;
			System.Drawing.Size imageSize = this.imageList.ImageSize;
			int height = (int)(width / (float)imageSize.Height);
			int num = this.firstItem;
			Graphics graphics = pe.Graphics;
			while (num < (int)this.completionData.Length && single < (float)base.Height)
			{
				RectangleF rectangleF = new RectangleF(1f, single, (float)(base.Width - 2), itemHeight);
				if (rectangleF.IntersectsWith(pe.ClipRectangle))
				{
					if (num != this.selectedItem)
					{
						graphics.FillRectangle(SystemBrushes.Window, rectangleF);
					}
					else
					{
						graphics.FillRectangle(SystemBrushes.Highlight, rectangleF);
					}
					int num1 = 0;
					if (this.imageList != null && this.completionData[num].ImageIndex < this.imageList.Images.Count)
					{
						graphics.DrawImage(this.imageList.Images[this.completionData[num].ImageIndex], new RectangleF(1f, single, (float)height, itemHeight));
						num1 = height;
					}
					if (num != this.selectedItem)
					{
						graphics.DrawString(this.completionData[num].Text, this.Font, SystemBrushes.WindowText, (float)num1, single);
					}
					else
					{
						graphics.DrawString(this.completionData[num].Text, this.Font, SystemBrushes.HighlightText, (float)num1, single);
					}
				}
				single += itemHeight;
				num++;
			}
			graphics.DrawRectangle(SystemPens.Control, new Rectangle(0, 0, base.Width - 1, base.Height - 1));
		}

		protected override void OnPaintBackground(PaintEventArgs pe)
		{
		}

		protected virtual void OnSelectedItemChanged(EventArgs e)
		{
			if (this.SelectedItemChanged != null)
			{
				this.SelectedItemChanged(this, e);
			}
		}

		public void PageDown()
		{
			this.SelectIndex(this.selectedItem + this.MaxVisibleItem);
		}

		public void PageUp()
		{
			this.SelectIndex(this.selectedItem - this.MaxVisibleItem);
		}

		public void SelectIndex(int index)
		{
			int num = this.selectedItem;
			int num1 = this.firstItem;
			index = Math.Max(0, index);
			this.selectedItem = Math.Max(0, Math.Min((int)this.completionData.Length - 1, index));
			if (this.selectedItem < this.firstItem)
			{
				this.FirstItem = this.selectedItem;
			}
			if (this.firstItem + this.MaxVisibleItem <= this.selectedItem)
			{
				this.FirstItem = this.selectedItem - this.MaxVisibleItem + 1;
			}
			if (num != this.selectedItem)
			{
				if (this.firstItem == num1)
				{
					int num2 = Math.Min(this.selectedItem, num) - this.firstItem;
					int num3 = Math.Max(this.selectedItem, num) - this.firstItem;
					base.Invalidate(new Rectangle(0, 1 + num2 * this.ItemHeight, base.Width, (num3 - num2 + 1) * this.ItemHeight));
				}
				else
				{
					base.Invalidate();
				}
				this.OnSelectedItemChanged(EventArgs.Empty);
			}
		}

		public void SelectItemWithStart(string startText)
		{
			int num;
			bool flag;
			if (startText == null || startText.Length == 0)
			{
				return;
			}
			string str = startText;
			startText = startText.ToLower();
			int num1 = -1;
			int num2 = -1;
			double num3 = 0;
			for (int i = 0; i < (int)this.completionData.Length; i++)
			{
				string text = this.completionData[i].Text;
				string lower = text.ToLower();
				if (lower.StartsWith(startText))
				{
					double priority = this.completionData[i].Priority;
					if (lower != startText)
					{
						num = (!text.StartsWith(str) ? 0 : 1);
					}
					else
					{
						num = (text != str ? 2 : 3);
					}
					if (num2 < num)
					{
						flag = true;
					}
					else if (num1 == this.selectedItem)
					{
						flag = false;
					}
					else if (i != this.selectedItem)
					{
						flag = (num2 != num ? false : num3 < priority);
					}
					else
					{
						flag = num2 == num;
					}
					if (flag)
					{
						num1 = i;
						num3 = priority;
						num2 = num;
					}
				}
			}
			if (num1 < 0)
			{
				this.ClearSelection();
				return;
			}
			if (num1 >= this.firstItem && this.firstItem + this.MaxVisibleItem > num1)
			{
				this.SelectIndex(num1);
				return;
			}
			this.SelectIndex(num1);
			this.CenterViewOn(num1);
		}

		public void SelectNextItem()
		{
			this.SelectIndex(this.selectedItem + 1);
		}

		public void SelectPrevItem()
		{
			this.SelectIndex(this.selectedItem - 1);
		}

		public event EventHandler FirstItemChanged;

		public event EventHandler SelectedItemChanged;
	}
}