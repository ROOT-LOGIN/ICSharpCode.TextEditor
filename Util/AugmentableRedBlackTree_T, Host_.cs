using System;
using System.Collections;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Util
{
	internal sealed class AugmentableRedBlackTree<T, Host> : ICollection<T>, IEnumerable<T>, IEnumerable
	where Host : IRedBlackTreeHost<T>
	{
		private const bool RED = true;

		private const bool BLACK = false;

		private readonly Host host;

		private int count;

		internal RedBlackTreeNode<T> root;

		public int Count
		{
			get
			{
				return this.count;
			}
		}

		bool System.Collections.Generic.ICollection<T>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public AugmentableRedBlackTree(Host host)
		{
			if (host == null)
			{
				throw new ArgumentNullException("host");
			}
			this.host = host;
		}

		public void Add(T item)
		{
			this.AddInternal(new RedBlackTreeNode<T>(item));
		}

		private void AddInternal(RedBlackTreeNode<T> newNode)
		{
			if (this.root == null)
			{
				this.count = 1;
				this.root = newNode;
				return;
			}
			RedBlackTreeNode<T> redBlackTreeNode = this.root;
			while (true)
			{
				if (this.host.Compare(newNode.val, redBlackTreeNode.val) > 0)
				{
					if (redBlackTreeNode.right == null)
					{
						break;
					}
					redBlackTreeNode = redBlackTreeNode.right;
				}
				else
				{
					if (redBlackTreeNode.left == null)
					{
						this.InsertAsLeft(redBlackTreeNode, newNode);
						return;
					}
					redBlackTreeNode = redBlackTreeNode.left;
				}
			}
			this.InsertAsRight(redBlackTreeNode, newNode);
		}

		public RedBlackTreeIterator<T> Begin()
		{
			if (this.root == null)
			{
				return new RedBlackTreeIterator<T>();
			}
			return new RedBlackTreeIterator<T>(this.root.LeftMost);
		}

		public void Clear()
		{
			this.root = null;
			this.count = 0;
		}

		public bool Contains(T item)
		{
			return this.Find(item).IsValid;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			foreach (T t in this)
			{
				int num = arrayIndex;
				arrayIndex = num + 1;
				array[num] = t;
			}
		}

		public RedBlackTreeIterator<T> Find(T item)
		{
			RedBlackTreeIterator<T> redBlackTreeIterator = this.LowerBound(item);
			while (redBlackTreeIterator.IsValid && this.host.Compare(redBlackTreeIterator.Current, item) == 0)
			{
				if (this.host.Equals(redBlackTreeIterator.Current, item))
				{
					return redBlackTreeIterator;
				}
				redBlackTreeIterator.MoveNext();
			}
			return new RedBlackTreeIterator<T>();
		}

		private void FixTreeOnDelete(RedBlackTreeNode<T> node, RedBlackTreeNode<T> parentNode)
		{
			if (parentNode == null)
			{
				return;
			}
			RedBlackTreeNode<T> redBlackTreeNode = AugmentableRedBlackTree<T, Host>.Sibling(node, parentNode);
			if (redBlackTreeNode.color)
			{
				parentNode.color = true;
				redBlackTreeNode.color = false;
				if (node != parentNode.left)
				{
					this.RotateRight(parentNode);
				}
				else
				{
					this.RotateLeft(parentNode);
				}
				redBlackTreeNode = AugmentableRedBlackTree<T, Host>.Sibling(node, parentNode);
			}
			if (!parentNode.color && !redBlackTreeNode.color && !AugmentableRedBlackTree<T, Host>.GetColor(redBlackTreeNode.left) && !AugmentableRedBlackTree<T, Host>.GetColor(redBlackTreeNode.right))
			{
				redBlackTreeNode.color = true;
				this.FixTreeOnDelete(parentNode, parentNode.parent);
				return;
			}
			if (parentNode.color && !redBlackTreeNode.color && !AugmentableRedBlackTree<T, Host>.GetColor(redBlackTreeNode.left) && !AugmentableRedBlackTree<T, Host>.GetColor(redBlackTreeNode.right))
			{
				redBlackTreeNode.color = true;
				parentNode.color = false;
				return;
			}
			if (node == parentNode.left && !redBlackTreeNode.color && AugmentableRedBlackTree<T, Host>.GetColor(redBlackTreeNode.left) && !AugmentableRedBlackTree<T, Host>.GetColor(redBlackTreeNode.right))
			{
				redBlackTreeNode.color = true;
				redBlackTreeNode.left.color = false;
				this.RotateRight(redBlackTreeNode);
			}
			else if (node == parentNode.right && !redBlackTreeNode.color && AugmentableRedBlackTree<T, Host>.GetColor(redBlackTreeNode.right) && !AugmentableRedBlackTree<T, Host>.GetColor(redBlackTreeNode.left))
			{
				redBlackTreeNode.color = true;
				redBlackTreeNode.right.color = false;
				this.RotateLeft(redBlackTreeNode);
			}
			redBlackTreeNode = AugmentableRedBlackTree<T, Host>.Sibling(node, parentNode);
			redBlackTreeNode.color = parentNode.color;
			parentNode.color = false;
			if (node == parentNode.left)
			{
				if (redBlackTreeNode.right != null)
				{
					redBlackTreeNode.right.color = false;
				}
				this.RotateLeft(parentNode);
				return;
			}
			if (redBlackTreeNode.left != null)
			{
				redBlackTreeNode.left.color = false;
			}
			this.RotateRight(parentNode);
		}

		private void FixTreeOnInsert(RedBlackTreeNode<T> node)
		{
			RedBlackTreeNode<T> redBlackTreeNode = node.parent;
			if (redBlackTreeNode == null)
			{
				node.color = false;
				return;
			}
			if (!redBlackTreeNode.color)
			{
				return;
			}
			RedBlackTreeNode<T> redBlackTreeNode1 = redBlackTreeNode.parent;
			RedBlackTreeNode<T> redBlackTreeNode2 = this.Sibling(redBlackTreeNode);
			if (redBlackTreeNode2 != null && redBlackTreeNode2.color)
			{
				redBlackTreeNode.color = false;
				redBlackTreeNode2.color = false;
				redBlackTreeNode1.color = true;
				this.FixTreeOnInsert(redBlackTreeNode1);
				return;
			}
			if (node == redBlackTreeNode.right && redBlackTreeNode == redBlackTreeNode1.left)
			{
				this.RotateLeft(redBlackTreeNode);
				node = node.left;
			}
			else if (node == redBlackTreeNode.left && redBlackTreeNode == redBlackTreeNode1.right)
			{
				this.RotateRight(redBlackTreeNode);
				node = node.right;
			}
			redBlackTreeNode = node.parent;
			redBlackTreeNode1 = redBlackTreeNode.parent;
			redBlackTreeNode.color = false;
			redBlackTreeNode1.color = true;
			if (node == redBlackTreeNode.left && redBlackTreeNode == redBlackTreeNode1.left)
			{
				this.RotateRight(redBlackTreeNode1);
				return;
			}
			this.RotateLeft(redBlackTreeNode1);
		}

		private static bool GetColor(RedBlackTreeNode<T> node)
		{
			if (node == null)
			{
				return false;
			}
			return node.color;
		}

		public RedBlackTreeIterator<T> GetEnumerator()
		{
			if (this.root == null)
			{
				return new RedBlackTreeIterator<T>();
			}
			RedBlackTreeNode<T> redBlackTreeNode = new RedBlackTreeNode<T>(default(T))
			{
				right = this.root
			};
			return new RedBlackTreeIterator<T>(redBlackTreeNode);
		}

		internal void InsertAsLeft(RedBlackTreeNode<T> parentNode, RedBlackTreeNode<T> newNode)
		{
			parentNode.left = newNode;
			newNode.parent = parentNode;
			newNode.color = true;
			this.host.UpdateAfterChildrenChange(parentNode);
			this.FixTreeOnInsert(newNode);
			this.count++;
		}

		internal void InsertAsRight(RedBlackTreeNode<T> parentNode, RedBlackTreeNode<T> newNode)
		{
			parentNode.right = newNode;
			newNode.parent = parentNode;
			newNode.color = true;
			this.host.UpdateAfterChildrenChange(parentNode);
			this.FixTreeOnInsert(newNode);
			this.count++;
		}

		public RedBlackTreeIterator<T> LowerBound(T item)
		{
			RedBlackTreeNode<T> redBlackTreeNode = this.root;
			RedBlackTreeNode<T> redBlackTreeNode1 = null;
			while (redBlackTreeNode != null)
			{
				if (this.host.Compare(redBlackTreeNode.val, item) >= 0)
				{
					redBlackTreeNode1 = redBlackTreeNode;
					redBlackTreeNode = redBlackTreeNode.left;
				}
				else
				{
					redBlackTreeNode = redBlackTreeNode.right;
				}
			}
			return new RedBlackTreeIterator<T>(redBlackTreeNode1);
		}

		public bool Remove(T item)
		{
			RedBlackTreeIterator<T> redBlackTreeIterator = this.Find(item);
			if (!redBlackTreeIterator.IsValid)
			{
				return false;
			}
			this.RemoveAt(redBlackTreeIterator);
			return true;
		}

		public void RemoveAt(RedBlackTreeIterator<T> iterator)
		{
			RedBlackTreeNode<T> redBlackTreeNode = iterator.node;
			if (redBlackTreeNode == null)
			{
				throw new ArgumentException("Invalid iterator");
			}
			while (redBlackTreeNode.parent != null)
			{
				redBlackTreeNode = redBlackTreeNode.parent;
			}
			if (redBlackTreeNode != this.root)
			{
				throw new ArgumentException("Iterator does not belong to this tree");
			}
			this.RemoveNode(iterator.node);
		}

		internal void RemoveNode(RedBlackTreeNode<T> removedNode)
		{
			if (removedNode.left == null || removedNode.right == null)
			{
				this.count--;
				RedBlackTreeNode<T> redBlackTreeNode = removedNode.parent;
				RedBlackTreeNode<T> redBlackTreeNode1 = removedNode.left ?? removedNode.right;
				this.ReplaceNode(removedNode, redBlackTreeNode1);
				if (redBlackTreeNode != null)
				{
					this.host.UpdateAfterChildrenChange(redBlackTreeNode);
				}
				if (!removedNode.color)
				{
					if (redBlackTreeNode1 != null && redBlackTreeNode1.color)
					{
						redBlackTreeNode1.color = false;
						return;
					}
					this.FixTreeOnDelete(redBlackTreeNode1, redBlackTreeNode);
				}
				return;
			}
			RedBlackTreeNode<T> leftMost = removedNode.right.LeftMost;
			this.RemoveNode(leftMost);
			this.ReplaceNode(removedNode, leftMost);
			leftMost.left = removedNode.left;
			if (leftMost.left != null)
			{
				leftMost.left.parent = leftMost;
			}
			leftMost.right = removedNode.right;
			if (leftMost.right != null)
			{
				leftMost.right.parent = leftMost;
			}
			leftMost.color = removedNode.color;
			this.host.UpdateAfterChildrenChange(leftMost);
			if (leftMost.parent != null)
			{
				this.host.UpdateAfterChildrenChange(leftMost.parent);
			}
		}

		private void ReplaceNode(RedBlackTreeNode<T> replacedNode, RedBlackTreeNode<T> newNode)
		{
			if (replacedNode.parent == null)
			{
				this.root = newNode;
			}
			else if (replacedNode.parent.left != replacedNode)
			{
				replacedNode.parent.right = newNode;
			}
			else
			{
				replacedNode.parent.left = newNode;
			}
			if (newNode != null)
			{
				newNode.parent = replacedNode.parent;
			}
			replacedNode.parent = null;
		}

		private void RotateLeft(RedBlackTreeNode<T> p)
		{
			RedBlackTreeNode<T> redBlackTreeNode = p.right;
			this.ReplaceNode(p, redBlackTreeNode);
			p.right = redBlackTreeNode.left;
			if (p.right != null)
			{
				p.right.parent = p;
			}
			redBlackTreeNode.left = p;
			p.parent = redBlackTreeNode;
			this.host.UpdateAfterRotateLeft(p);
		}

		private void RotateRight(RedBlackTreeNode<T> p)
		{
			RedBlackTreeNode<T> redBlackTreeNode = p.left;
			this.ReplaceNode(p, redBlackTreeNode);
			p.left = redBlackTreeNode.right;
			if (p.left != null)
			{
				p.left.parent = p;
			}
			redBlackTreeNode.right = p;
			p.parent = redBlackTreeNode;
			this.host.UpdateAfterRotateRight(p);
		}

		private RedBlackTreeNode<T> Sibling(RedBlackTreeNode<T> node)
		{
			if (node == node.parent.left)
			{
				return node.parent.right;
			}
			return node.parent.left;
		}

		private static RedBlackTreeNode<T> Sibling(RedBlackTreeNode<T> node, RedBlackTreeNode<T> parentNode)
		{
			if (node == parentNode.left)
			{
				return parentNode.right;
			}
			return parentNode.left;
		}

		IEnumerator<T> System.Collections.Generic.IEnumerable<T>.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public RedBlackTreeIterator<T> UpperBound(T item)
		{
			RedBlackTreeIterator<T> redBlackTreeIterator = this.LowerBound(item);
			while (redBlackTreeIterator.IsValid && this.host.Compare(redBlackTreeIterator.Current, item) == 0)
			{
				redBlackTreeIterator.MoveNext();
			}
			return redBlackTreeIterator;
		}
	}
}