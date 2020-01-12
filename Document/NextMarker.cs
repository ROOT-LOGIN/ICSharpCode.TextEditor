using System;
using System.Xml;

namespace ICSharpCode.TextEditor.Document
{
	public class NextMarker
	{
		private string what;

		private HighlightColor color;

		private bool markMarker;

		public HighlightColor Color
		{
			get
			{
				return this.color;
			}
		}

		public bool MarkMarker
		{
			get
			{
				return this.markMarker;
			}
		}

		public string What
		{
			get
			{
				return this.what;
			}
		}

		public NextMarker(XmlElement mark)
		{
			this.color = new HighlightColor(mark);
			this.what = mark.InnerText;
			if (mark.Attributes["markmarker"] != null)
			{
				this.markMarker = bool.Parse(mark.Attributes["markmarker"].InnerText);
			}
		}
	}
}