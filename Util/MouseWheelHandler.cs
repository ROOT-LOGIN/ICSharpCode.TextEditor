using System;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor.Util
{
	internal class MouseWheelHandler
	{
		private const int WHEEL_DELTA = 120;

		private int mouseWheelDelta;

		public MouseWheelHandler()
		{
		}

		public int GetScrollAmount(MouseEventArgs e)
		{
			this.mouseWheelDelta += e.Delta;
			int num = Math.Max(SystemInformation.MouseWheelScrollLines, 1);
			int num1 = this.mouseWheelDelta * num / 120;
			this.mouseWheelDelta %= Math.Max(1, 120 / num);
			return num1;
		}
	}
}