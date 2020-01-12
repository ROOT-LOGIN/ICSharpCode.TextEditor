using System;

namespace ICSharpCode.TextEditor.Document
{
	public interface ISegment
	{
		int Length
		{
			get;
			set;
		}

		int Offset
		{
			get;
			set;
		}
	}
}