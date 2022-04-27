using System;
using UnityEngine;

namespace PlayWay.Water
{
	public struct WaterWave : IComparable<WaterWave>
	{
		public readonly ushort u;

		public readonly ushort v;

		public readonly float kx;

		public readonly float kz;

		public readonly float nkx;

		public readonly float nky;

		public readonly float w;

		public readonly byte scaleIndex;

		public readonly float dotOffset;

		public float amplitude;

		public float cpuPriority;

		public float offset;

		public float k
		{
			get
			{
				return Mathf.Sqrt(kx * kx + kz * kz);
			}
		}

		public WaterWave(byte scaleIndex, float offsetX, float offsetZ, ushort u, ushort v, float kx, float kz, float k, float w, float amplitude, float cpuPriority)
		{
			this.scaleIndex = scaleIndex;
			dotOffset = offsetX * kx + offsetZ * kz;
			this.u = u;
			this.v = v;
			this.kx = kx;
			this.kz = kz;
			nkx = ((k == 0f) ? 0.707107f : (kx / k));
			nky = ((k == 0f) ? 0.707107f : (kz / k));
			this.amplitude = 2f * amplitude;
			offset = 0f;
			this.w = w;
			this.cpuPriority = ((!(amplitude >= 0f)) ? (0f - amplitude) : amplitude);
		}

		public void UpdateSpectralValues(Vector3[][,] spectrum, Vector2 windDirection, float directionalityInv, int resolution, float horizontalScale)
		{
			Vector3 vector = spectrum[scaleIndex][u, v];
			float num = windDirection.x * nkx + windDirection.y * nky;
			float num2 = Mathf.Acos(num * 0.999f);
			float num3 = Mathf.Sqrt(1f + vector.z * Mathf.Cos(2f * num2));
			if (num < 0f)
			{
				num3 *= directionalityInv;
			}
			float num4 = vector.x * num3;
			float num5 = vector.y * num3;
			amplitude = 2f * Mathf.Sqrt(num4 * num4 + num5 * num5);
			offset = Mathf.Atan2(Mathf.Abs(num4), Mathf.Abs(num5));
			if (num5 > 0f)
			{
				amplitude = 0f - amplitude;
				offset = 0f - offset;
			}
			if (num4 < 0f)
			{
				offset = 0f - offset;
			}
			offset += dotOffset;
			cpuPriority = ((!(amplitude >= 0f)) ? (0f - amplitude) : amplitude);
		}

		public Vector3 GetDisplacementAt(float x, float z, float t)
		{
			float num = kx * x + kz * z;
			float s;
			float c;
			FastMath.SinCos2048(num + t * w + offset, out s, out c);
			c *= amplitude;
			return new Vector3(nkx * c, s * amplitude, nky * c);
		}

		public Vector2 GetRawHorizontalDisplacementAt(float x, float z, float t)
		{
			float num = kx * x + kz * z;
			float num2 = amplitude * Mathf.Cos(num + t * w + offset);
			return new Vector2(nkx * num2, nky * num2);
		}

		public void GetForceAndHeightAt(float x, float z, float t, ref Vector4 result)
		{
			float num = kx * x + kz * z;
			int num2 = (int)((num + t * w + offset) * 325.949f) & 0x7FF;
			float num3 = FastMath.sines[num2];
			float num4 = FastMath.cosines[num2];
			float num5 = amplitude * num3;
			float num6 = amplitude * num4;
			result.x += nkx * num5;
			result.z += nky * num5;
			result.y += num6;
			result.w += num5;
		}

		public float GetHeightAt(float x, float z, float t)
		{
			float num = kx * x + kz * z;
			return amplitude * Mathf.Sin(num + t * w + offset);
		}

		public int CompareTo(WaterWave other)
		{
			return other.cpuPriority.CompareTo(cpuPriority);
		}

		private float DisplacementFunc(float r, float precomp)
		{
			return amplitude * Mathf.Cos(k * r + precomp) + r;
		}

		private float DisplacementFuncDerivative(float r, float precomp)
		{
			return 1f - k * amplitude * Mathf.Sin(k * r + precomp);
		}
	}
}
