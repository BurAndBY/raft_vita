using UnityEngine.Rendering;

namespace PlayWay.Water
{
	public class WaterVolumeSubtract : WaterVolumeBase
	{
		protected override CullMode CullMode
		{
			get
			{
				return CullMode.Front;
			}
		}

		protected override void Register(Water water)
		{
			if (water != null)
			{
				water.Volume.AddSubtractor(this);
			}
		}

		protected override void Unregister(Water water)
		{
			if (water != null)
			{
				water.Volume.RemoveSubtractor(this);
			}
		}
	}
}
