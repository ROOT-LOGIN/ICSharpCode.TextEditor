using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;

namespace ICSharpCode.TextEditor.Document
{
	public class DefaultHighlightingStrategy : IHighlightingStrategyUsingRuleSets, IHighlightingStrategy
	{
		private string name;

		private List<HighlightRuleSet> rules = new List<HighlightRuleSet>();

		private Dictionary<string, HighlightColor> environmentColors = new Dictionary<string, HighlightColor>();

		private Dictionary<string, string> properties = new Dictionary<string, string>();

		private string[] extensions;

		private HighlightColor digitColor;

		private HighlightRuleSet defaultRuleSet;

		private HighlightColor defaultTextColor;

		protected LineSegment currentLine;

		protected int currentLineNumber;

		protected SpanStack currentSpanStack;

		protected bool inSpan;

		protected Span activeSpan;

		protected HighlightRuleSet activeRuleSet;

		protected int currentOffset;

		protected int currentLength;

		public HighlightColor DefaultTextColor
		{
			get
			{
				return this.defaultTextColor;
			}
		}

		public HighlightColor DigitColor
		{
			get
			{
				return this.digitColor;
			}
			set
			{
				this.digitColor = value;
			}
		}

		public IEnumerable<KeyValuePair<string, HighlightColor>> EnvironmentColors
		{
			get
			{
				return this.environmentColors;
			}
		}

		public string[] Extensions
		{
			get
			{
				return JustDecompileGenerated_get_Extensions();
			}
			set
			{
				JustDecompileGenerated_set_Extensions(value);
			}
		}

		public string[] JustDecompileGenerated_get_Extensions()
		{
			return this.extensions;
		}

		public void JustDecompileGenerated_set_Extensions(string[] value)
		{
			this.extensions = value;
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public Dictionary<string, string> Properties
		{
			get
			{
				return this.properties;
			}
		}

		public List<HighlightRuleSet> Rules
		{
			get
			{
				return this.rules;
			}
		}

		public DefaultHighlightingStrategy() : this("Default")
		{
		}

		public DefaultHighlightingStrategy(string name)
		{
			this.name = name;
			this.digitColor = new HighlightColor(SystemColors.WindowText, false, false);
			this.defaultTextColor = new HighlightColor(SystemColors.WindowText, false, false);
			this.environmentColors["Default"] = new HighlightBackground("WindowText", "Window", false, false);
			this.environmentColors["Selection"] = new HighlightColor("HighlightText", "Highlight", false, false);
			this.environmentColors["VRuler"] = new HighlightColor("ControlLight", "Window", false, false);
			this.environmentColors["InvalidLines"] = new HighlightColor(Color.Red, false, false);
			this.environmentColors["CaretMarker"] = new HighlightColor(Color.Yellow, false, false);
			this.environmentColors["CaretLine"] = new HighlightBackground("ControlLight", "Window", false, false);
			this.environmentColors["LineNumbers"] = new HighlightBackground("ControlDark", "Window", false, false);
			this.environmentColors["FoldLine"] = new HighlightColor("ControlDark", false, false);
			this.environmentColors["FoldMarker"] = new HighlightColor("WindowText", "Window", false, false);
			this.environmentColors["SelectedFoldLine"] = new HighlightColor("WindowText", false, false);
			this.environmentColors["EOLMarkers"] = new HighlightColor("ControlLight", "Window", false, false);
			this.environmentColors["SpaceMarkers"] = new HighlightColor("ControlLight", "Window", false, false);
			this.environmentColors["TabMarkers"] = new HighlightColor("ControlLight", "Window", false, false);
		}

		public void AddRuleSet(HighlightRuleSet aRuleSet)
		{
			HighlightRuleSet highlightRuleSet = this.FindHighlightRuleSet(aRuleSet.Name);
			if (highlightRuleSet != null)
			{
				highlightRuleSet.MergeFrom(aRuleSet);
				return;
			}
			this.rules.Add(aRuleSet);
		}

		public HighlightRuleSet FindHighlightRuleSet(string name)
		{
			HighlightRuleSet highlightRuleSet;
			List<HighlightRuleSet>.Enumerator enumerator = this.rules.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					HighlightRuleSet current = enumerator.Current;
					if (current.Name != name)
					{
						continue;
					}
					highlightRuleSet = current;
					return highlightRuleSet;
				}
				return null;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return highlightRuleSet;
		}

		public HighlightColor GetColor(IDocument document, LineSegment currentSegment, int currentOffset, int currentLength)
		{
			return this.GetColor(this.defaultRuleSet, document, currentSegment, currentOffset, currentLength);
		}

		protected virtual HighlightColor GetColor(HighlightRuleSet ruleSet, IDocument document, LineSegment currentSegment, int currentOffset, int currentLength)
		{
			if (ruleSet == null)
			{
				return null;
			}
			if (ruleSet.Reference != null)
			{
				return ruleSet.Highlighter.GetColor(document, currentSegment, currentOffset, currentLength);
			}
			return (HighlightColor)ruleSet.KeyWords[document, currentSegment, currentOffset, currentLength];
		}

		public HighlightColor GetColorFor(string name)
		{
			HighlightColor highlightColor;
			if (this.environmentColors.TryGetValue(name, out highlightColor))
			{
				return highlightColor;
			}
			return this.defaultTextColor;
		}

		private static string GetRegString(LineSegment lineSegment, char[] expr, int index, IDocument document)
		{
			int num = 0;
			StringBuilder stringBuilder = new StringBuilder();
			int num1 = 0;
			while (num1 < (int)expr.Length && index + num < lineSegment.Length)
			{
				if (expr[num1] != '@')
				{
					if (expr[num1] != document.GetCharAt(lineSegment.Offset + index + num))
					{
						return stringBuilder.ToString();
					}
					stringBuilder.Append(document.GetCharAt(lineSegment.Offset + index + num));
				}
				else
				{
					num1++;
					if (num1 == (int)expr.Length)
					{
						throw new HighlightingDefinitionInvalidException("Unexpected end of @ sequence, use @@ to look for a single @.");
					}
					char chr = expr[num1];
					if (chr == '!')
					{
						StringBuilder stringBuilder1 = new StringBuilder();
						num1++;
						while (num1 < (int)expr.Length)
						{
							if (expr[num1] != '@')
							{
								int num2 = num1;
								num1 = num2 + 1;
								stringBuilder1.Append(expr[num2]);
							}
							else
							{
								break;
							}
						}
					}
					else if (chr == '@')
					{
						stringBuilder.Append(document.GetCharAt(lineSegment.Offset + index + num));
					}
				}
				num1++;
				num++;
			}
			return stringBuilder.ToString();
		}

		public HighlightRuleSet GetRuleSet(Span aSpan)
		{
			if (aSpan == null)
			{
				return this.defaultRuleSet;
			}
			if (aSpan.RuleSet == null)
			{
				return null;
			}
			if (aSpan.RuleSet.Reference == null)
			{
				return aSpan.RuleSet;
			}
			return aSpan.RuleSet.Highlighter.GetRuleSet(null);
		}

		protected void ImportSettingsFrom(DefaultHighlightingStrategy source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			this.properties = source.properties;
			this.extensions = source.extensions;
			this.digitColor = source.digitColor;
			this.defaultRuleSet = source.defaultRuleSet;
			this.name = source.name;
			this.rules = source.rules;
			this.environmentColors = source.environmentColors;
			this.defaultTextColor = source.defaultTextColor;
		}

		public virtual void MarkTokens(IDocument document)
		{
			LineSegment lineSegment;
			SpanStack spanStacks;
			SpanStack spanStacks1;
			if (this.Rules.Count == 0)
			{
				return;
			}
			for (int i = 0; i < document.TotalNumberOfLines; i++)
			{
				if (i > 0)
				{
					lineSegment = document.GetLineSegment(i - 1);
				}
				else
				{
					lineSegment = null;
				}
				LineSegment lineSegment1 = lineSegment;
				if (i >= document.LineSegmentCollection.Count)
				{
					break;
				}
				if (lineSegment1 == null || lineSegment1.HighlightSpanStack == null)
				{
					spanStacks = null;
				}
				else
				{
					spanStacks = lineSegment1.HighlightSpanStack.Clone();
				}
				this.currentSpanStack = spanStacks;
				if (this.currentSpanStack != null)
				{
					while (!this.currentSpanStack.IsEmpty && this.currentSpanStack.Peek().StopEOL)
					{
						this.currentSpanStack.Pop();
					}
					if (this.currentSpanStack.IsEmpty)
					{
						this.currentSpanStack = null;
					}
				}
				this.currentLine = document.LineSegmentCollection[i];
				if (this.currentLine.Length == -1)
				{
					return;
				}
				this.currentLineNumber = i;
				List<TextWord> textWords = this.ParseLine(document);
				if (this.currentLine.Words != null)
				{
					this.currentLine.Words.Clear();
				}
				this.currentLine.Words = textWords;
				LineSegment lineSegment2 = this.currentLine;
				if (this.currentSpanStack == null || this.currentSpanStack.IsEmpty)
				{
					spanStacks1 = null;
				}
				else
				{
					spanStacks1 = this.currentSpanStack;
				}
				lineSegment2.HighlightSpanStack = spanStacks1;
			}
			document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
			document.CommitUpdate();
			this.currentLine = null;
		}

		public virtual void MarkTokens(IDocument document, List<LineSegment> inputLines)
		{
			if (this.Rules.Count == 0)
			{
				return;
			}
			Dictionary<LineSegment, bool> lineSegments = new Dictionary<LineSegment, bool>();
			bool flag = false;
			int count = document.LineSegmentCollection.Count;
			foreach (LineSegment inputLine in inputLines)
			{
				if (lineSegments.ContainsKey(inputLine))
				{
					continue;
				}
				int lineNumber = inputLine.LineNumber;
				bool flag1 = true;
				if (lineNumber == -1)
				{
					continue;
				}
				while (flag1 && lineNumber < count)
				{
					flag1 = this.MarkTokensInLine(document, lineNumber, ref flag);
					lineSegments[this.currentLine] = true;
					lineNumber++;
				}
			}
			if (flag || inputLines.Count > 20)
			{
				document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
			}
			else
			{
				foreach (LineSegment lineSegment in inputLines)
				{
					document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, lineSegment.LineNumber));
				}
			}
			document.CommitUpdate();
			this.currentLine = null;
		}

		private bool MarkTokensInLine(IDocument document, int lineNumber, ref bool spanChanged)
		{
			LineSegment lineSegment;
			SpanStack spanStacks;
			SpanStack spanStacks1;
			this.currentLineNumber = lineNumber;
			bool flag = false;
			if (lineNumber > 0)
			{
				lineSegment = document.GetLineSegment(lineNumber - 1);
			}
			else
			{
				lineSegment = null;
			}
			LineSegment lineSegment1 = lineSegment;
			if (lineSegment1 == null || lineSegment1.HighlightSpanStack == null)
			{
				spanStacks = null;
			}
			else
			{
				spanStacks = lineSegment1.HighlightSpanStack.Clone();
			}
			this.currentSpanStack = spanStacks;
			if (this.currentSpanStack != null)
			{
				while (!this.currentSpanStack.IsEmpty && this.currentSpanStack.Peek().StopEOL)
				{
					this.currentSpanStack.Pop();
				}
				if (this.currentSpanStack.IsEmpty)
				{
					this.currentSpanStack = null;
				}
			}
			this.currentLine = document.LineSegmentCollection[lineNumber];
			if (this.currentLine.Length == -1)
			{
				return false;
			}
			List<TextWord> textWords = this.ParseLine(document);
			if (this.currentSpanStack != null && this.currentSpanStack.IsEmpty)
			{
				this.currentSpanStack = null;
			}
			if (this.currentLine.HighlightSpanStack == this.currentSpanStack)
			{
				flag = false;
			}
			else if (this.currentLine.HighlightSpanStack == null)
			{
				flag = false;
				foreach (Span span in this.currentSpanStack)
				{
					if (span.StopEOL)
					{
						continue;
					}
					spanChanged = true;
					flag = true;
					break;
				}
			}
			else if (this.currentSpanStack != null)
			{
				SpanStack.Enumerator enumerator = this.currentSpanStack.GetEnumerator();
				SpanStack.Enumerator enumerator1 = this.currentLine.HighlightSpanStack.GetEnumerator();
				bool flag1 = false;
				while (!flag1)
				{
					bool flag2 = false;
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.StopEOL)
						{
							continue;
						}
						flag2 = true;
						break;
					}
					bool flag3 = false;
					while (enumerator1.MoveNext())
					{
						if (enumerator1.Current.StopEOL)
						{
							continue;
						}
						flag3 = true;
						break;
					}
					if (!flag2 && !flag3)
					{
						flag1 = true;
						flag = false;
					}
					else if (!flag2 || !flag3)
					{
						spanChanged = true;
						flag1 = true;
						flag = true;
					}
					else
					{
						if (enumerator.Current == enumerator1.Current)
						{
							continue;
						}
						flag1 = true;
						flag = true;
						spanChanged = true;
					}
				}
			}
			else
			{
				flag = false;
				foreach (Span highlightSpanStack in this.currentLine.HighlightSpanStack)
				{
					if (highlightSpanStack.StopEOL)
					{
						continue;
					}
					spanChanged = true;
					flag = true;
					break;
				}
			}
			if (this.currentLine.Words != null)
			{
				this.currentLine.Words.Clear();
			}
			this.currentLine.Words = textWords;
			LineSegment lineSegment2 = this.currentLine;
			if (this.currentSpanStack == null || this.currentSpanStack.IsEmpty)
			{
				spanStacks1 = null;
			}
			else
			{
				spanStacks1 = this.currentSpanStack;
			}
			lineSegment2.HighlightSpanStack = spanStacks1;
			return flag;
		}

		private static bool MatchExpr(LineSegment lineSegment, char[] expr, int index, IDocument document, bool ignoreCase)
		{
			int i;
			int j;
			int num = 0;
			int num1 = 0;
			while (num < (int)expr.Length)
			{
				if (expr[num] != '@')
				{
					if (index + num1 >= lineSegment.Length)
					{
						return false;
					}
					char chr = (ignoreCase ? char.ToUpperInvariant(document.GetCharAt(lineSegment.Offset + index + num1)) : document.GetCharAt(lineSegment.Offset + index + num1));
					if (chr != (ignoreCase ? char.ToUpperInvariant(expr[num]) : expr[num]))
					{
						return false;
					}
				}
				else
				{
					num++;
					if (num == (int)expr.Length)
					{
						throw new HighlightingDefinitionInvalidException("Unexpected end of @ sequence, use @@ to look for a single @.");
					}
					char chr1 = expr[num];
					if (chr1 <= '-')
					{
						if (chr1 == '!')
						{
							StringBuilder stringBuilder = new StringBuilder();
							num++;
							while (num < (int)expr.Length && expr[num] != '@')
							{
								int num2 = num;
								num = num2 + 1;
								stringBuilder.Append(expr[num2]);
							}
							if (lineSegment.Offset + index + num1 + stringBuilder.Length < document.TextLength)
							{
								for (i = 0; i < stringBuilder.Length; i++)
								{
									char chr2 = (ignoreCase ? char.ToUpperInvariant(document.GetCharAt(lineSegment.Offset + index + num1 + i)) : document.GetCharAt(lineSegment.Offset + index + num1 + i));
									if (chr2 != (ignoreCase ? char.ToUpperInvariant(stringBuilder[i]) : stringBuilder[i]))
									{
										break;
									}
								}
								if (i >= stringBuilder.Length)
								{
									return false;
								}
							}
						}
						else if (chr1 == '-')
						{
							StringBuilder stringBuilder1 = new StringBuilder();
							num++;
							while (num < (int)expr.Length && expr[num] != '@')
							{
								int num3 = num;
								num = num3 + 1;
								stringBuilder1.Append(expr[num3]);
							}
							if (index - stringBuilder1.Length >= 0)
							{
								for (j = 0; j < stringBuilder1.Length; j++)
								{
									char chr3 = (ignoreCase ? char.ToUpperInvariant(document.GetCharAt(lineSegment.Offset + index - stringBuilder1.Length + j)) : document.GetCharAt(lineSegment.Offset + index - stringBuilder1.Length + j));
									if (chr3 != (ignoreCase ? char.ToUpperInvariant(stringBuilder1[j]) : stringBuilder1[j]))
									{
										break;
									}
								}
								if (j >= stringBuilder1.Length)
								{
									return false;
								}
							}
						}
					}
					else if (chr1 != '@')
					{
						if (chr1 == 'C' && index + num1 != lineSegment.Offset && index + num1 < lineSegment.Offset + lineSegment.Length)
						{
							char charAt = document.GetCharAt(lineSegment.Offset + index + num1);
							if (!char.IsWhiteSpace(charAt) && !char.IsPunctuation(charAt))
							{
								return false;
							}
						}
					}
					else if (index + num1 >= lineSegment.Length || 64 != document.GetCharAt(lineSegment.Offset + index + num1))
					{
						return false;
					}
				}
				num++;
				num1++;
			}
			return true;
		}

		protected virtual void OnParsedLine(IDocument document, LineSegment currentLine, List<TextWord> words)
		{
		}

		protected virtual bool OverrideSpan(string spanBegin, IDocument document, List<TextWord> words, Span span, ref int lineOffset)
		{
			return false;
		}

		private List<TextWord> ParseLine(IDocument document)
		{
			List<TextWord> textWords = new List<TextWord>();
			HighlightColor highlightColor = null;
			this.currentOffset = 0;
			this.currentLength = 0;
			this.UpdateSpanStateVariables();
			int length = this.currentLine.Length;
			int offset = this.currentLine.Offset;
			for (int i = 0; i < length; i++)
			{
				char charAt = document.GetCharAt(offset + i);
				char chr = charAt;
				switch (chr)
				{
					case '\t':
					{
						this.PushCurWord(document, ref highlightColor, textWords);
						if (this.activeSpan == null || !this.activeSpan.Color.HasBackground)
						{
							textWords.Add(TextWord.Tab);
						}
						else
						{
							textWords.Add(new TextWord.TabTextWord(this.activeSpan.Color));
						}
						this.currentOffset++;
						break;
					}
					case '\n':
					case '\r':
					{
						this.PushCurWord(document, ref highlightColor, textWords);
						this.currentOffset++;
						break;
					}
					case '\v':
					case '\f':
					{
						char escapeCharacter = '\0';
						if (this.activeSpan != null && this.activeSpan.EscapeCharacter != 0)
						{
							escapeCharacter = this.activeSpan.EscapeCharacter;
						}
						else if (this.activeRuleSet != null)
						{
							escapeCharacter = this.activeRuleSet.EscapeCharacter;
						}
						if (escapeCharacter != 0 && escapeCharacter == charAt)
						{
							if (this.activeSpan == null || this.activeSpan.End == null || (int)this.activeSpan.End.Length != 1 || escapeCharacter != this.activeSpan.End[0])
							{
								this.currentLength++;
								if (i + 1 < length)
								{
									this.currentLength++;
								}
								this.PushCurWord(document, ref highlightColor, textWords);
								i++;
								break;
							}
							else if (i + 1 < length && document.GetCharAt(offset + i + 1) == escapeCharacter)
							{
								this.currentLength += 2;
								this.PushCurWord(document, ref highlightColor, textWords);
								i++;
								break;
							}
						}
						if (!this.inSpan && (char.IsDigit(charAt) || charAt == '.' && i + 1 < length && char.IsDigit(document.GetCharAt(offset + i + 1))) && this.currentLength == 0)
						{
							bool flag = false;
							bool flag1 = false;
							if (charAt != '0' || i + 1 >= length || char.ToUpper(document.GetCharAt(offset + i + 1)) != 'X')
							{
								this.currentLength++;
								while (i + 1 < length && char.IsDigit(document.GetCharAt(offset + i + 1)))
								{
									i++;
									this.currentLength++;
								}
							}
							else
							{
								this.currentLength++;
								i++;
								this.currentLength++;
								flag = true;
								while (i + 1 < length)
								{
									if ("0123456789ABCDEF".IndexOf(char.ToUpper(document.GetCharAt(offset + i + 1))) != -1)
									{
										i++;
										this.currentLength++;
									}
									else
									{
										break;
									}
								}
							}
							if (!flag && i + 1 < length && document.GetCharAt(offset + i + 1) == '.')
							{
								flag1 = true;
								i++;
								this.currentLength++;
								while (i + 1 < length && char.IsDigit(document.GetCharAt(offset + i + 1)))
								{
									i++;
									this.currentLength++;
								}
							}
							if (i + 1 < length && char.ToUpper(document.GetCharAt(offset + i + 1)) == 'E')
							{
								flag1 = true;
								i++;
								this.currentLength++;
								if (i + 1 < length && (document.GetCharAt(offset + i + 1) == '+' || document.GetCharAt(this.currentLine.Offset + i + 1) == '-'))
								{
									i++;
									this.currentLength++;
								}
								while (i + 1 < this.currentLine.Length && char.IsDigit(document.GetCharAt(offset + i + 1)))
								{
									i++;
									this.currentLength++;
								}
							}
							if (i + 1 < this.currentLine.Length)
							{
								char upper = char.ToUpper(document.GetCharAt(offset + i + 1));
								if (upper == 'F' || upper == 'M' || upper == 'D')
								{
									flag1 = true;
									i++;
									this.currentLength++;
								}
							}
							if (!flag1)
							{
								bool flag2 = false;
								if (i + 1 < length && char.ToUpper(document.GetCharAt(offset + i + 1)) == 'U')
								{
									i++;
									this.currentLength++;
									flag2 = true;
								}
								if (i + 1 < length && char.ToUpper(document.GetCharAt(offset + i + 1)) == 'L')
								{
									i++;
									this.currentLength++;
									if (!flag2 && i + 1 < length && char.ToUpper(document.GetCharAt(offset + i + 1)) == 'U')
									{
										i++;
										this.currentLength++;
									}
								}
							}
							textWords.Add(new TextWord(document, this.currentLine, this.currentOffset, this.currentLength, this.DigitColor, false));
							this.currentOffset += this.currentLength;
							this.currentLength = 0;
							break;
						}
						else if (!this.inSpan || this.activeSpan.End == null || (int)this.activeSpan.End.Length <= 0 || !DefaultHighlightingStrategy.MatchExpr(this.currentLine, this.activeSpan.End, i, document, this.activeSpan.IgnoreCase))
						{
							if (this.activeRuleSet != null)
							{
								foreach (Span span in this.activeRuleSet.Spans)
								{
									if (span.IsBeginSingleWord && this.currentLength != 0)
									{
										continue;
									}
									if (span.IsBeginStartOfLine.HasValue)
									{
										if (span.IsBeginStartOfLine.Value != (this.currentLength != 0 ? false : textWords.TrueForAll((TextWord textWord) => textWord.Type != TextWordType.Word)))
										{
											continue;
										}
									}
									if (!DefaultHighlightingStrategy.MatchExpr(this.currentLine, span.Begin, i, document, this.activeRuleSet.IgnoreCase))
									{
										continue;
									}
									this.PushCurWord(document, ref highlightColor, textWords);
									string regString = DefaultHighlightingStrategy.GetRegString(this.currentLine, span.Begin, i, document);
									if (!this.OverrideSpan(regString, document, textWords, span, ref i))
									{
										this.currentLength += regString.Length;
										textWords.Add(new TextWord(document, this.currentLine, this.currentOffset, this.currentLength, span.BeginColor, false));
										this.currentOffset += this.currentLength;
										this.currentLength = 0;
										i = i + (regString.Length - 1);
										if (this.currentSpanStack == null)
										{
											this.currentSpanStack = new SpanStack();
										}
										this.currentSpanStack.Push(span);
										span.IgnoreCase = this.activeRuleSet.IgnoreCase;
										this.UpdateSpanStateVariables();
									}
                                        goto label0;
								}
							}
							if (this.activeRuleSet != null && charAt < 'Ā' && this.activeRuleSet.Delimiters[charAt])
							{
								this.PushCurWord(document, ref highlightColor, textWords);
								if (this.currentOffset + this.currentLength + 1 < this.currentLine.Length)
								{
									this.currentLength++;
									this.PushCurWord(document, ref highlightColor, textWords);
									break;
								}
							}
							this.currentLength++;
							break;
						}
						else
						{
							this.PushCurWord(document, ref highlightColor, textWords);
							string str = DefaultHighlightingStrategy.GetRegString(this.currentLine, this.activeSpan.End, i, document);
							this.currentLength += str.Length;
							textWords.Add(new TextWord(document, this.currentLine, this.currentOffset, this.currentLength, this.activeSpan.EndColor, false));
							this.currentOffset += this.currentLength;
							this.currentLength = 0;
							i = i + (str.Length - 1);
							this.currentSpanStack.Pop();
							this.UpdateSpanStateVariables();
							break;
						}
					}
					default:
					{
						if (chr == ' ')
						{
							this.PushCurWord(document, ref highlightColor, textWords);
							if (this.activeSpan == null || !this.activeSpan.Color.HasBackground)
							{
								textWords.Add(TextWord.Space);
							}
							else
							{
								textWords.Add(new TextWord.SpaceTextWord(this.activeSpan.Color));
							}
							this.currentOffset++;
							break;
						}
						else
						{
							goto case '\f';
						}
					}
				}
                label0:;
			}
			this.PushCurWord(document, ref highlightColor, textWords);
			this.OnParsedLine(document, this.currentLine, textWords);
			return textWords;
		}

		private void PushCurWord(IDocument document, ref HighlightColor markNext, List<TextWord> words)
		{
			if (this.currentLength > 0)
			{
				if (words.Count > 0 && this.activeRuleSet != null)
				{
					TextWord item = null;
					int count = words.Count - 1;
					while (count >= 0)
					{
						if (words[count].IsWhiteSpace)
						{
							count--;
						}
						else
						{
							item = words[count];
							if (!item.HasDefaultColor)
							{
								break;
							}
							PrevMarker prevMarker = (PrevMarker)this.activeRuleSet.PrevMarkers[document, this.currentLine, this.currentOffset, this.currentLength];
							if (prevMarker == null)
							{
								break;
							}
							item.SyntaxColor = prevMarker.Color;
							break;
						}
					}
				}
				if (!this.inSpan)
				{
					HighlightColor highlightColor = (markNext != null ? markNext : this.GetColor(this.activeRuleSet, document, this.currentLine, this.currentOffset, this.currentLength));
					if (highlightColor != null)
					{
						words.Add(new TextWord(document, this.currentLine, this.currentOffset, this.currentLength, highlightColor, false));
					}
					else
					{
						words.Add(new TextWord(document, this.currentLine, this.currentOffset, this.currentLength, this.DefaultTextColor, true));
					}
				}
				else
				{
					HighlightColor color = null;
					bool flag = true;
					if (this.activeSpan.Rule != null)
					{
						color = this.GetColor(this.activeRuleSet, document, this.currentLine, this.currentOffset, this.currentLength);
						flag = false;
					}
					else
					{
						color = this.activeSpan.Color;
					}
					if (color == null)
					{
						color = this.activeSpan.Color;
						if (color.Color == Color.Transparent)
						{
							color = this.DefaultTextColor;
						}
						flag = true;
					}
					words.Add(new TextWord(document, this.currentLine, this.currentOffset, this.currentLength, (markNext != null ? markNext : color), flag));
				}
				if (this.activeRuleSet != null)
				{
					NextMarker nextMarker = (NextMarker)this.activeRuleSet.NextMarkers[document, this.currentLine, this.currentOffset, this.currentLength];
					if (nextMarker == null)
					{
						markNext = null;
					}
					else
					{
						if (nextMarker.MarkMarker && words.Count > 0)
						{
							TextWord textWord = words[words.Count - 1];
							textWord.SyntaxColor = nextMarker.Color;
						}
						markNext = nextMarker.Color;
					}
				}
				this.currentOffset += this.currentLength;
				this.currentLength = 0;
			}
		}

		private void ResolveExternalReferences()
		{
			foreach (HighlightRuleSet rule in this.Rules)
			{
				rule.Highlighter = this;
				if (rule.Reference == null)
				{
					continue;
				}
				IHighlightingStrategy highlightingStrategy = HighlightingManager.Manager.FindHighlighter(rule.Reference);
				if (highlightingStrategy == null)
				{
					string[] reference = new string[] { "The mode defintion ", rule.Reference, " which is refered from the ", this.Name, " mode definition could not be found" };
					throw new HighlightingDefinitionInvalidException(string.Concat(reference));
				}
				if (!(highlightingStrategy is IHighlightingStrategyUsingRuleSets))
				{
					string[] strArrays = new string[] { "The mode defintion ", rule.Reference, " which is refered from the ", this.Name, " mode definition does not implement IHighlightingStrategyUsingRuleSets" };
					throw new HighlightingDefinitionInvalidException(string.Concat(strArrays));
				}
				rule.Highlighter = (IHighlightingStrategyUsingRuleSets)highlightingStrategy;
			}
		}

		public void ResolveReferences()
		{
			this.ResolveRuleSetReferences();
			this.ResolveExternalReferences();
		}

		private void ResolveRuleSetReferences()
		{
			foreach (HighlightRuleSet rule in this.Rules)
			{
				if (rule.Name == null)
				{
					this.defaultRuleSet = rule;
				}
				foreach (Span span in rule.Spans)
				{
					if (span.Rule == null)
					{
						span.RuleSet = null;
					}
					else
					{
						bool flag = false;
						foreach (HighlightRuleSet highlightRuleSet in this.Rules)
						{
							if (highlightRuleSet.Name != span.Rule)
							{
								continue;
							}
							flag = true;
							span.RuleSet = highlightRuleSet;
							break;
						}
						if (flag)
						{
							continue;
						}
						span.RuleSet = null;
						throw new HighlightingDefinitionInvalidException(string.Concat("The RuleSet ", span.Rule, " could not be found in mode definition ", this.Name));
					}
				}
			}
			if (this.defaultRuleSet == null)
			{
				throw new HighlightingDefinitionInvalidException(string.Concat("No default RuleSet is defined for mode definition ", this.Name));
			}
		}

		public void SetColorFor(string name, HighlightColor color)
		{
			if (name == "Default")
			{
				this.defaultTextColor = new HighlightColor(color.Color, color.Bold, color.Italic);
			}
			this.environmentColors[name] = color;
		}

		private void UpdateSpanStateVariables()
		{
			Span span;
			this.inSpan = (this.currentSpanStack == null ? false : !this.currentSpanStack.IsEmpty);
			if (this.inSpan)
			{
				span = this.currentSpanStack.Peek();
			}
			else
			{
				span = null;
			}
			this.activeSpan = span;
			this.activeRuleSet = this.GetRuleSet(this.activeSpan);
		}
	}
}