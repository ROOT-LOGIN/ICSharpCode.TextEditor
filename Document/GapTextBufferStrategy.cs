using System;
using System.Text;

namespace ICSharpCode.TextEditor.Document
{
	public class GapTextBufferStrategy : ITextBufferStrategy
	{
		private const int minGapLength = 128;

		private const int maxGapLength = 2048;

		private char[] buffer = new char[0];

		private string cachedContent;

		private int gapBeginOffset;

		private int gapEndOffset;

		private int gapLength;

		public int Length
		{
			get
			{
				return (int)this.buffer.Length - this.gapLength;
			}
		}

		public GapTextBufferStrategy()
		{
		}

		public char GetCharAt(int offset)
		{
			if (offset < 0 || offset >= this.Length)
			{
				object obj = offset;
				int length = this.Length;
				throw new ArgumentOutOfRangeException("offset", obj, string.Concat("0 <= offset < ", length.ToString()));
			}
			if (offset < this.gapBeginOffset)
			{
				return this.buffer[offset];
			}
			return this.buffer[offset + this.gapLength];
		}

		public string GetText(int offset, int length)
		{
			if (offset < 0 || offset > this.Length)
			{
				object obj = offset;
				int num = this.Length;
				throw new ArgumentOutOfRangeException("offset", obj, string.Concat("0 <= offset <= ", num.ToString()));
			}
			if (length < 0 || offset + length > this.Length)
			{
				object obj1 = length;
				object[] str = new object[] { "0 <= length, offset(", offset, ")+length <= ", null };
				str[3] = this.Length.ToString();
				throw new ArgumentOutOfRangeException("length", obj1, string.Concat(str));
			}
			if (offset != 0 || length != this.Length)
			{
				return this.GetTextInternal(offset, length);
			}
			if (this.cachedContent != null)
			{
				return this.cachedContent;
			}
			string textInternal = this.GetTextInternal(offset, length);
			string str1 = textInternal;
			this.cachedContent = textInternal;
			return str1;
		}

		private string GetTextInternal(int offset, int length)
		{
			int num = offset + length;
			if (num < this.gapBeginOffset)
			{
				return new string(this.buffer, offset, length);
			}
			if (offset > this.gapBeginOffset)
			{
				return new string(this.buffer, offset + this.gapLength, length);
			}
			int num1 = this.gapBeginOffset - offset;
			int num2 = num - this.gapBeginOffset;
			StringBuilder stringBuilder = new StringBuilder(num1 + num2);
			stringBuilder.Append(this.buffer, offset, num1);
			stringBuilder.Append(this.buffer, this.gapEndOffset, num2);
			return stringBuilder.ToString();
		}

		public void Insert(int offset, string text)
		{
			this.Replace(offset, 0, text);
		}

		private void MakeNewBuffer(int newGapOffset, int newGapLength)
		{
			if (newGapLength < 128)
			{
				newGapLength = 128;
			}
			char[] chrArray = new char[this.Length + newGapLength];
			if (newGapOffset >= this.gapBeginOffset)
			{
				Array.Copy(this.buffer, 0, chrArray, 0, this.gapBeginOffset);
				Array.Copy(this.buffer, this.gapEndOffset, chrArray, this.gapBeginOffset, newGapOffset - this.gapBeginOffset);
				int length = (int)chrArray.Length - (newGapOffset + newGapLength);
				Array.Copy(this.buffer, (int)this.buffer.Length - length, chrArray, newGapOffset + newGapLength, length);
			}
			else
			{
				Array.Copy(this.buffer, 0, chrArray, 0, newGapOffset);
				Array.Copy(this.buffer, newGapOffset, chrArray, newGapOffset + newGapLength, this.gapBeginOffset - newGapOffset);
				Array.Copy(this.buffer, this.gapEndOffset, chrArray, (int)chrArray.Length - ((int)this.buffer.Length - this.gapEndOffset), (int)this.buffer.Length - this.gapEndOffset);
			}
			this.gapBeginOffset = newGapOffset;
			this.gapEndOffset = newGapOffset + newGapLength;
			this.gapLength = newGapLength;
			this.buffer = chrArray;
		}

		private void PlaceGap(int newGapOffset, int minRequiredGapLength)
		{
			if (this.gapLength < minRequiredGapLength)
			{
				this.MakeNewBuffer(newGapOffset, minRequiredGapLength);
				return;
			}
			while (newGapOffset < this.gapBeginOffset)
			{
				char[] chrArray = this.buffer;
				GapTextBufferStrategy gapTextBufferStrategy = this;
				int num = gapTextBufferStrategy.gapEndOffset - 1;
				int num1 = num;
				gapTextBufferStrategy.gapEndOffset = num;
				GapTextBufferStrategy gapTextBufferStrategy1 = this;
				int num2 = gapTextBufferStrategy1.gapBeginOffset - 1;
				int num3 = num2;
				gapTextBufferStrategy1.gapBeginOffset = num2;
				chrArray[num1] = this.buffer[num3];
			}
			while (newGapOffset > this.gapBeginOffset)
			{
				char[] chrArray1 = this.buffer;
				GapTextBufferStrategy gapTextBufferStrategy2 = this;
				int num4 = gapTextBufferStrategy2.gapBeginOffset;
				int num5 = num4;
				gapTextBufferStrategy2.gapBeginOffset = num4 + 1;
				char[] chrArray2 = this.buffer;
				GapTextBufferStrategy gapTextBufferStrategy3 = this;
				int num6 = gapTextBufferStrategy3.gapEndOffset;
				int num7 = num6;
				gapTextBufferStrategy3.gapEndOffset = num6 + 1;
				chrArray1[num5] = chrArray2[num7];
			}
		}

		public void Remove(int offset, int length)
		{
			this.Replace(offset, length, string.Empty);
		}

		public void Replace(int offset, int length, string text)
		{
			if (text == null)
			{
				text = string.Empty;
			}
			if (offset < 0 || offset > this.Length)
			{
				object obj = offset;
				int num = this.Length;
				throw new ArgumentOutOfRangeException("offset", obj, string.Concat("0 <= offset <= ", num.ToString()));
			}
			if (length < 0 || offset + length > this.Length)
			{
				object obj1 = length;
				int num1 = this.Length;
				throw new ArgumentOutOfRangeException("length", obj1, string.Concat("0 <= length, offset+length <= ", num1.ToString()));
			}
			this.cachedContent = null;
			this.PlaceGap(offset, text.Length - length);
			this.gapEndOffset += length;
			text.CopyTo(0, this.buffer, this.gapBeginOffset, text.Length);
			this.gapBeginOffset += text.Length;
			this.gapLength = this.gapEndOffset - this.gapBeginOffset;
			if (this.gapLength > 2048)
			{
				this.MakeNewBuffer(this.gapBeginOffset, 128);
			}
		}

		public void SetContent(string text)
		{
			if (text == null)
			{
				text = string.Empty;
			}
			this.cachedContent = text;
			this.buffer = text.ToCharArray();
			int num = 0;
			int num1 = num;
			this.gapLength = num;
			int num2 = num1;
			int num3 = num2;
			this.gapEndOffset = num2;
			this.gapBeginOffset = num3;
		}
	}
}