using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ICSharpCode.TextEditor
{
	public class DrawableLine
	{
		private static StringFormat sf;

		private List<DrawableLine.SimpleTextWord> words = new List<DrawableLine.SimpleTextWord>();

		private SizeF spaceSize;

		private Font monospacedFont;

		private Font boldMonospacedFont;

		public int LineLength
		{
			get
			{
				int length = 0;
				foreach (DrawableLine.SimpleTextWord word in this.words)
				{
					length += word.Word.Length;
				}
				return length;
			}
		}

		static DrawableLine()
		{
			DrawableLine.sf = (StringFormat)StringFormat.GenericTypographic.Clone();
		}

		public DrawableLine(IDocument document, LineSegment line, Font monospacedFont, Font boldMonospacedFont)
		{
			this.monospacedFont = monospacedFont;
			this.boldMonospacedFont = boldMonospacedFont;
			if (line.Words == null)
			{
				this.words.Add(new DrawableLine.SimpleTextWord(TextWordType.Word, document.GetText(line), false, Color.Black));
			}
			else
			{
				foreach (TextWord word in line.Words)
				{
					if (word.Type == TextWordType.Space)
					{
						this.words.Add(DrawableLine.SimpleTextWord.Space);
					}
					else if (word.Type != TextWordType.Tab)
					{
						this.words.Add(new DrawableLine.SimpleTextWord(TextWordType.Word, word.Word, word.Bold, word.Color));
					}
					else
					{
						this.words.Add(DrawableLine.SimpleTextWord.Tab);
					}
				}
			}
		}

		public static float DrawDocumentWord(Graphics g, string word, PointF position, Font font, Color foreColor)
		{
			if (word == null || word.Length == 0)
			{
				return 0f;
			}
			SizeF sizeF = g.MeasureString(word, font, 32768, DrawableLine.sf);
			g.DrawString(word, font, BrushRegistry.GetBrush(foreColor), position, DrawableLine.sf);
			return sizeF.Width;
		}

		public void DrawLine(Graphics g, ref float xPos, float xOffset, float yPos, Color c)
		{
			SizeF spaceSize = this.GetSpaceSize(g);
			foreach (DrawableLine.SimpleTextWord word in this.words)
			{
				switch (word.Type)
				{
					case TextWordType.Word:
					{
						xPos = xPos + DrawableLine.DrawDocumentWord(g, word.Word, new PointF(xPos + xOffset, yPos), (word.Bold ? this.boldMonospacedFont : this.monospacedFont), (c == Color.Empty ? word.Color : c));
						continue;
					}
					case TextWordType.Space:
					{
						xPos += spaceSize.Width;
						continue;
					}
					case TextWordType.Tab:
					{
						float width = spaceSize.Width * 4f;
						xPos += width;
						xPos = (float)((int)((xPos + 2f) / width)) * width;
						continue;
					}
					default:
					{
						continue;
					}
				}
			}
		}

		public void DrawLine(Graphics g, ref float xPos, float xOffset, float yPos)
		{
			this.DrawLine(g, ref xPos, xOffset, yPos, Color.Empty);
		}

		public SizeF GetSpaceSize(Graphics g)
		{
			if (this.spaceSize.IsEmpty)
			{
				this.spaceSize = g.MeasureString("-", this.boldMonospacedFont, new PointF(0f, 0f), DrawableLine.sf);
			}
			return this.spaceSize;
		}

		public float MeasureWidth(Graphics g, float xPos)
		{
			SizeF spaceSize = this.GetSpaceSize(g);
			foreach (DrawableLine.SimpleTextWord word in this.words)
			{
				switch (word.Type)
				{
					case TextWordType.Word:
					{
						if (word.Word == null || word.Word.Length <= 0)
						{
							continue;
						}
						float single = xPos;
						SizeF sizeF = g.MeasureString(word.Word, (word.Bold ? this.boldMonospacedFont : this.monospacedFont), 32768, DrawableLine.sf);
						xPos = single + sizeF.Width;
						continue;
					}
					case TextWordType.Space:
					{
						xPos += spaceSize.Width;
						continue;
					}
					case TextWordType.Tab:
					{
						float width = spaceSize.Width * 4f;
						xPos += width;
						xPos = (float)((int)((xPos + 2f) / width)) * width;
						continue;
					}
					default:
					{
						continue;
					}
				}
			}
			return xPos;
		}

		public void SetBold(int startIndex, int endIndex, bool bold)
		{
			if (startIndex < 0)
			{
				throw new ArgumentException("startIndex must be >= 0");
			}
			if (startIndex > endIndex)
			{
				throw new ArgumentException("startIndex must be <= endIndex");
			}
			if (startIndex == endIndex)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i < this.words.Count; i++)
			{
				DrawableLine.SimpleTextWord item = this.words[i];
				if (num >= endIndex)
				{
					return;
				}
				int length = num + item.Word.Length;
				if (startIndex <= num && endIndex >= length)
				{
					item.Bold = bold;
				}
				else if (startIndex <= num)
				{
					int num1 = endIndex - num;
					DrawableLine.SimpleTextWord simpleTextWord = new DrawableLine.SimpleTextWord(item.Type, item.Word.Substring(num1), item.Bold, item.Color);
					this.words.Insert(i + 1, simpleTextWord);
					item.Bold = bold;
					item.Word = item.Word.Substring(0, num1);
				}
				else if (startIndex < length)
				{
					int num2 = startIndex - num;
					DrawableLine.SimpleTextWord simpleTextWord1 = new DrawableLine.SimpleTextWord(item.Type, item.Word.Substring(num2), item.Bold, item.Color);
					this.words.Insert(i + 1, simpleTextWord1);
					item.Word = item.Word.Substring(0, num2);
				}
				num = length;
			}
		}

		private class SimpleTextWord
		{
			internal TextWordType Type;

			internal string Word;

			internal bool Bold;

			internal Color Color;

			internal readonly static DrawableLine.SimpleTextWord Space;

			internal readonly static DrawableLine.SimpleTextWord Tab;

			static SimpleTextWord()
			{
				DrawableLine.SimpleTextWord.Space = new DrawableLine.SimpleTextWord(TextWordType.Space, " ", false, Color.Black);
				DrawableLine.SimpleTextWord.Tab = new DrawableLine.SimpleTextWord(TextWordType.Tab, "\t", false, Color.Black);
			}

			public SimpleTextWord(TextWordType Type, string Word, bool Bold, System.Drawing.Color Color)
			{
				this.Type = Type;
				this.Word = Word;
				this.Bold = Bold;
				this.Color = Color;
			}
		}
	}
}