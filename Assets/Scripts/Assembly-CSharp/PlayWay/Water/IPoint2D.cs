using UnityEngine;

namespace PlayWay.Water
{
	public interface IPoint2D
	{
		Vector2 Position { get; }

		void Destroy();
	}
}
