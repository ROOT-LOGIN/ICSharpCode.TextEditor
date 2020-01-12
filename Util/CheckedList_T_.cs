using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ICSharpCode.TextEditor.Util
{
	internal sealed class CheckedList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
	{
		private readonly int threadID;

		private readonly IList<T> baseList;

		private int enumeratorCount;

		public int Count
		{
			get
			{
				this.CheckRead();
				return this.baseList.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				this.CheckRead();
				return this.baseList.IsReadOnly;
			}
		}

		public T this[int index]
		{
			get
			{
				this.CheckRead();
				return this.baseList[index];
			}
			set
			{
				this.CheckWrite();
				this.baseList[index] = value;
			}
		}

		public CheckedList() : this(new List<T>())
		{
		}

		public CheckedList(IList<T> baseList)
		{
			if (baseList == null)
			{
				throw new ArgumentNullException("baseList");
			}
			this.baseList = baseList;
			this.threadID = Thread.CurrentThread.ManagedThreadId;
		}

		public void Add(T item)
		{
			this.CheckWrite();
			this.baseList.Add(item);
		}

		private void CheckRead()
		{
			if (Thread.CurrentThread.ManagedThreadId != this.threadID)
			{
				throw new InvalidOperationException("CheckList cannot be accessed from this thread!");
			}
		}

		private void CheckWrite()
		{
			if (Thread.CurrentThread.ManagedThreadId != this.threadID)
			{
				throw new InvalidOperationException("CheckList cannot be accessed from this thread!");
			}
			if (this.enumeratorCount != 0)
			{
				throw new InvalidOperationException("CheckList cannot be written to while enumerators are active!");
			}
		}

		public void Clear()
		{
			this.CheckWrite();
			this.baseList.Clear();
		}

		public bool Contains(T item)
		{
			this.CheckRead();
			return this.baseList.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			this.CheckRead();
			this.baseList.CopyTo(array, arrayIndex);
		}

		private IEnumerator<T> Enumerate()
		{
			this.CheckRead();
			try
			{
				this.enumeratorCount++;
				foreach (T t in this.baseList)
				{
					yield return t;
					this.CheckRead();
				}
			}
			finally
			{
				this.enumeratorCount--;
				this.CheckRead();
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			this.CheckRead();
			return this.Enumerate();
		}

		public int IndexOf(T item)
		{
			this.CheckRead();
			return this.baseList.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			this.CheckWrite();
			this.baseList.Insert(index, item);
		}

		public bool Remove(T item)
		{
			this.CheckWrite();
			return this.baseList.Remove(item);
		}

		public void RemoveAt(int index)
		{
			this.CheckWrite();
			this.baseList.RemoveAt(index);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			this.CheckRead();
			return this.Enumerate();
		}
	}
}