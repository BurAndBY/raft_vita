using System;
using UnityEngine;

public static class ButterflyFFTUtility
{
	private static void BitReverse(int[] indices, int N, int n)
	{
		int num = 1;
		for (int i = 0; i < N; i++)
		{
			int num2 = 0;
			int num3 = indices[i];
			for (int j = 0; j < n; j++)
			{
				int num4 = num & num3;
				num2 = (num2 << 1) | num4;
				num3 >>= 1;
			}
			indices[i] = num2;
		}
	}

	private static void ComputeWeights(Vector2[][] weights, int resolution, int numButterflies)
	{
		int num = resolution >> 1;
		int num2 = 1;
		float num3 = 1f / (float)resolution;
		for (int i = 0; i < numButterflies; i++)
		{
			int num4 = 0;
			int num5 = num2;
			Vector2[] array = weights[i];
			for (int j = 0; j < num; j++)
			{
				int num6 = num4;
				int num7 = 0;
				while (num6 < num5)
				{
					float f = (float)Math.PI * 2f * (float)num7 * (float)num * num3;
					float num8 = Mathf.Cos(f);
					float num9 = 0f - Mathf.Sin(f);
					array[num6].x = num8;
					array[num6].y = num9;
					array[num6 + num2].x = 0f - num8;
					array[num6 + num2].y = 0f - num9;
					num6++;
					num7++;
				}
				num4 += num2 << 1;
				num5 = num4 + num2;
			}
			num >>= 1;
			num2 <<= 1;
		}
	}

	private static void ComputeWeights(float[][] weights, int resolution, int numButterflies)
	{
		int num = resolution >> 1;
		int num2 = 2;
		float num3 = 1f / (float)resolution;
		for (int i = 0; i < numButterflies; i++)
		{
			int num4 = 0;
			int num5 = num2;
			float[] array = weights[i];
			for (int j = 0; j < num; j++)
			{
				int num6 = num4;
				int num7 = 0;
				while (num6 < num5)
				{
					float f = (float)Math.PI * 2f * (float)num7 * (float)num * num3;
					float num8 = Mathf.Cos(f);
					float num9 = 0f - Mathf.Sin(f);
					array[num6] = num8;
					array[num6 + 1] = num9;
					array[num6 + num2] = 0f - num8;
					array[num6 + num2 + 1] = 0f - num9;
					num6 += 2;
					num7++;
				}
				num4 += num2 << 1;
				num5 = num4 + num2;
			}
			num >>= 1;
			num2 <<= 1;
		}
	}

	private static void ComputeIndices(int[][] indices, int resolution, int numButterflies)
	{
		int num = resolution;
		int num2 = 1;
		for (int i = 0; i < numButterflies; i++)
		{
			num >>= 1;
			int num3 = num << 1;
			int num4 = 0;
			int num5 = 0;
			int num6 = num3;
			int[] array = indices[i];
			for (int j = 0; j < num2; j++)
			{
				int num7 = num5;
				int num8 = num4;
				int num9 = 0;
				while (num7 < num6)
				{
					array[num7] = num8;
					array[num7 + 1] = num8 + num;
					array[num9 + num6] = num8;
					array[num9 + num6 + 1] = num8 + num;
					num7 += 2;
					num9 += 2;
					num8++;
				}
				num5 += num3 << 1;
				num6 += num3 << 1;
				num4 += num3;
			}
			num2 <<= 1;
		}
		BitReverse(indices[numButterflies - 1], resolution << 1, numButterflies);
	}

	public static void ComputeButterfly(int resolution, int numButterflies, out int[][] indices, out Vector2[][] weights)
	{
		indices = new int[numButterflies][];
		weights = new Vector2[numButterflies][];
		for (int i = 0; i < numButterflies; i++)
		{
			indices[i] = new int[resolution << 1];
			weights[i] = new Vector2[resolution];
		}
		ComputeIndices(indices, resolution, numButterflies);
		ComputeWeights(weights, resolution, numButterflies);
	}

	public static void ComputeButterfly(int resolution, int numButterflies, out int[][] indices, out float[][] weights)
	{
		indices = new int[numButterflies][];
		weights = new float[numButterflies][];
		for (int i = 0; i < numButterflies; i++)
		{
			indices[i] = new int[resolution << 1];
			weights[i] = new float[resolution << 1];
		}
		ComputeIndices(indices, resolution, numButterflies);
		ComputeWeights(weights, resolution, numButterflies);
	}
}
