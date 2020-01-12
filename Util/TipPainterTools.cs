using System;
using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor.Util
{
	internal static class TipPainterTools
	{
		private const int SpacerSize = 4;

		public static Rectangle DrawingRectangle1;

		public static Rectangle DrawingRectangle2;

		public static Size DrawFixedWidthHelpTip(Control control, Graphics graphics, Font font, string countMessage, string basicDescription, string documentation)
		{
			object obj;
			object obj1;
			if (!TipPainterTools.IsVisibleText(countMessage) && !TipPainterTools.IsVisibleText(basicDescription) && !TipPainterTools.IsVisibleText(documentation))
			{
				return Size.Empty;
			}
			CountTipText countTipText = new CountTipText(graphics, font, countMessage);
			Graphics graphic = graphics;
			if (TipPainterTools.IsVisibleText(countMessage))
			{
				obj = 4;
			}
			else
			{
				obj = null;
			}
			TipSpacer tipSpacer = new TipSpacer(graphic, new SizeF((float)obj, 0f));
			TipText tipText = new TipText(graphics, font, basicDescription);
			Graphics graphic1 = graphics;
			if (TipPainterTools.IsVisibleText(documentation))
			{
				obj1 = 4;
			}
			else
			{
				obj1 = null;
			}
			TipSpacer tipSpacer1 = new TipSpacer(graphic1, new SizeF(0f, (float)obj1));
			TipText tipText1 = new TipText(graphics, font, documentation);
			TipSection[] tipSectionArray = new TipSection[] { tipText, tipSpacer1 };
			TipSplitter tipSplitter = new TipSplitter(graphics, false, tipSectionArray);
			TipSection[] tipSectionArray1 = new TipSection[] { countTipText, tipSpacer, tipSplitter };
			TipSplitter tipSplitter1 = new TipSplitter(graphics, true, tipSectionArray1);
			TipSection[] tipSectionArray2 = new TipSection[] { tipSplitter1, tipText1 };
			TipSplitter tipSplitter2 = new TipSplitter(graphics, false, tipSectionArray2);
			Size size = TipPainter.DrawFixedWidthTip(control, graphics, tipSplitter2);
			TipPainterTools.DrawingRectangle1 = countTipText.DrawingRectangle1;
			TipPainterTools.DrawingRectangle2 = countTipText.DrawingRectangle2;
			return size;
		}

		public static Size DrawFixedWidthHelpTipFromCombinedDescription(Control control, Graphics graphics, Font font, string countMessage, string description)
		{
			string str = null;
			string str1 = null;
			if (TipPainterTools.IsVisibleText(description))
			{
				char[] chrArray = new char[] { '\n' };
				string[] strArrays = description.Split(chrArray, 2);
				if ((int)strArrays.Length > 0)
				{
					str = strArrays[0];
					if ((int)strArrays.Length > 1)
					{
						str1 = strArrays[1].Trim();
					}
				}
			}
			return TipPainterTools.DrawFixedWidthHelpTip(control, graphics, font, countMessage, str, str1);
		}

		public static Size DrawHelpTip(Control control, Graphics graphics, Font font, string countMessage, string basicDescription, string documentation)
		{
			object obj;
			object obj1;
			if (!TipPainterTools.IsVisibleText(countMessage) && !TipPainterTools.IsVisibleText(basicDescription) && !TipPainterTools.IsVisibleText(documentation))
			{
				return Size.Empty;
			}
			CountTipText countTipText = new CountTipText(graphics, font, countMessage);
			Graphics graphic = graphics;
			if (TipPainterTools.IsVisibleText(countMessage))
			{
				obj = 4;
			}
			else
			{
				obj = null;
			}
			TipSpacer tipSpacer = new TipSpacer(graphic, new SizeF((float)obj, 0f));
			TipText tipText = new TipText(graphics, font, basicDescription);
			Graphics graphic1 = graphics;
			if (TipPainterTools.IsVisibleText(documentation))
			{
				obj1 = 4;
			}
			else
			{
				obj1 = null;
			}
			TipSpacer tipSpacer1 = new TipSpacer(graphic1, new SizeF(0f, (float)obj1));
			TipText tipText1 = new TipText(graphics, font, documentation);
			TipSection[] tipSectionArray = new TipSection[] { tipText, tipSpacer1 };
			TipSplitter tipSplitter = new TipSplitter(graphics, false, tipSectionArray);
			TipSection[] tipSectionArray1 = new TipSection[] { countTipText, tipSpacer, tipSplitter };
			TipSplitter tipSplitter1 = new TipSplitter(graphics, true, tipSectionArray1);
			TipSection[] tipSectionArray2 = new TipSection[] { tipSplitter1, tipText1 };
			TipSplitter tipSplitter2 = new TipSplitter(graphics, false, tipSectionArray2);
			Size size = TipPainter.DrawTip(control, graphics, tipSplitter2);
			TipPainterTools.DrawingRectangle1 = countTipText.DrawingRectangle1;
			TipPainterTools.DrawingRectangle2 = countTipText.DrawingRectangle2;
			return size;
		}

		public static Size DrawHelpTipFromCombinedDescription(Control control, Graphics graphics, Font font, string countMessage, string description)
		{
			string str = null;
			string str1 = null;
			if (TipPainterTools.IsVisibleText(description))
			{
				char[] chrArray = new char[] { '\n' };
				string[] strArrays = description.Split(chrArray, 2);
				if ((int)strArrays.Length > 0)
				{
					str = strArrays[0];
					if ((int)strArrays.Length > 1)
					{
						str1 = strArrays[1].Trim();
					}
				}
			}
			return TipPainterTools.DrawHelpTip(control, graphics, font, countMessage, str, str1);
		}

		public static Size GetDrawingSizeDrawHelpTip(Control control, Graphics graphics, Font font, string countMessage, string basicDescription, string documentation)
		{
			object obj;
			object obj1;
			if (!TipPainterTools.IsVisibleText(countMessage) && !TipPainterTools.IsVisibleText(basicDescription) && !TipPainterTools.IsVisibleText(documentation))
			{
				return Size.Empty;
			}
			CountTipText countTipText = new CountTipText(graphics, font, countMessage);
			Graphics graphic = graphics;
			if (TipPainterTools.IsVisibleText(countMessage))
			{
				obj = 4;
			}
			else
			{
				obj = null;
			}
			TipSpacer tipSpacer = new TipSpacer(graphic, new SizeF((float)obj, 0f));
			TipText tipText = new TipText(graphics, font, basicDescription);
			Graphics graphic1 = graphics;
			if (TipPainterTools.IsVisibleText(documentation))
			{
				obj1 = 4;
			}
			else
			{
				obj1 = null;
			}
			TipSpacer tipSpacer1 = new TipSpacer(graphic1, new SizeF(0f, (float)obj1));
			TipText tipText1 = new TipText(graphics, font, documentation);
			TipSection[] tipSectionArray = new TipSection[] { tipText, tipSpacer1 };
			TipSplitter tipSplitter = new TipSplitter(graphics, false, tipSectionArray);
			TipSection[] tipSectionArray1 = new TipSection[] { countTipText, tipSpacer, tipSplitter };
			TipSplitter tipSplitter1 = new TipSplitter(graphics, true, tipSectionArray1);
			TipSection[] tipSectionArray2 = new TipSection[] { tipSplitter1, tipText1 };
			TipSplitter tipSplitter2 = new TipSplitter(graphics, false, tipSectionArray2);
			Size tipSize = TipPainter.GetTipSize(control, graphics, tipSplitter2);
			TipPainterTools.DrawingRectangle1 = countTipText.DrawingRectangle1;
			TipPainterTools.DrawingRectangle2 = countTipText.DrawingRectangle2;
			return tipSize;
		}

		public static Size GetDrawingSizeHelpTipFromCombinedDescription(Control control, Graphics graphics, Font font, string countMessage, string description)
		{
			string str = null;
			string str1 = null;
			if (TipPainterTools.IsVisibleText(description))
			{
				char[] chrArray = new char[] { '\n' };
				string[] strArrays = description.Split(chrArray, 2);
				if ((int)strArrays.Length > 0)
				{
					str = strArrays[0];
					if ((int)strArrays.Length > 1)
					{
						str1 = strArrays[1].Trim();
					}
				}
			}
			return TipPainterTools.GetDrawingSizeDrawHelpTip(control, graphics, font, countMessage, str, str1);
		}

		public static Size GetLeftHandSideDrawingSizeDrawHelpTip(Control control, Graphics graphics, Font font, string countMessage, string basicDescription, string documentation, Point p)
		{
			object obj;
			object obj1;
			if (!TipPainterTools.IsVisibleText(countMessage) && !TipPainterTools.IsVisibleText(basicDescription) && !TipPainterTools.IsVisibleText(documentation))
			{
				return Size.Empty;
			}
			CountTipText countTipText = new CountTipText(graphics, font, countMessage);
			Graphics graphic = graphics;
			if (TipPainterTools.IsVisibleText(countMessage))
			{
				obj = 4;
			}
			else
			{
				obj = null;
			}
			TipSpacer tipSpacer = new TipSpacer(graphic, new SizeF((float)obj, 0f));
			TipText tipText = new TipText(graphics, font, basicDescription);
			Graphics graphic1 = graphics;
			if (TipPainterTools.IsVisibleText(documentation))
			{
				obj1 = 4;
			}
			else
			{
				obj1 = null;
			}
			TipSpacer tipSpacer1 = new TipSpacer(graphic1, new SizeF(0f, (float)obj1));
			TipText tipText1 = new TipText(graphics, font, documentation);
			TipSection[] tipSectionArray = new TipSection[] { tipText, tipSpacer1 };
			TipSplitter tipSplitter = new TipSplitter(graphics, false, tipSectionArray);
			TipSection[] tipSectionArray1 = new TipSection[] { countTipText, tipSpacer, tipSplitter };
			TipSplitter tipSplitter1 = new TipSplitter(graphics, true, tipSectionArray1);
			TipSection[] tipSectionArray2 = new TipSection[] { tipSplitter1, tipText1 };
			TipSplitter tipSplitter2 = new TipSplitter(graphics, false, tipSectionArray2);
			return TipPainter.GetLeftHandSideTipSize(control, graphics, tipSplitter2, p);
		}

		public static Size GetLeftHandSideDrawingSizeHelpTipFromCombinedDescription(Control control, Graphics graphics, Font font, string countMessage, string description, Point p)
		{
			string str = null;
			string str1 = null;
			if (TipPainterTools.IsVisibleText(description))
			{
				char[] chrArray = new char[] { '\n' };
				string[] strArrays = description.Split(chrArray, 2);
				if ((int)strArrays.Length > 0)
				{
					str = strArrays[0];
					if ((int)strArrays.Length > 1)
					{
						str1 = strArrays[1].Trim();
					}
				}
			}
			return TipPainterTools.GetLeftHandSideDrawingSizeDrawHelpTip(control, graphics, font, countMessage, str, str1, p);
		}

		private static bool IsVisibleText(string text)
		{
			if (text == null)
			{
				return false;
			}
			return text.Length > 0;
		}
	}
}