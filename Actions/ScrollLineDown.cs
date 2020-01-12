using ICSharpCode.TextEditor;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor.Actions
{
	public class ScrollLineDown : AbstractEditAction
	{
		public ScrollLineDown()
		{
		}

		public override void Execute(TextArea textArea)
		{
			textArea.AutoClearSelection = false;
			VScrollBar vScrollBar = textArea.MotherTextAreaControl.VScrollBar;
			int maximum = textArea.MotherTextAreaControl.VScrollBar.Maximum;
			Point virtualTop = textArea.VirtualTop;
			vScrollBar.Value = Math.Min(maximum, virtualTop.Y + textArea.TextView.FontHeight);
		}
	}
}