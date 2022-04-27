using System.Collections.Generic;

namespace PlayWay.Water
{
	public class UInt64EqualityComparer : IEqualityComparer<ulong>
	{
		private static UInt64EqualityComparer defaultInstance;

		public static UInt64EqualityComparer Default
		{
			get
			{
				return defaultInstance ?? (defaultInstance = new UInt64EqualityComparer());
			}
		}

		public bool Equals(ulong x, ulong y)
		{
			return x == y;
		}

		public int GetHashCode(ulong obj)
		{
			return (int)(obj ^ (obj >> 32));
		}
	}
}
