using ICSharpCode.TextEditor.Util;
using System;
using System.IO;
using System.Text;

namespace ICSharpCode.TextEditor.Document
{
	public class StringTextBufferStrategy : ITextBufferStrategy
	{
		private string storedText = "";

		public int Length
		{
			get
			{
				return this.storedText.Length;
			}
		}

		public StringTextBufferStrategy()
		{
		}

		public static ITextBufferStrategy CreateTextBufferFromFile(string fileName)
		{
			if (!File.Exists(fileName))
			{
				throw new FileNotFoundException(fileName);
			}
			StringTextBufferStrategy stringTextBufferStrategy = new StringTextBufferStrategy();
			stringTextBufferStrategy.SetContent(FileReader.ReadFileContent(fileName, Encoding.Default));
			return stringTextBufferStrategy;
		}

		public char GetCharAt(int offset)
		{
			if (offset == this.Length)
			{
				return '\0';
			}
			return this.storedText[offset];
		}

		public string GetText(int offset, int length)
		{
			if (length == 0)
			{
				return "";
			}
			if (offset == 0 && length >= this.storedText.Length)
			{
				return this.storedText;
			}
			return this.storedText.Substring(offset, Math.Min(length, this.storedText.Length - offset));
		}

		public void Insert(int offset, string text)
		{
			if (text != null)
			{
				this.storedText = this.storedText.Insert(offset, text);
			}
		}

		public void Remove(int offset, int length)
		{
			this.storedText = this.storedText.Remove(offset, length);
		}

		public void Replace(int offset, int length, string text)
		{
			this.Remove(offset, length);
			this.Insert(offset, text);
		}

		public void SetContent(string text)
		{
			this.storedText = text;
		}
	}
}