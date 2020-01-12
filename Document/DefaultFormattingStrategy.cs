using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Undo;
using System;
using System.Text;

namespace ICSharpCode.TextEditor.Document
{
	public class DefaultFormattingStrategy : IFormattingStrategy
	{
		private readonly static char[] whitespaceChars;

		static DefaultFormattingStrategy()
		{
			DefaultFormattingStrategy.whitespaceChars = new char[] { ' ', '\t' };
		}

		public DefaultFormattingStrategy()
		{
		}

		protected virtual int AutoIndentLine(TextArea textArea, int lineNumber)
		{
			string str = (lineNumber != 0 ? this.GetIndentation(textArea, lineNumber - 1) : "");
			if (str.Length > 0)
			{
				string str1 = string.Concat(str, TextUtilities.GetLineAsString(textArea.Document, lineNumber).Trim());
				LineSegment lineSegment = textArea.Document.GetLineSegment(lineNumber);
				DefaultFormattingStrategy.SmartReplaceLine(textArea.Document, lineSegment, str1);
			}
			return str.Length;
		}

		public virtual void FormatLine(TextArea textArea, int line, int cursorOffset, char ch)
		{
			if (ch == '\n')
			{
				textArea.Caret.Column = this.IndentLine(textArea, line);
			}
		}

		protected string GetIndentation(TextArea textArea, int lineNumber)
		{
			if (lineNumber < 0 || lineNumber > textArea.Document.TotalNumberOfLines)
			{
				throw new ArgumentOutOfRangeException("lineNumber");
			}
			string lineAsString = TextUtilities.GetLineAsString(textArea.Document, lineNumber);
			StringBuilder stringBuilder = new StringBuilder();
			string str = lineAsString;
			for (int i = 0; i < str.Length; i++)
			{
				char chr = str[i];
				if (!char.IsWhiteSpace(chr))
				{
					break;
				}
				stringBuilder.Append(chr);
			}
			return stringBuilder.ToString();
		}

		public int IndentLine(TextArea textArea, int line)
		{
			int num;
			textArea.Document.UndoStack.StartUndoGroup();
			switch (textArea.Document.TextEditorProperties.IndentStyle)
			{
				case IndentStyle.None:
				{
					num = 0;
					break;
				}
				case IndentStyle.Auto:
				{
					num = this.AutoIndentLine(textArea, line);
					break;
				}
				case IndentStyle.Smart:
				{
					num = this.SmartIndentLine(textArea, line);
					break;
				}
				default:
				{
					throw new NotSupportedException(string.Concat("Unsupported value for IndentStyle: ", textArea.Document.TextEditorProperties.IndentStyle));
				}
			}
			textArea.Document.UndoStack.EndUndoGroup();
			return num;
		}

		public virtual void IndentLines(TextArea textArea, int begin, int end)
		{
			textArea.Document.UndoStack.StartUndoGroup();
			for (int i = begin; i <= end; i++)
			{
				this.IndentLine(textArea, i);
			}
			textArea.Document.UndoStack.EndUndoGroup();
		}

		public virtual int SearchBracketBackward(IDocument document, int offset, char openBracket, char closingBracket)
		{
			int num = -1;
			for (int i = offset; i >= 0; i--)
			{
				char charAt = document.GetCharAt(i);
				if (charAt == openBracket)
				{
					num++;
					if (num == 0)
					{
						return i;
					}
				}
				else if (charAt == closingBracket)
				{
					num--;
				}
				else if (charAt == '\"' || charAt == '\'' || charAt == '/' && i > 0 && (document.GetCharAt(i - 1) == '/' || document.GetCharAt(i - 1) == '*'))
				{
					break;
				}
			}
			return -1;
		}

		public virtual int SearchBracketForward(IDocument document, int offset, char openBracket, char closingBracket)
		{
			int num = 1;
			for (int i = offset; i < document.TextLength; i++)
			{
				char charAt = document.GetCharAt(i);
				if (charAt == openBracket)
				{
					num++;
				}
				else if (charAt != closingBracket)
				{
					if (charAt == '\"' || charAt == '\'')
					{
						break;
					}
					if (charAt == '/' && i > 0)
					{
						if (document.GetCharAt(i - 1) == '/')
						{
							break;
						}
					}
					else if (charAt == '*' && i > 0 && document.GetCharAt(i - 1) == '/')
					{
						break;
					}
				}
				else
				{
					num--;
					if (num == 0)
					{
						return i;
					}
				}
			}
			return -1;
		}

		protected virtual int SmartIndentLine(TextArea textArea, int line)
		{
			return this.AutoIndentLine(textArea, line);
		}

		public static void SmartReplaceLine(IDocument document, LineSegment line, string newLineText)
		{
			int i;
			if (document == null)
			{
				throw new ArgumentNullException("document");
			}
			if (line == null)
			{
				throw new ArgumentNullException("line");
			}
			if (newLineText == null)
			{
				throw new ArgumentNullException("newLineText");
			}
			string str = newLineText.Trim(DefaultFormattingStrategy.whitespaceChars);
			string text = document.GetText(line);
			if (text == newLineText)
			{
				return;
			}
			int num = text.IndexOf(str);
			if (str.Length <= 0 || num < 0)
			{
				document.Replace(line.Offset, line.Length, newLineText);
			}
			else
			{
				document.UndoStack.StartUndoGroup();
				try
				{
					for (i = 0; i < newLineText.Length; i++)
					{
						char chr = newLineText[i];
						if (chr != ' ' && chr != '\t')
						{
							break;
						}
					}
					int length = newLineText.Length - str.Length - i;
					int offset = line.Offset;
					document.Replace(offset + num + str.Length, line.Length - num - str.Length, newLineText.Substring(newLineText.Length - length));
					document.Replace(offset, num, newLineText.Substring(0, i));
				}
				finally
				{
					document.UndoStack.EndUndoGroup();
				}
			}
		}
	}
}