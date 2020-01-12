using ICSharpCode.TextEditor.Document;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor
{
	public class TextView : AbstractMargin, IDisposable
	{
		private const int additionalFoldTextSize = 1;

		private const int MaximumWordLength = 1000;

		private const int MaximumCacheSize = 2000;

		private const TextFormatFlags textFormatFlags = TextFormatFlags.NoPrefix | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.NoPadding;

		private const int MinTabWidth = 4;

		private int fontHeight;

		private ICSharpCode.TextEditor.Highlight highlight;

		private int physicalColumn;

		private int spaceWidth;

		private int wideSpaceWidth;

		private Font lastFont;

		private List<TextView.MarkerToDraw> markersToDraw = new List<TextView.MarkerToDraw>();

		private Dictionary<TextView.WordFontPair, int> measureCache = new Dictionary<TextView.WordFontPair, int>();

		private Dictionary<Font, Dictionary<char, int>> fontBoundCharWidth = new Dictionary<Font, Dictionary<char, int>>();

		public int FirstPhysicalLine
		{
			get
			{
				return this.textArea.VirtualTop.Y / this.fontHeight;
			}
		}

		public int FirstVisibleLine
		{
			get
			{
				IDocument document = this.textArea.Document;
				Point virtualTop = this.textArea.VirtualTop;
				return document.GetFirstLogicalLine(virtualTop.Y / this.fontHeight);
			}
			set
			{
				if (this.FirstVisibleLine != value)
				{
					ICSharpCode.TextEditor.TextArea point = this.textArea;
					Point virtualTop = this.textArea.VirtualTop;
					point.VirtualTop = new Point(virtualTop.X, this.textArea.Document.GetVisibleLine(value) * this.fontHeight);
				}
			}
		}

		public int FontHeight
		{
			get
			{
				return this.fontHeight;
			}
		}

		public ICSharpCode.TextEditor.Highlight Highlight
		{
			get
			{
				return this.highlight;
			}
			set
			{
				this.highlight = value;
			}
		}

		public int LineHeightRemainder
		{
			get
			{
				return this.textArea.VirtualTop.Y % this.fontHeight;
			}
		}

		public int SpaceWidth
		{
			get
			{
				return this.spaceWidth;
			}
		}

		public int VisibleColumnCount
		{
			get
			{
				return base.DrawingPosition.Width / this.WideSpaceWidth - 1;
			}
		}

		public int VisibleLineCount
		{
			get
			{
				return 1 + base.DrawingPosition.Height / this.fontHeight;
			}
		}

		public int VisibleLineDrawingRemainder
		{
			get
			{
				return this.textArea.VirtualTop.Y % this.fontHeight;
			}
		}

		public int WideSpaceWidth
		{
			get
			{
				return this.wideSpaceWidth;
			}
		}

		public TextView(ICSharpCode.TextEditor.TextArea textArea) : base(textArea)
		{
			base.Cursor = Cursors.IBeam;
			this.OptionsChanged();
		}

		private float CountColumns(ref int column, int start, int end, int logicalLine, Graphics g)
		{
			if (start > end)
			{
				throw new ArgumentException("start > end");
			}
			if (start == end)
			{
				return 0f;
			}
			float spaceWidth = (float)this.SpaceWidth;
			float wideSpaceWidth = 0f;
			int tabIndent = base.Document.TextEditorProperties.TabIndent;
			LineSegment lineSegment = base.Document.GetLineSegment(logicalLine);
			List<TextWord> words = lineSegment.Words;
			if (words == null)
			{
				return 0f;
			}
			int count = words.Count;
			int length = 0;
			FontContainer fontContainer = base.TextEditorProperties.FontContainer;
			for (int i = 0; i < count; i++)
			{
				TextWord item = words[i];
				if (length >= end)
				{
					break;
				}
				if (length + item.Length >= start)
				{
					switch (item.Type)
					{
						case TextWordType.Word:
						{
							int num = Math.Max(length, start);
							int num1 = Math.Min(length + item.Length, end) - num;
							string text = base.Document.GetText(lineSegment.Offset + num, num1);
							wideSpaceWidth = wideSpaceWidth + (float)this.MeasureStringWidth(g, text, item.GetFont(fontContainer) ?? fontContainer.RegularFont);
							break;
						}
						case TextWordType.Space:
						{
							wideSpaceWidth += spaceWidth;
							break;
						}
						case TextWordType.Tab:
						{
							wideSpaceWidth = (float)((int)((wideSpaceWidth + 4f) / (float)tabIndent / (float)this.WideSpaceWidth) * tabIndent * this.WideSpaceWidth);
							wideSpaceWidth += (float)(tabIndent * this.WideSpaceWidth);
							break;
						}
					}
				}
				length += item.Length;
			}
			for (int j = lineSegment.Length; j < end; j++)
			{
				wideSpaceWidth += (float)this.WideSpaceWidth;
			}
			column += (int)((wideSpaceWidth + 1f) / (float)this.WideSpaceWidth);
			return wideSpaceWidth;
		}

		public void Dispose()
		{
			this.measureCache.Clear();
		}

		private void DrawBracketHighlight(Graphics g, Rectangle rect)
		{
			g.FillRectangle(BrushRegistry.GetBrush(Color.FromArgb(50, 0, 0, 255)), rect);
			g.DrawRectangle(Pens.Blue, rect);
		}

		private int DrawDocumentWord(Graphics g, string word, Point position, Font font, Color foreColor, Brush backBrush)
		{
			if (word == null || word.Length == 0)
			{
				return 0;
			}
			if (word.Length <= 1000)
			{
				int num = this.MeasureStringWidth(g, word, font);
				g.FillRectangle(backBrush, new RectangleF((float)position.X, (float)position.Y, (float)(num + 1), (float)this.FontHeight));
				this.DrawString(g, word, font, foreColor, position.X, position.Y);
				return num;
			}
			int num1 = 0;
			for (int i = 0; i < word.Length; i += 1000)
			{
				Point x = position;
				x.X = x.X + num1;
				if (i + 1000 >= word.Length)
				{
					num1 += this.DrawDocumentWord(g, word.Substring(i, word.Length - i), x, font, foreColor, backBrush);
				}
				else
				{
					num1 += this.DrawDocumentWord(g, word.Substring(i, 1000), x, font, foreColor, backBrush);
				}
			}
			return num1;
		}

		private int DrawEOLMarker(Graphics g, Color color, Brush backBrush, int x, int y)
		{
			HighlightColor colorFor = this.textArea.Document.HighlightingStrategy.GetColorFor("EOLMarkers");
			int width = this.GetWidth('\u00B6', colorFor.GetFont(base.TextEditorProperties.FontContainer));
			g.FillRectangle(backBrush, new RectangleF((float)x, (float)y, (float)width, (float)this.fontHeight));
			this.DrawString(g, "¶", colorFor.GetFont(base.TextEditorProperties.FontContainer), color, x, y);
			return width;
		}

		private void DrawInvalidLineMarker(Graphics g, int x, int y)
		{
			HighlightColor colorFor = this.textArea.Document.HighlightingStrategy.GetColorFor("InvalidLines");
			this.DrawString(g, "~", colorFor.GetFont(base.TextEditorProperties.FontContainer), colorFor.Color, x, y);
		}

		private bool DrawLineMarkerAtLine(int lineNumber)
		{
			if (lineNumber != this.textArea.Caret.Line)
			{
				return false;
			}
			return this.textArea.MotherTextAreaControl.TextEditorProperties.LineViewerStyle == LineViewerStyle.FullRow;
		}

		private void DrawMarker(Graphics g, TextMarker marker, RectangleF drawingRect)
		{
			this.markersToDraw.Add(new TextView.MarkerToDraw(marker, drawingRect));
		}

		private void DrawMarkerDraw(Graphics g)
		{
			foreach (TextView.MarkerToDraw markerToDraw in this.markersToDraw)
			{
				TextMarker textMarker = markerToDraw.marker;
				RectangleF rectangleF = markerToDraw.drawingRect;
				float bottom = rectangleF.Bottom - 1f;
				switch (textMarker.TextMarkerType)
				{
					case TextMarkerType.SolidBlock:
					{
						g.FillRectangle(BrushRegistry.GetBrush(textMarker.Color), rectangleF);
						continue;
					}
					case TextMarkerType.Underlined:
					{
						g.DrawLine(BrushRegistry.GetPen(textMarker.Color), rectangleF.X, bottom, rectangleF.Right, bottom);
						continue;
					}
					case TextMarkerType.WaveLine:
					{
						int x = (int)rectangleF.X % 6;
						for (float i = (float)((int)rectangleF.X - x); i < rectangleF.Right; i += 6f)
						{
							g.DrawLine(BrushRegistry.GetPen(textMarker.Color), i, bottom + 3f - 4f, i + 3f, bottom + 1f - 4f);
							if (i + 3f < rectangleF.Right)
							{
								g.DrawLine(BrushRegistry.GetPen(textMarker.Color), i + 3f, bottom + 1f - 4f, i + 6f, bottom + 3f - 4f);
							}
						}
						continue;
					}
					default:
					{
						continue;
					}
				}
			}
			this.markersToDraw.Clear();
		}

		private void DrawSpaceMarker(Graphics g, Color color, int x, int y)
		{
			HighlightColor colorFor = this.textArea.Document.HighlightingStrategy.GetColorFor("SpaceMarkers");
			this.DrawString(g, "·", colorFor.GetFont(base.TextEditorProperties.FontContainer), color, x, y);
		}

		private void DrawString(Graphics g, string text, Font font, Color color, int x, int y)
		{
			TextRenderer.DrawText(g, text, font, new Point(x, y), color, TextFormatFlags.NoPrefix | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.NoPadding);
		}

		private void DrawTabMarker(Graphics g, Color color, int x, int y)
		{
			HighlightColor colorFor = this.textArea.Document.HighlightingStrategy.GetColorFor("TabMarkers");
			this.DrawString(g, "»", colorFor.GetFont(base.TextEditorProperties.FontContainer), color, x, y);
		}

		private void DrawVerticalRuler(Graphics g, Rectangle lineRectangle)
		{
			int wideSpaceWidth = this.WideSpaceWidth * base.TextEditorProperties.VerticalRulerRow - this.textArea.VirtualTop.X;
			if (wideSpaceWidth <= 0)
			{
				return;
			}
			HighlightColor colorFor = this.textArea.Document.HighlightingStrategy.GetColorFor("VRuler");
			g.DrawLine(BrushRegistry.GetPen(colorFor.Color), this.drawingPosition.Left + wideSpaceWidth, lineRectangle.Top, this.drawingPosition.Left + wideSpaceWidth, lineRectangle.Bottom);
		}

		private FoldMarker FindNextFoldedFoldingOnLineAfterColumn(int lineNumber, int column)
		{
			List<FoldMarker> foldedFoldingsWithStartAfterColumn = base.Document.FoldingManager.GetFoldedFoldingsWithStartAfterColumn(lineNumber, column);
			if (foldedFoldingsWithStartAfterColumn.Count == 0)
			{
				return null;
			}
			return foldedFoldingsWithStartAfterColumn[0];
		}

		private Brush GetBgColorBrush(int lineNumber)
		{
			if (this.DrawLineMarkerAtLine(lineNumber))
			{
				HighlightColor colorFor = this.textArea.Document.HighlightingStrategy.GetColorFor("CaretMarker");
				return BrushRegistry.GetBrush(colorFor.Color);
			}
			HighlightColor highlightColor = this.textArea.Document.HighlightingStrategy.GetColorFor("Default");
			return BrushRegistry.GetBrush(highlightColor.BackgroundColor);
		}

		public int GetDrawingXPos(int logicalLine, int logicalColumn)
		{
			int i;
			float single;
			List<FoldMarker> topLevelFoldedFoldings = base.Document.FoldingManager.GetTopLevelFoldedFoldings();
			FoldMarker item = null;
			for (i = topLevelFoldedFoldings.Count - 1; i >= 0; i--)
			{
				item = topLevelFoldedFoldings[i];
				if (item.StartLine < logicalLine || item.StartLine == logicalLine && item.StartColumn < logicalColumn)
				{
					break;
				}
				FoldMarker foldMarker = topLevelFoldedFoldings[i / 2];
				if (foldMarker.StartLine > logicalLine || foldMarker.StartLine == logicalLine && foldMarker.StartColumn >= logicalColumn)
				{
					i /= 2;
				}
			}
			int num = 0;
			int num1 = 0;
			int length = 0;
			int tabIndent = base.Document.TextEditorProperties.TabIndent;
			Graphics graphic = this.textArea.CreateGraphics();
			if (item == null || item.StartLine >= logicalLine && (item.StartLine != logicalLine || item.StartColumn >= logicalColumn))
			{
				single = this.CountColumns(ref length, 0, logicalColumn, logicalLine, graphic);
				return (int)(single - (float)this.textArea.VirtualTop.X);
			}
			if (item.EndLine > logicalLine || item.EndLine == logicalLine && item.EndColumn > logicalColumn)
			{
				logicalColumn = item.StartColumn;
				logicalLine = item.StartLine;
				i--;
			}
			num = i;
			while (i >= 0)
			{
				item = topLevelFoldedFoldings[i];
				if (item.EndLine < logicalLine)
				{
					break;
				}
				i--;
			}
			num1 = i + 1;
			if (num < num1)
			{
				single = this.CountColumns(ref length, 0, logicalColumn, logicalLine, graphic);
				return (int)(single - (float)this.textArea.VirtualTop.X);
			}
			int endColumn = 0;
			single = 0f;
			for (i = num1; i <= num; i++)
			{
				item = topLevelFoldedFoldings[i];
				single += this.CountColumns(ref length, endColumn, item.StartColumn, item.StartLine, graphic);
				endColumn = item.EndColumn;
				length += item.FoldText.Length;
				single += 1f;
				single += (float)this.MeasureStringWidth(graphic, item.FoldText, base.TextEditorProperties.FontContainer.RegularFont);
			}
			single += this.CountColumns(ref length, endColumn, logicalColumn, logicalLine, graphic);
			graphic.Dispose();
			return (int)(single - (float)this.textArea.VirtualTop.X);
		}

		public FoldMarker GetFoldMarkerFromPosition(int visualPosX, int visualPosY)
		{
			FoldMarker foldMarker;
			this.GetLogicalColumn(this.GetLogicalLine(visualPosY), visualPosX, out foldMarker);
			return foldMarker;
		}

		private static int GetFontHeight(Font font)
		{
			int height = TextRenderer.MeasureText("_", font).Height;
			int num = (int)Math.Ceiling((double)font.GetHeight());
			return Math.Max(height, num) + 1;
		}

		internal TextLocation GetLogicalColumn(int lineNumber, int visualPosX, out FoldMarker inFoldMarker)
		{
			int logicalColumnInternal;
			FoldMarker foldMarker;
			int num;
			TextLocation textLocation = new TextLocation();
			visualPosX += this.textArea.VirtualTop.X;
			inFoldMarker = null;
			if (lineNumber >= base.Document.TotalNumberOfLines)
			{
				return new TextLocation(visualPosX / this.WideSpaceWidth, lineNumber);
			}
			if (visualPosX <= 0)
			{
				return new TextLocation(0, lineNumber);
			}
			int endColumn = 0;
			int num1 = 0;
			using (Graphics graphic = this.textArea.CreateGraphics())
			{
				while (true)
				{
					LineSegment lineSegment = base.Document.GetLineSegment(lineNumber);
					foldMarker = this.FindNextFoldedFoldingOnLineAfterColumn(lineNumber, endColumn - 1);
					int num2 = (foldMarker != null ? foldMarker.StartColumn : 2147483647);
					logicalColumnInternal = this.GetLogicalColumnInternal(graphic, lineSegment, endColumn, num2, ref num1, visualPosX);
					if (logicalColumnInternal < num2)
					{
						break;
					}
					lineNumber = foldMarker.EndLine;
					endColumn = foldMarker.EndColumn;
					num = num1 + 1 + this.MeasureStringWidth(graphic, foldMarker.FoldText, base.TextEditorProperties.FontContainer.RegularFont);
					if (num >= visualPosX)
					{
						goto Label1;
					}
					num1 = num;
				}
				return new TextLocation(logicalColumnInternal, lineNumber);
			}
			return textLocation;
		Label1:
			inFoldMarker = foldMarker;
			if (!TextView.IsNearerToAThanB(visualPosX, num1, num))
			{
				textLocation = new TextLocation(foldMarker.EndColumn, foldMarker.EndLine);
				return textLocation;
			}
			else
			{
				textLocation = new TextLocation(foldMarker.StartColumn, foldMarker.StartLine);
				return textLocation;
			}
		}

		private int GetLogicalColumnInternal(Graphics g, LineSegment line, int start, int end, ref int drawingPos, int targetVisualPosX)
		{
			int num;
			if (start == end)
			{
				return end;
			}
			int tabIndent = base.Document.TextEditorProperties.TabIndent;
			FontContainer fontContainer = base.TextEditorProperties.FontContainer;
			List<TextWord> words = line.Words;
			if (words == null)
			{
				return 0;
			}
			int length = 0;
			for (int i = 0; i < words.Count; i++)
			{
				TextWord item = words[i];
				if (length >= end)
				{
					return length;
				}
				if (length + item.Length >= start)
				{
					switch (item.Type)
					{
						case TextWordType.Word:
						{
							int num1 = Math.Max(length, start);
							int num2 = Math.Min(length + item.Length, end) - num1;
							string text = base.Document.GetText(line.Offset + num1, num2);
							Font font = item.GetFont(fontContainer) ?? fontContainer.RegularFont;
							num = drawingPos + this.MeasureStringWidth(g, text, font);
							if (num < targetVisualPosX)
							{
								break;
							}
							for (int j = 0; j < text.Length; j++)
							{
								char chr = text[j];
								num = drawingPos + this.MeasureStringWidth(g, chr.ToString(), font);
								if (num >= targetVisualPosX)
								{
									if (TextView.IsNearerToAThanB(targetVisualPosX, drawingPos, num))
									{
										return num1 + j;
									}
									return num1 + j + 1;
								}
								drawingPos = num;
							}
							return num1 + text.Length;
						}
						case TextWordType.Space:
						{
							num = drawingPos + this.spaceWidth;
							if (num < targetVisualPosX)
							{
								break;
							}
							if (TextView.IsNearerToAThanB(targetVisualPosX, drawingPos, num))
							{
								return length;
							}
							return length + 1;
						}
						case TextWordType.Tab:
						{
							drawingPos = (drawingPos + 4) / tabIndent / this.WideSpaceWidth * tabIndent * this.WideSpaceWidth;
							num = drawingPos + tabIndent * this.WideSpaceWidth;
							if (num < targetVisualPosX)
							{
								break;
							}
							if (TextView.IsNearerToAThanB(targetVisualPosX, drawingPos, num))
							{
								return length;
							}
							return length + 1;
						}
						default:
						{
							throw new NotSupportedException();
						}
					}
					drawingPos = num;
				}
				length += item.Length;
			}
			return length;
		}

		public int GetLogicalLine(int visualPosY)
		{
			Point virtualTop = this.textArea.VirtualTop;
			int num = Math.Max(0, (visualPosY + virtualTop.Y) / this.fontHeight);
			return base.Document.GetFirstLogicalLine(num);
		}

		public TextLocation GetLogicalPosition(Point mousePosition)
		{
			FoldMarker foldMarker;
			return this.GetLogicalColumn(this.GetLogicalLine(mousePosition.Y), mousePosition.X, out foldMarker);
		}

		public TextLocation GetLogicalPosition(int visualPosX, int visualPosY)
		{
			FoldMarker foldMarker;
			return this.GetLogicalColumn(this.GetLogicalLine(visualPosY), visualPosX, out foldMarker);
		}

		private Brush GetMarkerBrushAt(int offset, int length, ref Color foreColor, out IList<TextMarker> markers)
		{
			Brush brush;
			markers = base.Document.MarkerStrategy.GetMarkers(offset, length);
			using (IEnumerator<TextMarker> enumerator = markers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					TextMarker current = enumerator.Current;
					if (current.TextMarkerType != TextMarkerType.SolidBlock)
					{
						continue;
					}
					if (current.OverrideForeColor)
					{
						foreColor = current.ForeColor;
					}
					brush = BrushRegistry.GetBrush(current.Color);
					return brush;
				}
				return null;
			}
			return brush;
		}

		public int GetVisualColumn(int logicalLine, int logicalColumn)
		{
			int num = 0;
			using (Graphics graphic = this.textArea.CreateGraphics())
			{
				this.CountColumns(ref num, 0, logicalColumn, logicalLine, graphic);
			}
			return num;
		}

		public int GetVisualColumnFast(LineSegment line, int logicalColumn)
		{
			char chr;
			int offset = line.Offset;
			int tabIndent = base.Document.TextEditorProperties.TabIndent;
			int num = 0;
			for (int i = 0; i < logicalColumn; i++)
			{
				chr = (i < line.Length ? base.Document.GetCharAt(offset + i) : ' ');
				if (chr != '\t')
				{
					num++;
				}
				else
				{
					num += tabIndent;
					num = num / tabIndent * tabIndent;
				}
			}
			return num;
		}

		public int GetWidth(char ch, Font font)
		{
			int width;
			if (!this.fontBoundCharWidth.ContainsKey(font))
			{
				this.fontBoundCharWidth.Add(font, new Dictionary<char, int>());
			}
			if (this.fontBoundCharWidth[font].ContainsKey(ch))
			{
				return this.fontBoundCharWidth[font][ch];
			}
			using (Graphics graphic = this.textArea.CreateGraphics())
			{
				width = this.GetWidth(graphic, ch, font);
			}
			return width;
		}

		public int GetWidth(Graphics g, char ch, Font font)
		{
			if (!this.fontBoundCharWidth.ContainsKey(font))
			{
				this.fontBoundCharWidth.Add(font, new Dictionary<char, int>());
			}
			if (!this.fontBoundCharWidth[font].ContainsKey(ch))
			{
				this.fontBoundCharWidth[font].Add(ch, this.MeasureStringWidth(g, ch.ToString(), font));
			}
			return this.fontBoundCharWidth[font][ch];
		}

		private static bool IsNearerToAThanB(int num, int a, int b)
		{
			return Math.Abs(a - num) < Math.Abs(b - num);
		}

		private int MeasureStringWidth(Graphics g, string word, Font font)
		{
			int width;
			if (word == null || word.Length == 0)
			{
				return 0;
			}
			if (word.Length <= 1000)
			{
				if (this.measureCache.TryGetValue(new TextView.WordFontPair(word, font), out width))
				{
					return width;
				}
				if (this.measureCache.Count > 2000)
				{
					this.measureCache.Clear();
				}
				System.Drawing.Size size = TextRenderer.MeasureText(g, word, font, new System.Drawing.Size(32767, 32767), TextFormatFlags.NoPrefix | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.NoPadding);
				width = size.Width;
				this.measureCache.Add(new TextView.WordFontPair(word, font), width);
				return width;
			}
			width = 0;
			for (int i = 0; i < word.Length; i += 1000)
			{
				if (i + 1000 >= word.Length)
				{
					width += this.MeasureStringWidth(g, word.Substring(i, word.Length - i), font);
				}
				else
				{
					width += this.MeasureStringWidth(g, word.Substring(i, 1000), font);
				}
			}
			return width;
		}

		public void OptionsChanged()
		{
			this.lastFont = base.TextEditorProperties.FontContainer.RegularFont;
			this.fontHeight = TextView.GetFontHeight(this.lastFont);
			this.spaceWidth = Math.Max(this.GetWidth(' ', this.lastFont), 1);
			this.wideSpaceWidth = Math.Max(this.spaceWidth, this.GetWidth('x', this.lastFont));
		}

		public override void Paint(Graphics g, Rectangle rect)
		{
			if (rect.Width <= 0 || rect.Height <= 0)
			{
				return;
			}
			if (this.lastFont != base.TextEditorProperties.FontContainer.RegularFont)
			{
				this.OptionsChanged();
				this.textArea.Invalidate();
			}
			int x = this.textArea.VirtualTop.X;
			if (x > 0)
			{
				g.SetClip(base.DrawingPosition);
			}
			for (int i = 0; i < (base.DrawingPosition.Height + this.VisibleLineDrawingRemainder) / this.fontHeight + 1; i++)
			{
				Rectangle drawingPosition = base.DrawingPosition;
				Rectangle rectangle = base.DrawingPosition;
				int top = rectangle.Top + i * this.fontHeight - this.VisibleLineDrawingRemainder;
				Rectangle drawingPosition1 = base.DrawingPosition;
				Rectangle rectangle1 = new Rectangle(drawingPosition.X - x, top, drawingPosition1.Width + x, this.fontHeight);
				if (rect.IntersectsWith(rectangle1))
				{
					this.textArea.Document.GetVisibleLine(this.FirstVisibleLine);
					int firstLogicalLine = this.textArea.Document.GetFirstLogicalLine(this.textArea.Document.GetVisibleLine(this.FirstVisibleLine) + i);
					this.PaintDocumentLine(g, firstLogicalLine, rectangle1);
				}
			}
			this.DrawMarkerDraw(g);
			if (x > 0)
			{
				g.ResetClip();
			}
			this.textArea.Caret.PaintCaret(g);
		}

		private void PaintDocumentLine(Graphics g, int lineNumber, Rectangle lineRectangle)
		{
			bool flag;
			Brush bgColorBrush = this.GetBgColorBrush(lineNumber);
			Brush brush = (this.textArea.Enabled ? bgColorBrush : SystemBrushes.InactiveBorder);
			if (lineNumber >= this.textArea.Document.TotalNumberOfLines)
			{
				g.FillRectangle(brush, lineRectangle);
				if (base.TextEditorProperties.ShowInvalidLines)
				{
					this.DrawInvalidLineMarker(g, lineRectangle.Left, lineRectangle.Top);
				}
				if (base.TextEditorProperties.ShowVerticalRuler)
				{
					this.DrawVerticalRuler(g, lineRectangle);
				}
				return;
			}
			int x = lineRectangle.X;
			int endColumn = 0;
			this.physicalColumn = 0;
			if (!base.TextEditorProperties.EnableFolding)
			{
				x = this.PaintLinePart(g, lineNumber, 0, this.textArea.Document.GetLineSegment(lineNumber).Length, lineRectangle, x);
			}
			else
			{
				while (true)
				{
					List<FoldMarker> foldedFoldingsWithStartAfterColumn = this.textArea.Document.FoldingManager.GetFoldedFoldingsWithStartAfterColumn(lineNumber, endColumn - 1);
					if (foldedFoldingsWithStartAfterColumn == null || foldedFoldingsWithStartAfterColumn.Count <= 0)
					{
						break;
					}
					FoldMarker item = foldedFoldingsWithStartAfterColumn[0];
					foreach (FoldMarker foldMarker in foldedFoldingsWithStartAfterColumn)
					{
						if (foldMarker.StartColumn >= item.StartColumn)
						{
							continue;
						}
						item = foldMarker;
					}
					foldedFoldingsWithStartAfterColumn.Clear();
					x = this.PaintLinePart(g, lineNumber, endColumn, item.StartColumn, lineRectangle, x);
					endColumn = item.EndColumn;
					lineNumber = item.EndLine;
					if (lineNumber >= this.textArea.Document.TotalNumberOfLines)
					{
						goto Label0;
					}
					ColumnRange selectionAtLine = this.textArea.SelectionManager.GetSelectionAtLine(lineNumber);
					if (ColumnRange.WholeColumn.Equals(selectionAtLine))
					{
						flag = true;
					}
					else
					{
						flag = (item.StartColumn < selectionAtLine.StartColumn ? false : item.EndColumn <= selectionAtLine.EndColumn);
					}
					bool flag1 = flag;
					x = this.PaintFoldingText(g, lineNumber, x, lineRectangle, item.FoldText, flag1);
				}
				if (lineNumber < this.textArea.Document.TotalNumberOfLines)
				{
					x = this.PaintLinePart(g, lineNumber, endColumn, this.textArea.Document.GetLineSegment(lineNumber).Length, lineRectangle, x);
				}
			}
		Label0:
			if (lineNumber < this.textArea.Document.TotalNumberOfLines)
			{
				ColumnRange columnRange = this.textArea.SelectionManager.GetSelectionAtLine(lineNumber);
				LineSegment lineSegment = this.textArea.Document.GetLineSegment(lineNumber);
				HighlightColor colorFor = this.textArea.Document.HighlightingStrategy.GetColorFor("Selection");
				bool flag2 = (columnRange.EndColumn > lineSegment.Length ? true : ColumnRange.WholeColumn.Equals(columnRange));
				if (base.TextEditorProperties.ShowEOLMarker)
				{
					HighlightColor highlightColor = this.textArea.Document.HighlightingStrategy.GetColorFor("EOLMarkers");
					x = x + this.DrawEOLMarker(g, highlightColor.Color, (flag2 ? bgColorBrush : brush), x, lineRectangle.Y);
				}
				else if (flag2)
				{
					g.FillRectangle(BrushRegistry.GetBrush(colorFor.BackgroundColor), new RectangleF((float)x, (float)lineRectangle.Y, (float)this.WideSpaceWidth, (float)lineRectangle.Height));
					x += this.WideSpaceWidth;
				}
				Brush brush1 = (!flag2 || !base.TextEditorProperties.AllowCaretBeyondEOL ? brush : bgColorBrush);
				g.FillRectangle(brush1, new RectangleF((float)x, (float)lineRectangle.Y, (float)(lineRectangle.Width - x + lineRectangle.X), (float)lineRectangle.Height));
			}
			if (base.TextEditorProperties.ShowVerticalRuler)
			{
				this.DrawVerticalRuler(g, lineRectangle);
			}
		}

		private int PaintFoldingText(Graphics g, int lineNumber, int physicalXPos, Rectangle lineRectangle, string text, bool drawSelected)
		{
			HighlightColor colorFor = this.textArea.Document.HighlightingStrategy.GetColorFor("Selection");
			Brush brush = (drawSelected ? BrushRegistry.GetBrush(colorFor.BackgroundColor) : this.GetBgColorBrush(lineNumber));
			Brush brush1 = (this.textArea.Enabled ? brush : SystemBrushes.InactiveBorder);
			Font regularFont = this.textArea.TextEditorProperties.FontContainer.RegularFont;
			int num = this.MeasureStringWidth(g, text, regularFont) + 1;
			Rectangle rectangle = new Rectangle(physicalXPos, lineRectangle.Y, num, lineRectangle.Height - 1);
			g.FillRectangle(brush1, rectangle);
			this.physicalColumn += text.Length;
			this.DrawString(g, text, regularFont, (drawSelected ? colorFor.Color : Color.Gray), rectangle.X + 1, rectangle.Y);
			g.DrawRectangle(BrushRegistry.GetPen((drawSelected ? Color.DarkGray : Color.Gray)), rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
			return physicalXPos + num + 1;
		}

		private int PaintLinePart(Graphics g, int lineNumber, int startColumn, int endColumn, Rectangle lineRectangle, int physicalXPos)
		{
			IList<TextMarker> textMarkers;
			Color color;
			RectangleF rectangleF;
			IList<TextMarker> markers;
			bool flag = this.DrawLineMarkerAtLine(lineNumber);
			Brush brush = (this.textArea.Enabled ? this.GetBgColorBrush(lineNumber) : SystemBrushes.InactiveBorder);
			HighlightColor colorFor = this.textArea.Document.HighlightingStrategy.GetColorFor("Selection");
			ColumnRange selectionAtLine = this.textArea.SelectionManager.GetSelectionAtLine(lineNumber);
			HighlightColor highlightColor = this.textArea.Document.HighlightingStrategy.GetColorFor("TabMarkers");
			HighlightColor colorFor1 = this.textArea.Document.HighlightingStrategy.GetColorFor("SpaceMarkers");
			LineSegment lineSegment = this.textArea.Document.GetLineSegment(lineNumber);
			Brush brush1 = BrushRegistry.GetBrush(colorFor.BackgroundColor);
			if (lineSegment.Words == null)
			{
				return physicalXPos;
			}
			int length = 0;
			TextWord textWord = null;
			FontContainer fontContainer = base.TextEditorProperties.FontContainer;
			for (int i = 0; i < lineSegment.Words.Count; i++)
			{
				TextWord item = lineSegment.Words[i];
				if (length >= startColumn)
				{
					while (length < endColumn && physicalXPos < lineRectangle.Right)
					{
						int num = length + item.Length - 1;
						TextWordType type = item.Type;
						if (type != TextWordType.Space)
						{
							color = (type != TextWordType.Tab ? item.Color : highlightColor.Color);
						}
						else
						{
							color = colorFor1.Color;
						}
						Brush markerBrushAt = this.GetMarkerBrushAt(lineSegment.Offset + length, item.Length, ref color, out textMarkers);
						if (item.Length > 1)
						{
							int num1 = 2147483647;
							if (this.highlight != null)
							{
								if (this.highlight.OpenBrace.Y == lineNumber && this.highlight.OpenBrace.X >= length && this.highlight.OpenBrace.X <= num)
								{
									TextLocation openBrace = this.highlight.OpenBrace;
									num1 = Math.Min(num1, openBrace.X - length);
								}
								if (this.highlight.CloseBrace.Y == lineNumber && this.highlight.CloseBrace.X >= length && this.highlight.CloseBrace.X <= num)
								{
									TextLocation closeBrace = this.highlight.CloseBrace;
									num1 = Math.Min(num1, closeBrace.X - length);
								}
								if (num1 == 0)
								{
									num1 = 1;
								}
							}
							if (endColumn < num)
							{
								num1 = Math.Min(num1, endColumn - length);
							}
							if (selectionAtLine.StartColumn > length && selectionAtLine.StartColumn <= num)
							{
								num1 = Math.Min(num1, selectionAtLine.StartColumn - length);
							}
							else if (selectionAtLine.EndColumn > length && selectionAtLine.EndColumn <= num)
							{
								num1 = Math.Min(num1, selectionAtLine.EndColumn - length);
							}
							foreach (TextMarker textMarker in textMarkers)
							{
								int offset = textMarker.Offset - lineSegment.Offset;
								int endOffset = textMarker.EndOffset - lineSegment.Offset + 1;
								if (offset <= length || offset > num)
								{
									if (endOffset <= length || endOffset > num)
									{
										continue;
									}
									num1 = Math.Min(num1, endOffset - length);
								}
								else
								{
									num1 = Math.Min(num1, offset - length);
								}
							}
							if (num1 != 2147483647)
							{
								if (textWord != null)
								{
									throw new ApplicationException("split part invalid: first part cannot be splitted further");
								}
								textWord = TextWord.Split(ref item, num1);
								continue;
							}
						}
						if (ColumnRange.WholeColumn.Equals(selectionAtLine) || selectionAtLine.StartColumn <= length && selectionAtLine.EndColumn > num)
						{
							markerBrushAt = brush1;
							if (colorFor.HasForeground)
							{
								color = colorFor.Color;
							}
						}
						else if (flag)
						{
							markerBrushAt = brush;
						}
						if (markerBrushAt == null)
						{
							markerBrushAt = (item.SyntaxColor == null || !item.SyntaxColor.HasBackground ? brush : BrushRegistry.GetBrush(item.SyntaxColor.BackgroundColor));
						}
						if (item.Type == TextWordType.Space)
						{
							this.physicalColumn++;
							rectangleF = new RectangleF((float)physicalXPos, (float)lineRectangle.Y, (float)this.SpaceWidth, (float)lineRectangle.Height);
							g.FillRectangle(markerBrushAt, rectangleF);
							if (base.TextEditorProperties.ShowSpaces)
							{
								this.DrawSpaceMarker(g, color, physicalXPos, lineRectangle.Y);
							}
							physicalXPos += this.SpaceWidth;
						}
						else if (item.Type != TextWordType.Tab)
						{
							int num2 = this.DrawDocumentWord(g, item.Word, new Point(physicalXPos, lineRectangle.Y), item.GetFont(fontContainer), color, markerBrushAt);
							rectangleF = new RectangleF((float)physicalXPos, (float)lineRectangle.Y, (float)num2, (float)lineRectangle.Height);
							physicalXPos += num2;
						}
						else
						{
							this.physicalColumn += base.TextEditorProperties.TabIndent;
							this.physicalColumn = this.physicalColumn / base.TextEditorProperties.TabIndent * base.TextEditorProperties.TabIndent;
							int wideSpaceWidth = (physicalXPos + 4 - lineRectangle.X) / this.WideSpaceWidth / base.TextEditorProperties.TabIndent * this.WideSpaceWidth * base.TextEditorProperties.TabIndent + lineRectangle.X;
							wideSpaceWidth = wideSpaceWidth + this.WideSpaceWidth * base.TextEditorProperties.TabIndent;
							rectangleF = new RectangleF((float)physicalXPos, (float)lineRectangle.Y, (float)(wideSpaceWidth - physicalXPos), (float)lineRectangle.Height);
							g.FillRectangle(markerBrushAt, rectangleF);
							if (base.TextEditorProperties.ShowTabs)
							{
								this.DrawTabMarker(g, color, physicalXPos, lineRectangle.Y);
							}
							physicalXPos = wideSpaceWidth;
						}
						foreach (TextMarker textMarker1 in textMarkers)
						{
							if (textMarker1.TextMarkerType == TextMarkerType.SolidBlock)
							{
								continue;
							}
							this.DrawMarker(g, textMarker1, rectangleF);
						}
						if (this.highlight != null && (this.highlight.OpenBrace.Y == lineNumber && this.highlight.OpenBrace.X == length || this.highlight.CloseBrace.Y == lineNumber && this.highlight.CloseBrace.X == length))
						{
							this.DrawBracketHighlight(g, new Rectangle((int)rectangleF.X, lineRectangle.Y, (int)rectangleF.Width - 1, lineRectangle.Height - 1));
						}
						length += item.Length;
						if (textWord == null)
						{
                            goto label0;
						}
						item = textWord;
						textWord = null;
					}
					if (physicalXPos < lineRectangle.Right && endColumn >= lineSegment.Length)
					{
						markers = base.Document.MarkerStrategy.GetMarkers(lineSegment.Offset + lineSegment.Length);
						foreach (TextMarker marker in markers)
						{
							if (marker.TextMarkerType == TextMarkerType.SolidBlock)
							{
								continue;
							}
							this.DrawMarker(g, marker, new RectangleF((float)physicalXPos, (float)lineRectangle.Y, (float)this.WideSpaceWidth, (float)lineRectangle.Height));
						}
					}
					return physicalXPos;
				}
				else
				{
					length += item.Length;
				}
                label0:;
			}
			if (physicalXPos < lineRectangle.Right && endColumn >= lineSegment.Length)
			{
				markers = base.Document.MarkerStrategy.GetMarkers(lineSegment.Offset + lineSegment.Length);
				foreach (TextMarker marker1 in markers)
				{
					if (marker1.TextMarkerType == TextMarkerType.SolidBlock)
					{
						continue;
					}
					this.DrawMarker(g, marker1, new RectangleF((float)physicalXPos, (float)lineRectangle.Y, (float)this.WideSpaceWidth, (float)lineRectangle.Height));
				}
			}
			return physicalXPos;
		}

		private struct MarkerToDraw
		{
			internal TextMarker marker;

			internal RectangleF drawingRect;

			public MarkerToDraw(TextMarker marker, RectangleF drawingRect)
			{
				this.marker = marker;
				this.drawingRect = drawingRect;
			}
		}

		private struct WordFontPair
		{
			private string word;

			private Font font;

			public WordFontPair(string word, Font font)
			{
				this.word = word;
				this.font = font;
			}

			public override bool Equals(object obj)
			{
				TextView.WordFontPair wordFontPair = (TextView.WordFontPair)obj;
				if (!this.word.Equals(wordFontPair.word))
				{
					return false;
				}
				return this.font.Equals(wordFontPair.font);
			}

			public override int GetHashCode()
			{
				return this.word.GetHashCode() ^ this.font.GetHashCode();
			}
		}
	}
}