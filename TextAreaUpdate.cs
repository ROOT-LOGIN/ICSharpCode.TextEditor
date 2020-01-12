using System;

namespace ICSharpCode.TextEditor
{
	public class TextAreaUpdate
	{
		private TextLocation position;

		private ICSharpCode.TextEditor.TextAreaUpdateType type;

		public TextLocation Position
		{
			get
			{
				return this.position;
			}
		}

		public ICSharpCode.TextEditor.TextAreaUpdateType TextAreaUpdateType
		{
			get
			{
				return this.type;
			}
		}

		public TextAreaUpdate(ICSharpCode.TextEditor.TextAreaUpdateType type)
		{
			this.type = type;
		}

		public TextAreaUpdate(ICSharpCode.TextEditor.TextAreaUpdateType type, TextLocation position)
		{
			this.type = type;
			this.position = position;
		}

		public TextAreaUpdate(ICSharpCode.TextEditor.TextAreaUpdateType type, int startLine, int endLine)
		{
			this.type = type;
			this.position = new TextLocation(startLine, endLine);
		}

		public TextAreaUpdate(ICSharpCode.TextEditor.TextAreaUpdateType type, int singleLine)
		{
			this.type = type;
			this.position = new TextLocation(0, singleLine);
		}

		public override string ToString()
		{
			return string.Format("[TextAreaUpdate: Type={0}, Position={1}]", this.type, this.position);
		}
	}
}