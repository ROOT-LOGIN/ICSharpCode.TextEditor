using ICSharpCode.TextEditor.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ICSharpCode.TextEditor.Document
{
	internal sealed class LineSegmentTree : IList<LineSegment>, ICollection<LineSegment>, IEnumerable<LineSegment>, IEnumerable
	{
		private readonly AugmentableRedBlackTree<LineSegmentTree.RBNode, LineSegmentTree.MyHost> tree = new AugmentableRedBlackTree<LineSegmentTree.RBNode, LineSegmentTree.MyHost>(new LineSegmentTree.MyHost());

		public int Count
		{
			get
			{
				return this.tree.Count;
			}
		}

		public LineSegment this[int index]
		{
			get
			{
				return this.GetNode(index).val.lineSegment;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		bool System.Collections.Generic.ICollection<ICSharpCode.TextEditor.Document.LineSegment>.IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public int TotalLength
		{
			get
			{
				if (this.tree.root == null)
				{
					return 0;
				}
				return this.tree.root.val.totalLength;
			}
		}

		public LineSegmentTree()
		{
			this.Clear();
		}

		public void Clear()
		{
			this.tree.Clear();
			LineSegment lineSegment = new LineSegment()
			{
				TotalLength = 0,
				DelimiterLength = 0
			};
			this.tree.Add(new LineSegmentTree.RBNode(lineSegment));
			lineSegment.treeEntry = this.GetEnumeratorForIndex(0);
		}

		public bool Contains(LineSegment item)
		{
			return this.IndexOf(item) >= 0;
		}

		public void CopyTo(LineSegment[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			foreach (LineSegment lineSegment in this)
			{
				int num = arrayIndex;
				arrayIndex = num + 1;
				array[num] = lineSegment;
			}
		}

		public LineSegment GetByOffset(int offset)
		{
			return this.GetNodeByOffset(offset).val.lineSegment;
		}

		public LineSegmentTree.Enumerator GetEnumerator()
		{
			return new LineSegmentTree.Enumerator(this.tree.GetEnumerator());
		}

		public LineSegmentTree.Enumerator GetEnumeratorForIndex(int index)
		{
			return new LineSegmentTree.Enumerator(new RedBlackTreeIterator<LineSegmentTree.RBNode>(this.GetNode(index)));
		}

		public LineSegmentTree.Enumerator GetEnumeratorForOffset(int offset)
		{
			return new LineSegmentTree.Enumerator(new RedBlackTreeIterator<LineSegmentTree.RBNode>(this.GetNodeByOffset(offset)));
		}

		private static int GetIndexFromNode(RedBlackTreeNode<LineSegmentTree.RBNode> node)
		{
			int num = (node.left != null ? node.left.val.count : 0);
			while (node.parent != null)
			{
				if (node == node.parent.right)
				{
					if (node.parent.left != null)
					{
						num += node.parent.left.val.count;
					}
					num++;
				}
				node = node.parent;
			}
			return num;
		}

		private RedBlackTreeNode<LineSegmentTree.RBNode> GetNode(int index)
		{
			if (index < 0 || index >= this.tree.Count)
			{
				throw new ArgumentOutOfRangeException("index", (object)index, string.Concat("index should be between 0 and ", this.tree.Count - 1));
			}
			RedBlackTreeNode<LineSegmentTree.RBNode> redBlackTreeNode = this.tree.root;
			while (true)
			{
				if (redBlackTreeNode.left == null || index >= redBlackTreeNode.left.val.count)
				{
					if (redBlackTreeNode.left != null)
					{
						index -= redBlackTreeNode.left.val.count;
					}
					if (index == 0)
					{
						break;
					}
					index--;
					redBlackTreeNode = redBlackTreeNode.right;
				}
				else
				{
					redBlackTreeNode = redBlackTreeNode.left;
				}
			}
			return redBlackTreeNode;
		}

		private RedBlackTreeNode<LineSegmentTree.RBNode> GetNodeByOffset(int offset)
		{
			if (offset < 0 || offset > this.TotalLength)
			{
				throw new ArgumentOutOfRangeException("offset", (object)offset, string.Concat("offset should be between 0 and ", this.TotalLength));
			}
			if (offset == this.TotalLength)
			{
				if (this.tree.root == null)
				{
					throw new InvalidOperationException("Cannot call GetNodeByOffset while tree is empty.");
				}
				return this.tree.root.RightMost;
			}
			RedBlackTreeNode<LineSegmentTree.RBNode> redBlackTreeNode = this.tree.root;
			while (true)
			{
				if (redBlackTreeNode.left == null || offset >= redBlackTreeNode.left.val.totalLength)
				{
					if (redBlackTreeNode.left != null)
					{
						offset -= redBlackTreeNode.left.val.totalLength;
					}
					offset -= redBlackTreeNode.val.lineSegment.TotalLength;
					if (offset < 0)
					{
						break;
					}
					redBlackTreeNode = redBlackTreeNode.right;
				}
				else
				{
					redBlackTreeNode = redBlackTreeNode.left;
				}
			}
			return redBlackTreeNode;
		}

		private static int GetOffsetFromNode(RedBlackTreeNode<LineSegmentTree.RBNode> node)
		{
			int totalLength = (node.left != null ? node.left.val.totalLength : 0);
			while (node.parent != null)
			{
				if (node == node.parent.right)
				{
					if (node.parent.left != null)
					{
						totalLength += node.parent.left.val.totalLength;
					}
					totalLength += node.parent.val.lineSegment.TotalLength;
				}
				node = node.parent;
			}
			return totalLength;
		}

		public int IndexOf(LineSegment item)
		{
			int lineNumber = item.LineNumber;
			if (lineNumber < 0 || lineNumber >= this.Count)
			{
				return -1;
			}
			if (item != this[lineNumber])
			{
				return -1;
			}
			return lineNumber;
		}

		private LineSegmentTree.Enumerator InsertAfter(RedBlackTreeNode<LineSegmentTree.RBNode> node, LineSegment newSegment)
		{
			RedBlackTreeNode<LineSegmentTree.RBNode> redBlackTreeNode = new RedBlackTreeNode<LineSegmentTree.RBNode>(new LineSegmentTree.RBNode(newSegment));
			if (node.right != null)
			{
				this.tree.InsertAsLeft(node.right.LeftMost, redBlackTreeNode);
			}
			else
			{
				this.tree.InsertAsRight(node, redBlackTreeNode);
			}
			return new LineSegmentTree.Enumerator(new RedBlackTreeIterator<LineSegmentTree.RBNode>(redBlackTreeNode));
		}

		public LineSegment InsertSegmentAfter(LineSegment segment, int length)
		{
			LineSegment lineSegment = new LineSegment()
			{
				TotalLength = length,
				DelimiterLength = segment.DelimiterLength,
			};
            lineSegment.treeEntry = this.InsertAfter(segment.treeEntry.it.node, lineSegment);

            return lineSegment;
		}

		public void RemoveSegment(LineSegment segment)
		{
			this.tree.RemoveAt(segment.treeEntry.it);
		}

		public void SetSegmentLength(LineSegment segment, int newTotalLength)
		{
			if (segment == null)
			{
				throw new ArgumentNullException("segment");
			}
			RedBlackTreeNode<LineSegmentTree.RBNode> redBlackTreeNode = segment.treeEntry.it.node;
			segment.TotalLength = newTotalLength;
			(new LineSegmentTree.MyHost()).UpdateAfterChildrenChange(redBlackTreeNode);
		}

		void System.Collections.Generic.ICollection<ICSharpCode.TextEditor.Document.LineSegment>.Add(LineSegment item)
		{
			throw new NotSupportedException();
		}

		bool System.Collections.Generic.ICollection<ICSharpCode.TextEditor.Document.LineSegment>.Remove(LineSegment item)
		{
			throw new NotSupportedException();
		}

		IEnumerator<LineSegment> System.Collections.Generic.IEnumerable<ICSharpCode.TextEditor.Document.LineSegment>.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		void System.Collections.Generic.IList<ICSharpCode.TextEditor.Document.LineSegment>.Insert(int index, LineSegment item)
		{
			throw new NotSupportedException();
		}

		void System.Collections.Generic.IList<ICSharpCode.TextEditor.Document.LineSegment>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public struct Enumerator : IEnumerator<LineSegment>, IDisposable, IEnumerator
		{
			public readonly static LineSegmentTree.Enumerator Invalid;

			internal RedBlackTreeIterator<LineSegmentTree.RBNode> it;

			public LineSegment Current
			{
				get
				{
					return this.it.Current.lineSegment;
				}
			}

			public int CurrentIndex
			{
				get
				{
					if (this.it.node == null)
					{
						throw new InvalidOperationException();
					}
					return LineSegmentTree.GetIndexFromNode(this.it.node);
				}
			}

			public int CurrentOffset
			{
				get
				{
					if (this.it.node == null)
					{
						throw new InvalidOperationException();
					}
					return LineSegmentTree.GetOffsetFromNode(this.it.node);
				}
			}

			public bool IsValid
			{
				get
				{
					return this.it.IsValid;
				}
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					return this.it.Current.lineSegment;
				}
			}

			static Enumerator()
			{
				LineSegmentTree.Enumerator.Invalid = new LineSegmentTree.Enumerator();
			}

			internal Enumerator(RedBlackTreeIterator<LineSegmentTree.RBNode> it)
			{
				this.it = it;
			}

			public void Dispose()
			{
			}

			public bool MoveBack()
			{
				return this.it.MoveBack();
			}

			public bool MoveNext()
			{
				return this.it.MoveNext();
			}

			void System.Collections.IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}
		}

		private struct MyHost : IRedBlackTreeHost<LineSegmentTree.RBNode>, IComparer<LineSegmentTree.RBNode>
		{
			public int Compare(LineSegmentTree.RBNode x, LineSegmentTree.RBNode y)
			{
				throw new NotImplementedException();
			}

			public bool Equals(LineSegmentTree.RBNode a, LineSegmentTree.RBNode b)
			{
				throw new NotImplementedException();
			}

			public void UpdateAfterChildrenChange(RedBlackTreeNode<LineSegmentTree.RBNode> node)
			{
				int num = 1;
				int totalLength = node.val.lineSegment.TotalLength;
				if (node.left != null)
				{
					num += node.left.val.count;
					totalLength += node.left.val.totalLength;
				}
				if (node.right != null)
				{
					num += node.right.val.count;
					totalLength += node.right.val.totalLength;
				}
				if (num != node.val.count || totalLength != node.val.totalLength)
				{
					node.val.count = num;
					node.val.totalLength = totalLength;
					if (node.parent != null)
					{
						this.UpdateAfterChildrenChange(node.parent);
					}
				}
			}

			public void UpdateAfterRotateLeft(RedBlackTreeNode<LineSegmentTree.RBNode> node)
			{
				this.UpdateAfterChildrenChange(node);
				this.UpdateAfterChildrenChange(node.parent);
			}

			public void UpdateAfterRotateRight(RedBlackTreeNode<LineSegmentTree.RBNode> node)
			{
				this.UpdateAfterChildrenChange(node);
				this.UpdateAfterChildrenChange(node.parent);
			}
		}

		internal struct RBNode
		{
			internal LineSegment lineSegment;

			internal int count;

			internal int totalLength;

			public RBNode(LineSegment lineSegment)
			{
				this.lineSegment = lineSegment;
				this.count = 1;
				this.totalLength = lineSegment.TotalLength;
			}

			public override string ToString()
			{
				object[] lineNumber = new object[] { "[RBNode count=", this.count, " totalLength=", this.totalLength, " lineSegment.LineNumber=", this.lineSegment.LineNumber, " lineSegment.Offset=", this.lineSegment.Offset, " lineSegment.TotalLength=", this.lineSegment.TotalLength, " lineSegment.DelimiterLength=", this.lineSegment.DelimiterLength, "]" };
				return string.Concat(lineNumber);
			}
		}
	}
}