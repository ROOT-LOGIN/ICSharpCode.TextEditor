using ICSharpCode.TextEditor;
using System;

namespace ICSharpCode.TextEditor.Gui.InsightWindow
{
	public interface IInsightDataProvider
	{
		int DefaultIndex
		{
			get;
		}

		int InsightDataCount
		{
			get;
		}

		bool CaretOffsetChanged();

		string GetInsightData(int number);

		void SetupDataProvider(string fileName, TextArea textArea);
	}
}