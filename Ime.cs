using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor
{
	internal class Ime
	{
		private const int WM_IME_CONTROL = 643;

		private const int IMC_SETCOMPOSITIONWINDOW = 12;

		private const int CFS_POINT = 2;

		private const int IMC_SETCOMPOSITIONFONT = 10;

		private System.Drawing.Font font;

		private IntPtr hIMEWnd;

		private IntPtr hWnd;

		private Ime.LOGFONT lf;

		private static bool disableIME;

		public System.Drawing.Font Font
		{
			get
			{
				return this.font;
			}
			set
			{
				if (!value.Equals(this.font))
				{
					this.font = value;
					this.lf = null;
					this.SetIMEWindowFont(value);
				}
			}
		}

		public IntPtr HWnd
		{
			set
			{
				if (this.hWnd != value)
				{
					this.hWnd = value;
					if (!Ime.disableIME)
					{
						this.hIMEWnd = Ime.ImmGetDefaultIMEWnd(value);
					}
					this.SetIMEWindowFont(this.font);
				}
			}
		}

		public Ime(IntPtr hWnd, System.Drawing.Font font)
		{
			string environmentVariable = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432");
			if (environmentVariable == "IA64" || environmentVariable == "AMD64" || Environment.OSVersion.Platform == PlatformID.Unix || Environment.Version >= new Version(4, 0))
			{
				Ime.disableIME = true;
			}
			else
			{
				this.hIMEWnd = Ime.ImmGetDefaultIMEWnd(hWnd);
			}
			this.hWnd = hWnd;
			this.font = font;
			this.SetIMEWindowFont(font);
		}

		private void Handle(Exception ex)
		{
			Console.WriteLine(ex);
			if (!Ime.disableIME)
			{
				Ime.disableIME = true;
				MessageBox.Show(string.Concat("Error calling IME: ", ex.Message, "\nIME is disabled."), "IME error");
			}
		}

		[DllImport("imm32.dll", CharSet=CharSet.None, ExactSpelling=false)]
		private static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

		[DllImport("user32.dll", CharSet=CharSet.None, ExactSpelling=false)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, Ime.COMPOSITIONFORM lParam);

		[DllImport("user32.dll", CharSet=CharSet.None, ExactSpelling=false)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, [In] Ime.LOGFONT lParam);

		private void SetIMEWindowFont(System.Drawing.Font f)
		{
			if (Ime.disableIME || this.hIMEWnd == IntPtr.Zero)
			{
				return;
			}
			if (this.lf == null)
			{
				this.lf = new Ime.LOGFONT();
				f.ToLogFont(this.lf);
				this.lf.lfFaceName = f.Name;
			}
			try
			{
				Ime.SendMessage(this.hIMEWnd, 643, new IntPtr(10), this.lf);
			}
			catch (AccessViolationException accessViolationException)
			{
				this.Handle(accessViolationException);
			}
		}

		public void SetIMEWindowLocation(int x, int y)
		{
			if (Ime.disableIME || this.hIMEWnd == IntPtr.Zero)
			{
				return;
			}
			Ime.POINT pOINT = new Ime.POINT()
			{
				x = x,
				y = y
			};
			Ime.COMPOSITIONFORM cOMPOSITIONFORM = new Ime.COMPOSITIONFORM()
			{
				dwStyle = 2,
				ptCurrentPos = pOINT,
				rcArea = new Ime.RECT()
			};
			try
			{
				Ime.SendMessage(this.hIMEWnd, 643, new IntPtr(12), cOMPOSITIONFORM);
			}
			catch (AccessViolationException accessViolationException)
			{
				this.Handle(accessViolationException);
			}
		}

		private class COMPOSITIONFORM
		{
			public int dwStyle;

			public Ime.POINT ptCurrentPos;

			public Ime.RECT rcArea;

			public COMPOSITIONFORM()
			{
			}
		}

		private class LOGFONT
		{
			public int lfHeight;

			public int lfWidth;

			public int lfEscapement;

			public int lfOrientation;

			public int lfWeight;

			public byte lfItalic;

			public byte lfUnderline;

			public byte lfStrikeOut;

			public byte lfCharSet;

			public byte lfOutPrecision;

			public byte lfClipPrecision;

			public byte lfQuality;

			public byte lfPitchAndFamily;

			public string lfFaceName;

			public LOGFONT()
			{
			}
		}

		private class POINT
		{
			public int x;

			public int y;

			public POINT()
			{
			}
		}

		private class RECT
		{
			public int left;

			public int top;

			public int right;

			public int bottom;

			public RECT()
			{
			}
		}
	}
}