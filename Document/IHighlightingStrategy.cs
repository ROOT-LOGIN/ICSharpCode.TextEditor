using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Document
{
	public interface IHighlightingStrategy
	{
		string[] Extensions
		{
			get;
		}

		string Name
		{
			get;
		}

		Dictionary<string, string> Properties
		{
			get;
		}

		HighlightColor GetColorFor(string name);

		void MarkTokens(IDocument document, List<LineSegment> lines);

		void MarkTokens(IDocument document);
	}
}