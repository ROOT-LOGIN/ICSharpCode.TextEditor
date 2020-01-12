using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ICSharpCode.TextEditor.Util
{
	public class WeakCollection<T> : IEnumerable<T>, IEnumerable
	where T : class
	{
		private readonly List<WeakReference> innerList;

		private bool hasEnumerator;

		public WeakCollection()
		{
		}

		public void Add(T item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			this.CheckNoEnumerator();
			if (this.innerList.Count == this.innerList.Capacity || this.innerList.Count % 32 == 31)
			{
				this.innerList.RemoveAll((WeakReference r) => !r.IsAlive);
			}
			this.innerList.Add(new WeakReference((object)item));
		}

		private void CheckNoEnumerator()
		{
			if (this.hasEnumerator)
			{
				throw new InvalidOperationException("The WeakCollection is already being enumerated, it cannot be modified at the same time. Ensure you dispose the first enumerator before modifying the WeakCollection.");
			}
		}

		public void Clear()
		{
			this.innerList.Clear();
			this.CheckNoEnumerator();
		}

		public bool Contains(T item)
		{
			bool flag;
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			this.CheckNoEnumerator();
			using (IEnumerator<T> enumerator = this.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!item.Equals(enumerator.Current))
					{
						continue;
					}
					flag = true;
					return flag;
				}
				return false;
			}
			return flag;
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (this.hasEnumerator)
			{
				throw new InvalidOperationException("The WeakCollection is already being enumerated, it cannot be enumerated twice at the same time. Ensure you dispose the first enumerator before using another enumerator.");
			}
			try
			{
				this.hasEnumerator = true;
				int num = 0;
				while (num < this.innerList.Count)
				{
					T target = (T)this.innerList[num].Target;
					if (target != null)
					{
						yield return target;
						num++;
					}
					else
					{
						this.RemoveAt(num);
					}
				}
			}
			finally
			{
				this.hasEnumerator = false;
			}
		}

		public bool Remove(T item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			this.CheckNoEnumerator();
			int num = 0;
			while (num < this.innerList.Count)
			{
				T target = (T)this.innerList[num].Target;
				if (target != null)
				{
					if ((object)target == (object)item)
					{
						this.RemoveAt(num);
						return true;
					}
					num++;
				}
				else
				{
					this.RemoveAt(num);
				}
			}
			return false;
		}

		private void RemoveAt(int i)
		{
			int count = this.innerList.Count - 1;
			this.innerList[i] = this.innerList[count];
			this.innerList.RemoveAt(count);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}