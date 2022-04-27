using System;
using UnityEngine;

namespace PlayWay.Water
{
	public class PhillipsSpectrum : WaterWavesSpectrum
	{
		private float cutoffFactor;

		public PhillipsSpectrum(float tileSize, float gravity, float windSpeed, float amplitude, float cutoffFactor)
			: base(tileSize, gravity, windSpeed, amplitude)
		{
			this.cutoffFactor = cutoffFactor;
		}

		public override void ComputeSpectrum(Vector3[,] spectrum, float tileSizeMultiplier, int maxResolution, System.Random random)
		{
			float num = base.TileSize * tileSizeMultiplier;
			float num2 = amplitude * ComputeWaveAmplitude(windSpeed);
			float num3 = 1f / num;
			int length = spectrum.GetLength(0);
			int num4 = length / 2;
			float num5 = windSpeed;
			float num6 = num5 * num5 / gravity;
			float num7 = num6 * num6;
			float num8 = FastMath.Pow2(num6 / cutoffFactor);
			float num9 = Mathf.Sqrt(num2 * Mathf.Pow(100f / num, 2.35f) / 2000000f);
			for (int i = 0; i < length; i++)
			{
				float num10 = (float)Math.PI * 2f * (float)(i - num4) * num3;
				for (int j = 0; j < length; j++)
				{
					float num11 = (float)Math.PI * 2f * (float)(j - num4) * num3;
					float num12 = Mathf.Sqrt(num10 * num10 + num11 * num11);
					float num13 = num12 * num12;
					float num14 = num13 * num13;
					float f = Mathf.Exp(-1f / (num13 * num7) - num13 * num8) / num14;
					f = num9 * Mathf.Sqrt(f);
					float x = FastMath.Gauss01() * f;
					float y = FastMath.Gauss01() * f;
					int num15 = (i + num4) % length;
					int num16 = (j + num4) % length;
					if (i == num4 && j == num4)
					{
						x = 0f;
						y = 0f;
					}
					spectrum[num15, num16] = new Vector3(x, y, 1f);
				}
			}
		}

		private static float ComputeWaveAmplitude(float windSpeed)
		{
			return 0.002f * windSpeed * windSpeed * windSpeed;
		}
	}
}
