using System;
using System.Collections;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Util
{
	internal struct RedBlackTreeIterator<T> : IEnumerator<T>, IDisposable, IEnumerator
	{
		internal RedBlackTreeNode<T> node;

		public T Current
		{
			get
			{
				if (this.node == null)
				{
					throw new InvalidOperationException();
				}
				return this.node.val;
			}
		}

		public bool IsValid
		{
			get
			{
				return this.node != null;
			}
		}

		object System.Collections.IEnumerator.Current
		{
			get
			{
				return this.Current;
			}
		}

		internal RedBlackTreeIterator(RedBlackTreeNode<T> node)
		{
			this.node = node;
		}

		public bool MoveBack()
		{
			RedBlackTreeNode<T> redBlackTreeNode;
			if (this.node == null)
			{
				return false;
			}
			if (this.node.left == null)
			{
				do
				{
					redBlackTreeNode = this.node;
					this.node = this.node.parent;
				}
				while (this.node != null && this.node.left == redBlackTreeNode);
			}
			else
			{
				this.node = this.node.left.RightMost;
			}
			return this.node != null;
		}

		public bool MoveNext()
		{
			RedBlackTreeNode<T> redBlackTreeNode;
			if (this.node == null)
			{
				return false;
			}
			if (this.node.right == null)
			{
				do
				{
					redBlackTreeNode = this.node;
					this.node = this.node.parent;
				}
				while (this.node != null && this.node.right == redBlackTreeNode);
			}
			else
			{
				this.node = this.node.right.LeftMost;
			}
			return this.node != null;
		}

		void System.Collections.IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		void System.IDisposable.Dispose()
		{
		}
	}
}