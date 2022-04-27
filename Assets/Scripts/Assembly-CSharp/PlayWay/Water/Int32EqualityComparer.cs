using System.Collections.Generic;

namespace PlayWay.Water
{
	public class Int32EqualityComparer : IEqualityComparer<int>
	{
		private static Int32EqualityComparer defaultInstance;

		public static Int32EqualityComparer Default
		{
			get
			{
				return defaultInstance ?? (defaultInstance = new Int32EqualityComparer());
			}
		}

		public bool Equals(int x, int y)
		{
			return x == y;
		}

		public int GetHashCode(int obj)
		{
			return obj;
		}
	}
}
