using System;
using System.Collections.Generic;

namespace PlayWay.Water
{
	public class FloatEqualityComparer : IEqualityComparer<float>
	{
		private static FloatEqualityComparer defaultInstance;

		public static FloatEqualityComparer Default
		{
			get
			{
				return defaultInstance ?? (defaultInstance = new FloatEqualityComparer());
			}
		}

		public bool Equals(float x, float y)
		{
			return x == y;
		}

		public int GetHashCode(float obj)
		{
			return (int)BitConverter.DoubleToInt64Bits(obj);
		}
	}
}
