using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Undo;
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor
{
	public class TextAreaDragDropHandler
	{
		public static Action<Exception> OnDragDropException;

		private TextArea textArea;

		static TextAreaDragDropHandler()
		{
			TextAreaDragDropHandler.OnDragDropException = (Exception ex) => MessageBox.Show(ex.ToString());
		}

		public TextAreaDragDropHandler()
		{
		}

		public void Attach(TextArea textArea)
		{
			this.textArea = textArea;
			textArea.AllowDrop = true;
			textArea.DragEnter += TextAreaDragDropHandler.MakeDragEventHandler(new DragEventHandler(this.OnDragEnter));
			textArea.DragDrop += TextAreaDragDropHandler.MakeDragEventHandler(new DragEventHandler(this.OnDragDrop));
			textArea.DragOver += TextAreaDragDropHandler.MakeDragEventHandler(new DragEventHandler(this.OnDragOver));
		}

		private static DragDropEffects GetDragDropEffect(DragEventArgs e)
		{
			if ((e.AllowedEffect & DragDropEffects.Move) > DragDropEffects.None && (e.AllowedEffect & DragDropEffects.Copy) > DragDropEffects.None)
			{
				if ((e.KeyState & 8) <= 0)
				{
					return DragDropEffects.Move;
				}
				return DragDropEffects.Copy;
			}
			if ((e.AllowedEffect & DragDropEffects.Move) > DragDropEffects.None)
			{
				return DragDropEffects.Move;
			}
			if ((e.AllowedEffect & DragDropEffects.Copy) > DragDropEffects.None)
			{
				return DragDropEffects.Copy;
			}
			return DragDropEffects.None;
		}

		private void InsertString(int offset, string str)
		{
			this.textArea.Document.Insert(offset, str);
			this.textArea.SelectionManager.SetSelection(new DefaultSelection(this.textArea.Document, this.textArea.Document.OffsetToPosition(offset), this.textArea.Document.OffsetToPosition(offset + str.Length)));
			this.textArea.Caret.Position = this.textArea.Document.OffsetToPosition(offset + str.Length);
			this.textArea.Refresh();
		}

		private static DragEventHandler MakeDragEventHandler(DragEventHandler h)
		{
			return (object sender, DragEventArgs e) => {
				try
				{
					h(sender, e);
				}
				catch (Exception exception)
				{
					TextAreaDragDropHandler.OnDragDropException(exception);
				}
			};
		}

		protected void OnDragDrop(object sender, DragEventArgs e)
		{
			this.textArea.PointToClient(new Point(e.X, e.Y));
			if (e.Data.GetDataPresent(typeof(string)))
			{
				this.textArea.BeginUpdate();
				this.textArea.Document.UndoStack.StartUndoGroup();
				try
				{
					int offset = this.textArea.Caret.Offset;
					if (!this.textArea.IsReadOnly(offset))
					{
						if (e.Data.GetDataPresent(typeof(DefaultSelection)))
						{
							ISelection data = (ISelection)e.Data.GetData(typeof(DefaultSelection));
							if (data.ContainsPosition(this.textArea.Caret.Position))
							{
								return;
							}
							else if (TextAreaDragDropHandler.GetDragDropEffect(e) == DragDropEffects.Move)
							{
								if (!SelectionManager.SelectionIsReadOnly(this.textArea.Document, data))
								{
									int length = data.Length;
									this.textArea.Document.Remove(data.Offset, length);
									if (data.Offset < offset)
									{
										offset -= length;
									}
								}
								else
								{
									return;
								}
							}
						}
						this.textArea.SelectionManager.ClearSelection();
						this.InsertString(offset, (string)e.Data.GetData(typeof(string)));
						this.textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
					}
				}
				finally
				{
					this.textArea.Document.UndoStack.EndUndoGroup();
					this.textArea.EndUpdate();
				}
			}
		}

		protected void OnDragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(string)))
			{
				e.Effect = TextAreaDragDropHandler.GetDragDropEffect(e);
			}
		}

		protected void OnDragOver(object sender, DragEventArgs e)
		{
			if (!this.textArea.Focused)
			{
				this.textArea.Focus();
			}
			Point client = this.textArea.PointToClient(new Point(e.X, e.Y));
			if (!this.textArea.TextView.DrawingPosition.Contains(client.X, client.Y))
			{
				e.Effect = DragDropEffects.None;
				return;
			}
			TextView textView = this.textArea.TextView;
			int x = client.X - this.textArea.TextView.DrawingPosition.X;
			int y = client.Y;
			Rectangle drawingPosition = this.textArea.TextView.DrawingPosition;
			TextLocation logicalPosition = textView.GetLogicalPosition(x, y - drawingPosition.Y);
			int num = Math.Min(this.textArea.Document.TotalNumberOfLines - 1, Math.Max(0, logicalPosition.Y));
			this.textArea.Caret.Position = new TextLocation(logicalPosition.X, num);
			this.textArea.SetDesiredColumn();
			if (!e.Data.GetDataPresent(typeof(string)) || this.textArea.IsReadOnly(this.textArea.Caret.Offset))
			{
				e.Effect = DragDropEffects.None;
				return;
			}
			e.Effect = TextAreaDragDropHandler.GetDragDropEffect(e);
		}
	}
}