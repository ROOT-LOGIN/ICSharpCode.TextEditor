using ICSharpCode.TextEditor.Document;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor
{
	public class GutterMargin : AbstractMargin, IDisposable
	{
		private StringFormat numberStringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();

		public static System.Windows.Forms.Cursor RightLeftCursor;

		public override System.Windows.Forms.Cursor Cursor
		{
			get
			{
				return GutterMargin.RightLeftCursor;
			}
		}

		public override bool IsVisible
		{
			get
			{
				return this.textArea.TextEditorProperties.ShowLineNumbers;
			}
		}

		public override System.Drawing.Size Size
		{
			get
			{
				return new System.Drawing.Size(this.textArea.TextView.WideSpaceWidth * Math.Max(3, (int)Math.Log10((double)this.textArea.Document.TotalNumberOfLines) + 1), -1);
			}
		}

		static GutterMargin()
		{
			Stream manifestResourceStream = Assembly.GetCallingAssembly().GetManifestResourceStream("BigBug.ICSharpCode.TextEditor.xshd.RightArrow.cur");
            if (manifestResourceStream == null)
			{
				throw new Exception("could not find cursor resource");
			}
			GutterMargin.RightLeftCursor =  new System.Windows.Forms.Cursor(manifestResourceStream);
			manifestResourceStream.Close();
		}

		public GutterMargin(ICSharpCode.TextEditor.TextArea textArea) : base(textArea)
		{
			this.numberStringFormat.LineAlignment = StringAlignment.Far;
			this.numberStringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
		}

		public void Dispose()
		{
			this.numberStringFormat.Dispose();
		}

		public override void HandleMouseDown(Point mousepos, MouseButtons mouseButtons)
		{
			TextLocation position;
			this.textArea.SelectionManager.selectFrom.@where = 1;
			int logicalLine = this.textArea.TextView.GetLogicalLine(mousepos.Y);
			if (logicalLine >= 0 && logicalLine < this.textArea.Document.TotalNumberOfLines)
			{
				if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
				{
					if (this.textArea.SelectionManager.HasSomethingSelected || logicalLine == this.textArea.Caret.Position.Y)
					{
						MouseEventArgs mouseEventArg = new MouseEventArgs(mouseButtons, 1, mousepos.X, mousepos.Y, 0);
						this.textArea.RaiseMouseMove(mouseEventArg);
						return;
					}
					if (logicalLine < this.textArea.Caret.Position.Y)
					{
						position = this.textArea.Caret.Position;
						this.textArea.SelectionManager.SetSelection(new DefaultSelection(this.textArea.Document, position, new TextLocation(position.X, position.Y)));
						this.textArea.SelectionManager.ExtendSelection(new TextLocation(position.X, position.Y), new TextLocation(0, logicalLine));
						this.textArea.Caret.Position = new TextLocation(0, logicalLine);
						return;
					}
					position = this.textArea.Caret.Position;
					if (logicalLine < this.textArea.Document.TotalNumberOfLines - 1)
					{
						this.textArea.SelectionManager.SetSelection(new DefaultSelection(this.textArea.Document, position, new TextLocation(0, logicalLine + 1)));
						this.textArea.Caret.Position = new TextLocation(0, logicalLine + 1);
						return;
					}
					this.textArea.SelectionManager.SetSelection(new DefaultSelection(this.textArea.Document, position, new TextLocation(this.textArea.Document.GetLineSegment(logicalLine).Length + 1, logicalLine)));
					this.textArea.Caret.Position = new TextLocation(this.textArea.Document.GetLineSegment(logicalLine).Length + 1, logicalLine);
					return;
				}
				this.textArea.mousepos = mousepos;
				position = new TextLocation(0, logicalLine);
				this.textArea.SelectionManager.ClearSelection();
				if (logicalLine < this.textArea.Document.TotalNumberOfLines - 1)
				{
					this.textArea.SelectionManager.SetSelection(new DefaultSelection(this.textArea.Document, position, new TextLocation(position.X, position.Y + 1)));
					this.textArea.Caret.Position = new TextLocation(position.X, position.Y + 1);
					return;
				}
				this.textArea.SelectionManager.SetSelection(new DefaultSelection(this.textArea.Document, new TextLocation(0, logicalLine), new TextLocation(this.textArea.Document.GetLineSegment(logicalLine).Length + 1, position.Y)));
				this.textArea.Caret.Position = new TextLocation(this.textArea.Document.GetLineSegment(logicalLine).Length + 1, position.Y);
			}
		}

		public override void Paint(Graphics g, Rectangle rect)
		{
			if (rect.Width <= 0 || rect.Height <= 0)
			{
				return;
			}
			HighlightColor colorFor = this.textArea.Document.HighlightingStrategy.GetColorFor("LineNumbers");
			int fontHeight = this.textArea.TextView.FontHeight;
			Brush brush = (this.textArea.Enabled ? BrushRegistry.GetBrush(colorFor.BackgroundColor) : SystemBrushes.InactiveBorder);
			Brush brush1 = BrushRegistry.GetBrush(colorFor.Color);
			for (int i = 0; i < (base.DrawingPosition.Height + this.textArea.TextView.VisibleLineDrawingRemainder) / fontHeight + 1; i++)
			{
				int y = this.drawingPosition.Y + fontHeight * i - this.textArea.TextView.VisibleLineDrawingRemainder;
				Rectangle rectangle = new Rectangle(this.drawingPosition.X, y, this.drawingPosition.Width, fontHeight);
				if (rect.IntersectsWith(rectangle))
				{
					g.FillRectangle(brush, rectangle);
					int firstLogicalLine = this.textArea.Document.GetFirstLogicalLine(this.textArea.Document.GetVisibleLine(this.textArea.TextView.FirstVisibleLine) + i);
					if (firstLogicalLine < this.textArea.Document.TotalNumberOfLines)
					{
						int num = firstLogicalLine + 1;
						g.DrawString(num.ToString(), colorFor.GetFont(base.TextEditorProperties.FontContainer), brush1, rectangle, this.numberStringFormat);
					}
				}
			}
		}
	}
}