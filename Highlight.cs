using System;
using System.Runtime.CompilerServices;

namespace ICSharpCode.TextEditor
{
	public class Highlight
	{
		public TextLocation CloseBrace
		{
			get;
			set;
		}

		public TextLocation OpenBrace
		{
			get;
			set;
		}

		public Highlight(TextLocation openBrace, TextLocation closeBrace)
		{
			this.OpenBrace = openBrace;
			this.CloseBrace = closeBrace;
		}
	}
}