using System;
using System.IO;
using System.Text;

namespace ICSharpCode.TextEditor.Util
{
	public static class FileReader
	{
		private static StreamReader AutoDetect(Stream fs, byte firstByte, byte secondByte, Encoding defaultEncoding)
		{
			byte num;
			int num1 = (int)Math.Min(fs.Length, (long)500000);
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < num1; i++)
			{
				if (i != 0)
				{
					num = (i != 1 ? (byte)fs.ReadByte() : secondByte);
				}
				else
				{
					num = firstByte;
				}
				if (num < 128)
				{
					if (num2 == 3)
					{
						num2 = 1;
						break;
					}
				}
				else if (num < 192)
				{
					if (num2 != 3)
					{
						num2 = 1;
						break;
					}
					else
					{
						num3--;
						if (num3 < 0)
						{
							num2 = 1;
							break;
						}
						else if (num3 == 0)
						{
							num2 = 2;
						}
					}
				}
				else if (num < 194 || num >= 245)
				{
					num2 = 1;
					break;
				}
				else if (num2 == 2 || num2 == 0)
				{
					num2 = 3;
					if (num >= 224)
					{
						num3 = (num >= 240 ? 3 : 2);
					}
					else
					{
						num3 = 1;
					}
				}
				else
				{
					num2 = 1;
					break;
				}
			}
			fs.Position = (long)0;
			switch (num2)
			{
				case 0:
				case 1:
				{
					if (FileReader.IsUnicode(defaultEncoding))
					{
						defaultEncoding = Encoding.Default;
					}
					return new StreamReader(fs, defaultEncoding);
				}
			}
			return new StreamReader(fs);
		}

		public static bool IsUnicode(Encoding encoding)
		{
			int codePage = encoding.CodePage;
			if (codePage == 65001 || codePage == 65000 || codePage == 1200)
			{
				return true;
			}
			return codePage == 1201;
		}

		public static StreamReader OpenStream(Stream fs, Encoding defaultEncoding)
		{
			if (fs == null)
			{
				throw new ArgumentNullException("fs");
			}
			if (fs.Length < (long)2)
			{
				if (defaultEncoding == null)
				{
					return new StreamReader(fs);
				}
				return new StreamReader(fs, defaultEncoding);
			}
			int num = fs.ReadByte();
			int num1 = fs.ReadByte();
			int num2 = num << 8 | num1;
			if (num2 <= 61371)
			{
				if (num2 == 0 || num2 == 61371)
				{
					fs.Position = (long)0;
					return new StreamReader(fs);
				}
				return FileReader.AutoDetect(fs, (byte)num, (byte)num1, defaultEncoding);
			}
			else if (num2 != 65279 && num2 != 65534)
			{
				return FileReader.AutoDetect(fs, (byte)num, (byte)num1, defaultEncoding);
			}
			fs.Position = (long)0;
			return new StreamReader(fs);
		}

		public static string ReadFileContent(Stream fs, ref Encoding encoding)
		{
			string end;
			using (StreamReader streamReader = FileReader.OpenStream(fs, encoding))
			{
				streamReader.Peek();
				encoding = streamReader.CurrentEncoding;
				end = streamReader.ReadToEnd();
			}
			return end;
		}

		public static string ReadFileContent(string fileName, Encoding encoding)
		{
			string str;
			using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				str = FileReader.ReadFileContent(fileStream, ref encoding);
			}
			return str;
		}
	}
}