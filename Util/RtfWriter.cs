using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ICSharpCode.TextEditor.Util
{
	public class RtfWriter
	{
		private static Dictionary<string, int> colors;

		private static int colorNum;

		private static StringBuilder colorString;

		public RtfWriter()
		{
		}

		private static void AppendText(StringBuilder rtfOutput, string text)
		{
			string str = text;
			for (int i = 0; i < str.Length; i++)
			{
				char chr = str[i];
				char chr1 = chr;
				if (chr1 == '\\')
				{
					rtfOutput.Append("\\\\");
				}
				else
				{
					switch (chr1)
					{
						case '{':
						{
							rtfOutput.Append("\\{");
							break;
						}
						case '|':
						{
							if (chr >= 'Ā')
							{
								short num = (short)chr;
								rtfOutput.Append(string.Concat("\\u", num.ToString(), "?"));
								break;
							}
							else
							{
								rtfOutput.Append(chr);
								break;
							}
						}
						case '}':
						{
							rtfOutput.Append("\\}");
							break;
						}
						default:
						{
							goto case '|';
						}
					}
				}
			}
		}

		private static void BuildColorTable(IDocument doc, StringBuilder rtf)
		{
			rtf.Append("{\\colortbl ;");
			rtf.Append(RtfWriter.colorString.ToString());
			rtf.Append("}");
		}

		private static string BuildFileContent(TextArea textArea)
		{
			string str;
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			Color black = Color.Black;
			bool italic = false;
			bool bold = false;
			bool flag1 = false;
			foreach (ISelection selectionCollection in textArea.SelectionManager.SelectionCollection)
			{
				int offset = textArea.Document.PositionToOffset(selectionCollection.StartPosition);
				int num = textArea.Document.PositionToOffset(selectionCollection.EndPosition);
				for (int i = selectionCollection.StartPosition.Y; i <= selectionCollection.EndPosition.Y; i++)
				{
					LineSegment lineSegment = textArea.Document.GetLineSegment(i);
					int length = lineSegment.Offset;
					if (lineSegment.Words != null)
					{
						foreach (TextWord word in lineSegment.Words)
						{
							switch (word.Type)
							{
								case TextWordType.Word:
								{
									Color color = word.Color;
									if (length + word.Word.Length > offset && length < num)
									{
										object[] r = new object[] { color.R, ", ", color.G, ", ", color.B };
										string str1 = string.Concat(r);
										if (!RtfWriter.colors.ContainsKey(str1))
										{
											int num1 = RtfWriter.colorNum + 1;
											RtfWriter.colorNum = num1;
											RtfWriter.colors[str1] = num1;
											StringBuilder stringBuilder1 = RtfWriter.colorString;
											object[] objArray = new object[] { "\\red", color.R, "\\green", color.G, "\\blue", color.B, ";" };
											stringBuilder1.Append(string.Concat(objArray));
										}
										if (color != black || flag)
										{
											int item = RtfWriter.colors[str1];
											stringBuilder.Append(string.Concat("\\cf", item.ToString()));
											black = color;
											flag1 = true;
										}
										if (italic != word.Italic)
										{
											if (!word.Italic)
											{
												stringBuilder.Append("\\i0");
											}
											else
											{
												stringBuilder.Append("\\i");
											}
											italic = word.Italic;
											flag1 = true;
										}
										if (bold != word.Bold)
										{
											if (!word.Bold)
											{
												stringBuilder.Append("\\b0");
											}
											else
											{
												stringBuilder.Append("\\b");
											}
											bold = word.Bold;
											flag1 = true;
										}
										if (flag)
										{
											stringBuilder.Append(string.Concat("\\f0\\fs", textArea.TextEditorProperties.Font.Size * 2f));
											flag = false;
										}
										if (flag1)
										{
											stringBuilder.Append(' ');
											flag1 = false;
										}
										if (length >= offset)
										{
											str = (length + word.Word.Length <= num ? word.Word : word.Word.Substring(0, length + word.Word.Length - num));
										}
										else
										{
											str = word.Word.Substring(offset - length);
										}
										RtfWriter.AppendText(stringBuilder, str);
									}
									length += word.Length;
									continue;
								}
								case TextWordType.Space:
								{
									if (selectionCollection.ContainsOffset(length))
									{
										stringBuilder.Append(' ');
									}
									length++;
									continue;
								}
								case TextWordType.Tab:
								{
									if (selectionCollection.ContainsOffset(length))
									{
										stringBuilder.Append("\\tab");
									}
									length++;
									flag1 = true;
									continue;
								}
								default:
								{
									continue;
								}
							}
						}
						if (length < num)
						{
							stringBuilder.Append("\\par");
						}
						stringBuilder.Append('\n');
					}
				}
			}
			return stringBuilder.ToString();
		}

		private static void BuildFontTable(IDocument doc, StringBuilder rtf)
		{
			rtf.Append("{\\fonttbl");
			rtf.Append(string.Concat("{\\f0\\fmodern\\fprq1\\fcharset0 ", doc.TextEditorProperties.Font.Name, ";}"));
			rtf.Append("}");
		}

		public static string GenerateRtf(TextArea textArea)
		{
			RtfWriter.colors = new Dictionary<string, int>();
			RtfWriter.colorNum = 0;
			RtfWriter.colorString = new StringBuilder();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1031");
			RtfWriter.BuildFontTable(textArea.Document, stringBuilder);
			stringBuilder.Append('\n');
			string str = RtfWriter.BuildFileContent(textArea);
			RtfWriter.BuildColorTable(textArea.Document, stringBuilder);
			stringBuilder.Append('\n');
			stringBuilder.Append("\\viewkind4\\uc1\\pard");
			stringBuilder.Append(str);
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
	}
}