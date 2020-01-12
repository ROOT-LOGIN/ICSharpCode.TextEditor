using System;

namespace ICSharpCode.TextEditor.Document
{
	public interface ITextBufferStrategy
	{
		int Length
		{
			get;
		}

		char GetCharAt(int offset);

		string GetText(int offset, int length);

		void Insert(int offset, string text);

		void Remove(int offset, int length);

		void Replace(int offset, int length, string text);

		void SetContent(string text);
	}
}