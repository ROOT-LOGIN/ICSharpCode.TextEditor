using System;

namespace ICSharpCode.TextEditor.Document
{
	public interface IHighlightingStrategyUsingRuleSets : IHighlightingStrategy
	{
		HighlightColor GetColor(IDocument document, LineSegment keyWord, int index, int length);

		HighlightRuleSet GetRuleSet(Span span);
	}
}