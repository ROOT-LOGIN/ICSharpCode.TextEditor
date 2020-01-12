using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor
{
	public class TextAreaMouseHandler
	{
		private TextArea textArea;

		private bool doubleclick;

		private bool clickedOnSelectedText;

		private MouseButtons button;

		private readonly static Point nilPoint;

		private Point mousedownpos = TextAreaMouseHandler.nilPoint;

		private Point lastmousedownpos = TextAreaMouseHandler.nilPoint;

		private bool gotmousedown;

		private bool dodragdrop;

		private TextLocation minSelection = TextLocation.Empty;

		private TextLocation maxSelection = TextLocation.Empty;

		static TextAreaMouseHandler()
		{
			TextAreaMouseHandler.nilPoint = new Point(-1, -1);
		}

		public TextAreaMouseHandler(TextArea ttextArea)
		{
			this.textArea = ttextArea;
		}

		public void Attach()
		{
			this.textArea.Click += new EventHandler(this.TextAreaClick);
			this.textArea.MouseMove += new MouseEventHandler(this.TextAreaMouseMove);
			this.textArea.MouseDown += new MouseEventHandler(this.OnMouseDown);
			this.textArea.DoubleClick += new EventHandler(this.OnDoubleClick);
			this.textArea.MouseLeave += new EventHandler(this.OnMouseLeave);
			this.textArea.MouseUp += new MouseEventHandler(this.OnMouseUp);
			this.textArea.LostFocus += new EventHandler(this.TextAreaLostFocus);
			this.textArea.ToolTipRequest += new ToolTipRequestEventHandler(this.OnToolTipRequest);
		}

		private void DoubleClickSelectionExtend()
		{
			Point point = this.textArea.mousepos;
			this.textArea.SelectionManager.ClearSelection();
			if (this.textArea.TextView.DrawingPosition.Contains(point.X, point.Y))
			{
				TextView textView = this.textArea.TextView;
				int x = point.X - this.textArea.TextView.DrawingPosition.X;
				int y = point.Y;
				Rectangle drawingPosition = this.textArea.TextView.DrawingPosition;
				FoldMarker foldMarkerFromPosition = textView.GetFoldMarkerFromPosition(x, y - drawingPosition.Y);
				if (foldMarkerFromPosition != null && foldMarkerFromPosition.IsFolded)
				{
					foldMarkerFromPosition.IsFolded = false;
					this.textArea.MotherTextAreaControl.AdjustScrollBars();
				}
				if (this.textArea.Caret.Offset < this.textArea.Document.TextLength)
				{
					if (this.textArea.Document.GetCharAt(this.textArea.Caret.Offset) != '\"')
					{
						this.minSelection = this.textArea.Document.OffsetToPosition(this.FindWordStart(this.textArea.Document, this.textArea.Caret.Offset));
						this.maxSelection = this.textArea.Document.OffsetToPosition(this.FindWordEnd(this.textArea.Document, this.textArea.Caret.Offset));
					}
					else if (this.textArea.Caret.Offset < this.textArea.Document.TextLength)
					{
						int num = this.FindNext(this.textArea.Document, this.textArea.Caret.Offset + 1, '\"');
						this.minSelection = this.textArea.Caret.Position;
						if (num > this.textArea.Caret.Offset && num < this.textArea.Document.TextLength)
						{
							num++;
						}
						this.maxSelection = this.textArea.Document.OffsetToPosition(num);
					}
					this.textArea.Caret.Position = this.maxSelection;
					this.textArea.SelectionManager.ExtendSelection(this.minSelection, this.maxSelection);
				}
				if (this.textArea.SelectionManager.selectionCollection.Count > 0)
				{
					ISelection item = this.textArea.SelectionManager.selectionCollection[0];
					item.StartPosition = this.minSelection;
					item.EndPosition = this.maxSelection;
					this.textArea.SelectionManager.SelectionStart = this.minSelection;
				}
				this.textArea.SetDesiredColumn();
				this.textArea.Refresh();
			}
		}

		private void ExtendSelectionToMouse()
		{
			Point point = this.textArea.mousepos;
			TextView textView = this.textArea.TextView;
			int x = point.X;
			Rectangle drawingPosition = this.textArea.TextView.DrawingPosition;
			int num = Math.Max(0, x - drawingPosition.X);
			int y = point.Y;
			Rectangle rectangle = this.textArea.TextView.DrawingPosition;
			TextLocation logicalPosition = textView.GetLogicalPosition(num, y - rectangle.Y);
			int y1 = logicalPosition.Y;
			logicalPosition = this.textArea.Caret.ValidatePosition(logicalPosition);
			TextLocation position = this.textArea.Caret.Position;
			if (position == logicalPosition && this.textArea.SelectionManager.selectFrom.@where != 1)
			{
				return;
			}
			if (this.textArea.SelectionManager.selectFrom.@where != 1)
			{
				this.textArea.Caret.Position = logicalPosition;
			}
			else if (logicalPosition.Y >= this.textArea.SelectionManager.SelectionStart.Y)
			{
				this.textArea.Caret.Position = this.textArea.SelectionManager.NextValidPosition(logicalPosition.Y);
			}
			else
			{
				this.textArea.Caret.Position = new TextLocation(0, logicalPosition.Y);
			}
			if (this.minSelection.IsEmpty || this.textArea.SelectionManager.SelectionCollection.Count <= 0 || this.textArea.SelectionManager.selectFrom.@where != 2)
			{
				this.textArea.SelectionManager.ExtendSelection(position, this.textArea.Caret.Position);
			}
			else
			{
				ISelection item = this.textArea.SelectionManager.SelectionCollection[0];
				TextLocation textLocation = (this.textArea.SelectionManager.GreaterEqPos(this.minSelection, this.maxSelection) ? this.maxSelection : this.minSelection);
				TextLocation position1 = (this.textArea.SelectionManager.GreaterEqPos(this.minSelection, this.maxSelection) ? this.minSelection : this.maxSelection);
				if (this.textArea.SelectionManager.GreaterEqPos(position1, logicalPosition) && this.textArea.SelectionManager.GreaterEqPos(logicalPosition, textLocation))
				{
					this.textArea.SelectionManager.SetSelection(textLocation, position1);
				}
				else if (!this.textArea.SelectionManager.GreaterEqPos(position1, logicalPosition))
				{
					int offset = this.textArea.Document.PositionToOffset(logicalPosition);
					position1 = this.textArea.Document.OffsetToPosition(this.FindWordEnd(this.textArea.Document, offset));
					this.textArea.SelectionManager.SetSelection(textLocation, position1);
				}
				else
				{
					int offset1 = this.textArea.Document.PositionToOffset(logicalPosition);
					textLocation = this.textArea.Document.OffsetToPosition(this.FindWordStart(this.textArea.Document, offset1));
					this.textArea.SelectionManager.SetSelection(textLocation, position1);
				}
			}
			this.textArea.SetDesiredColumn();
		}

		private int FindNext(IDocument document, int offset, char ch)
		{
			LineSegment lineSegmentForOffset = document.GetLineSegmentForOffset(offset);
			int num = lineSegmentForOffset.Offset + lineSegmentForOffset.Length;
			while (offset < num && document.GetCharAt(offset) != ch)
			{
				offset++;
			}
			return offset;
		}

		private int FindWordEnd(IDocument document, int offset)
		{
			LineSegment lineSegmentForOffset = document.GetLineSegmentForOffset(offset);
			if (lineSegmentForOffset.Length == 0)
			{
				return offset;
			}
			int num = lineSegmentForOffset.Offset + lineSegmentForOffset.Length;
			offset = Math.Min(offset, num - 1);
			if (!this.IsSelectableChar(document.GetCharAt(offset)))
			{
				if (!char.IsWhiteSpace(document.GetCharAt(offset)))
				{
					return Math.Max(0, offset + 1);
				}
				if (offset > 0 && char.IsWhiteSpace(document.GetCharAt(offset - 1)))
				{
					while (offset < num)
					{
						if (char.IsWhiteSpace(document.GetCharAt(offset)))
						{
							offset++;
						}
						else
						{
							break;
						}
					}
				}
			}
			else
			{
				while (offset < num)
				{
					if (this.IsSelectableChar(document.GetCharAt(offset)))
					{
						offset++;
					}
					else
					{
						break;
					}
				}
			}
			return offset;
		}

		private int FindWordStart(IDocument document, int offset)
		{
			LineSegment lineSegmentForOffset = document.GetLineSegmentForOffset(offset);
			if (offset <= 0 || !char.IsWhiteSpace(document.GetCharAt(offset - 1)) || !char.IsWhiteSpace(document.GetCharAt(offset)))
			{
				if (!this.IsSelectableChar(document.GetCharAt(offset)))
				{
					if (offset > 0 && char.IsWhiteSpace(document.GetCharAt(offset)) && this.IsSelectableChar(document.GetCharAt(offset - 1)))
					{
						goto Label2;
					}
					if (offset > 0 && !char.IsWhiteSpace(document.GetCharAt(offset - 1)) && !this.IsSelectableChar(document.GetCharAt(offset - 1)))
					{
						return Math.Max(0, offset - 1);
					}
					else
					{
						return offset;
					}
				}
			Label2:
				while (offset > lineSegmentForOffset.Offset)
				{
					if (this.IsSelectableChar(document.GetCharAt(offset - 1)))
					{
						offset--;
					}
					else
					{
						break;
					}
				}
			}
			else
			{
				while (offset > lineSegmentForOffset.Offset)
				{
					if (char.IsWhiteSpace(document.GetCharAt(offset - 1)))
					{
						offset--;
					}
					else
					{
						break;
					}
				}
			}
			return offset;
		}

		private bool IsSelectableChar(char ch)
		{
			if (char.IsLetterOrDigit(ch))
			{
				return true;
			}
			return ch == '\u005F';
		}

		private void OnDoubleClick(object sender, EventArgs e)
		{
			if (this.dodragdrop)
			{
				return;
			}
			this.textArea.SelectionManager.selectFrom.@where = 2;
			this.doubleclick = true;
		}

		private void OnMouseDown(object sender, MouseEventArgs e)
		{
			this.textArea.mousepos = e.Location;
			Point location = e.Location;
			if (this.dodragdrop)
			{
				return;
			}
			if (this.doubleclick)
			{
				this.doubleclick = false;
				return;
			}
			if (this.textArea.TextView.DrawingPosition.Contains(location.X, location.Y))
			{
				this.gotmousedown = true;
				this.textArea.SelectionManager.selectFrom.@where = 2;
				this.button = e.Button;
				if (this.button == MouseButtons.Left && e.Clicks == 2)
				{
					int num = Math.Abs(this.lastmousedownpos.X - e.X);
					int num1 = Math.Abs(this.lastmousedownpos.Y - e.Y);
					if (num <= SystemInformation.DoubleClickSize.Width && num1 <= SystemInformation.DoubleClickSize.Height)
					{
						this.DoubleClickSelectionExtend();
						this.lastmousedownpos = new Point(e.X, e.Y);
						if (this.textArea.SelectionManager.selectFrom.@where == 1 && !this.minSelection.IsEmpty && !this.maxSelection.IsEmpty && this.textArea.SelectionManager.SelectionCollection.Count > 0)
						{
							this.textArea.SelectionManager.SelectionCollection[0].StartPosition = this.minSelection;
							this.textArea.SelectionManager.SelectionCollection[0].EndPosition = this.maxSelection;
							this.textArea.SelectionManager.SelectionStart = this.minSelection;
							this.minSelection = TextLocation.Empty;
							this.maxSelection = TextLocation.Empty;
						}
						return;
					}
				}
				this.minSelection = TextLocation.Empty;
				this.maxSelection = TextLocation.Empty;
				Point point = new Point(e.X, e.Y);
				Point point1 = point;
				this.mousedownpos = point;
				this.lastmousedownpos = point1;
				if (this.button == MouseButtons.Left)
				{
					TextView textView = this.textArea.TextView;
					int x = location.X - this.textArea.TextView.DrawingPosition.X;
					int y = location.Y;
					Rectangle drawingPosition = this.textArea.TextView.DrawingPosition;
					FoldMarker foldMarkerFromPosition = textView.GetFoldMarkerFromPosition(x, y - drawingPosition.Y);
					if (foldMarkerFromPosition != null && foldMarkerFromPosition.IsFolded)
					{
						if (this.textArea.SelectionManager.HasSomethingSelected)
						{
							this.clickedOnSelectedText = true;
						}
						TextLocation textLocation = new TextLocation(foldMarkerFromPosition.StartColumn, foldMarkerFromPosition.StartLine);
						TextLocation textLocation1 = new TextLocation(foldMarkerFromPosition.EndColumn, foldMarkerFromPosition.EndLine);
						this.textArea.SelectionManager.SetSelection(new DefaultSelection(this.textArea.TextView.Document, textLocation, textLocation1));
						this.textArea.Caret.Position = textLocation;
						this.textArea.SetDesiredColumn();
						this.textArea.Focus();
						return;
					}
					if ((Control.ModifierKeys & Keys.Shift) != Keys.Shift)
					{
						TextView textView1 = this.textArea.TextView;
						int x1 = location.X - this.textArea.TextView.DrawingPosition.X;
						int y1 = location.Y;
						Rectangle rectangle = this.textArea.TextView.DrawingPosition;
						TextLocation logicalPosition = textView1.GetLogicalPosition(x1, y1 - rectangle.Y);
						this.clickedOnSelectedText = false;
						int offset = this.textArea.Document.PositionToOffset(logicalPosition);
						if (!this.textArea.SelectionManager.HasSomethingSelected || !this.textArea.SelectionManager.IsSelected(offset))
						{
							this.textArea.SelectionManager.ClearSelection();
							if (location.Y > 0 && location.Y < this.textArea.TextView.DrawingPosition.Height)
							{
								TextLocation textLocation2 = new TextLocation()
								{
									Y = Math.Min(this.textArea.Document.TotalNumberOfLines - 1, logicalPosition.Y),
									X = logicalPosition.X
								};
								this.textArea.Caret.Position = textLocation2;
								this.textArea.SetDesiredColumn();
							}
						}
						else
						{
							this.clickedOnSelectedText = true;
						}
					}
					else
					{
						this.ExtendSelectionToMouse();
					}
				}
				else if (this.button == MouseButtons.Right)
				{
					TextView textView2 = this.textArea.TextView;
					int x2 = location.X - this.textArea.TextView.DrawingPosition.X;
					int y2 = location.Y;
					Rectangle drawingPosition1 = this.textArea.TextView.DrawingPosition;
					TextLocation logicalPosition1 = textView2.GetLogicalPosition(x2, y2 - drawingPosition1.Y);
					int offset1 = this.textArea.Document.PositionToOffset(logicalPosition1);
					if (!this.textArea.SelectionManager.HasSomethingSelected || !this.textArea.SelectionManager.IsSelected(offset1))
					{
						this.textArea.SelectionManager.ClearSelection();
						if (location.Y > 0 && location.Y < this.textArea.TextView.DrawingPosition.Height)
						{
							TextLocation textLocation3 = new TextLocation()
							{
								Y = Math.Min(this.textArea.Document.TotalNumberOfLines - 1, logicalPosition1.Y),
								X = logicalPosition1.X
							};
							this.textArea.Caret.Position = textLocation3;
							this.textArea.SetDesiredColumn();
						}
					}
				}
			}
			this.textArea.Focus();
		}

		private void OnMouseLeave(object sender, EventArgs e)
		{
			this.ShowHiddenCursorIfMovedOrLeft();
			this.gotmousedown = false;
			this.mousedownpos = TextAreaMouseHandler.nilPoint;
		}

		private void OnMouseUp(object sender, MouseEventArgs e)
		{
			this.textArea.SelectionManager.selectFrom.@where = 0;
			this.gotmousedown = false;
			this.mousedownpos = TextAreaMouseHandler.nilPoint;
		}

		private void OnToolTipRequest(object sender, ToolTipRequestEventArgs e)
		{
			if (e.ToolTipShown)
			{
				return;
			}
			Point mousePosition = e.MousePosition;
			TextView textView = this.textArea.TextView;
			int x = mousePosition.X - this.textArea.TextView.DrawingPosition.X;
			int y = mousePosition.Y;
			Rectangle drawingPosition = this.textArea.TextView.DrawingPosition;
			FoldMarker foldMarkerFromPosition = textView.GetFoldMarkerFromPosition(x, y - drawingPosition.Y);
			if (foldMarkerFromPosition == null || !foldMarkerFromPosition.IsFolded)
			{
				foreach (TextMarker marker in this.textArea.Document.MarkerStrategy.GetMarkers(e.LogicalPosition))
				{
					if (marker.ToolTip == null)
					{
						continue;
					}
					e.ShowToolTip(marker.ToolTip.Replace("\t", "    "));
					return;
				}
				return;
			}
			StringBuilder stringBuilder = new StringBuilder(foldMarkerFromPosition.InnerText);
			int num = 0;
			for (int i = 0; i < stringBuilder.Length; i++)
			{
				if (stringBuilder[i] == '\n')
				{
					num++;
					if (num >= 10)
					{
						stringBuilder.Remove(i + 1, stringBuilder.Length - i - 1);
						stringBuilder.Append(Environment.NewLine);
						stringBuilder.Append("...");
						break;
					}
				}
			}
			stringBuilder.Replace("\t", "    ");
			e.ShowToolTip(stringBuilder.ToString());
		}

		private void ShowHiddenCursorIfMovedOrLeft()
		{
			bool flag;
			TextArea textArea = this.textArea;
			if (!this.textArea.Focused)
			{
				flag = true;
			}
			else
			{
				Rectangle clientRectangle = this.textArea.ClientRectangle;
				flag = !clientRectangle.Contains(this.textArea.PointToClient(Cursor.Position));
			}
			textArea.ShowHiddenCursor(flag);
		}

		private void TextAreaClick(object sender, EventArgs e)
		{
			Point point = this.textArea.mousepos;
			if (this.dodragdrop)
			{
				return;
			}
			if (this.clickedOnSelectedText && this.textArea.TextView.DrawingPosition.Contains(point.X, point.Y))
			{
				this.textArea.SelectionManager.ClearSelection();
				TextView textView = this.textArea.TextView;
				int x = point.X - this.textArea.TextView.DrawingPosition.X;
				int y = point.Y;
				Rectangle drawingPosition = this.textArea.TextView.DrawingPosition;
				TextLocation logicalPosition = textView.GetLogicalPosition(x, y - drawingPosition.Y);
				this.textArea.Caret.Position = logicalPosition;
				this.textArea.SetDesiredColumn();
			}
		}

		private void TextAreaLostFocus(object sender, EventArgs e)
		{
			this.textArea.BeginInvoke(new MethodInvoker(this.ShowHiddenCursorIfMovedOrLeft));
		}

		private void TextAreaMouseMove(object sender, MouseEventArgs e)
		{
			this.textArea.mousepos = e.Location;
			switch (this.textArea.SelectionManager.selectFrom.@where)
			{
				case 1:
				{
					this.ExtendSelectionToMouse();
					return;
				}
				case 2:
				{
					this.textArea.ShowHiddenCursor(false);
					if (this.dodragdrop)
					{
						this.dodragdrop = false;
						return;
					}
					this.doubleclick = false;
					this.textArea.mousepos = new Point(e.X, e.Y);
					if (!this.clickedOnSelectedText)
					{
						if (e.Button == MouseButtons.Left && this.gotmousedown && this.textArea.SelectionManager.selectFrom.@where == 2)
						{
							this.ExtendSelectionToMouse();
						}
						return;
					}
					if (Math.Abs(this.mousedownpos.X - e.X) >= SystemInformation.DragSize.Width / 2 || Math.Abs(this.mousedownpos.Y - e.Y) >= SystemInformation.DragSize.Height / 2)
					{
						this.clickedOnSelectedText = false;
						ISelection selectionAt = this.textArea.SelectionManager.GetSelectionAt(this.textArea.Caret.Offset);
						if (selectionAt != null)
						{
							string selectedText = selectionAt.SelectedText;
							bool flag = SelectionManager.SelectionIsReadOnly(this.textArea.Document, selectionAt);
							if (selectedText != null && selectedText.Length > 0)
							{
								DataObject dataObject = new DataObject();
								dataObject.SetData(DataFormats.UnicodeText, true, selectedText);
								dataObject.SetData(selectionAt);
								this.dodragdrop = true;
								this.textArea.DoDragDrop(dataObject, (flag ? DragDropEffects.Copy | DragDropEffects.Scroll : DragDropEffects.All));
							}
						}
					}
					return;
				}
				default:
				{
					goto case 2;
				}
			}
		}
	}
}