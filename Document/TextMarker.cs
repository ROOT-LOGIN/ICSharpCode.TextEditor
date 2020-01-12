using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace ICSharpCode.TextEditor.Document
{
	public class TextMarker : AbstractSegment
	{
		private ICSharpCode.TextEditor.Document.TextMarkerType textMarkerType;

		private System.Drawing.Color color;

		private System.Drawing.Color foreColor;

		private string toolTip;

		private bool overrideForeColor;

		public System.Drawing.Color Color
		{
			get
			{
				return this.color;
			}
		}

		public int EndOffset
		{
			get
			{
				return this.Offset + this.Length - 1;
			}
		}

		public System.Drawing.Color ForeColor
		{
			get
			{
				return this.foreColor;
			}
		}

		public bool IsReadOnly
		{
			get;
			set;
		}

		public bool OverrideForeColor
		{
			get
			{
				return this.overrideForeColor;
			}
		}

		public ICSharpCode.TextEditor.Document.TextMarkerType TextMarkerType
		{
			get
			{
				return this.textMarkerType;
			}
		}

		public string ToolTip
		{
			get
			{
				return this.toolTip;
			}
			set
			{
				this.toolTip = value;
			}
		}

		public TextMarker(int offset, int length, ICSharpCode.TextEditor.Document.TextMarkerType textMarkerType) : this(offset, length, textMarkerType, System.Drawing.Color.Red)
		{
		}

		public TextMarker(int offset, int length, ICSharpCode.TextEditor.Document.TextMarkerType textMarkerType, System.Drawing.Color color)
		{
			if (length < 1)
			{
				length = 1;
			}
			this.offset = offset;
			this.length = length;
			this.textMarkerType = textMarkerType;
			this.color = color;
		}

		public TextMarker(int offset, int length, ICSharpCode.TextEditor.Document.TextMarkerType textMarkerType, System.Drawing.Color color, System.Drawing.Color foreColor)
		{
			if (length < 1)
			{
				length = 1;
			}
			this.offset = offset;
			this.length = length;
			this.textMarkerType = textMarkerType;
			this.color = color;
			this.foreColor = foreColor;
			this.overrideForeColor = true;
		}
	}
}