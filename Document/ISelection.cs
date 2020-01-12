using ICSharpCode.TextEditor;
using System;

namespace ICSharpCode.TextEditor.Document
{
	public interface ISelection
	{
		int EndOffset
		{
			get;
		}

		TextLocation EndPosition
		{
			get;
			set;
		}

		bool IsEmpty
		{
			get;
		}

		bool IsRectangularSelection
		{
			get;
		}

		int Length
		{
			get;
		}

		int Offset
		{
			get;
		}

		string SelectedText
		{
			get;
		}

		TextLocation StartPosition
		{
			get;
			set;
		}

		bool ContainsOffset(int offset);

		bool ContainsPosition(TextLocation position);
	}
}