using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Xml;

namespace ICSharpCode.TextEditor.Document
{
	public class HighlightColor
	{
		private System.Drawing.Color color;

		private System.Drawing.Color backgroundcolor = System.Drawing.Color.WhiteSmoke;

		private bool bold;

		private bool italic;

		private bool hasForeground;

		private bool hasBackground;

		public System.Drawing.Color BackgroundColor
		{
			get
			{
				return this.backgroundcolor;
			}
		}

		public bool Bold
		{
			get
			{
				return this.bold;
			}
		}

		public System.Drawing.Color Color
		{
			get
			{
				return this.color;
			}
		}

		public bool HasBackground
		{
			get
			{
				return this.hasBackground;
			}
		}

		public bool HasForeground
		{
			get
			{
				return this.hasForeground;
			}
		}

		public bool Italic
		{
			get
			{
				return this.italic;
			}
		}

		public HighlightColor(XmlElement el)
		{
			if (el.Attributes["bold"] != null)
			{
				this.bold = bool.Parse(el.Attributes["bold"].InnerText);
			}
			if (el.Attributes["italic"] != null)
			{
				this.italic = bool.Parse(el.Attributes["italic"].InnerText);
			}
			if (el.Attributes["color"] == null)
			{
				this.color = System.Drawing.Color.Transparent;
			}
			else
			{
				string innerText = el.Attributes["color"].InnerText;
				if (innerText[0] == '#')
				{
					this.color = HighlightColor.ParseColor(innerText);
				}
				else if (!innerText.StartsWith("SystemColors."))
				{
					this.color = (System.Drawing.Color)this.Color.GetType().InvokeMember(innerText, BindingFlags.GetProperty, null, this.Color, new object[0]);
				}
				else
				{
					this.color = this.ParseColorString(innerText.Substring("SystemColors.".Length));
				}
				this.hasForeground = true;
			}
			if (el.Attributes["bgcolor"] != null)
			{
				string str = el.Attributes["bgcolor"].InnerText;
				if (str[0] == '#')
				{
					this.backgroundcolor = HighlightColor.ParseColor(str);
				}
				else if (!str.StartsWith("SystemColors."))
				{
					this.backgroundcolor = (System.Drawing.Color)this.Color.GetType().InvokeMember(str, BindingFlags.GetProperty, null, this.Color, new object[0]);
				}
				else
				{
					this.backgroundcolor = this.ParseColorString(str.Substring("SystemColors.".Length));
				}
				this.hasBackground = true;
			}
		}

		public HighlightColor(XmlElement el, HighlightColor defaultColor)
		{
			if (el.Attributes["bold"] == null)
			{
				this.bold = defaultColor.Bold;
			}
			else
			{
				this.bold = bool.Parse(el.Attributes["bold"].InnerText);
			}
			if (el.Attributes["italic"] == null)
			{
				this.italic = defaultColor.Italic;
			}
			else
			{
				this.italic = bool.Parse(el.Attributes["italic"].InnerText);
			}
			if (el.Attributes["color"] == null)
			{
				this.color = defaultColor.color;
			}
			else
			{
				string innerText = el.Attributes["color"].InnerText;
				if (innerText[0] == '#')
				{
					this.color = HighlightColor.ParseColor(innerText);
				}
				else if (!innerText.StartsWith("SystemColors."))
				{
					this.color = (System.Drawing.Color)this.Color.GetType().InvokeMember(innerText, BindingFlags.GetProperty, null, this.Color, new object[0]);
				}
				else
				{
					this.color = this.ParseColorString(innerText.Substring("SystemColors.".Length));
				}
				this.hasForeground = true;
			}
			if (el.Attributes["bgcolor"] == null)
			{
				this.backgroundcolor = defaultColor.BackgroundColor;
				return;
			}
			string str = el.Attributes["bgcolor"].InnerText;
			if (str[0] == '#')
			{
				this.backgroundcolor = HighlightColor.ParseColor(str);
			}
			else if (!str.StartsWith("SystemColors."))
			{
				this.backgroundcolor = (System.Drawing.Color)this.Color.GetType().InvokeMember(str, BindingFlags.GetProperty, null, this.Color, new object[0]);
			}
			else
			{
				this.backgroundcolor = this.ParseColorString(str.Substring("SystemColors.".Length));
			}
			this.hasBackground = true;
		}

		public HighlightColor(System.Drawing.Color color, bool bold, bool italic)
		{
			this.hasForeground = true;
			this.color = color;
			this.bold = bold;
			this.italic = italic;
		}

		public HighlightColor(System.Drawing.Color color, System.Drawing.Color backgroundcolor, bool bold, bool italic)
		{
			this.hasForeground = true;
			this.hasBackground = true;
			this.color = color;
			this.backgroundcolor = backgroundcolor;
			this.bold = bold;
			this.italic = italic;
		}

		public HighlightColor(string systemColor, string systemBackgroundColor, bool bold, bool italic)
		{
			this.hasForeground = true;
			this.hasBackground = true;
			this.color = this.ParseColorString(systemColor);
			this.backgroundcolor = this.ParseColorString(systemBackgroundColor);
			this.bold = bold;
			this.italic = italic;
		}

		public HighlightColor(string systemColor, bool bold, bool italic)
		{
			this.hasForeground = true;
			this.color = this.ParseColorString(systemColor);
			this.bold = bold;
			this.italic = italic;
		}

		public Font GetFont(FontContainer fontContainer)
		{
			if (this.Bold)
			{
				if (!this.Italic)
				{
					return fontContainer.BoldFont;
				}
				return fontContainer.BoldItalicFont;
			}
			if (!this.Italic)
			{
				return fontContainer.RegularFont;
			}
			return fontContainer.ItalicFont;
		}

		private static System.Drawing.Color ParseColor(string c)
		{
			int num = 255;
			int num1 = 0;
			if (c.Length > 7)
			{
				num1 = 2;
				num = int.Parse(c.Substring(1, 2), NumberStyles.HexNumber);
			}
			int num2 = int.Parse(c.Substring(1 + num1, 2), NumberStyles.HexNumber);
			int num3 = int.Parse(c.Substring(3 + num1, 2), NumberStyles.HexNumber);
			int num4 = int.Parse(c.Substring(5 + num1, 2), NumberStyles.HexNumber);
			return System.Drawing.Color.FromArgb(num, num2, num3, num4);
		}

		private System.Drawing.Color ParseColorString(string colorName)
		{
			string[] strArrays = colorName.Split(new char[] { '*' });
			PropertyInfo property = typeof(SystemColors).GetProperty(strArrays[0], BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
			System.Drawing.Color value = (System.Drawing.Color)property.GetValue(null, null);
			if ((int)strArrays.Length == 2)
			{
				double num = double.Parse(strArrays[1]) / 100;
				value = System.Drawing.Color.FromArgb((int)((double)value.R * num), (int)((double)value.G * num), (int)((double)value.B * num));
			}
			return value;
		}

		public override string ToString()
		{
			object[] bold = new object[] { "[HighlightColor: Bold = ", this.Bold, ", Italic = ", this.Italic, ", Color = ", this.Color, ", BackgroundColor = ", this.BackgroundColor, "]" };
			return string.Concat(bold);
		}
	}
}