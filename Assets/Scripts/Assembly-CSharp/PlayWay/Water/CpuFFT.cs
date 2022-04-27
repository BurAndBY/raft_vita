using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayWay.Water
{
	public class CpuFFT
	{
		public class FFTBuffers
		{
			public float[] timed;

			public float[] pingPongA;

			public float[] pingPongB;

			public int[][] indices;

			public float[][] weights;

			public int numButterflies;

			private int resolution;

			private Dictionary<float, Vector3[]> precomputedKMap = new Dictionary<float, Vector3[]>(new FloatEqualityComparer());

			public FFTBuffers(int resolution)
			{
				this.resolution = resolution;
				timed = new float[resolution * resolution * 12];
				pingPongA = new float[resolution * 12];
				pingPongB = new float[resolution * 12];
				numButterflies = (int)(Mathf.Log(resolution) / Mathf.Log(2f));
				ButterflyFFTUtility.ComputeButterfly(resolution, numButterflies, out indices, out weights);
				for (int i = 0; i < indices.Length; i++)
				{
					int[] array = indices[i];
					for (int j = 0; j < array.Length; j++)
					{
						array[j] *= 12;
					}
				}
			}

			public Vector3[] GetPrecomputedK(float tileSize)
			{
				Vector3[] value;
				if (!precomputedKMap.TryGetValue(tileSize, out value))
				{
					int num = resolution >> 1;
					float num2 = (float)Math.PI * 2f / tileSize;
					value = new Vector3[resolution * resolution];
					int num3 = 0;
					for (int i = 0; i < resolution; i++)
					{
						int num4 = (i + num) % resolution;
						float num5 = num2 * (float)(num4 - num);
						for (int j = 0; j < resolution; j++)
						{
							int num6 = (j + num) % resolution;
							float num7 = num2 * (float)(num6 - num);
							float num8 = Mathf.Sqrt(num7 * num7 + num5 * num5);
							value[num3++] = new Vector3((num8 == 0f) ? 0f : (num7 / num8), (num8 == 0f) ? 0f : (num5 / num8), num8);
						}
					}
					precomputedKMap[tileSize] = value;
				}
				return value;
			}
		}

		private WaterTileSpectrum targetSpectrum;

		private float time;

		private int resolution;

		private static Dictionary<int, FFTBuffers> buffersCache = new Dictionary<int, FFTBuffers>();

		public void Compute(WaterTileSpectrum targetSpectrum, float time, int outputBufferIndex)
		{
			this.targetSpectrum = targetSpectrum;
			this.time = time;
			Vector2[] directionalSpectrum;
			Vector2[] displacements;
			Vector4[] forceAndHeight;
			lock (targetSpectrum)
			{
				resolution = targetSpectrum.ResolutionFFT;
				directionalSpectrum = targetSpectrum.directionalSpectrum;
				displacements = targetSpectrum.displacements[outputBufferIndex];
				forceAndHeight = targetSpectrum.forceAndHeight[outputBufferIndex];
			}
			FFTBuffers value;
			if (!buffersCache.TryGetValue(resolution, out value))
			{
				value = (buffersCache[resolution] = new FFTBuffers(resolution));
			}
			float tileSize = targetSpectrum.windWaves.UnscaledTileSizes[targetSpectrum.tileIndex];
			Vector3[] precomputedK = value.GetPrecomputedK(tileSize);
			if (targetSpectrum.directionalSpectrumDirty > 0)
			{
				ComputeDirectionalSpectra(targetSpectrum.tileIndex, directionalSpectrum, precomputedK);
				targetSpectrum.directionalSpectrumDirty--;
			}
			ComputeTimedSpectra(directionalSpectrum, value.timed, precomputedK);
			ComputeFFT(value.timed, displacements, forceAndHeight, value.indices, value.weights, value.pingPongA, value.pingPongB);
		}

		private void ComputeDirectionalSpectra(int scaleIndex, Vector2[] directional, Vector3[] kMap)
		{
			float num = 1f - targetSpectrum.water.Directionality;
			Dictionary<WaterWavesSpectrum, WaterWavesSpectrumData> cachedSpectraDirect = targetSpectrum.windWaves.SpectrumResolver.GetCachedSpectraDirect();
			int num2 = resolution * resolution;
			Vector2 windDirection = targetSpectrum.windWaves.SpectrumResolver.WindDirection;
			for (int i = 0; i < num2; i++)
			{
				directional[i].x = 0f;
				directional[i].y = 0f;
			}
			lock (cachedSpectraDirect)
			{
				foreach (WaterWavesSpectrumData value in cachedSpectraDirect.Values)
				{
					float weight = value.Weight;
					if (weight <= 0.005f)
					{
						continue;
					}
					int num3 = 0;
					Vector3[,] array = value.GetSpectrumValues(resolution)[scaleIndex];
					for (int j = 0; j < resolution; j++)
					{
						for (int k = 0; k < resolution; k++)
						{
							float x = kMap[num3].x;
							float y = kMap[num3].y;
							if (x == 0f && y == 0f)
							{
								x = windDirection.x;
								y = windDirection.y;
							}
							float num4 = windDirection.x * x + windDirection.y * y;
							float num5 = Mathf.Acos(num4 * 0.999f);
							float num6 = Mathf.Sqrt(1f + array[k, j].z * Mathf.Cos(2f * num5));
							if (num4 < 0f)
							{
								num6 *= num;
							}
							directional[num3].x += array[k, j].x * num6 * weight;
							directional[num3++].y += array[k, j].y * num6 * weight;
						}
					}
				}
			}
		}

		private void ComputeTimedSpectra(Vector2[] directional, float[] timed, Vector3[] kMap)
		{
			Vector2 windDirection = targetSpectrum.windWaves.SpectrumResolver.WindDirection;
			float gravity = targetSpectrum.water.Gravity;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < resolution; i++)
			{
				for (int j = 0; j < resolution; j++)
				{
					float x = kMap[num].x;
					float y = kMap[num].y;
					float z = kMap[num].z;
					if (x == 0f && y == 0f)
					{
						x = windDirection.x;
						y = windDirection.y;
					}
					int num3 = resolution * ((resolution - i) % resolution) + (resolution - j) % resolution;
					Vector2 vector = directional[num];
					Vector2 vector2 = directional[num3];
					float f = time * Mathf.Sqrt(gravity * z);
					float num4 = Mathf.Sin(f);
					float num5 = Mathf.Cos(f);
					float num6 = (vector.x + vector2.x) * num5 - (vector.y + vector2.y) * num4;
					float num7 = (vector.x - vector2.x) * num4 + (vector.y - vector2.y) * num5;
					timed[num2++] = num7 * x;
					timed[num2++] = num7 * y;
					timed[num2++] = 0f - num6;
					timed[num2++] = num7;
					timed[num2++] = num6 * x;
					timed[num2++] = num6 * y;
					timed[num2++] = num7;
					timed[num2++] = num6;
					timed[num2++] = num7 * x;
					timed[num2++] = (0f - num6) * x;
					timed[num2++] = num7 * y;
					timed[num2++] = (0f - num6) * y;
					num++;
				}
			}
		}

		private void ComputeFFT(float[] data, Vector2[] displacements, Vector4[] forceAndHeight, int[][] indices, float[][] weights, float[] pingPongA, float[] pingPongB)
		{
			int num = pingPongA.Length;
			int num2 = 0;
			for (int num3 = resolution - 1; num3 >= 0; num3--)
			{
				Array.Copy(data, num2, pingPongA, 0, num);
				FFT(indices, weights, ref pingPongA, ref pingPongB);
				Array.Copy(pingPongA, 0, data, num2, num);
				num2 += num;
			}
			num2 = resolution * (resolution + 1) * 12;
			for (int num4 = resolution - 1; num4 >= 0; num4--)
			{
				num2 -= 12;
				int num5 = num2;
				for (int num6 = num - 12; num6 >= 0; num6 -= 12)
				{
					num5 -= num;
					for (int i = 0; i < 12; i++)
					{
						pingPongA[num6 + i] = data[num5 + i];
					}
				}
				FFT(indices, weights, ref pingPongA, ref pingPongB);
				num5 = num2 / 12;
				for (int num7 = num - 12; num7 >= 0; num7 -= 12)
				{
					num5 -= resolution;
					forceAndHeight[num5] = new Vector4(pingPongA[num7], pingPongA[num7 + 2], pingPongA[num7 + 1], pingPongA[num7 + 7]);
					displacements[num5] = new Vector2(pingPongA[num7 + 8], pingPongA[num7 + 10]);
				}
			}
		}

		private void FFT(int[][] indices, float[][] weights, ref float[] pingPongA, ref float[] pingPongB)
		{
			int num = weights.Length;
			for (int i = 0; i < num; i++)
			{
				int[] array = indices[num - i - 1];
				float[] array2 = weights[i];
				int num2 = (resolution - 1) * 12;
				for (int num3 = array.Length - 2; num3 >= 0; num3 -= 2)
				{
					int num4 = array[num3];
					int num5 = array[num3 + 1];
					float num6 = array2[num3];
					float num7 = array2[num3 + 1];
					int num8 = num5 + 4;
					pingPongB[num2++] = pingPongA[num4++] + num7 * pingPongA[num8++] + num6 * pingPongA[num5++];
					pingPongB[num2++] = pingPongA[num4++] + num7 * pingPongA[num8++] + num6 * pingPongA[num5++];
					pingPongB[num2++] = pingPongA[num4++] + num7 * pingPongA[num8++] + num6 * pingPongA[num5++];
					pingPongB[num2++] = pingPongA[num4++] + num7 * pingPongA[num8++] + num6 * pingPongA[num5++];
					num8 = num5;
					num5 -= 4;
					pingPongB[num2++] = pingPongA[num4++] + num6 * pingPongA[num8++] - num7 * pingPongA[num5++];
					pingPongB[num2++] = pingPongA[num4++] + num6 * pingPongA[num8++] - num7 * pingPongA[num5++];
					pingPongB[num2++] = pingPongA[num4++] + num6 * pingPongA[num8++] - num7 * pingPongA[num5++];
					pingPongB[num2++] = pingPongA[num4++] + num6 * pingPongA[num8++] - num7 * pingPongA[num5++];
					num5 = num8;
					pingPongB[num2++] = pingPongA[num4++] + num6 * pingPongA[num8++] - num7 * pingPongA[num5 + 1];
					pingPongB[num2++] = pingPongA[num4++] + num6 * pingPongA[num8++] + num7 * pingPongA[num5];
					pingPongB[num2++] = pingPongA[num4++] + num6 * pingPongA[num8++] - num7 * pingPongA[num5 + 3];
					pingPongB[num2] = pingPongA[num4] + num6 * pingPongA[num8] + num7 * pingPongA[num5 + 2];
					num2 -= 23;
				}
				float[] array3 = pingPongA;
				pingPongA = pingPongB;
				pingPongB = array3;
			}
		}
	}
}
