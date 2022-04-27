using System;
using UnityEngine;

namespace PlayWay.Water
{
	public class UnifiedSpectrum : WaterWavesSpectrum
	{
		private float fetch;

		public UnifiedSpectrum(float tileSize, float gravity, float windSpeed, float amplitude, float fetch)
			: base(tileSize, gravity, windSpeed, amplitude)
		{
			this.fetch = fetch;
		}

		public override void ComputeSpectrum(Vector3[,] spectrum, float tileSizeMultiplier, int maxResolution, System.Random random)
		{
			int length = spectrum.GetLength(0);
			int num = length / 2;
			int num2 = (maxResolution - length) / 2;
			if (num2 < 0)
			{
				num2 = 0;
			}
			float num3 = (float)Math.PI * 2f / (base.TileSize * tileSizeMultiplier);
			float num4 = windSpeed;
			float num5 = 0.84f * Mathf.Pow((float)Math.Tanh(Mathf.Pow(fetch / 22000f, 0.4f)), -0.75f);
			float num6 = Mathf.Sqrt(10f);
			float num7 = 2f * gravity / 0.0529f;
			float num8 = gravity * FastMath.Pow2(num5 / num4);
			float num9 = PhaseSpeed(num8, num7);
			float num10 = num4 / num9;
			float num11 = 0.006f * Mathf.Sqrt(num10);
			float num12 = 0.08f * (1f + 4f * Mathf.Pow(num5, -3f));
			float num13 = 3.7E-05f * num4 * num4 / gravity * Mathf.Pow(num4 / num9, 0.9f);
			float num14 = num4 * 0.41f / Mathf.Log(10f / num13);
			float num15 = Mathf.Log(2f) / 4f;
			float num16 = 4f;
			float num17 = 0.13f * num14 / 0.23f;
			float num18 = 0.01f * ((!(num14 < 0.23f)) ? (1f + 3f * Mathf.Log(num14 / 0.23f)) : (1f + Mathf.Log(num14 / 0.23f)));
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < maxResolution; j++)
				{
					UnityEngine.Random.Range(1E-06f, 1f);
					float value = UnityEngine.Random.value;
					UnityEngine.Random.Range(1E-06f, 1f);
					value = UnityEngine.Random.value;
				}
			}
			for (int k = 0; k < length; k++)
			{
				float num19 = num3 * (float)(k - num);
				for (int l = 0; l < num2; l++)
				{
					UnityEngine.Random.Range(1E-06f, 1f);
					float value2 = UnityEngine.Random.value;
					UnityEngine.Random.Range(1E-06f, 1f);
					value2 = UnityEngine.Random.value;
				}
				for (int m = 0; m < length; m++)
				{
					float num20 = num3 * (float)(m - num);
					float num21 = Mathf.Sqrt(num19 * num19 + num20 * num20);
					float num22 = PhaseSpeed(num21, num7);
					float num23 = Mathf.Exp(-1.25f * FastMath.Pow2(num8 / num21));
					float f = ((!(num5 <= 1f)) ? (1.7f + 6f * Mathf.Log(num5)) : 1.7f);
					float p = Mathf.Exp((0f - FastMath.Pow2(Mathf.Sqrt(num21 / num8) - 1f)) / (2f * num12 * num12));
					float num24 = Mathf.Pow(f, p);
					float num25 = num23 * num24 * Mathf.Exp((0f - num10 / num6) * (Mathf.Sqrt(num21 / num8) - 1f));
					float num26 = 0.5f * num11 * (num9 / num22) * num25;
					float num27 = Mathf.Exp(-0.25f * FastMath.Pow2(num21 / num7 - 1f));
					float num28 = 0.5f * num18 * (0.23f / num22) * num27 * num23;
					float z = (float)Math.Tanh(num15 + num16 * Mathf.Pow(num22 / num9, 2.5f) + num17 * Mathf.Pow(0.23f / num22, 2.5f));
					float num29 = amplitude * (num26 + num28) / (num21 * num21 * num21 * num21 * 2f * (float)Math.PI);
					num29 = ((!(num29 > 0f)) ? 0f : (Mathf.Sqrt(num29) * num3 * 0.5f));
					float x = FastMath.Gauss01() * num29;
					float y = FastMath.Gauss01() * num29;
					int num30 = (k + num) % length;
					int num31 = (m + num) % length;
					if (k == num && m == num)
					{
						x = 0f;
						y = 0f;
						z = 0f;
					}
					spectrum[num30, num31] = new Vector3(x, y, z);
				}
				for (int n = 0; n < num2; n++)
				{
					UnityEngine.Random.Range(1E-06f, 1f);
					float value3 = UnityEngine.Random.value;
					UnityEngine.Random.Range(1E-06f, 1f);
					value3 = UnityEngine.Random.value;
				}
			}
			for (int num32 = 0; num32 < num2; num32++)
			{
				for (int num33 = 0; num33 < maxResolution; num33++)
				{
					UnityEngine.Random.Range(1E-06f, 1f);
					float value4 = UnityEngine.Random.value;
					UnityEngine.Random.Range(1E-06f, 1f);
					value4 = UnityEngine.Random.value;
				}
			}
		}

		private float PhaseSpeed(float k, float km)
		{
			return Mathf.Sqrt(gravity / k * (1f + FastMath.Pow2(k / km)));
		}
	}
}
