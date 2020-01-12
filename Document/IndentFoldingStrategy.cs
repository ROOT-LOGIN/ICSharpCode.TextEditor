using System;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Document
{
	public class IndentFoldingStrategy : IFoldingStrategy
	{
		public IndentFoldingStrategy()
		{
		}

		public List<FoldMarker> GenerateFoldMarkers(IDocument document, string fileName, object parseInformation)
		{
			List<FoldMarker> foldMarkers = new List<FoldMarker>();
			Stack<int> nums = new Stack<int>();
			Stack<string> strs = new Stack<string>();
			return foldMarkers;
		}

		private int GetLevel(IDocument document, int offset)
		{
			int num = 0;
			int num1 = 0;
			for (int i = offset; i < document.TextLength; i++)
			{
				char charAt = document.GetCharAt(i);
				if (charAt != '\t')
				{
					if (charAt != ' ')
					{
						break;
					}
					int num2 = num1 + 1;
					num1 = num2;
					if (num2 != 4)
					{
						break;
					}
				}
				num1 = 0;
				num++;
			}
			return num;
		}
	}
}