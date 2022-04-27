using System.Collections.Generic;

namespace PlayWay.Water
{
	public class WaterGlobals
	{
		private static WaterGlobals instance;

		private List<Water> waters;

		private List<Water> boundlessWaters;

		public static WaterGlobals Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new WaterGlobals();
				}
				return instance;
			}
		}

		public List<Water> Waters
		{
			get
			{
				return waters;
			}
		}

		public List<Water> BoundlessWaters
		{
			get
			{
				return boundlessWaters;
			}
		}

		private WaterGlobals()
		{
			waters = new List<Water>();
			boundlessWaters = new List<Water>();
		}

		public void AddWater(Water water)
		{
			if (!waters.Contains(water))
			{
				waters.Add(water);
			}
			if ((water.Volume == null || water.Volume.Boundless) && !boundlessWaters.Contains(water))
			{
				boundlessWaters.Add(water);
			}
		}

		public void RemoveWater(Water water)
		{
			waters.Remove(water);
			boundlessWaters.Remove(water);
		}
	}
}
