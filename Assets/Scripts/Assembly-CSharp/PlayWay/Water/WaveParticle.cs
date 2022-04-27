using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayWay.Water
{
	public sealed class WaveParticle : IPoint2D
	{
		public Vector2 position;

		public Vector2 direction;

		public float speed;

		public float targetSpeed = 1f;

		public float baseFrequency;

		public float frequency;

		public float baseAmplitude;

		public float amplitude;

		public float fadeFactor;

		public float energyBalance;

		public float targetEnergyBalance;

		public float shoaling;

		public float invkh = 1f;

		public float targetInvKh = 1f;

		public float baseSpeed;

		public float lifetime;

		public float amplitudeModifiers;

		public float expansionEnergyLoss;

		public bool isShoreWave;

		public bool isAlive = true;

		public bool disallowSubdivision;

		public WaveParticle leftNeighbour;

		public WaveParticle rightNeighbour;

		public WaveParticlesGroup group;

		private static Stack<WaveParticle> waveParticlesCache;

		private static float[] amplitudeFuncPrecomp;

		private static float[] frequencyFuncPrecomp;

		public Vector2 Position
		{
			get
			{
				return position;
			}
		}

		public Vector4 PackedParticleData
		{
			get
			{
				return new Vector4(direction.x * 2f * (float)Math.PI / frequency, direction.y * 2f * (float)Math.PI / frequency, shoaling, frequency);
			}
		}

		public Vector3 VertexData
		{
			get
			{
				return new Vector3(position.x, position.y, amplitude);
			}
		}

		static WaveParticle()
		{
			waveParticlesCache = new Stack<WaveParticle>();
			amplitudeFuncPrecomp = new float[2048];
			frequencyFuncPrecomp = new float[2048];
			for (int i = 0; i < 2048; i++)
			{
				double num = ((float)i + 0.49f) / 2047f;
				double kh = 4.0 * (1.0 - Math.Pow(1.0 - num, 0.33333333));
				amplitudeFuncPrecomp[i] = ComputeAmplitudeAtShore(kh);
				frequencyFuncPrecomp[i] = Mathf.Sqrt(1f / ComputeWavelengthAtShore(kh));
			}
		}

		private WaveParticle(Vector2 position, Vector2 direction, float baseFrequency, float baseAmplitude, float lifetime, bool isShoreWave)
		{
			this.position = position;
			this.direction = direction;
			this.baseFrequency = baseFrequency;
			this.baseAmplitude = baseAmplitude;
			fadeFactor = 0f;
			frequency = baseFrequency;
			amplitude = baseAmplitude;
			this.isShoreWave = isShoreWave;
			baseSpeed = 3.2f * Mathf.Sqrt(9.81f / baseFrequency);
			this.lifetime = lifetime;
			CostlyUpdate(null, 0.1f);
		}

		public static WaveParticle Create(Vector2 position, Vector2 direction, float baseFrequency, float baseAmplitude, float lifetime, bool isShoreWave)
		{
			WaveParticle waveParticle;
			if (waveParticlesCache.Count != 0)
			{
				waveParticle = waveParticlesCache.Pop();
				waveParticle.position = position;
				waveParticle.direction = direction;
				waveParticle.baseFrequency = baseFrequency;
				waveParticle.baseAmplitude = baseAmplitude;
				waveParticle.fadeFactor = 0f;
				waveParticle.isShoreWave = isShoreWave;
				waveParticle.baseSpeed = 3.2f * Mathf.Sqrt(9.81f / baseFrequency);
				waveParticle.amplitude = baseAmplitude;
				waveParticle.frequency = baseFrequency;
				waveParticle.targetSpeed = 1f;
				waveParticle.invkh = 1f;
				waveParticle.targetInvKh = 1f;
				waveParticle.energyBalance = 0f;
				waveParticle.shoaling = 0f;
				waveParticle.speed = 0f;
				waveParticle.lifetime = lifetime;
				waveParticle.amplitudeModifiers = 0f;
				waveParticle.expansionEnergyLoss = 0f;
				waveParticle.isAlive = true;
				waveParticle.disallowSubdivision = false;
				if (waveParticle.leftNeighbour != null || waveParticle.rightNeighbour != null)
				{
					waveParticle.leftNeighbour = null;
					waveParticle.rightNeighbour = null;
				}
				waveParticle.CostlyUpdate(null, 0.1f);
			}
			else
			{
				waveParticle = new WaveParticle(position, direction, baseFrequency, baseAmplitude, lifetime, isShoreWave);
			}
			return (waveParticle.baseAmplitude == 0f) ? null : waveParticle;
		}

		public void Destroy()
		{
			baseAmplitude = (amplitude = 0f);
			isAlive = false;
			if (leftNeighbour != null)
			{
				leftNeighbour.rightNeighbour = rightNeighbour;
				leftNeighbour.disallowSubdivision = true;
			}
			if (rightNeighbour != null)
			{
				rightNeighbour.leftNeighbour = leftNeighbour;
				rightNeighbour.disallowSubdivision = true;
				if (leftNeighbour == null)
				{
					group.leftParticle = rightNeighbour;
				}
			}
			leftNeighbour = null;
			rightNeighbour = null;
		}

		public void AddToCache()
		{
			waveParticlesCache.Push(this);
		}

		public WaveParticle Clone(Vector2 position)
		{
			WaveParticle waveParticle = Create(position, direction, baseFrequency, baseAmplitude, lifetime, isShoreWave);
			if (waveParticle != null)
			{
				waveParticle.amplitude = amplitude;
				waveParticle.frequency = frequency;
				waveParticle.speed = speed;
				waveParticle.targetSpeed = targetSpeed;
				waveParticle.energyBalance = energyBalance;
				waveParticle.shoaling = shoaling;
				waveParticle.group = group;
			}
			return waveParticle;
		}

		public void Update(float deltaTime, float step, float invStep)
		{
			if (lifetime > 0f)
			{
				if (fadeFactor != 1f)
				{
					fadeFactor += deltaTime;
					if (fadeFactor > 1f)
					{
						fadeFactor = 1f;
					}
				}
			}
			else
			{
				fadeFactor -= deltaTime;
				if (fadeFactor <= 0f)
				{
					Destroy();
					return;
				}
			}
			if (targetEnergyBalance < energyBalance)
			{
				float num = step * 0.05f;
				energyBalance = energyBalance * (1f - num) + targetEnergyBalance * num;
			}
			else
			{
				float num2 = step * 0.008f;
				energyBalance = energyBalance * (1f - num2) + targetEnergyBalance * num2;
			}
			baseAmplitude += deltaTime * energyBalance;
			baseAmplitude *= step * expansionEnergyLoss + 1f;
			if (baseAmplitude <= 0.01f)
			{
				Destroy();
				return;
			}
			speed = invStep * speed + step * targetSpeed;
			float num3 = speed + energyBalance * -20f;
			invkh = invStep * invkh + step * targetInvKh;
			int num4 = (int)(2047f * (1f - invkh * invkh * invkh) - 0.49f);
			frequency = baseFrequency * ((num4 < 2048) ? frequencyFuncPrecomp[num4] : 1f);
			amplitude = fadeFactor * baseAmplitude * ((num4 < 2048) ? amplitudeFuncPrecomp[num4] : 1f);
			shoaling = amplitudeModifiers * (0f - energyBalance) / amplitude;
			amplitude *= amplitudeModifiers;
			float num5 = num3 * deltaTime;
			position.x += direction.x * num5;
			position.y += direction.y * num5;
		}

		public int CostlyUpdate(WaveParticlesQuadtree quadtree, float deltaTime)
		{
			float num;
			if (frequency < 0.025f)
			{
				float x = position.x + direction.x / frequency;
				float z = position.y + direction.y / frequency;
				num = Mathf.Max(StaticWaterInteraction.GetTotalDepthAt(position.x, position.y), StaticWaterInteraction.GetTotalDepthAt(x, z));
			}
			else
			{
				num = StaticWaterInteraction.GetTotalDepthAt(position.x, position.y);
			}
			if (num <= 0.001f)
			{
				Destroy();
				return 0;
			}
			UpdateWaveParameters(deltaTime, num);
			int numSubdivisions = 0;
			if (quadtree != null && !disallowSubdivision)
			{
				if (leftNeighbour != null)
				{
					Subdivide(quadtree, leftNeighbour, this, ref numSubdivisions);
				}
				if (rightNeighbour != null)
				{
					Subdivide(quadtree, this, rightNeighbour, ref numSubdivisions);
				}
			}
			return numSubdivisions;
		}

		private void UpdateWaveParameters(float deltaTime, float depth)
		{
			lifetime -= deltaTime;
			targetInvKh = 1f - 0.25f * baseFrequency * depth;
			if (targetInvKh < 0f)
			{
				targetInvKh = 0f;
			}
			int num = (int)(baseFrequency * depth * 512f);
			targetSpeed = baseSpeed * ((num < 2048) ? FastMath.positiveTanhSqrtNoZero[num] : 1f);
			if (targetSpeed < 0.5f)
			{
				targetSpeed = 0.5f;
			}
			targetEnergyBalance = baseFrequency * -0.0004f;
			float num2 = 0.224f / frequency;
			if (num2 < amplitude)
			{
				targetEnergyBalance += 0.04f * (num2 - amplitude);
			}
			if (leftNeighbour != null && rightNeighbour != null && !disallowSubdivision)
			{
				Vector2 vector = new Vector2(rightNeighbour.position.y - leftNeighbour.position.y, leftNeighbour.position.x - rightNeighbour.position.x);
				float num3 = Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y);
				if (num3 > 0.001f)
				{
					if (vector.x * direction.x + vector.y * direction.y < 0f)
					{
						num3 = 0f - num3;
					}
					vector.x /= num3;
					vector.y /= num3;
					float num4 = 0.06f * deltaTime;
					if (num4 > 0.06f)
					{
						num4 = 0.06f;
					}
					direction.x = direction.x * (1f - num4) + vector.x * num4;
					direction.y = direction.y * (1f - num4) + vector.y * num4;
					float num5 = Mathf.Sqrt(direction.x * direction.x + direction.y * direction.y);
					direction.x /= num5;
					direction.y /= num5;
				}
				expansionEnergyLoss = -1f + 0.5f * (direction.x * (leftNeighbour.direction.x + rightNeighbour.direction.x) + direction.y * (leftNeighbour.direction.y + rightNeighbour.direction.y));
				if (expansionEnergyLoss < -1f)
				{
					expansionEnergyLoss = -1f;
				}
				if (leftNeighbour.disallowSubdivision)
				{
					leftNeighbour.expansionEnergyLoss = expansionEnergyLoss;
				}
				if (rightNeighbour.disallowSubdivision)
				{
					rightNeighbour.expansionEnergyLoss = expansionEnergyLoss;
				}
				amplitudeModifiers = 1f;
			}
			else
			{
				amplitudeModifiers = 0.5f;
			}
			if (isShoreWave)
			{
				int num6 = (int)(depth * 5.12f);
				if (num6 < 2048)
				{
					amplitudeModifiers *= 1f - FastMath.positiveTanhSqrtNoZero[num6];
				}
			}
		}

		private void Subdivide(WaveParticlesQuadtree quadtree, WaveParticle left, WaveParticle right, ref int numSubdivisions)
		{
			Vector2 vector = left.position - right.position;
			float magnitude = vector.magnitude;
			if (!(magnitude * frequency > 1f) || !(magnitude > 1f) || quadtree.FreeSpace == 0)
			{
				return;
			}
			WaveParticle waveParticle = Create(right.position + vector * 0.5f, (left.direction + right.direction) * 0.5f, (left.baseFrequency + right.baseFrequency) * 0.5f, (left.baseAmplitude + right.baseAmplitude) * 0.5f, (left.lifetime + right.lifetime) * 0.5f, left.isShoreWave);
			if (waveParticle != null)
			{
				waveParticle.group = left.group;
				waveParticle.amplitude = (left.amplitude + right.amplitude) * 0.5f;
				waveParticle.frequency = (left.frequency + right.frequency) * 0.5f;
				waveParticle.speed = (left.speed + right.speed) * 0.5f;
				waveParticle.targetSpeed = (left.targetSpeed + right.targetSpeed) * 0.5f;
				waveParticle.energyBalance = (left.energyBalance + right.energyBalance) * 0.5f;
				waveParticle.shoaling = (left.shoaling + right.shoaling) * 0.5f;
				if (quadtree.AddElement(waveParticle))
				{
					waveParticle.leftNeighbour = left;
					waveParticle.rightNeighbour = right;
					left.rightNeighbour = waveParticle;
					right.leftNeighbour = waveParticle;
				}
				numSubdivisions++;
			}
		}

		private static float ComputeAmplitudeAtShore(double kh)
		{
			double num = Math.Cosh(kh);
			return (float)Math.Sqrt(2.0 * num * num / (Math.Sinh(2.0 * kh) + 2.0 * kh));
		}

		private static float ComputeWavelengthAtShore(double kh)
		{
			return (float)Math.Pow(Math.Tanh(Math.Pow(kh * Math.Tanh(kh), 0.75)), 0.666666);
		}
	}
}
