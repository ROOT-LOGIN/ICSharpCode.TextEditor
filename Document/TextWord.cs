using System;
using System.Drawing;

namespace ICSharpCode.TextEditor.Document
{
	public class TextWord
	{
		private HighlightColor color;

		private LineSegment line;

		private IDocument document;

		private int offset;

		private int length;

		private static TextWord spaceWord;

		private static TextWord tabWord;

		private bool hasDefaultColor;

		public bool Bold
		{
			get
			{
				if (this.color == null)
				{
					return false;
				}
				return this.color.Bold;
			}
		}

		public System.Drawing.Color Color
		{
			get
			{
				if (this.color == null)
				{
					return System.Drawing.Color.Black;
				}
				return this.color.Color;
			}
		}

		public bool HasDefaultColor
		{
			get
			{
				return this.hasDefaultColor;
			}
		}

		public virtual bool IsWhiteSpace
		{
			get
			{
				return false;
			}
		}

		public bool Italic
		{
			get
			{
				if (this.color == null)
				{
					return false;
				}
				return this.color.Italic;
			}
		}

		public int Length
		{
			get
			{
				return this.length;
			}
		}

		public int Offset
		{
			get
			{
				return this.offset;
			}
		}

		public static TextWord Space
		{
			get
			{
				return TextWord.spaceWord;
			}
		}

		public HighlightColor SyntaxColor
		{
			get
			{
				return this.color;
			}
			set
			{
				this.color = value;
			}
		}

		public static TextWord Tab
		{
			get
			{
				return TextWord.tabWord;
			}
		}

		public virtual TextWordType Type
		{
			get
			{
				return TextWordType.Word;
			}
		}

		public string Word
		{
			get
			{
				if (this.document == null)
				{
					return string.Empty;
				}
				return this.document.GetText(this.line.Offset + this.offset, this.length);
			}
		}

		static TextWord()
		{
			TextWord.spaceWord = new TextWord.SpaceTextWord();
			TextWord.tabWord = new TextWord.TabTextWord();
		}

		protected TextWord()
		{
		}

		public TextWord(IDocument document, LineSegment line, int offset, int length, HighlightColor color, bool hasDefaultColor)
		{
			this.document = document;
			this.line = line;
			this.offset = offset;
			this.length = length;
			this.color = color;
			this.hasDefaultColor = hasDefaultColor;
		}

		public virtual Font GetFont(FontContainer fontContainer)
		{
			return this.color.GetFont(fontContainer);
		}

		public static TextWord Split(ref TextWord word, int pos)
		{
			TextWord textWord = new TextWord(word.document, word.line, word.offset + pos, word.length - pos, word.color, word.hasDefaultColor);
			word = new TextWord(word.document, word.line, word.offset, pos, word.color, word.hasDefaultColor);
			return textWord;
		}

		public override string ToString()
		{
			object[] word = new object[] { "[TextWord: Word = ", this.Word, ", Color = ", this.Color, "]" };
			return string.Concat(word);
		}

		public sealed class SpaceTextWord : TextWord
		{
			public override bool IsWhiteSpace
			{
				get
				{
					return true;
				}
			}

			public override TextWordType Type
			{
				get
				{
					return TextWordType.Space;
				}
			}

			public SpaceTextWord()
			{
				this.length = 1;
			}

			public SpaceTextWord(HighlightColor color)
			{
				this.length = 1;
				base.SyntaxColor = color;
			}

			public override Font GetFont(FontContainer fontContainer)
			{
				return null;
			}
		}

		public sealed class TabTextWord : TextWord
		{
			public override bool IsWhiteSpace
			{
				get
				{
					return true;
				}
			}

			public override TextWordType Type
			{
				get
				{
					return TextWordType.Tab;
				}
			}

			public TabTextWord()
			{
				this.length = 1;
			}

			public TabTextWord(HighlightColor color)
			{
				this.length = 1;
				base.SyntaxColor = color;
			}

			public override Font GetFont(FontContainer fontContainer)
			{
				return null;
			}
		}
	}
}