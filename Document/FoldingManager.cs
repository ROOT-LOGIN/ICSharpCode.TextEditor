using ICSharpCode.TextEditor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;

namespace ICSharpCode.TextEditor.Document
{
	public class FoldingManager
	{
		private List<ICSharpCode.TextEditor.Document.FoldMarker> foldMarker = new List<ICSharpCode.TextEditor.Document.FoldMarker>();

		private List<ICSharpCode.TextEditor.Document.FoldMarker> foldMarkerByEnd = new List<ICSharpCode.TextEditor.Document.FoldMarker>();

		private IFoldingStrategy foldingStrategy;

		private IDocument document;

		public IFoldingStrategy FoldingStrategy
		{
			get
			{
				return this.foldingStrategy;
			}
			set
			{
				this.foldingStrategy = value;
			}
		}

		public IList<ICSharpCode.TextEditor.Document.FoldMarker> FoldMarker
		{
			get
			{
				return this.foldMarker.AsReadOnly();
			}
		}

		internal FoldingManager(IDocument document, LineManager lineTracker)
		{
			this.document = document;
			document.DocumentChanged += new DocumentEventHandler(this.DocumentChanged);
		}

		public void DeserializeFromString(string str)
		{
			try
			{
				string[] strArrays = str.Split(new char[] { '\n' });
				for (int i = 0; i < (int)strArrays.Length && strArrays[i].Length > 0; i += 4)
				{
					int num = int.Parse(strArrays[i]);
					int num1 = int.Parse(strArrays[i + 1]);
					string str1 = strArrays[i + 2];
					bool flag = bool.Parse(strArrays[i + 3]);
					bool flag1 = false;
					foreach (ICSharpCode.TextEditor.Document.FoldMarker foldMarker in this.foldMarker)
					{
						if (foldMarker.Offset != num || foldMarker.Length != num1)
						{
							continue;
						}
						foldMarker.IsFolded = flag;
						flag1 = true;
						break;
					}
					if (!flag1)
					{
						this.foldMarker.Add(new ICSharpCode.TextEditor.Document.FoldMarker(this.document, num, num1, str1, flag));
					}
				}
				if ((int)strArrays.Length > 0)
				{
					this.NotifyFoldingsChanged(EventArgs.Empty);
				}
			}
			catch (Exception exception)
			{
			}
		}

		private void DocumentChanged(object sender, DocumentEventArgs e)
		{
			int count = this.foldMarker.Count;
			this.document.UpdateSegmentListOnDocumentChange<ICSharpCode.TextEditor.Document.FoldMarker>(this.foldMarker, e);
			if (count != this.foldMarker.Count)
			{
				this.document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
			}
		}

		public List<ICSharpCode.TextEditor.Document.FoldMarker> GetFoldedFoldingsWithEnd(int lineNumber)
		{
			return this.GetFoldingsByEndAfterColumn(lineNumber, -1, true);
		}

		public List<ICSharpCode.TextEditor.Document.FoldMarker> GetFoldedFoldingsWithStart(int lineNumber)
		{
			return this.GetFoldingsByStartAfterColumn(lineNumber, -1, true);
		}

		public List<ICSharpCode.TextEditor.Document.FoldMarker> GetFoldedFoldingsWithStartAfterColumn(int lineNumber, int column)
		{
			return this.GetFoldingsByStartAfterColumn(lineNumber, column, true);
		}

		private List<ICSharpCode.TextEditor.Document.FoldMarker> GetFoldingsByEndAfterColumn(int lineNumber, int column, bool forceFolded)
		{
			List<ICSharpCode.TextEditor.Document.FoldMarker> foldMarkers = new List<ICSharpCode.TextEditor.Document.FoldMarker>();
			if (this.foldMarker != null)
			{
				int num = this.foldMarkerByEnd.BinarySearch(new ICSharpCode.TextEditor.Document.FoldMarker(this.document, lineNumber, column, lineNumber, column), FoldingManager.EndComparer.Instance);
				if (num < 0)
				{
					num = ~num;
				}
				while (num < this.foldMarkerByEnd.Count)
				{
					ICSharpCode.TextEditor.Document.FoldMarker item = this.foldMarkerByEnd[num];
					if (item.EndLine > lineNumber)
					{
						break;
					}
					if (item.EndColumn > column && (!forceFolded || item.IsFolded))
					{
						foldMarkers.Add(item);
					}
					num++;
				}
			}
			return foldMarkers;
		}

		private List<ICSharpCode.TextEditor.Document.FoldMarker> GetFoldingsByStartAfterColumn(int lineNumber, int column, bool forceFolded)
		{
			List<ICSharpCode.TextEditor.Document.FoldMarker> foldMarkers = new List<ICSharpCode.TextEditor.Document.FoldMarker>();
			if (this.foldMarker != null)
			{
				int num = this.foldMarker.BinarySearch(new ICSharpCode.TextEditor.Document.FoldMarker(this.document, lineNumber, column, lineNumber, column), FoldingManager.StartComparer.Instance);
				if (num < 0)
				{
					num = ~num;
				}
				while (num < this.foldMarker.Count)
				{
					ICSharpCode.TextEditor.Document.FoldMarker item = this.foldMarker[num];
					if (item.StartLine > lineNumber)
					{
						break;
					}
					if (item.StartColumn > column && (!forceFolded || item.IsFolded))
					{
						foldMarkers.Add(item);
					}
					num++;
				}
			}
			return foldMarkers;
		}

		public List<ICSharpCode.TextEditor.Document.FoldMarker> GetFoldingsContainsLineNumber(int lineNumber)
		{
			List<ICSharpCode.TextEditor.Document.FoldMarker> foldMarkers = new List<ICSharpCode.TextEditor.Document.FoldMarker>();
			if (this.foldMarker != null)
			{
				foreach (ICSharpCode.TextEditor.Document.FoldMarker foldMarker in this.foldMarker)
				{
					if (foldMarker.StartLine >= lineNumber || lineNumber >= foldMarker.EndLine)
					{
						continue;
					}
					foldMarkers.Add(foldMarker);
				}
			}
			return foldMarkers;
		}

		public List<ICSharpCode.TextEditor.Document.FoldMarker> GetFoldingsFromPosition(int line, int column)
		{
			List<ICSharpCode.TextEditor.Document.FoldMarker> foldMarkers = new List<ICSharpCode.TextEditor.Document.FoldMarker>();
			if (this.foldMarker != null)
			{
				for (int i = 0; i < this.foldMarker.Count; i++)
				{
					ICSharpCode.TextEditor.Document.FoldMarker item = this.foldMarker[i];
					if (item.StartLine == line && column > item.StartColumn && (item.EndLine != line || column < item.EndColumn) || item.EndLine == line && column < item.EndColumn && (item.StartLine != line || column > item.StartColumn) || line > item.StartLine && line < item.EndLine)
					{
						foldMarkers.Add(item);
					}
				}
			}
			return foldMarkers;
		}

		public List<ICSharpCode.TextEditor.Document.FoldMarker> GetFoldingsWithEnd(int lineNumber)
		{
			return this.GetFoldingsByEndAfterColumn(lineNumber, -1, false);
		}

		public List<ICSharpCode.TextEditor.Document.FoldMarker> GetFoldingsWithStart(int lineNumber)
		{
			return this.GetFoldingsByStartAfterColumn(lineNumber, -1, false);
		}

		public List<ICSharpCode.TextEditor.Document.FoldMarker> GetTopLevelFoldedFoldings()
		{
			List<ICSharpCode.TextEditor.Document.FoldMarker> foldMarkers = new List<ICSharpCode.TextEditor.Document.FoldMarker>();
			if (this.foldMarker != null)
			{
				Point point = new Point(0, 0);
				foreach (ICSharpCode.TextEditor.Document.FoldMarker foldMarker in this.foldMarker)
				{
					if (!foldMarker.IsFolded || foldMarker.StartLine <= point.Y && (foldMarker.StartLine != point.Y || foldMarker.StartColumn < point.X))
					{
						continue;
					}
					foldMarkers.Add(foldMarker);
					point = new Point(foldMarker.EndColumn, foldMarker.EndLine);
				}
			}
			return foldMarkers;
		}

		public bool IsBetweenFolding(int lineNumber)
		{
			return this.GetFoldingsContainsLineNumber(lineNumber).Count > 0;
		}

		public bool IsFoldEnd(int lineNumber)
		{
			return this.GetFoldingsWithEnd(lineNumber).Count > 0;
		}

		public bool IsFoldStart(int lineNumber)
		{
			return this.GetFoldingsWithStart(lineNumber).Count > 0;
		}

		public bool IsLineVisible(int lineNumber)
		{
			bool flag;
			List<ICSharpCode.TextEditor.Document.FoldMarker>.Enumerator enumerator = this.GetFoldingsContainsLineNumber(lineNumber).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.IsFolded)
					{
						continue;
					}
					flag = false;
					return flag;
				}
				return true;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return flag;
		}

		public void NotifyFoldingsChanged(EventArgs e)
		{
			if (this.FoldingsChanged != null)
			{
				this.FoldingsChanged(this, e);
			}
		}

		public string SerializeToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ICSharpCode.TextEditor.Document.FoldMarker foldMarker in this.foldMarker)
			{
				stringBuilder.Append(foldMarker.Offset);
				stringBuilder.Append("\n");
				stringBuilder.Append(foldMarker.Length);
				stringBuilder.Append("\n");
				stringBuilder.Append(foldMarker.FoldText);
				stringBuilder.Append("\n");
				stringBuilder.Append(foldMarker.IsFolded);
				stringBuilder.Append("\n");
			}
			return stringBuilder.ToString();
		}

		public void UpdateFoldings(string fileName, object parseInfo)
		{
			this.UpdateFoldings(this.foldingStrategy.GenerateFoldMarkers(this.document, fileName, parseInfo));
		}

		public void UpdateFoldings(List<ICSharpCode.TextEditor.Document.FoldMarker> newFoldings)
		{
			int count = this.foldMarker.Count;
			lock (this)
			{
				if (newFoldings != null && newFoldings.Count != 0)
				{
					newFoldings.Sort();
					if (this.foldMarker.Count != newFoldings.Count)
					{
						int num = 0;
						int isFolded = 0;
						while (num < this.foldMarker.Count && isFolded < newFoldings.Count)
						{
							int num1 = newFoldings[isFolded].CompareTo(this.foldMarker[num]);
							if (num1 <= 0)
							{
								if (num1 == 0)
								{
									newFoldings[isFolded].IsFolded = this.foldMarker[num].IsFolded;
								}
								isFolded++;
							}
							else
							{
								num++;
							}
						}
					}
					else
					{
						for (int i = 0; i < this.foldMarker.Count; i++)
						{
							newFoldings[i].IsFolded = this.foldMarker[i].IsFolded;
						}
						this.foldMarker = newFoldings;
					}
				}
				if (newFoldings == null)
				{
					this.foldMarker.Clear();
					this.foldMarkerByEnd.Clear();
				}
				else
				{
					this.foldMarker = newFoldings;
					this.foldMarkerByEnd = new List<ICSharpCode.TextEditor.Document.FoldMarker>(newFoldings);
					this.foldMarkerByEnd.Sort(FoldingManager.EndComparer.Instance);
				}
			}
			if (count != this.foldMarker.Count)
			{
				this.document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
				this.document.CommitUpdate();
			}
		}

		public event EventHandler FoldingsChanged;

		private class EndComparer : IComparer<ICSharpCode.TextEditor.Document.FoldMarker>
		{
			public readonly static FoldingManager.EndComparer Instance;

			static EndComparer()
			{
				FoldingManager.EndComparer.Instance = new FoldingManager.EndComparer();
			}

			public EndComparer()
			{
			}

			public int Compare(ICSharpCode.TextEditor.Document.FoldMarker x, ICSharpCode.TextEditor.Document.FoldMarker y)
			{
				if (x.EndLine < y.EndLine)
				{
					return -1;
				}
				if (x.EndLine != y.EndLine)
				{
					return 1;
				}
				return x.EndColumn.CompareTo(y.EndColumn);
			}
		}

		private class StartComparer : IComparer<ICSharpCode.TextEditor.Document.FoldMarker>
		{
			public readonly static FoldingManager.StartComparer Instance;

			static StartComparer()
			{
				FoldingManager.StartComparer.Instance = new FoldingManager.StartComparer();
			}

			public StartComparer()
			{
			}

			public int Compare(ICSharpCode.TextEditor.Document.FoldMarker x, ICSharpCode.TextEditor.Document.FoldMarker y)
			{
				if (x.StartLine < y.StartLine)
				{
					return -1;
				}
				if (x.StartLine != y.StartLine)
				{
					return 1;
				}
				return x.StartColumn.CompareTo(y.StartColumn);
			}
		}
	}
}