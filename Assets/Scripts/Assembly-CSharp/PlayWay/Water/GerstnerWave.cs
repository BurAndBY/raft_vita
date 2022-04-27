using UnityEngine;

namespace PlayWay.Water
{
	public class GerstnerWave
	{
		public Vector2 direction;

		public float amplitude;

		public float offset;

		public float frequency;

		public float speed;

		public GerstnerWave()
		{
			direction = new Vector2(0f, 1f);
			frequency = 1f;
		}

		public GerstnerWave(Vector2 direction, float amplitude, float offset, float frequency, float speed)
		{
			this.direction = direction;
			this.amplitude = amplitude;
			this.offset = offset;
			this.frequency = frequency;
			this.speed = speed;
		}
	}
}
