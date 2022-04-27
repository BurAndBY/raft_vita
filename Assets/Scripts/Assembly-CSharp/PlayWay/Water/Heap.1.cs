using System;
using System.Collections;
using System.Collections.Generic;

namespace PlayWay.Water
{
	public sealed class Heap<T> : IEnumerable<T>, IEnumerable where T : IComparable<T>
	{
		private T[] elements;

		private int numElements;

		public int Count
		{
			get
			{
				return numElements;
			}
		}

		public T Max
		{
			get
			{
				return elements[0];
			}
		}

		public Heap()
			: this(8)
		{
		}

		public Heap(int capacity)
		{
			elements = new T[capacity];
		}

		public T ExtractMax()
		{
			if (numElements == 0)
			{
				throw new InvalidOperationException("Heap is empty.");
			}
			T result = elements[0];
			elements[0] = elements[--numElements];
			elements[numElements] = default(T);
			BubbleDown(0);
			return result;
		}

		public void Insert(T element)
		{
			if (elements.Length <= numElements)
			{
				Resize(elements.Length * 2);
			}
			elements[numElements++] = element;
			BubbleUp(numElements - 1, element);
		}

		public void Remove(T element)
		{
			for (int i = 0; i < numElements; i++)
			{
				if (elements[i].Equals(element))
				{
					elements[i] = elements[--numElements];
					elements[numElements] = default(T);
					BubbleDown(i);
					break;
				}
			}
		}

		public void Clear()
		{
			numElements = 0;
		}

		private void BubbleUp(int index, T element)
		{
			while (index != 0)
			{
				int num = index - 1 >> 1;
				T other = elements[num];
				if (element.CompareTo(other) <= 0)
				{
					break;
				}
				elements[index] = elements[num];
				elements[num] = element;
				index = num;
			}
		}

		private void BubbleDown(int index)
		{
			T val = elements[0];
			for (int num = 1; num < numElements; num = (index << 1) + 1)
			{
				T val2 = elements[num];
				int num2;
				if (num + 1 < numElements)
				{
					T val3 = elements[num + 1];
					if (val2.CompareTo(val3) > 0)
					{
						num2 = num;
					}
					else
					{
						val2 = val3;
						num2 = num + 1;
					}
				}
				else
				{
					num2 = num;
				}
				if (val.CompareTo(val2) >= 0)
				{
					break;
				}
				elements[num2] = val;
				elements[index] = val2;
				index = num2;
			}
		}

		public void Resize(int len)
		{
			Array.Resize(ref elements, len);
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (elements.Length != numElements)
			{
				Array.Resize(ref elements, numElements);
			}
			return ((IEnumerable<T>)elements).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (elements.Length != numElements)
			{
				Array.Resize(ref elements, numElements);
			}
			return elements.GetEnumerator();
		}
	}
}
