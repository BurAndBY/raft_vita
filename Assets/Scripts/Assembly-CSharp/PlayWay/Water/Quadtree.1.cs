using System;
using UnityEngine;

namespace PlayWay.Water
{
	public class Quadtree<T> where T : class, IPoint2D
	{
		protected Rect rect;

		protected Rect marginRect;

		protected Vector2 center;

		protected Quadtree<T> a;

		protected Quadtree<T> b;

		protected Quadtree<T> c;

		protected Quadtree<T> d;

		protected Quadtree<T> root;

		protected T[] elements;

		protected int numElements;

		private int freeSpace;

		private int lastIndex;

		private int depth;

		public Rect Rect
		{
			get
			{
				return rect;
			}
			set
			{
				rect = value;
				center = rect.center;
				marginRect = rect;
				float num = rect.width * 0.0025f;
				marginRect.xMin -= num;
				marginRect.yMin -= num;
				marginRect.xMax += num;
				marginRect.yMax += num;
				if (a != null)
				{
					float width = rect.width * 0.5f;
					float height = rect.height * 0.5f;
					a.Rect = new Rect(rect.xMin, center.y, width, height);
					b.Rect = new Rect(center.x, center.y, width, height);
					c.Rect = new Rect(rect.xMin, rect.yMin, width, height);
					d.Rect = new Rect(center.x, rect.yMin, width, height);
				}
			}
		}

		public int Count
		{
			get
			{
				return (a == null) ? numElements : (a.Count + b.Count + c.Count + d.Count);
			}
		}

		public int FreeSpace
		{
			get
			{
				return root.freeSpace;
			}
		}

		public Quadtree(Rect rect, int maxElementsPerNode, int maxTotalElements)
		{
			root = this;
			Rect = rect;
			elements = new T[maxElementsPerNode];
			numElements = 0;
			freeSpace = maxTotalElements;
		}

		private Quadtree(Quadtree<T> root, Rect rect, int maxElementsPerNode)
			: this(rect, maxElementsPerNode, 0)
		{
			this.root = root;
		}

		public bool AddElement(T element)
		{
			Vector2 position = element.Position;
			if (root.freeSpace <= 0 || float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsInfinity(position.x) || float.IsInfinity(position.y))
			{
				element.Destroy();
				return false;
			}
			if (root == this && !rect.Contains(element.Position))
			{
				ExpandToContain(element.Position);
			}
			return AddElementInternal(element);
		}

		private bool AddElementInternal(T element)
		{
			if (element == null)
			{
				throw new ArgumentException("Element null");
			}
			if (a != null)
			{
				Vector2 position = element.Position;
				if (position.x < center.x)
				{
					if (position.y > center.y)
					{
						return a.AddElementInternal(element);
					}
					return c.AddElementInternal(element);
				}
				if (position.y > center.y)
				{
					return b.AddElementInternal(element);
				}
				return d.AddElementInternal(element);
			}
			if (numElements != elements.Length)
			{
				while (lastIndex < elements.Length)
				{
					if (elements[lastIndex] == null)
					{
						AddElementAt(element, lastIndex);
						return true;
					}
					lastIndex++;
				}
				for (lastIndex = 0; lastIndex < elements.Length; lastIndex++)
				{
					if (elements[lastIndex] == null)
					{
						AddElementAt(element, lastIndex);
						return true;
					}
				}
				throw new InvalidOperationException("PlayWay Water: Code supposed to be unreachable.");
			}
			if (depth < 80)
			{
				T[] array = elements;
				SpawnChildNodes();
				a.depth = (b.depth = (c.depth = (d.depth = depth + 1)));
				elements = null;
				numElements = 0;
				root.freeSpace += array.Length;
				for (int i = 0; i < array.Length; i++)
				{
					AddElementInternal(array[i]);
				}
				return AddElementInternal(element);
			}
			throw new Exception("PlayWay Water: Quadtree depth limit reached.");
		}

		public void UpdateElements(Quadtree<T> root)
		{
			if (a != null)
			{
				a.UpdateElements(root);
				b.UpdateElements(root);
				c.UpdateElements(root);
				d.UpdateElements(root);
			}
			if (elements == null)
			{
				return;
			}
			for (int i = 0; i < elements.Length; i++)
			{
				T val = elements[i];
				if (val != null && !marginRect.Contains(val.Position))
				{
					RemoveElementAt(i);
					root.AddElement(val);
				}
			}
		}

		public void ExpandToContain(Vector2 point)
		{
			Rect rect = this.rect;
			do
			{
				float num = rect.width * 0.5f;
				rect.xMin -= num;
				rect.yMin -= num;
				rect.xMax += num;
				rect.yMax += num;
			}
			while (!rect.Contains(point));
			Rect = rect;
			UpdateElements(root);
		}

		public virtual void Destroy()
		{
			elements = null;
			if (a != null)
			{
				a.Destroy();
				b.Destroy();
				c.Destroy();
				d.Destroy();
				a = (b = (c = (d = null)));
			}
		}

		protected virtual void AddElementAt(T element, int index)
		{
			numElements++;
			root.freeSpace--;
			elements[index] = element;
		}

		protected virtual void RemoveElementAt(int index)
		{
			numElements--;
			root.freeSpace++;
			elements[index] = (T)null;
		}

		protected virtual void SpawnChildNodes()
		{
			float width = rect.width * 0.5f;
			float height = rect.height * 0.5f;
			a = new Quadtree<T>(root, new Rect(rect.xMin, center.y, width, height), elements.Length);
			b = new Quadtree<T>(root, new Rect(center.x, center.y, width, height), elements.Length);
			c = new Quadtree<T>(root, new Rect(rect.xMin, rect.yMin, width, height), elements.Length);
			d = new Quadtree<T>(root, new Rect(center.x, rect.yMin, width, height), elements.Length);
		}
	}
}
