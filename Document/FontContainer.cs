using System;
using System.Drawing;

namespace ICSharpCode.TextEditor.Document
{
	public class FontContainer
	{
		private Font defaultFont;

		private Font regularfont;

		private Font boldfont;

		private Font italicfont;

		private Font bolditalicfont;

		private static float twipsPerPixelY;

		public Font BoldFont
		{
			get
			{
				return this.boldfont;
			}
		}

		public Font BoldItalicFont
		{
			get
			{
				return this.bolditalicfont;
			}
		}

		public Font DefaultFont
		{
			get
			{
				return this.defaultFont;
			}
			set
			{
				float single = (float)Math.Round((double)(value.SizeInPoints * 20f / FontContainer.TwipsPerPixelY));
				this.defaultFont = value;
				this.regularfont = new Font(value.FontFamily, single * FontContainer.TwipsPerPixelY / 20f, FontStyle.Regular);
				this.boldfont = new Font(this.regularfont, FontStyle.Bold);
				this.italicfont = new Font(this.regularfont, FontStyle.Italic);
				this.bolditalicfont = new Font(this.regularfont, FontStyle.Bold | FontStyle.Italic);
			}
		}

		public Font ItalicFont
		{
			get
			{
				return this.italicfont;
			}
		}

		public Font RegularFont
		{
			get
			{
				return this.regularfont;
			}
		}

		public static float TwipsPerPixelY
		{
			get
			{
				if (FontContainer.twipsPerPixelY == 0f)
				{
					using (Bitmap bitmap = new Bitmap(1, 1))
					{
						using (Graphics graphic = Graphics.FromImage(bitmap))
						{
							FontContainer.twipsPerPixelY = 1440f / graphic.DpiY;
						}
					}
				}
				return FontContainer.twipsPerPixelY;
			}
		}

		public FontContainer(Font defaultFont)
		{
			this.DefaultFont = defaultFont;
		}

		public static Font ParseFont(string font)
		{
			char[] chrArray = new char[] { ',', '=' };
			string[] strArrays = font.Split(chrArray);
			return new Font(strArrays[1], float.Parse(strArrays[3]));
		}
	}
}