using System;

namespace ICSharpCode.TextEditor.Gui.CompletionWindow
{
	public interface IDeclarationViewWindow
	{
		string Description
		{
			get;
			set;
		}

		void CloseDeclarationViewWindow();

		void ShowDeclarationViewWindow();
	}
}