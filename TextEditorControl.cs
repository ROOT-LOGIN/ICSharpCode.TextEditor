using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Undo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor
{
	[ToolboxBitmap("ICSharpCode.TextEditor.Resources.TextEditorControl.bmp")]
	[ToolboxItem(true)]
	public class TextEditorControl : TextEditorControlBase
	{
		protected Panel textAreaPanel = new Panel();

		private TextAreaControl primaryTextArea;

		private Splitter textAreaSplitter;

		private TextAreaControl secondaryTextArea;

		private System.Drawing.Printing.PrintDocument printDocument;

		private TextAreaControl activeTextAreaControl;

		private int curLineNr;

		private float curTabIndent;

		private StringFormat printingStringFormat;

		public override TextAreaControl ActiveTextAreaControl
		{
			get
			{
				return this.activeTextAreaControl;
			}
		}

		[Browsable(false)]
		public bool EnableRedo
		{
			get
			{
				return base.Document.UndoStack.CanRedo;
			}
		}

		[Browsable(false)]
		public bool EnableUndo
		{
			get
			{
				return base.Document.UndoStack.CanUndo;
			}
		}

		[Browsable(false)]
		public System.Drawing.Printing.PrintDocument PrintDocument
		{
			get
			{
				if (this.printDocument == null)
				{
					this.printDocument = new System.Drawing.Printing.PrintDocument();
					this.printDocument.BeginPrint += new PrintEventHandler(this.BeginPrint);
					this.printDocument.PrintPage += new PrintPageEventHandler(this.PrintPage);
				}
				return this.printDocument;
			}
		}

		public TextEditorControl()
		{
			base.SetStyle(ControlStyles.ContainerControl, true);
			this.textAreaPanel.Dock = DockStyle.Fill;
			base.Document = (new DocumentFactory()).CreateDocument();
			base.Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy();
			this.primaryTextArea = new TextAreaControl(this);
			this.activeTextAreaControl = this.primaryTextArea;
			this.primaryTextArea.TextArea.GotFocus += new EventHandler((object argument0, EventArgs argument1) => this.SetActiveTextAreaControl(this.primaryTextArea));
			this.primaryTextArea.Dock = DockStyle.Fill;
			this.textAreaPanel.Controls.Add(this.primaryTextArea);
			this.InitializeTextAreaControl(this.primaryTextArea);
			base.Controls.Add(this.textAreaPanel);
			base.ResizeRedraw = true;
			base.Document.UpdateCommited += new EventHandler(this.CommitUpdateRequested);
			this.OptionsChanged();
		}

		private void Advance(ref float x, ref float y, float maxWidth, float size, float fontHeight)
		{
			if (x + size < maxWidth)
			{
				x += size;
				return;
			}
			x = this.curTabIndent;
			y += fontHeight;
		}

		private void BeginPrint(object sender, PrintEventArgs ev)
		{
			this.curLineNr = 0;
			this.printingStringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
			float[] tabIndent = new float[100];
			for (int i = 0; i < (int)tabIndent.Length; i++)
			{
				tabIndent[i] = (float)(base.TabIndent * this.primaryTextArea.TextArea.TextView.WideSpaceWidth);
			}
			this.printingStringFormat.SetTabStops(0f, tabIndent);
		}

		private void CommitUpdateRequested(object sender, EventArgs e)
		{
			if (base.IsInUpdate)
			{
				return;
			}
			foreach (TextAreaUpdate updateQueue in base.Document.UpdateQueue)
			{
				switch (updateQueue.TextAreaUpdateType)
				{
					case TextAreaUpdateType.WholeTextArea:
					{
						this.primaryTextArea.TextArea.Invalidate();
						if (this.secondaryTextArea == null)
						{
							continue;
						}
						this.secondaryTextArea.TextArea.Invalidate();
						continue;
					}
					case TextAreaUpdateType.SingleLine:
					case TextAreaUpdateType.PositionToLineEnd:
					{
						this.primaryTextArea.TextArea.UpdateLine(updateQueue.Position.Y);
						if (this.secondaryTextArea == null)
						{
							continue;
						}
						this.secondaryTextArea.TextArea.UpdateLine(updateQueue.Position.Y);
						continue;
					}
					case TextAreaUpdateType.SinglePosition:
					{
						this.primaryTextArea.TextArea.UpdateLine(updateQueue.Position.Y, updateQueue.Position.X, updateQueue.Position.X);
						if (this.secondaryTextArea == null)
						{
							continue;
						}
						this.secondaryTextArea.TextArea.UpdateLine(updateQueue.Position.Y, updateQueue.Position.X, updateQueue.Position.X);
						continue;
					}
					case TextAreaUpdateType.PositionToEnd:
					{
						this.primaryTextArea.TextArea.UpdateToEnd(updateQueue.Position.Y);
						if (this.secondaryTextArea == null)
						{
							continue;
						}
						this.secondaryTextArea.TextArea.UpdateToEnd(updateQueue.Position.Y);
						continue;
					}
					case TextAreaUpdateType.LinesBetween:
					{
						this.primaryTextArea.TextArea.UpdateLines(updateQueue.Position.X, updateQueue.Position.Y);
						if (this.secondaryTextArea == null)
						{
							continue;
						}
						this.secondaryTextArea.TextArea.UpdateLines(updateQueue.Position.X, updateQueue.Position.Y);
						continue;
					}
					default:
					{
						continue;
					}
				}
			}
			base.Document.UpdateQueue.Clear();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.printDocument != null)
				{
					this.printDocument.BeginPrint -= new PrintEventHandler(this.BeginPrint);
					this.printDocument.PrintPage -= new PrintPageEventHandler(this.PrintPage);
					this.printDocument = null;
				}
				base.Document.UndoStack.ClearAll();
				base.Document.UpdateCommited -= new EventHandler(this.CommitUpdateRequested);
				if (this.textAreaPanel != null)
				{
					if (this.secondaryTextArea != null)
					{
						this.secondaryTextArea.Dispose();
						this.textAreaSplitter.Dispose();
						this.secondaryTextArea = null;
						this.textAreaSplitter = null;
					}
					if (this.primaryTextArea != null)
					{
						this.primaryTextArea.Dispose();
					}
					this.textAreaPanel.Dispose();
					this.textAreaPanel = null;
				}
			}
			base.Dispose(disposing);
		}

		private void DrawLine(Graphics g, LineSegment line, float yPos, RectangleF margin)
		{
			float single = 0f;
			float height = this.Font.GetHeight(g);
			this.curTabIndent = 0f;
			FontContainer fontContainer = base.TextEditorProperties.FontContainer;
			foreach (TextWord word in line.Words)
			{
				switch (word.Type)
				{
					case TextWordType.Word:
					{
						g.DrawString(word.Word, word.GetFont(fontContainer), BrushRegistry.GetBrush(word.Color), single + margin.X, yPos);
						SizeF sizeF = g.MeasureString(word.Word, word.GetFont(fontContainer), new SizeF(margin.Width, height * 100f), this.printingStringFormat);
						this.Advance(ref single, ref yPos, margin.Width, sizeF.Width, height);
						continue;
					}
					case TextWordType.Space:
					{
						this.Advance(ref single, ref yPos, margin.Width, (float)this.primaryTextArea.TextArea.TextView.SpaceWidth, height);
						continue;
					}
					case TextWordType.Tab:
					{
						this.Advance(ref single, ref yPos, margin.Width, (float)(base.TabIndent * this.primaryTextArea.TextArea.TextView.WideSpaceWidth), height);
						continue;
					}
					default:
					{
						continue;
					}
				}
			}
		}

		public override void EndUpdate()
		{
			base.EndUpdate();
			base.Document.CommitUpdate();
			if (!base.IsInUpdate)
			{
				this.ActiveTextAreaControl.Caret.OnEndUpdate();
			}
		}

		protected virtual void InitializeTextAreaControl(TextAreaControl newControl)
		{
		}

		private float MeasurePrintingHeight(Graphics g, LineSegment line, float maxWidth)
		{
			float single = 0f;
			float single1 = 0f;
			float height = this.Font.GetHeight(g);
			this.curTabIndent = 0f;
			FontContainer fontContainer = base.TextEditorProperties.FontContainer;
			foreach (TextWord word in line.Words)
			{
				switch (word.Type)
				{
					case TextWordType.Word:
					{
						SizeF sizeF = g.MeasureString(word.Word, word.GetFont(fontContainer), new SizeF(maxWidth, height * 100f), this.printingStringFormat);
						this.Advance(ref single, ref single1, maxWidth, sizeF.Width, height);
						continue;
					}
					case TextWordType.Space:
					{
						this.Advance(ref single, ref single1, maxWidth, (float)this.primaryTextArea.TextArea.TextView.SpaceWidth, height);
						continue;
					}
					case TextWordType.Tab:
					{
						this.Advance(ref single, ref single1, maxWidth, (float)(base.TabIndent * this.primaryTextArea.TextArea.TextView.WideSpaceWidth), height);
						continue;
					}
					default:
					{
						continue;
					}
				}
			}
			return single1 + height;
		}

		public override void OptionsChanged()
		{
			this.primaryTextArea.OptionsChanged();
			if (this.secondaryTextArea != null)
			{
				this.secondaryTextArea.OptionsChanged();
			}
		}

		private void PrintPage(object sender, PrintPageEventArgs ev)
		{
			Graphics graphics = ev.Graphics;
			float top = (float)ev.MarginBounds.Top;
			while (this.curLineNr < base.Document.TotalNumberOfLines)
			{
				LineSegment lineSegment = base.Document.GetLineSegment(this.curLineNr);
				if (lineSegment.Words != null)
				{
					Rectangle marginBounds = ev.MarginBounds;
					float single = this.MeasurePrintingHeight(graphics, lineSegment, (float)marginBounds.Width);
					if (single + top > (float)ev.MarginBounds.Bottom)
					{
						break;
					}
					this.DrawLine(graphics, lineSegment, top, ev.MarginBounds);
					top += single;
				}
				this.curLineNr++;
			}
			ev.HasMorePages = this.curLineNr < base.Document.TotalNumberOfLines;
		}

		public void Redo()
		{
			if (base.Document.ReadOnly)
			{
				return;
			}
			if (base.Document.UndoStack.CanRedo)
			{
				this.BeginUpdate();
				base.Document.UndoStack.Redo();
				base.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
				this.primaryTextArea.TextArea.UpdateMatchingBracket();
				if (this.secondaryTextArea != null)
				{
					this.secondaryTextArea.TextArea.UpdateMatchingBracket();
				}
				this.EndUpdate();
			}
		}

		protected void SetActiveTextAreaControl(TextAreaControl value)
		{
			if (this.activeTextAreaControl != value)
			{
				this.activeTextAreaControl = value;
				if (this.ActiveTextAreaControlChanged != null)
				{
					this.ActiveTextAreaControlChanged(this, EventArgs.Empty);
				}
			}
		}

		public virtual void SetHighlighting(string name)
		{
			base.Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy(name);
		}

		public void Split()
		{
			if (this.secondaryTextArea != null)
			{
				this.SetActiveTextAreaControl(this.primaryTextArea);
				this.textAreaPanel.Controls.Remove(this.secondaryTextArea);
				this.textAreaPanel.Controls.Remove(this.textAreaSplitter);
				this.secondaryTextArea.Dispose();
				this.textAreaSplitter.Dispose();
				this.secondaryTextArea = null;
				this.textAreaSplitter = null;
				return;
			}
			this.secondaryTextArea = new TextAreaControl(this)
			{
				Dock = DockStyle.Bottom,
				Height = base.Height / 2
			};
			this.secondaryTextArea.TextArea.GotFocus += new EventHandler((object argument0, EventArgs argument1) => this.SetActiveTextAreaControl(this.secondaryTextArea));
			this.textAreaSplitter = new Splitter()
			{
				BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle,
				Height = 8,
				Dock = DockStyle.Bottom
			};
			this.textAreaPanel.Controls.Add(this.textAreaSplitter);
			this.textAreaPanel.Controls.Add(this.secondaryTextArea);
			this.InitializeTextAreaControl(this.secondaryTextArea);
			this.secondaryTextArea.OptionsChanged();
		}

		public void Undo()
		{
			if (base.Document.ReadOnly)
			{
				return;
			}
			if (base.Document.UndoStack.CanUndo)
			{
				this.BeginUpdate();
				base.Document.UndoStack.Undo();
				base.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
				this.primaryTextArea.TextArea.UpdateMatchingBracket();
				if (this.secondaryTextArea != null)
				{
					this.secondaryTextArea.TextArea.UpdateMatchingBracket();
				}
				this.EndUpdate();
			}
		}

		public event EventHandler ActiveTextAreaControlChanged;
	}
}