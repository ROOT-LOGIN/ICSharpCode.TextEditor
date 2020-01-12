using System;
using System.Drawing;

namespace ICSharpCode.TextEditor
{
	public class ToolTipRequestEventArgs
	{
		private Point mousePosition;

		private TextLocation logicalPosition;

		private bool inDocument;

		internal string toolTipText;

		public bool InDocument
		{
			get
			{
				return this.inDocument;
			}
		}

		public TextLocation LogicalPosition
		{
			get
			{
				return this.logicalPosition;
			}
		}

		public Point MousePosition
		{
			get
			{
				return this.mousePosition;
			}
		}

		public bool ToolTipShown
		{
			get
			{
				return this.toolTipText != null;
			}
		}

		public ToolTipRequestEventArgs(Point mousePosition, TextLocation logicalPosition, bool inDocument)
		{
			this.mousePosition = mousePosition;
			this.logicalPosition = logicalPosition;
			this.inDocument = inDocument;
		}

		public void ShowToolTip(string text)
		{
			this.toolTipText = text;
		}
	}
}