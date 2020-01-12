using ICSharpCode.TextEditor;
using System;
using System.Text;

namespace ICSharpCode.TextEditor.Document
{
	public sealed class TextUtilities
	{
		public TextUtilities()
		{
		}

		public static int FindNextWordStart(IDocument document, int offset)
		{
			LineSegment lineSegmentForOffset = document.GetLineSegmentForOffset(offset);
			int num = lineSegmentForOffset.Offset + lineSegmentForOffset.Length;
			TextUtilities.CharacterType characterType = TextUtilities.GetCharacterType(document.GetCharAt(offset));
			while (offset < num)
			{
				if (TextUtilities.GetCharacterType(document.GetCharAt(offset)) == characterType)
				{
					offset++;
				}
				else
				{
					break;
				}
			}
			while (offset < num && TextUtilities.GetCharacterType(document.GetCharAt(offset)) == TextUtilities.CharacterType.WhiteSpace)
			{
				offset++;
			}
			return offset;
		}

		public static int FindPrevWordStart(IDocument document, int offset)
		{
			if (offset > 0)
			{
				LineSegment lineSegmentForOffset = document.GetLineSegmentForOffset(offset);
				TextUtilities.CharacterType characterType = TextUtilities.GetCharacterType(document.GetCharAt(offset - 1));
				while (offset > lineSegmentForOffset.Offset && TextUtilities.GetCharacterType(document.GetCharAt(offset - 1)) == characterType)
				{
					offset--;
				}
				if (characterType == TextUtilities.CharacterType.WhiteSpace && offset > lineSegmentForOffset.Offset)
				{
					characterType = TextUtilities.GetCharacterType(document.GetCharAt(offset - 1));
					while (offset > lineSegmentForOffset.Offset && TextUtilities.GetCharacterType(document.GetCharAt(offset - 1)) == characterType)
					{
						offset--;
					}
				}
			}
			return offset;
		}

		public static int FindWordEnd(IDocument document, int offset)
		{
			LineSegment lineSegmentForOffset = document.GetLineSegmentForOffset(offset);
			int num = lineSegmentForOffset.Offset + lineSegmentForOffset.Length;
			while (offset < num && TextUtilities.IsLetterDigitOrUnderscore(document.GetCharAt(offset)))
			{
				offset++;
			}
			return offset;
		}

		public static int FindWordStart(IDocument document, int offset)
		{
			int num = document.GetLineSegmentForOffset(offset).Offset;
			while (offset > num && TextUtilities.IsLetterDigitOrUnderscore(document.GetCharAt(offset - 1)))
			{
				offset--;
			}
			return offset;
		}

		public static TextUtilities.CharacterType GetCharacterType(char c)
		{
			if (TextUtilities.IsLetterDigitOrUnderscore(c))
			{
				return TextUtilities.CharacterType.LetterDigitOrUnderscore;
			}
			if (char.IsWhiteSpace(c))
			{
				return TextUtilities.CharacterType.WhiteSpace;
			}
			return TextUtilities.CharacterType.Other;
		}

		public static string GetExpressionBeforeOffset(TextArea textArea, int initialOffset)
		{
			IDocument document = textArea.Document;
			int num = initialOffset;
			while (num - 1 > 0)
			{
				char charAt = document.GetCharAt(num - 1);
				if (charAt <= ')')
				{
					if (charAt > '\r')
					{
						if (charAt == '\"')
						{
							if (num < initialOffset - 1)
							{
								return null;
							}
							return "\"\"";
						}
						switch (charAt)
						{
							case '\'':
							{
								if (num < initialOffset - 1)
								{
									return null;
								}
								return "'a'";
							}
							case ')':
							{
								num = TextUtilities.SearchBracketBackward(document, num - 2, '(', ')');
								continue;
							}
						}
					}
					else if (charAt == '\n' || charAt == '\r')
					{
						break;
					}
				}
				else if (charAt <= '>')
				{
					if (charAt == '.')
					{
						num--;
						continue;
					}
					else if (charAt == '>')
					{
						if (document.GetCharAt(num - 2) != '-')
						{
							break;
						}
						num -= 2;
						continue;
					}
				}
				else if (charAt == ']')
				{
					num = TextUtilities.SearchBracketBackward(document, num - 2, '[', ']');
					continue;
				}
				else if (charAt == '}')
				{
					break;
				}
				if (!char.IsWhiteSpace(document.GetCharAt(num - 1)))
				{
					int num1 = num - 1;
					if (TextUtilities.IsLetterDigitOrUnderscore(document.GetCharAt(num1)))
					{
						while (num1 > 0 && TextUtilities.IsLetterDigitOrUnderscore(document.GetCharAt(num1 - 1)))
						{
							num1--;
						}
						string str = document.GetText(num1, num - num1).Trim();
						string str1 = str;
						string str2 = str1;
						if (str1 != null && (str2 == "ref" || str2 == "out" || str2 == "in" || str2 == "return" || str2 == "throw" || str2 == "case") || str.Length > 0 && !TextUtilities.IsLetterDigitOrUnderscore(str[0]))
						{
							break;
						}
						num = num1;
					}
					else
					{
						break;
					}
				}
				else
				{
					num--;
				}
			}
			if (num < 0)
			{
				return string.Empty;
			}
			string str3 = document.GetText(num, textArea.Caret.Offset - num).Trim();
			int num2 = str3.LastIndexOf('\n');
			if (num2 >= 0)
			{
				num = num + num2 + 1;
			}
			string str4 = document.GetText(num, textArea.Caret.Offset - num).Trim();
			return str4;
		}

		public static int GetFirstNonWSChar(IDocument document, int offset)
		{
			while (offset < document.TextLength && char.IsWhiteSpace(document.GetCharAt(offset)))
			{
				offset++;
			}
			return offset;
		}

		public static string GetLineAsString(IDocument document, int lineNumber)
		{
			LineSegment lineSegment = document.GetLineSegment(lineNumber);
			return document.GetText(lineSegment.Offset, lineSegment.Length);
		}

		public static string GetWordAt(IDocument document, int offset)
		{
			if (offset < 0 || offset >= document.TextLength - 1 || !TextUtilities.IsWordPart(document.GetCharAt(offset)))
			{
				return string.Empty;
			}
			int num = offset;
			int num1 = offset;
			while (num > 0)
			{
				if (TextUtilities.IsWordPart(document.GetCharAt(num - 1)))
				{
					num--;
				}
				else
				{
					break;
				}
			}
			while (num1 < document.TextLength - 1 && TextUtilities.IsWordPart(document.GetCharAt(num1 + 1)))
			{
				num1++;
			}
			return document.GetText(num, num1 - num + 1);
		}

		public static bool IsEmptyLine(IDocument document, int lineNumber)
		{
			return TextUtilities.IsEmptyLine(document, document.GetLineSegment(lineNumber));
		}

		public static bool IsEmptyLine(IDocument document, LineSegment line)
		{
			for (int i = line.Offset; i < line.Offset + line.Length; i++)
			{
				if (!char.IsWhiteSpace(document.GetCharAt(i)))
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsLetterDigitOrUnderscore(char c)
		{
			if (char.IsLetterOrDigit(c))
			{
				return true;
			}
			return c == '\u005F';
		}

		private static bool IsWordPart(char ch)
		{
			if (TextUtilities.IsLetterDigitOrUnderscore(ch))
			{
				return true;
			}
			return ch == '.';
		}

		public static string LeadingWhiteSpaceToTabs(string line, int tabIndent)
		{
			StringBuilder stringBuilder = new StringBuilder(line.Length);
			int num = 0;
			int i = 0;
			for (i = 0; i < line.Length; i++)
			{
				if (line[i] != ' ')
				{
					if (line[i] != '\t')
					{
						break;
					}
					stringBuilder.Append('\t');
					num = 0;
				}
				else
				{
					num++;
					if (num == tabIndent)
					{
						stringBuilder.Append('\t');
						num = 0;
					}
				}
			}
			if (i < line.Length)
			{
				stringBuilder.Append(line.Substring(i - num));
			}
			return stringBuilder.ToString();
		}

		public static int SearchBracketBackward(IDocument document, int offset, char openBracket, char closingBracket)
		{
			return document.FormattingStrategy.SearchBracketBackward(document, offset, openBracket, closingBracket);
		}

		public static int SearchBracketForward(IDocument document, int offset, char openBracket, char closingBracket)
		{
			return document.FormattingStrategy.SearchBracketForward(document, offset, openBracket, closingBracket);
		}

		public enum CharacterType
		{
			LetterDigitOrUnderscore,
			WhiteSpace,
			Other
		}
	}
}