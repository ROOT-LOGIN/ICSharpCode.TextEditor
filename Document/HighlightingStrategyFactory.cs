using System;
using System.Collections;

namespace ICSharpCode.TextEditor.Document
{
	public class HighlightingStrategyFactory
	{
		public HighlightingStrategyFactory()
		{
		}

		public static IHighlightingStrategy CreateHighlightingStrategy()
		{
			return (IHighlightingStrategy)HighlightingManager.Manager.HighlightingDefinitions["Default"];
		}

		public static IHighlightingStrategy CreateHighlightingStrategy(string name)
		{
			IHighlightingStrategy highlightingStrategy = HighlightingManager.Manager.FindHighlighter(name);
			if (highlightingStrategy == null)
			{
				return HighlightingStrategyFactory.CreateHighlightingStrategy();
			}
			return highlightingStrategy;
		}

		public static IHighlightingStrategy CreateHighlightingStrategyForFile(string fileName)
		{
			IHighlightingStrategy highlightingStrategy = HighlightingManager.Manager.FindHighlighterForFile(fileName);
			if (highlightingStrategy == null)
			{
				return HighlightingStrategyFactory.CreateHighlightingStrategy();
			}
			return highlightingStrategy;
		}
	}
}