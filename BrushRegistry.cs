using System;
using System.Collections.Generic;
using System.Drawing;

namespace ICSharpCode.TextEditor
{
	public class BrushRegistry
	{
		private static Dictionary<Color, Brush> brushes;

		private static Dictionary<Color, Pen> pens;

		private static Dictionary<Color, Pen> dotPens;

		private readonly static float[] dotPattern;

		static BrushRegistry()
		{
			BrushRegistry.brushes = new Dictionary<Color, Brush>();
			BrushRegistry.pens = new Dictionary<Color, Pen>();
			BrushRegistry.dotPens = new Dictionary<Color, Pen>();
			BrushRegistry.dotPattern = new float[] { 1f, 1f, 1f, 1f };
		}

		public BrushRegistry()
		{
		}

		public static Brush GetBrush(Color color)
		{
			Brush solidBrush;
			Brush brush;
			lock (BrushRegistry.brushes)
			{
				if (!BrushRegistry.brushes.TryGetValue(color, out solidBrush))
				{
					solidBrush = new SolidBrush(color);
					BrushRegistry.brushes.Add(color, solidBrush);
				}
				brush = solidBrush;
			}
			return brush;
		}

		public static Pen GetDotPen(Color color)
		{
			Pen pen;
			Pen pen1;
			lock (BrushRegistry.dotPens)
			{
				if (!BrushRegistry.dotPens.TryGetValue(color, out pen))
				{
					pen = new Pen(color)
					{
						DashPattern = BrushRegistry.dotPattern
					};
					BrushRegistry.dotPens.Add(color, pen);
				}
				pen1 = pen;
			}
			return pen1;
		}

		public static Pen GetPen(Color color)
		{
			Pen pen;
			Pen pen1;
			lock (BrushRegistry.pens)
			{
				if (!BrushRegistry.pens.TryGetValue(color, out pen))
				{
					pen = new Pen(color);
					BrushRegistry.pens.Add(color, pen);
				}
				pen1 = pen;
			}
			return pen1;
		}
	}
}