using System;
using System.Collections;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Document
{
	public sealed class SpanStack : ICloneable, IEnumerable<Span>, IEnumerable
	{
		private SpanStack.StackNode top;

		public bool IsEmpty
		{
			get
			{
				return this.top == null;
			}
		}

		public SpanStack()
		{
		}

		public SpanStack Clone()
		{
			return new SpanStack()
			{
				top = this.top
			};
		}

		public SpanStack.Enumerator GetEnumerator()
		{
			return new SpanStack.Enumerator(new SpanStack.StackNode(this.top, null));
		}

		public Span Peek()
		{
			return this.top.Data;
		}

		public Span Pop()
		{
			Span data = this.top.Data;
			this.top = this.top.Previous;
			return data;
		}

		public void Push(Span s)
		{
			this.top = new SpanStack.StackNode(this.top, s);
		}

		IEnumerator<Span> System.Collections.Generic.IEnumerable<ICSharpCode.TextEditor.Document.Span>.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		object System.ICloneable.Clone()
		{
			return this.Clone();
		}

		public struct Enumerator : IEnumerator<Span>, IDisposable, IEnumerator
		{
			private SpanStack.StackNode c;

			public Span Current
			{
				get
				{
					return this.c.Data;
				}
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					return this.c.Data;
				}
			}

			internal Enumerator(SpanStack.StackNode node)
			{
				this.c = node;
			}

			public void Dispose()
			{
				this.c = null;
			}

			public bool MoveNext()
			{
				this.c = this.c.Previous;
				return this.c != null;
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}
		}

		internal sealed class StackNode
		{
			public readonly SpanStack.StackNode Previous;

			public readonly Span Data;

			public StackNode(SpanStack.StackNode previous, Span data)
			{
				this.Previous = previous;
				this.Data = data;
			}
		}
	}
}