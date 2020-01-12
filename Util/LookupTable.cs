using ICSharpCode.TextEditor.Document;
using System;
using System.Reflection;

namespace ICSharpCode.TextEditor.Util
{
	public class LookupTable
	{
		private LookupTable.Node root = new LookupTable.Node(null, null);

		private bool casesensitive;

		private int length;

		public int Count
		{
			get
			{
				return this.length;
			}
		}

		public object this[IDocument document, LineSegment line, int offset, int length]
		{
			get
			{
				if (length == 0)
				{
					return null;
				}
				LookupTable.Node item = this.root;
				int num = line.Offset + offset;
				if (!this.casesensitive)
				{
					for (int i = 0; i < length; i++)
					{
						int upper = char.ToUpper(document.GetCharAt(num + i)) % 'Ā';
						item = item[upper];
						if (item == null)
						{
							return null;
						}
						if (item.color != null && TextUtility.RegionMatches(document, this.casesensitive, num, length, item.word))
						{
							return item.color;
						}
					}
				}
				else
				{
					for (int j = 0; j < length; j++)
					{
						int charAt = document.GetCharAt(num + j) % 'Ā';
						item = item[charAt];
						if (item == null)
						{
							return null;
						}
						if (item.color != null && TextUtility.RegionMatches(document, num, length, item.word))
						{
							return item.color;
						}
					}
				}
				return null;
			}
		}

		public object this[string keyword]
		{
			set
			{
				LookupTable.Node node = this.root;
				LookupTable.Node item = this.root;
				if (!this.casesensitive)
				{
					keyword = keyword.ToUpper();
				}
				this.length++;
				for (int i = 0; i < keyword.Length; i++)
				{
					int num = keyword[i] % 'Ā';
					char chr = keyword[i];
					item = item[num];
					if (item == null)
					{
						node[num] = new LookupTable.Node(value, keyword);
						return;
					}
					if (item.word != null && item.word.Length != i)
					{
						string str = item.word;
						object obj = item.color;
						object obj1 = null;
						string str1 = (string)obj1;
						item.word = (string)obj1;
						item.color = str1;
						this[str] = obj;
					}
					if (i == keyword.Length - 1)
					{
						item.word = keyword;
						item.color = value;
						return;
					}
					node = item;
				}
			}
		}

		public LookupTable(bool casesensitive)
		{
			this.casesensitive = casesensitive;
		}

		private class Node
		{
			public string word;

			public object color;

			private LookupTable.Node[] children;

			public LookupTable.Node this[int index]
			{
				get
				{
					if (this.children == null)
					{
						return null;
					}
					return this.children[index];
				}
				set
				{
					if (this.children == null)
					{
						this.children = new LookupTable.Node[256];
					}
					this.children[index] = value;
				}
			}

			public Node(object color, string word)
			{
				this.word = word;
				this.color = color;
			}
		}
	}
}