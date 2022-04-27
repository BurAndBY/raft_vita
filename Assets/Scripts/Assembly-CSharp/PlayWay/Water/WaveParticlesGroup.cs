using UnityEngine;

namespace PlayWay.Water
{
	public class WaveParticlesGroup
	{
		public float lastUpdateTime;

		public float lastCostlyUpdateTime;

		public WaveParticle leftParticle;

		public int ParticleCount
		{
			get
			{
				int num = 0;
				for (WaveParticle rightNeighbour = leftParticle; rightNeighbour != null; rightNeighbour = rightNeighbour.rightNeighbour)
				{
					num++;
				}
				return num;
			}
		}

		public WaveParticlesGroup(float startTime)
		{
			lastUpdateTime = (lastCostlyUpdateTime = startTime);
		}

		public void Update(float time)
		{
			WaveParticle rightNeighbour = leftParticle;
			float num = time - lastUpdateTime;
			lastUpdateTime = time;
			float num2 = ((!(num < 1f)) ? 1f : num);
			float invStep = 1f - num2;
			do
			{
				WaveParticle waveParticle = rightNeighbour;
				rightNeighbour = rightNeighbour.rightNeighbour;
				waveParticle.Update(num, num2, invStep);
			}
			while (rightNeighbour != null);
		}

		public void CostlyUpdate(WaveParticlesQuadtree quadtree, float time)
		{
			WaveParticle rightNeighbour = leftParticle;
			float deltaTime = time - lastCostlyUpdateTime;
			lastCostlyUpdateTime = time;
			int num = 0;
			do
			{
				WaveParticle waveParticle = rightNeighbour;
				rightNeighbour = rightNeighbour.rightNeighbour;
				num += waveParticle.CostlyUpdate((num >= 30) ? null : quadtree, deltaTime);
			}
			while (rightNeighbour != null);
			rightNeighbour = leftParticle;
			WaveParticle waveParticle2 = rightNeighbour;
			int num2 = 0;
			do
			{
				WaveParticle waveParticle3 = rightNeighbour;
				rightNeighbour = rightNeighbour.rightNeighbour;
				num2++;
				if (waveParticle3 != waveParticle2 && (waveParticle3.disallowSubdivision || rightNeighbour == null))
				{
					if (num2 > 3)
					{
						FilterRefractedDirections(waveParticle2, waveParticle3, num2);
					}
					waveParticle2 = rightNeighbour;
					num2 = 0;
				}
			}
			while (rightNeighbour != null);
		}

		private void FilterRefractedDirections(WaveParticle left, WaveParticle right, int waveLength)
		{
			WaveParticle waveParticle = left;
			int num = waveLength / 2;
			Vector2 a = default(Vector2);
			for (int i = 0; i < num; i++)
			{
				a += waveParticle.direction;
				waveParticle = waveParticle.rightNeighbour;
			}
			Vector2 b = default(Vector2);
			for (int j = num; j < waveLength; j++)
			{
				b += waveParticle.direction;
				waveParticle = waveParticle.rightNeighbour;
			}
			a.Normalize();
			b.Normalize();
			waveParticle = left;
			for (int k = 0; k < waveLength; k++)
			{
				waveParticle.direction = Vector2.Lerp(a, b, k / (waveLength - 1));
				waveParticle = waveParticle.rightNeighbour;
			}
		}
	}
}
