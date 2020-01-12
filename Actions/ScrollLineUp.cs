using ICSharpCode.TextEditor;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor.Actions
{
	public class ScrollLineUp : AbstractEditAction
	{
		public ScrollLineUp()
		{
		}

		public override void Execute(TextArea textArea)
		{
			textArea.AutoClearSelection = false;
			VScrollBar vScrollBar = textArea.MotherTextAreaControl.VScrollBar;
			int minimum = textArea.MotherTextAreaControl.VScrollBar.Minimum;
			Point virtualTop = textArea.VirtualTop;
			vScrollBar.Value = Math.Max(minimum, virtualTop.Y - textArea.TextView.FontHeight);
		}
	}
}